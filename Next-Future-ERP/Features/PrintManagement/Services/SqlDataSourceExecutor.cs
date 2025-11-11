using Microsoft.Data.SqlClient;
using Next_Future_ERP.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Next_Future_ERP.Features.PrintManagement.Services
{
    public class SqlDataSourceExecutor : IDataSourceExecutor
    {
        private readonly AppDbContext _context;

        public SqlDataSourceExecutor(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, object>> GetDocumentDataAsync(
            int templateVersionId, int documentTypeId, long documentId,
            int companyId, int? branchId, string? locale)
        {
            // 1) جيب الـ Main DataSource للإصدار
            var ds = await _context.TemplateDataSources
                .AsNoTracking()
                .Where(d => d.TemplateVersionId == templateVersionId && d.IsMain)
                .FirstOrDefaultAsync();

            if (ds == null) throw new InvalidOperationException("لا يوجد Main DataSource لهذه النسخة.");
            if (!string.Equals(ds.SourceType, "proc", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("حالياً ندعم نوع proc فقط.");

            // 2) نفّذ البروسيدجر
            var dsResult = await ExecProcAsync(ds.SourceName, new (string, object?)[] {
                ("@DocumentId", documentId),
                ("@CompanyId", companyId),
                ("@BranchId", branchId),
                ("@Locale", locale)
            }, timeoutSec: ds.TimeoutSec);

            // 3) شكّل الداتا حسب نوع المستند للتماشي مع القالب
            return documentTypeId switch
            {
                6 => ShapeReceiptVoucher(dsResult), // RV
                // زوّد لاحقاً: 7 => ShapePaymentVoucher(dsResult), 8/9 => ShapeCredit/DebitNote(dsResult), 1 => ShapeJournalVoucher(dsResult)...
                _ => throw new NotSupportedException($"Mapping غير مضاف لنوع المستند {documentTypeId}.")
            };
        }

        private async Task<DataSet> ExecProcAsync(string procName, (string, object?)[] parameters, int timeoutSec)
        {
            var connString = _context.Database.GetConnectionString()!; // نفس قاعدة NFDB
            using var conn = new SqlConnection(connString);
            using var cmd = new SqlCommand(procName, conn) { CommandType = CommandType.StoredProcedure, CommandTimeout = timeoutSec };
            foreach (var (n, v) in parameters) cmd.Parameters.AddWithValue(n, v ?? DBNull.Value);
            using var da = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            await conn.OpenAsync();
            await Task.Run(() => da.Fill(ds));
            return ds;
        }

        // ==== RV (سند قبض) ====
        private Dictionary<string, object> ShapeReceiptVoucher(DataSet ds)
        {
            var t = ds.Tables.Count > 0 ? ds.Tables[0] : null;
            if (t == null || t.Rows.Count == 0) throw new InvalidOperationException("الإجراء لم يرجع بيانات.");
            var row = t.Rows[0];

            string Get(params string[] names)
            {
                foreach (var n in names) if (t.Columns.Contains(n)) return Convert.ToString(row[n]) ?? "";
                return "";
            }
            DateTime? GetDate(params string[] names)
            {
                foreach (var n in names)
                    if (t.Columns.Contains(n) && row[n] != DBNull.Value) return Convert.ToDateTime(row[n]);
                return null;
            }

            var receipt = new Dictionary<string, object?>
            {
                ["number"] = Get("VoucherNumber", "ReceiptNo", "DocumentNumber"),
                ["date"] = (GetDate("VoucherDate", "ReceiptDate", "DocumentDate") ?? DateTime.Now).ToString("yyyy-MM-dd"),
                ["amount"] = Get("Amount", "TotalAmount"),
                ["method"] = Get("PaymentMethod", "Method"),
                ["description"] = Get("Description", "Notes", "Narration")
            };

            var payer = new Dictionary<string, object?> { ["name"] = Get("PayerName", "CustomerName", "VendorName", "AccountName") };

            var company = new Dictionary<string, object?>
            {
                ["name"] = Get("CompanyName"),
                ["address"] = Get("CompanyAddress")
            };

            var currency = Get("Currency", "CurrencyCode", "CurrencyName");
            if (string.IsNullOrWhiteSpace(currency)) currency = "SAR";

            return new Dictionary<string, object>
            {
                ["receipt"] = receipt,
                ["payer"] = payer,
                ["company"] = company,
                ["currency"] = currency
            };
        }
    }
}