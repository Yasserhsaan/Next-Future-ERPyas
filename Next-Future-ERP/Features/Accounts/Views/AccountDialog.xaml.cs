using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Next_Future_ERP.Core.Services.Contracts;
using Next_Future_ERP.Data;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Next_Future_ERP.Views
{
    public partial class AccountDialog : IWindow
    {
        public event EventHandler AccountSaved;
        public Account Account { get; private set; }
        private Account _parentAccount;
        private readonly bool isEdit;
        private AccountLevelInfo? _levelInfo;
        private ICollectionView? _categoryOptionsView;
        private TextBox? _categoryEditBox;
        private string? _selectedCategoryKey;

        // إضافة بسيطة: فلاغ لمنع اعتبار تغييرات التهيئة كتغيير مستخدم
        private bool _initCategorySelection;
        private readonly AccountLevelPrivligeService _service;
        private readonly IAccountClassService _accountClassService;

        private void cmbCategoryOptions_Loaded(object sender, RoutedEventArgs e)
        {
            _categoryOptionsView = CollectionViewSource.GetDefaultView(cmbCategoryOptions.ItemsSource);
            if (_categoryOptionsView != null)
                _categoryOptionsView.Filter = CategoryFilter;

            _categoryEditBox = (TextBox?)cmbCategoryOptions.Template.FindName("PART_EditableTextBox", cmbCategoryOptions);
            if (_categoryEditBox != null)
            {
                _categoryEditBox.TextChanged += (_, __) =>
                {
                    _categoryOptionsView?.Refresh();
                    if (!cmbCategoryOptions.IsDropDownOpen) cmbCategoryOptions.IsDropDownOpen = true;
                };
                _categoryEditBox.KeyDown += CategoryEditBox_KeyDown; // إدخال ↵ يثبّت الاختيار
            }
        }

        private void CategoryEditBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            // خذ أول عنصر ظاهر بعد الفلترة
            AccountCategoryOption? first = null;

            if (_categoryOptionsView != null)
                first = _categoryOptionsView.Cast<object>()
                                            .OfType<AccountCategoryOption>()
                                            .FirstOrDefault();

            if (first == null && cmbCategoryOptions.ItemsSource is IEnumerable<AccountCategoryOption> all)
            {
                var q = _categoryEditBox?.Text?.Trim();
                if (!string.IsNullOrEmpty(q))
                {
                    first = all.FirstOrDefault(o =>
                        string.Equals(o.CategoryKey, q, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(o.CategoryNameAr, q, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(o.CategoryNameEn, q, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (first != null)
            {
                cmbCategoryOptions.SelectedItem = first;   // سيشغّل SelectionChanged ويحدّث المفتاح
                cmbCategoryOptions.IsDropDownOpen = false;
            }
        }

        private void cmbCategoryOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var key = cmbCategoryOptions.SelectedValue as string;

            if (!string.IsNullOrWhiteSpace(key))
            {
                _selectedCategoryKey = key;
                if (Account != null) Account.AccountCategoryKey = key;
            }
            else
            {
                _selectedCategoryKey = null;
                if (Account != null) Account.AccountCategoryKey = null;
            }

            if (_categoryEditBox != null && cmbCategoryOptions.SelectedItem is AccountCategoryOption opt)
                _categoryEditBox.Text = opt.CategoryNameAr;
        }


        private bool CategoryFilter(object obj)
        {
            if (obj is not AccountCategoryOption o) return false;
            var q = _categoryEditBox?.Text;
            if (string.IsNullOrWhiteSpace(q)) return true;
            q = q.Trim();
            return (o.CategoryNameAr?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                || (o.CategoryNameEn?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                || (o.CategoryKey?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void CategoryEditBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            // حدّث الفلترة مع كل حرف
            _categoryOptionsView?.Refresh();

            // افتح القائمة أثناء الكتابة لعرض النتائج
            if (!cmbCategoryOptions.IsDropDownOpen)
                cmbCategoryOptions.IsDropDownOpen = true;
        }

        public AccountDialog(Account parentAccount = null, Account editAccount = null)
        {
            try
            {
                InitializeComponent();
                _service = new AccountLevelPrivligeService(new AppDbContext());
                _accountClassService = App.ServiceProvider.GetRequiredService<IAccountClassService>();
                _parentAccount = parentAccount;
                Account = editAccount;
                isEdit = editAccount != null;

                if (isEdit)
                {
                    // في حالة التعديل
                    lblParentAccount.Text = $"تعديل الحساب: {Account.AccountCode} - {Account.AccountNameAr}";
                    txtCode.Text = !string.IsNullOrEmpty(parentAccount?.AccountCode)
                      ? Account.AccountCode.Replace(parentAccount.AccountCode, "")
                      : Account.AccountCode;
                    Account.AccountGroupId = Account.AccountGroupId;
                    txtNameAr.Text = Account.AccountNameAr;
                    txtNameEn.Text = Account.AccountNameEn;
                    txtNotes.Text = Account.Notes;
                    chkUsesCostCenter.IsChecked = Account.UsesCostCenter;
                    chkIsActive.IsChecked = Account.IsActive;

                    if (Account.AccountClassification.HasValue)
                        cmbAccountClassification.SelectedValue = Account.AccountClassification;

                    if (Account.Nature == 1)
                        radioDebit.IsChecked = true;
                    else if (Account.Nature == 2)
                        radioCredit.IsChecked = true;

                    // SelectedValuePath=Tag (string) في XAML، وعناصر الكومبو تحمل Tag="1" و Tag="2"
                    // لذلك يجب تمرير قيمة نصية مطابقة لوسم العنصر، لا قيمة رقمية بايت
                    cmbAccounttype.SelectedValue = Account.AccountType.ToString();

                    cmbCashFlowType.SelectedIndex = Account.TypeOfCashFlow switch
                    {
                        1 => 0,
                        2 => 1,
                        3 => 2,
                        0 => 3,
                        _ => -1
                    };

                    // تحميل عملات الحساب عند التعديل (إن وجدت)
                    LoadAccountCurrencies();
                }
                else if (parentAccount != null)
                {
                    // في حالة الإضافة
                    lblParentAccount.Text = $"الحساب الرئيسي: {parentAccount.AccountCode} - {parentAccount.AccountNameAr}";
                    txtParentCode.Text = parentAccount.AccountCode;
                    txtAccountType.Text = $"نوع الحساب: {GetAccountTypeName(parentAccount.AccountType)}";
                    txtAccountLevel.Text = $"المستوى: {parentAccount.AccountLevel}";
                    cmbAccounttype.SelectedValue = 1;
                    _parentAccount.AccountGroupId = _parentAccount.AccountGroupId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ أثناء تهيئة نافذة الحساب:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadAccountCurrencies()
        {
            try
            {
                using var db = new AppDbContext();
                var rows = await db.AccountCurrencies
                    .Where(x => x.AccountId == Account.AccountId)
                    .Include(x => x.Currency)
                    .ToListAsync();

                var items = rows
                    .Select(x => $"{x.Currency?.CurrencyNameAr} ({x.Currency?.CurrencySymbol})")
                    .ToList();

                lstAccountCurrencies.ItemsSource = items;
                if (lstAccountCurrenciesBottom != null)
                    lstAccountCurrenciesBottom.ItemsSource = items;
            }
            catch { }
        }

        private async Task downloadPrivligeAsync()
        {
            var list = await _service.GetAllAsync();

            cmbPriv.ItemsSource = list;
            cmbPriv.DisplayMemberPath = nameof(AccountLevelPrivlige.AccountPrivligeAname);
            cmbPriv.SelectedValuePath = nameof(AccountLevelPrivlige.AccountPrivligeId);

            // عرض القيمة الحالية عند التعديل (إن وجدت)
            if (isEdit && Account?.AccountLevelPrivlige != null)
            {
                cmbPriv.SelectedValue = Account.AccountLevelPrivlige; // int?
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadAccountClassifications();
             await   downloadPrivligeAsync();
                string codeForLevelInfo = null;

                if (!isEdit && _parentAccount != null)
                {
                    codeForLevelInfo = _parentAccount.AccountCode;
                }
                else if (isEdit && Account != null)
                {
                    codeForLevelInfo = !string.IsNullOrWhiteSpace(Account?.ParentAccountCode)
                        ? Account.ParentAccountCode
                        : Account.AccountCode;
                }

                if (!string.IsNullOrWhiteSpace(codeForLevelInfo))
                    await LoadLevelInfoAsync(codeForLevelInfo);

                await LoadCategoryOptionsAsync();

                // إذا كان النوع الحالي فرعي، حمّل العملات
                if (cmbAccounttype.SelectedValue?.ToString() == "2")
                {
                    LoadAccountCurrencies();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل أثناء التحميل:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbAccounttype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // عند تغيير النوع إلى فرعي، حمّل عملات الحساب إن كنا في وضع تعديل
            if (cmbAccounttype.SelectedValue?.ToString() == "2")
            {
                if (isEdit && Account != null)
                    LoadAccountCurrencies();
                // في وضع الإضافة، ليست هناك عملات بعد
            }
        }

        private async Task LoadAccountClassifications()
        {
            try
            {
                var accountClasses = await _accountClassService.GetAllAsync();

                cmbAccountClassification.ItemsSource = accountClasses;
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل التصنيفات:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveNewAccountAsync()
        {
            if (_parentAccount.AccountType == 2)
            {
                MessageBox.Show("لا يمكن إضافة حساب فرعي على حساب فرعي.");
                return;
            }

            Account = new Account();

            FillAccountData(); // نستخدم دالة تعبئة البيانات المشتركة

            var service = new AccountsService();
            await service.AddAsync(Account);

            MessageBox.Show("✅ تم إضافة الحساب بنجاح.");
        }

        private async Task UpdateAccountAsync()
        {
            if (Account == null)
            {
                MessageBox.Show("لا يوجد حساب لتعديله.");
                return;
            }

            FillAccountData(); // نستخدم نفس الدالة لتحديث القيم

            var service = new AccountsService();
            await service.UpdateAsync(Account);

            MessageBox.Show("✅ تم تعديل الحساب بنجاح.");
        }

        private void FillAccountData()
        {
            string prefix = _parentAccount?.AccountCode ?? "";
            Account.AccountCode = prefix + txtCode.Text.Trim();

            if (Account.AccountCode.Length > 20)
                throw new Exception("تجاوز في طول رقم الحساب الحد الأقصى للطول (20).");

            Account.AccountNameAr = txtNameAr.Text.Trim();
            Account.AccountNameEn = txtNameEn.Text.Trim();
            Account.Notes = txtNotes.Text.Trim();

            // Account Classification
            if (cmbAccountClassification.SelectedValue != null &&
                byte.TryParse(cmbAccountClassification.SelectedValue.ToString(), out byte classification))
            {
                Account.AccountClassification = classification;
            }
            else
            {
                Account.AccountClassification = null;
            }
            // التقاط قيمة الكومبو وتخزينها في العمود AccountLevelPrivlige (int?)
            if (cmbPriv?.SelectedValue is int i)
            {
                Account.AccountLevelPrivlige = i;
            }
            else if (cmbPriv?.SelectedValue is byte b) // احتياط لو المصدر كان tinyint
            {
                Account.AccountLevelPrivlige = (int)b;
            }
            else
            {
                // لم يُختر شيء
                Account.AccountLevelPrivlige = null;
            }

            // Nature
            Account.Nature = radioDebit.IsChecked == true ? (byte)1
                             : radioCredit.IsChecked == true ? (byte)2
                             : (byte?)null;

            // Account Type
            if (cmbAccounttype.SelectedValue is string accTypeStr && byte.TryParse(accTypeStr, out var accType))
                Account.AccountType = accType;
            else if (cmbAccounttype.SelectedItem is ComboBoxItem accItem && byte.TryParse(accItem.Tag?.ToString(), out accType))
                Account.AccountType = accType;
            else
                Account.AccountType = 1;

            // Cash Flow Type (SelectedValuePath=Tag)، نتوقع Tag = "0"/"1"/"2"/"3"
            if (cmbCashFlowType.SelectedValue != null &&
                byte.TryParse(cmbCashFlowType.SelectedValue.ToString(), out byte cashFlowType))
            {
                Account.TypeOfCashFlow = cashFlowType;
            }

            Account.UsesCostCenter = chkUsesCostCenter.IsChecked ?? false;
            Account.IsActive = chkIsActive.IsChecked ?? true;
            Account.AccountGroupId = _parentAccount != null ? _parentAccount.AccountGroupId : Account.AccountGroupId;

            Account.ParentAccountCode = _parentAccount?.AccountCode;
            Account.AccountLevel = (byte)((_parentAccount?.AccountLevel ?? 0) + 1);
            Account.CreatedAt = DateTime.Now;

            // CategoryKey من الكومبو/البحث
            if (!string.IsNullOrWhiteSpace(_selectedCategoryKey))
            {
                Account.AccountCategoryKey = _selectedCategoryKey;
            }
            else if (cmbCategoryOptions.SelectedValue is string keyFromCombo)
            {
                Account.AccountCategoryKey = keyFromCombo;
            }
            else if (cmbCategoryOptions.SelectedItem is AccountCategoryOption sel)
            {
                Account.AccountCategoryKey = sel.CategoryKey;
            }
            else
            {
                // محاولة أخيرة: من نص البحث إن تطابق بالضبط
                var q = _categoryEditBox?.Text?.Trim();
                if (!string.IsNullOrEmpty(q) && cmbCategoryOptions.ItemsSource is IEnumerable<AccountCategoryOption> all)
                {
                    var m = all.FirstOrDefault(o =>
                        string.Equals(o.CategoryKey, q, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(o.CategoryNameAr, q, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(o.CategoryNameEn, q, StringComparison.OrdinalIgnoreCase));
                    Account.AccountCategoryKey = m?.CategoryKey;
                }
            }

            // إضافة آمنة لا تغيّر المنطق: التقط SelectedValue النهائي إن وُجد
            var sv = cmbCategoryOptions.SelectedValue as string;
            if (!string.IsNullOrWhiteSpace(sv))
                Account.AccountCategoryKey = sv;
        }
        private void CommitCategoryFromEditor()
        {
            // لو فيه اختيار فعلي، حدّث الكاش وارجع
            if (cmbCategoryOptions.SelectedItem is AccountCategoryOption sel)
            {
                _selectedCategoryKey = sel.CategoryKey;
                return;
            }

            // لو المستخدم كتب نص فقط بدون اختيار، حاول مطابقته بعنصر
            var q = _categoryEditBox?.Text?.Trim();
            if (string.IsNullOrEmpty(q)) return;
            if (cmbCategoryOptions.ItemsSource is not IEnumerable<AccountCategoryOption> all) return;

            var m = all.FirstOrDefault(o =>
                string.Equals(o.CategoryKey, q, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.CategoryNameAr, q, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.CategoryNameEn, q, StringComparison.OrdinalIgnoreCase));

            if (m != null)
            {
                // ثبّت الاختيار فعليًا (سيشغّل SelectionChanged ويحدّث _selectedCategoryKey)
                cmbCategoryOptions.SelectedItem = m;
            }
        }
        private void CommitPrivFromEditor()
        {
            if (cmbPriv.SelectedItem is AccountLevelPrivlige) return;

            if (cmbPriv.ItemsSource is IEnumerable<AccountLevelPrivlige> all &&
                cmbPriv.Template?.FindName("PART_EditableTextBox", cmbPriv) is TextBox tb)
            {
                var q = tb.Text?.Trim();
                if (string.IsNullOrEmpty(q)) return;

                var match = all.FirstOrDefault(o =>
                       string.Equals(o.AccountPrivligeAname, q, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(o.AccountPrivligeEname, q, StringComparison.OrdinalIgnoreCase)
                    || o.AccountPrivligeId.ToString() == q);

                if (match != null) cmbPriv.SelectedItem = match; // سيحدّث SelectedValue
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtNameAr.Text))
            {
                MessageBox.Show("يرجى تعبئة رمز الحساب والاسم بالعربية.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CommitPrivFromEditor();
                CommitCategoryFromEditor();
                if (!isEdit)
                    await SaveNewAccountAsync();
                else
                    await UpdateAccountAsync();

                DialogResult = true;
                AccountSaved?.Invoke(this, EventArgs.Empty);
                Close();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show("خطأ في قاعدة البيانات:\n" + inner, "خطأ EF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private string GetAccountTypeName(byte type)
        {
            return type switch
            {
                1 => "رئيسي",
                2 => "فرعي",
                3 => "عام",
                _ => "غير معروف"
            };
        }

        private async Task LoadCategoryOptionsAsync()
        {
            try
            {
                // لو في التعديل: استخدم المفتاح المخزّن بالحساب (تم الإبقاء على منطقك كما هو مع تعديل بسيط للفلاغ فقط)
                string? keyFromEdit = null;
                if (isEdit)
                {
                    keyFromEdit = _levelInfo?.AccountCategory?.Trim(); // إن كان اسمها هكذا
                    if (string.IsNullOrWhiteSpace(keyFromEdit))
                        keyFromEdit = _levelInfo?.AccountCategory?.Trim(); // أو هكذا عند بعض النماذج
                }

                // لو في الإضافة: استخدم المفتاح القادم من معلومات المستوى
                string? keyFromLevel = _levelInfo?.AccountCategory?.Trim();

                string? key = !string.IsNullOrWhiteSpace(keyFromEdit) ? keyFromEdit : keyFromLevel;
                MessageBox.Show("key=" + key);
                var options = await _accountClassService.GetAccountCategoryOptionsAsync(
                    string.IsNullOrWhiteSpace(key) ? null : key
                );
                cmbCategoryOptions.ItemsSource = options;

                if (isEdit)
                {
                    keyFromEdit = Account?.AccountCategoryKey?.Trim()
                                   ?? Account?.AccountCategoryKey?.Trim(); // لو اسم قديم

                    if (!string.IsNullOrWhiteSpace(keyFromEdit))
                    {
                        // لفّ التعيين بالفلاغ حتى لا يُحسب تغيير مستخدم
                        _initCategorySelection = true;
                        cmbCategoryOptions.SelectedValue = keyFromEdit;
                        _initCategorySelection = false;

                        _selectedCategoryKey = keyFromEdit; // خزّنها للاستعمال في FillAccountData
                    }
                }
                else if (options.Count == 1)
                {
                    _initCategorySelection = true;
                    cmbCategoryOptions.SelectedValue = options[0].CategoryKey;
                    _initCategorySelection = false;

                    _selectedCategoryKey = options[0].CategoryKey;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل فئات التصنيف:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadLevelInfoAsync(string accountCode)
        {
            try
            {
                // ملاحظة: عدّل طريقة إنشاء الخدمة حسب مشروعك (DI أو new)
                var accountsService = new AccountsService();

                _levelInfo = await accountsService.GetAccountLevelInfoAsync(accountCode);
                if (_levelInfo == null) return;

                // عرض المستوى الحالي/الأقصى في رأس النافذة (بدون تغيير المنطق الأساسي)
                txtAccountLevel.Text = $"المستوى: {_levelInfo.CurrentLevel} / {_levelInfo.MaxLevel}";

                // لو لا يسمح بإنشاء ابن جديد
                if (!_levelInfo.CanCreateChild)
                {
                    MessageBox.Show("لا يمكن إضافة حساب فرعي لهذا الحساب وفق إعدادات مستويات الحساب.",
                                    "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // ممكن تعطيل زر الحفظ إن رغبت:
                    // btnSave.IsEnabled = false;  // لو عندك اسم للزر
                }

                // اقتراح كود الابن التالي
                // إن كانت الدالة ترجع الكود كاملاً (NextChildCode = "10101")
                // ونريد عرض الجزء اللاحق فقط في txtCode (مع بقاء txtParentCode يحوي كود الأب):
                if (!string.IsNullOrWhiteSpace(_parentAccount?.AccountCode) &&
                    !string.IsNullOrWhiteSpace(_levelInfo.NextChildCode) &&
                    _levelInfo.NextChildCode.StartsWith(_parentAccount.AccountCode))
                {
                    var suffix = _levelInfo.NextChildCode.Substring(_parentAccount.AccountCode.Length);
                    txtCode.Text = suffix; // يعرض الجزء المطلوب إدخاله فقط
                }
                else
                {
                    // في حال الإضافة على الجذر مثلاً أو لم يطابق البادئة
                    txtCode.Text = _levelInfo.NextChildCode;
                }

                // يمكنك أيضاً إظهار الفئة/التصنيف لو حابب:
                txtAccountType.Text = $"الفئة: {_levelInfo.AccountCategory}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل استعلام معلومات مستوى الحساب:\n" + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
