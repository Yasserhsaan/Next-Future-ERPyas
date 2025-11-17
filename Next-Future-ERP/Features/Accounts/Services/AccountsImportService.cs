// Features/Accounts/Services/AccountsImportService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Next_Future_ERP.Data;
using Next_Future_ERP.Models;

public class AccountsImportService
{
    private readonly AppDbContext _ctx;

    public AccountsImportService(AppDbContext ctx) => _ctx = ctx;

    // إنشاء نموذج Excel
    public async Task CreateTemplateAsync(string filePath)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("ChartOfAccounts");

        // رأس الأعمدة
        ws.Cell(1, 1).Value = "AccountCode";         // إجباري
        ws.Cell(1, 2).Value = "AccountNameAr";       // إجباري
        ws.Cell(1, 3).Value = "AccountNameEn";       // اختياري
        ws.Cell(1, 4).Value = "ParentAccountCode";   // اختياري، فارغ لمستوى 1
        ws.Cell(1, 5).Value = "AccountType";         // 1=Header, 2=Leaf (إجباري)
        ws.Cell(1, 6).Value = "AccountCategoryKey";  // إجباري للحركي (Leaf)
        ws.Cell(1, 7).Value = "UsesCostCenter";      // true/false
        ws.Cell(1, 8).Value = "Nature";              // 1=مدين, 2=دائن, (اختياري)
        ws.Cell(1, 9).Value = "ClosingAccountType";  // 1=P&L, 2=Balance (اختياري)
        ws.Cell(1, 10).Value = "IsActive";           // true/false

        // تنسيق
        ws.Row(1).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();

        // تبويب معلومات
        var info = wb.AddWorksheet("Readme");
        info.Cell(1, 1).Value = "التعليمات";
        info.Cell(2, 1).Value =
            "- AccountCode يتبع هيكل الشركة من V_AccountStructureSettings (مثل 1-2-2-3).\n" +
            "- ParentAccountCode فارغ للجذور (Level1). وإلا يجب أن يكون موجوداً.\n" +
            "- AccountType: 1=Header (غير حركي), 2=Leaf (حركي). Leaf يتطلب AccountCategoryKey.\n" +
            "- سيتم التحقق من عدم التكرار ومن صحة الأطوال والبادئات.";

        // تلميح StartNumber من الجدول (اختياري)
        var sets = await _ctx.Set<V_AccountStructureSettingsRow>()
            .FromSqlRaw("SELECT * FROM dbo.V_AccountStructureSettings")
            .ToListAsync();

        var hint = string.Join(", ",
     sets.Select(s =>
         $"{s.CategoryNameEn}:{s.StartNumber} ({s.AccountNature}={(s.AccountNature == 1 ? "مدين" : "دائن")})"));

        info.Cell(5, 1).Value = $"StartNumbers: {hint}";
        var categories = await _ctx.Set<AccountCategoryRoll>()
    .AsNoTracking()
    .ToListAsync();

        if (categories.Count > 0)
        {
            // نستخدم خاصية Display الموجودة في الموديل
            //var catHint = string.Join(", ", categories.Select(c => c.Display));
            // أو لو حابب تضيف النوع كود (أصول/خصوم...) مع CategoryType:
            var catHint = string.Join(", ", categories.Select(c =>
               $"{c.CategoryKey} - {c.CategoryNameAr} / {c.CategoryNameEn} ({c.CategoryType})"));

            info.Cell(6, 1).Value = $"فئات الحسابات الفرعية: {catHint}";
        }


        wb.SaveAs(filePath);
    }

    // استيراد من Excel مع التحققات
    public async Task<ImportResult> ImportAsync(string filePath, int companyId, int? branchId = null, bool ensureRoots = true, CancellationToken ct = default)
    {
        var errors = new List<string>();
        var toInsert = new List<Account>();

        // إعدادات الهيكل
        var s = await _ctx.Set<V_AccountStructureSettingsRow>()
            .FromSqlRaw("SELECT * FROM dbo.V_AccountStructureSettings")
            .AsNoTracking()
            .ToListAsync(ct);

        if (s.Count == 0)
            return ImportResultFactory.Fail("لا توجد إعدادات دليل في V_AccountStructureSettings.");

        int accountLen = s[0].AccountNumberLength;   // الطول الكلي
        int levels = s[0].NumberLever;           // عدد المستويات

        // وزّع الأطوال ديناميكياً: 1 + (2 * (levels-2)) + last
        var levelLengths = BuildLevelLengths(accountLen, levels);   // [1,2,2,3] مثلاً
        var cum = Cumulate(levelLengths);                           // [1,3,5,8] مثلاً

        // تجهيز خرائط للتحقق السريع
        var existingCodeList = await _ctx.Accounts
            .Where(a => a.CompanyId == companyId && a.BranchId == (branchId ?? a.BranchId))
            .Select(a => a.AccountCode)
            .ToListAsync(ct);

        var existingCodes = new HashSet<string>(existingCodeList, StringComparer.OrdinalIgnoreCase);

        var startDigits = new HashSet<string>(s.Select(x => x.StartNumber.ToString()), StringComparer.OrdinalIgnoreCase);

        // اختياري: تهيئة الجذور أولاً
        if (ensureRoots)
        {
            await _ctx.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_EnsureMainAccountsSeeded @CompanyId={0}, @BranchId={1}",
                companyId, branchId);
        }

        using var wb = new XLWorkbook(filePath);
        var ws = wb.Worksheet("ChartOfAccounts");

        int row = 2;
        while (!ws.Row(row).IsEmpty())
        {
            string r(string col) => ws.Cell($"{col}{row}").GetString().Trim();

            var code = r("A");
            var nameAr = r("B");
            var nameEn = r("C");
            var parent = r("D");
            var typeS = r("E");
            var catKey = r("F");
            var usesCC = r("G");
            var nature = r("H");
            var closeT = r("I");
            var isAct = r("J");

            // تحقق أساسي
            if (string.IsNullOrWhiteSpace(code))
                errors.Add($"سطر {row}: AccountCode إجباري.");
            if (string.IsNullOrWhiteSpace(nameAr))
                errors.Add($"سطر {row}: AccountNameAr إجباري.");
            if (!int.TryParse(typeS, out int type) || (type != 1 && type != 2))
                errors.Add($"سطر {row}: AccountType يجب أن يكون 1 أو 2.");

            // الطول والبادئة
            if (code.Length > 0 && code.Length > accountLen)
                errors.Add($"سطر {row}: طول AccountCode={code.Length} يتجاوز المسموح ({accountLen}).");

            // تحقق توزيع الطول (يجب أن يساوي أحد المجاميع التراكمية)
            int level = IndexOfEqual(cum, code.Length) + 1;
            if (level <= 0)
                errors.Add($"سطر {row}: طول الكود {code.Length} لا يطابق مستويات الدليل.");

            // تحقق من StartNumber للفئة (أول خانة)
            var start = code.Substring(0, Math.Min(1, code.Length));
            if (!startDigits.Contains(start))
                errors.Add($"سطر {row}: البادئة '{start}' غير معرفة ضمن الإعدادات.");

            // تحقق الأب
            if (level > 1 && string.IsNullOrWhiteSpace(parent))
                errors.Add($"سطر {row}: ParentAccountCode مطلوب للمستويات > 1.");
            if (level > 1 && !string.IsNullOrWhiteSpace(parent))
            {
                if (parent.Length != cum[level - 2])
                    errors.Add($"سطر {row}: طول ParentAccountCode لا يطابق مستوى الأب المتوقع.");
            }

            // نوع الحساب مقابل المستوى الأخير
            if (type == 1 && level == levels)
                errors.Add($"سطر {row}: لا يمكن أن يكون الحساب الأخير Header (يجب Leaf).");
            if (type == 2 && level < levels)
                errors.Add($"سطر {row}: لا يمكن أن يكون Leaf قبل آخر مستوى.");

            // فئة الحساب للحركي
            if (type == 2 && string.IsNullOrWhiteSpace(catKey))
                errors.Add($"سطر {row}: AccountCategoryKey إجباري للحسابات الحركية.");

            // تكرار
            if (existingCodes.Contains(code))
                errors.Add($"سطر {row}: الكود '{code}' موجود مسبقاً في النظام.");
            var pendingCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // داخل الحلقة لكل صف:
            if (!pendingCodes.Add(code))
                errors.Add($"سطر {row}: الكود '{code}' مكرر داخل الملف.");

       

            // حضّر الكيان (سوف يتم إدخاله لاحقًا إذا لم توجد أخطاء)
            toInsert.Add(new Account
            {
                CompanyId = companyId,
                BranchId = branchId ?? 0,
                AccountCode = code,
                AccountNameAr = nameAr,
                AccountNameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn,
                ParentAccountCode = string.IsNullOrWhiteSpace(parent) ? null : parent,
                AccountLevel = (byte)Math.Max(level, 1),
                AccountType = (byte)type,
                AccountCategoryKey = string.IsNullOrWhiteSpace(catKey) ? null : catKey,
                UsesCostCenter = ParseBool(usesCC),
                Nature = ParseByte(nature),
                ClosingAccountType = ParseByte(closeT),
                IsActive = ParseBool(isAct) ?? true,
                CreatedAt = DateTime.Now
            });

            row++;
        }

        // إن وُجدت أخطاء، أعدّها للمستخدم
        if (errors.Count > 0)
            return ImportResultFactory.Fail(errors);

        // تحقق وجود الآباء فعليًا (موجود سابقًا أو ضمن الملف)
        var allCodes = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);
        foreach (var c in toInsert.Select(x => x.AccountCode))
            allCodes.Add(c);

        foreach (var acc in toInsert.Where(x => x.AccountLevel > 1))
        {
            if (acc.ParentAccountCode is null || !allCodes.Contains(acc.ParentAccountCode))
                errors.Add($"الكود '{acc.AccountCode}': الأب '{acc.ParentAccountCode}' غير موجود (ملف/نظام).");
        }
        if (errors.Count > 0)
            return ImportResultFactory.Fail(errors);

        // إدراج فعلي (Batch)
        _ctx.Accounts.AddRange(toInsert);
        await _ctx.SaveChangesAsync(ct);

        return ImportResultFactory.Success(toInsert.Count);
    }

    // ====== Helpers ======
    private static List<int> BuildLevelLengths(int totalLen, int levels)
    {
        var list = new List<int> { 1 };
        int remain = totalLen - 1;
        int midCount = Math.Max(0, levels - 2);
        int midLen = midCount > 0 ? 2 : 0;
        for (int i = 0; i < midCount; i++) list.Add(midLen);
        int last = remain - (midCount * midLen);
        list.Add(last);
        return list;
    }

    private static List<int> Cumulate(List<int> lens)
    {
        var res = new List<int>(lens.Count);
        int sum = 0;
        foreach (var d in lens) { sum += d; res.Add(sum); }
        return res;
    }

    private static int IndexOfEqual(List<int> list, int value)
    {
        for (int i = 0; i < list.Count; i++) if (list[i] == value) return i;
        return -1;
    }

    private static bool? ParseBool(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Trim().ToLower() switch
        {
            "1" or "true" or "yes" or "y" => true,
            "0" or "false" or "no" or "n" => false,
            _ => null
        };
    }

    private static byte? ParseByte(string s)
    {
        if (byte.TryParse(s, out var b)) return b;
        return null;
    }
}

public record ImportResult(bool Ok, string? Message, List<string>? Errors, int Inserted);

public static class ImportResultFactory
{
    public static ImportResult Success(int n) => new(true, $"تم استيراد {n} حساب(ات) بنجاح.", null, n);
    public static ImportResult Fail(string msg) => new(false, msg, new List<string> { msg }, 0);
    public static ImportResult Fail(List<string> errs) => new(false, "فشل الاستيراد.", errs, 0);
}

