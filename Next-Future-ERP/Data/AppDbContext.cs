using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Next_Future_ERP.Data.Models;
using Next_Future_ERP.Features.Accounts.Models;
using Next_Future_ERP.Features.InitialSystem.Models;

using Next_Future_ERP.Features.Inventory.Models;
using Next_Future_ERP.Features.Items.Models;
using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Models.Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Permissions.Models;
using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Models.Next_Future_ERP.Features.Payments.Models;

using Next_Future_ERP.Features.Payments.Models;
using Next_Future_ERP.Features.Payments.Models.Next_Future_ERP.Features.Payments.Models;

using Next_Future_ERP.Features.PrintManagement.Models;
using Next_Future_ERP.Features.Suppliers.Models;
using Next_Future_ERP.Features.Warehouses.Models;
using Next_Future_ERP.Features.Inventory.Models;
using Next_Future_ERP.Features.Purchases.Models;
using Next_Future_ERP.Features.StoreReceipts.Models;
using Next_Future_ERP.Features.PurchaseInvoices.Models;
using Next_Future_ERP.Features.StoreIssues.Models;
using Next_Future_ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Data
{
    public class AppDbContext : DbContext
    {

        public DbSet<Nextuser> Nextuser { get; set; }
        public DbSet<CostCenter> CostCenter { get; set; }
        public DbSet<AccountClass> AccountClasses { get; set; }
        // public DbSet<AccountCategoryRoll> AccountCategoryRolls { get; set; } // جديد
        public DbSet<AccountLevelPrivlige> AccountLevelPrivliges { get; set; }
        public DbSet<V_AccountStructureSettingsRow> V_AccountStructureSettings => Set<V_AccountStructureSettingsRow>();
        public DbSet<AccountLevelInfo> AccountLevelInfos => Set<AccountLevelInfo>();
        public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();
        public DbSet<Account> Accounts { get; set; }
        public DbSet<CompanyTaxProfile> CompanyTaxProfiles { get; set; }
        public DbSet<NextCurrency> NextCurrencies { get; set; }
        public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }
        public DbSet<AccountCurrency> AccountCurrencies { get; set; }
        public DbSet<Next_Future_ERP.Features.Accounts.Models.OpeningBalanceBatch> OpeningBalanceBatches { get; set; }
        public DbSet<Next_Future_ERP.Features.Accounts.Models.OpeningBalanceLine> OpeningBalanceLines { get; set; }
        public DbSet<Next_Future_ERP.Features.Accounts.Models.AccountBalance> AccountBalances { get; set; }

        // Print Management Entities
        public DbSet<PrintTemplate> PrintTemplates { get; set; }
        public DbSet<TemplateVersion> TemplateVersions { get; set; }
        public DbSet<TemplateContent> TemplateContents { get; set; }
        public DbSet<TemplateDataSource> TemplateDataSources { get; set; }
        public DbSet<PrintJob> PrintJobs { get; set; }
        public DbSet<PrintAsset> PrintAssets { get; set; }

        public DbSet<SalesSetting> SalesSettings { get; set; }
        public DbSet<PosSetting> PosSettings { get; set; }
        public DbSet<Next_Future_ERP.Features.PosStations.Models.PosStation> PosStations { get; set; }
        public DbSet<Next_Future_ERP.Features.PosOperators.Models.PosOperator> PosOperators { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<GeneralJournalEntry> GeneralJournalEntries { get; set; }
        public DbSet<GeneralJournalEntryDetail> GeneralJournalEntryDetails { get; set; }
        public DbSet<UnitModel> Units { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }

        public DbSet<Bank> Banks { get; set; }

        public DbSet<FundCurrencyLimit> FundCurrencyLimits { get; set; }
        public DbSet<AccountCategoryRoll> AccountCategoryRoll { get; set; }
        
        // Initial System Entities
        public DbSet<CompanyInfoModel> CompanyInfo { get; set; }
        public DbSet<BranchModel> Branches { get; set; }
        public DbSet<AccountingSetupModel> AccountingSetup { get; set; }
        public DbSet<FinancialPeriodsSettingModlel> FinancialPeriods { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<BankCurrencyDetail> BankCurrencyDetails { get; set; }
        public DbSet<PaymentVoucher> PaymentVouchers { get; set; }
        public DbSet<PaymentVoucherDetail> PaymentVoucherDetails { get; set; }
        public DbSet<ReceiptVoucher> ReceiptVouchers { get; set; }
        public DbSet<ReceiptVoucherDetail> ReceiptVoucherDetails { get; set; }
        // using Next_Future_ERP.Models;  <-- تأكد من وجود هذا الـusing
        public DbSet<DebitCreditNotification> DebitCreditNotifications => Set<DebitCreditNotification>();
        public DbSet<DebitCreditNoteDetail> DebitCreditNoteDetails => Set<DebitCreditNoteDetail>();

        public DbSet<ValuationGroup> ValuationGroups { get; set; }
        public DbSet<ValuationGroupAccount> ValuationGroupAccounts { get; set; }
        public DbSet<PaymentTerm> PaymentTerms { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierPaymentMethod> SupplierPaymentMethods { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemUnit> ItemUnits { get; set; }
        public DbSet<ItemPrice> ItemPrices { get; set; }
        public DbSet<Next_Future_ERP.Features.Items.Models.ItemCost> ItemCosts { get; set; }
        public DbSet<ItemSupplier> ItemSuppliers { get; set; }
        public DbSet<ItemType> ItemTypes { get; set; }
        public DbSet<ItemBatch> ItemBatches { get; set; }
        public DbSet<ItemComponent> ItemComponents { get; set; }
        public DbSet<InventoryBalance> InventoryBalances { get; set; }
        
        // Inventory Opening Tables
        public DbSet<InventoryOpeningHeader> InventoryOpeningHeaders { get; set; }
        public DbSet<InventoryOpeningDetail> InventoryOpeningDetails { get; set; }
		// Purchases unified transaction tables
		public DbSet<PurchaseTxn> PurchaseTxns { get; set; }
     	public DbSet<PurchaseTxnDetail> PurchaseTxnDetails { get; set; }
        public DbSet<StoreReceipt> StoreReceipts { get; set; }
        public DbSet<StoreReceiptDetailed> StoreReceiptsDetailed { get; set; }
        
        // Store Issues
        public DbSet<IssueDestination> IssueDestinations { get; set; }
        public DbSet<StoreIssue> StoreIssues { get; set; }
        public DbSet<StoreIssueDetail> StoreIssuesDetailed { get; set; }
        
        public DbSet<PurchaseAP> PurchaseAPs { get; set; }
        public DbSet<PurchaseAPDetail> PurchaseAPDetails { get; set; }
        
        // Audit Trail
        
        // Permissions System
        public DbSet<MenuForm> MenuForms { get; set; }
        public DbSet<SysRole> SysRoles { get; set; }
        public DbSet<MenuRole> MenuRoles { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }



      


        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    optionsBuilder.UseSqlServer("Server=.;Database=NextFutureERP;Trusted_Connection=True;");
            //}
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Nextuser>()
             .ToTable("Nextuser")
             .HasIndex(u => u.Name)
             .IsUnique();
            modelBuilder.Entity<CostCenter>()
              .ToTable("CostCenters")
              .HasIndex(u => u.CostCenterId)
              .IsUnique();
            // AccountClasses
            modelBuilder.Entity<AccountLevelPrivlige>(e =>
            {
                e.ToTable("AccountLevelPrivlige", "dbo");   // ← الاسم الصحيح
                e.HasKey(x => x.AccountPrivligeId);         // أو e.HasNoKey() لو بدون PK/قراءة فقط
            });
            modelBuilder.Entity<AccountCategoryOption>().HasNoKey();
            var b = modelBuilder.Entity<AccountClass>();
            b.ToTable("AccountClasses");
            b.Ignore(b => b.CategoryNameArDisplay);
            b.HasKey(e => e.AccountClassId);

            // هوية تُولَّد تلقائياً من SQL Server
            b.Property(e => e.AccountClassId)
             .ValueGeneratedOnAdd()
             .UseIdentityColumn(1, 1);

            b.Property(e => e.AccountClassAname).IsRequired().HasMaxLength(200);
            b.Property(e => e.AccountClassEname).IsRequired().HasMaxLength(200);
            b.Property(e => e.CategoryKey).HasMaxLength(50);

            modelBuilder.Entity<AccountLevelInfo>().HasNoKey();
            modelBuilder.Entity<V_AccountStructureSettingsRow>().HasNoKey();

            modelBuilder.Entity<Account>()
           .ToTable("Accounts")
           .HasIndex(u => u.AccountId)
           .IsUnique();

           
            modelBuilder.Entity<NextCurrency>()
          .ToTable("NextCurrencies")
          .HasIndex(u => u.CurrencyId)
          .IsUnique();
            modelBuilder.Entity<CurrencyExchangeRate>()
           .ToTable("CurrencyExchangeRates")
           .HasIndex(u => u.CurrencyId)
           .IsUnique();

            modelBuilder.Entity<AccountCurrency>(e =>
            {
                e.ToTable("AccountCurrencies", "dbo");
                e.HasKey(x => x.AccountCurrencyId);
                e.Property(x => x.AccountCurrencyId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.IsStopped).HasDefaultValue(false);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("sysdatetime()");
            });

            // ===== Opening Balance Configuration =====
            modelBuilder.Entity<Next_Future_ERP.Features.Accounts.Models.OpeningBalanceBatch>(e =>
            {
                e.ToTable("OpeningBalanceBatch", "dbo");
                e.HasKey(x => x.BatchId);
                e.Property(x => x.BatchId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.DocNo).HasMaxLength(30);
                e.Property(x => x.Description).HasMaxLength(200);
                e.Property(x => x.DocDate).HasColumnType("date");
                e.Property(x => x.CreatedAt).HasDefaultValueSql("sysutcdatetime()");
                e.Property(x => x.PostedAt).HasColumnType("datetime2");
            });

            modelBuilder.Entity<Next_Future_ERP.Features.Accounts.Models.OpeningBalanceLine>(e =>
            {
                e.ToTable("OpeningBalanceLine", "dbo");
                e.HasKey(x => x.LineId);
                e.Property(x => x.LineId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.TransactionDebit).HasColumnType("decimal(18,4)");
                e.Property(x => x.TransactionCredit).HasColumnType("decimal(18,4)");
                e.Property(x => x.CompanyDebit).HasColumnType("decimal(18,4)");
                e.Property(x => x.CompanyCredit).HasColumnType("decimal(18,4)");
                e.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)");
                e.Property(x => x.Note).HasMaxLength(200);
            });

            modelBuilder.Entity<Next_Future_ERP.Features.Accounts.Models.AccountBalance>(e =>
            {
                e.ToTable("AccountBalances", "dbo");
                e.HasKey(x => x.BalanceId);
                e.Property(x => x.BalanceId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.OpeningDebit).HasColumnType("decimal(18,4)");
                e.Property(x => x.OpeningCredit).HasColumnType("decimal(18,4)");
                e.Property(x => x.PeriodDebit).HasColumnType("decimal(18,4)");
                e.Property(x => x.PeriodCredit).HasColumnType("decimal(18,4)");
                e.Property(x => x.ClosingDebit).HasColumnType("decimal(18,4)");
                e.Property(x => x.ClosingCredit).HasColumnType("decimal(18,4)");
                e.Property(x => x.LastMovementAt).HasColumnType("datetime2");
                e.Property(x => x.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysutcdatetime()");
                
                // فهرس مركب فريد
                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.AccountId, x.CurrencyId, x.PeriodType, x.FiscalYear, x.FiscalMonth, x.FiscalDay, x.CostCenterId })
                 .IsUnique()
                 .HasDatabaseName("UQ_AccountBalances_Key");
            });

            modelBuilder.Entity<SalesSetting>()
       .ToTable("Salessettings")
       .HasIndex(u => u.SalesSettingId)
       .IsUnique();
            modelBuilder.Entity<PosSetting>()
   .ToTable("Possettings")
   .HasIndex(u => u.PosSettingId)
   .IsUnique();
            modelBuilder.Entity<DocumentType>()
.ToTable("DocumentTypes")
.HasIndex(u => u.DocumentTypeId)
.IsUnique();
            modelBuilder.Entity<GeneralJournalEntry>()
.ToTable("GeneralJournalEntries")
.HasIndex(u => u.JournalEntryId)
.IsUnique();
            modelBuilder.Entity<GeneralJournalEntryDetail>()
.ToTable("GeneralJournalEntriesDetailed")
.HasIndex(u => u.JournalEntryId)
.IsUnique();
            // ===== Company_Tax_Profile =====
            modelBuilder.Entity<CompanyTaxProfile>(e =>
            {
                e.ToTable("Company_Tax_Profile");
                e.HasKey(x => x.ProfileId);

                e.Property(x => x.VATRegistrationNumber).HasMaxLength(30).IsRequired();
                e.Property(x => x.BranchVATNumber).HasMaxLength(30);
                e.Property(x => x.TaxOffice).HasMaxLength(100);
                e.Property(x => x.ActivityCode).HasMaxLength(20);

                // فهارس مفيدة للبحث
                e.HasIndex(x => x.CompanyId);
                e.HasIndex(x => x.BranchId);
                e.HasIndex(x => x.VATRegistrationNumber);
                e.HasIndex(x => x.CreatedAt);
            });

            // ===== كيانات البحث (Keyless) =====
            // CompanyOption: CompanyId, CompanyName  (من جدول Companies)
            modelBuilder.Entity<CompanyOption>().HasNoKey();

            // BranchOption: BranchId, CompanyId, BranchName (من جدول Branches)
            modelBuilder.Entity<BranchOption>().HasNoKey();

            // نتيجة البحث المجمّعة مع JOIN (تطابق أعمدة الخدمة)
            modelBuilder.Entity<CompanyTaxProfileLookup>().HasNoKey();


            // Warehouses - Units
            var unit = modelBuilder.Entity<UnitModel>();
            unit.ToTable("Units");
            unit.HasKey(u => u.UnitID);

            // Warehouses - Categories
            var category = modelBuilder.Entity<CategoryModel>();
            category.ToTable("Categories");
            category.HasKey(c => c.CategoryID);
            category.Property(c => c.CategoryID)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn(1, 1);
            category.Property(c => c.CategoryCode).IsRequired().HasMaxLength(20);
            category.Property(c => c.CategoryName).IsRequired().HasMaxLength(100);
            category.Property(c => c.Description).HasMaxLength(500);

            // Self-referencing relationship for parent-child categories
            category.HasOne<CategoryModel>()
                .WithMany()
                .HasForeignKey(c => c.ParentCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // Initial System Entity Configurations
            modelBuilder.Entity<CompanyInfoModel>()
                .ToTable("SystemSettings")
                  .Ignore(c => c.LogoUrl)
                .HasKey(c => c.CompId);
            modelBuilder.Entity<DocumentSequence>()
                 .HasKey(x => new { x.DocumentTypeId, x.BranchId });

            modelBuilder.Entity<BranchModel>()
                .ToTable("Branches").HasKey(k => k.BranchId);


            modelBuilder.Entity<AccountingSetupModel>()
                .ToTable("AccountingSettings").HasKey(c => c.ComiId);


            modelBuilder.Entity<FinancialPeriodsSettingModlel>()
                 .Ignore(c => c.GeneratedPeriods)
                .ToTable("Financialperiodssettings")
               ;
            // ===== Funds =====
            modelBuilder.Entity<Fund>()
                .ToTable("Funds")
                .HasKey(f => f.FundId);

            modelBuilder.Entity<Fund>()
                .HasIndex(f => f.FundId).IsUnique();

            modelBuilder.Entity<Fund>()
                .Property(f => f.FundName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Fund>()
                .Property(f => f.AccountNumber)
                .HasMaxLength(50)
                .IsRequired();

            // enum -> tinyint
            modelBuilder.Entity<Fund>()
                .Property(f => f.FundType)
                .HasConversion<byte>()
                .IsRequired();

            // StopDate كنوع date (بدل datetime2 الافتراضي)
            modelBuilder.Entity<Fund>()
                .Property(f => f.StopDate)
                .HasColumnType("date");

            // العلاقات مع الشركة والفرع
            modelBuilder.Entity<Fund>()
                .HasOne(f => f.Company)
                .WithMany() // إن كان لديك ICollection<Fund> على CompanyInfoModel غيّرها لـ WithMany(c => c.Funds)
                .HasForeignKey(f => f.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Fund>()
                .HasOne(f => f.Branch)
                .WithMany() // إن كان لديك ICollection<Fund> على BranchModel غيّرها لـ WithMany(b => b.Funds)
                .HasForeignKey(f => f.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // فهارس مساعدة اختيارية
            modelBuilder.Entity<Fund>()
                .HasIndex(f => new { f.CompanyId, f.BranchId, f.FundName });


            // ===== FundCurrencyLimits =====
            modelBuilder.Entity<FundCurrencyLimit>()
                .ToTable("FundCurrencyLimits")
                .HasKey(l => l.LimitId);
            modelBuilder.Entity<AccountCategoryRoll>(entity =>
            {
                entity.ToTable("AccountCategoryRoll");
                entity.HasKey(e => e.CategoryKey);
            });



            modelBuilder.Entity<FundCurrencyLimit>()
                .HasIndex(l => l.LimitId).IsUnique();

            // دقة الأعمدة العشرية لتطابق DDL (decimal(18,2))
            modelBuilder.Entity<FundCurrencyLimit>()
                .Property(l => l.MinCash).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<FundCurrencyLimit>()
                .Property(l => l.MaxCash).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<FundCurrencyLimit>()
                .Property(l => l.MinSettlement).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<FundCurrencyLimit>()
                .Property(l => l.MaxSettlement).HasColumnType("decimal(18,2)");

            // علاقة: صندوق واحد له عدة حدود عملات
            modelBuilder.Entity<FundCurrencyLimit>()
                .HasOne(l => l.Fund)
                .WithMany(f => f.CurrencyLimits)
                .HasForeignKey(l => l.FundId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة العملة (نربط بـ NextCurrencies)
            modelBuilder.Entity<FundCurrencyLimit>()
                .HasOne(l => l.Currency)
                .WithMany() // لو عندك ICollection<FundCurrencyLimit> داخل NextCurrency غيّرها لـ WithMany(c => c.FundCurrencyLimits)
                .HasForeignKey(l => l.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // قيد فريد: كل صندوق لا يمتلك إلا صف واحد لكل عملة
            modelBuilder.Entity<FundCurrencyLimit>()
                .HasIndex(l => new { l.FundId, l.CurrencyId })
                .IsUnique();

            modelBuilder.Entity<PaymentVoucher>()
        .ToTable("PaymentVouchers")
        .HasKey(v => v.VoucherID);

            modelBuilder.Entity<PaymentVoucher>()
                .Property(v => v.VoucherType)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<PaymentVoucher>()
                .Property(v => v.ExchangeRate)
                .HasColumnType("decimal(18,6)");

            modelBuilder.Entity<PaymentVoucher>()
                .Property(v => v.LocalAmount)
                .HasColumnType("decimal(18,3)");

            modelBuilder.Entity<PaymentVoucher>()
                .Property(v => v.ForeignAmount)
                .HasColumnType("decimal(18,3)");

            // علاقة الرأس-تفاصيل
            modelBuilder.Entity<PaymentVoucher>()
                .HasMany(v => v.Details)
                .WithOne()
                .HasForeignKey(d => d.VoucherID)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== PaymentVoucherDetail =====
            modelBuilder.Entity<PaymentVoucherDetail>()
                .ToTable("PaymentVoucherDetails")
                .HasKey(d => d.DetailID);

            modelBuilder.Entity<PaymentVoucherDetail>()
                .Property(d => d.DebitCurncy).HasColumnType("decimal(18,3)");
            modelBuilder.Entity<PaymentVoucherDetail>()
                .Property(d => d.CreditCurncy).HasColumnType("decimal(18,3)");
            modelBuilder.Entity<PaymentVoucherDetail>()
                .Property(d => d.DebitCompCurncy).HasColumnType("decimal(18,3)");
            modelBuilder.Entity<PaymentVoucherDetail>()
                .Property(d => d.CrediComptCurncy).HasColumnType("decimal(18,3)");
            // Banks
            modelBuilder.Entity<Bank>(e =>
            {
                e.ToTable("Banks");
                e.HasKey(x => x.BankId);

                e.Property(x => x.BankName).HasMaxLength(100).IsRequired();
                e.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
                e.Property(x => x.ContactInfo).HasMaxLength(500);
                e.Property(x => x.StopReason).HasMaxLength(255);
                e.Property(x => x.StopDate).HasColumnType("date");
                e.Property(x => x.CreatedAt).HasColumnType("datetime2");
                e.Property(x => x.UpdatedAt).HasColumnType("datetime2");

                // فهرس فريد اختياري: عدم تكرار اسم بنك بنفس الشركة والفرع
                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.BankName }).IsUnique();

                e.HasMany(x => x.CurrencyDetails)
                 .WithOne(d => d.Bank!)
                 .HasForeignKey(d => d.BankId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // BankCurrencyDetails
            modelBuilder.Entity<BankCurrencyDetail>(e =>
            {
                e.ToTable("BankCurrencyDetails");
                e.HasKey(x => x.DetailId);

                e.Property(x => x.BankAccountNumber).HasMaxLength(50).IsRequired();
                e.Property(x => x.CreatedAt).HasColumnType("datetime2");

                // عملة واحدة لكل بنك (تفصيل وحيد لكل عملة)
                e.HasIndex(x => new { x.BankId, x.CurrencyId }).IsUnique();
            });


            modelBuilder.Entity<ReceiptVoucher>()
        .ToTable("ReceiptVouchers")
        .HasKey(v => v.VoucherID);

            modelBuilder.Entity<ReceiptVoucher>()
                .Property(v => v.VoucherType)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<ReceiptVoucher>()
                .Property(v => v.ExchangeRate)
                .HasColumnType("decimal(18,6)");

            modelBuilder.Entity<ReceiptVoucher>()
                .Property(v => v.LocalAmount)
                .HasColumnType("decimal(18,3)");

            modelBuilder.Entity<ReceiptVoucher>()
                .Property(v => v.ForeignAmount)
                .HasColumnType("decimal(18,3)");

            // علاقة الرأس-تفاصيل
            modelBuilder.Entity<ReceiptVoucher>()
                .HasMany(v => v.Details)
                .WithOne()
                .HasForeignKey(d => d.VoucherID)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== ReceiptVoucherDetail =====
            modelBuilder.Entity<ReceiptVoucherDetail>()
                .ToTable("ReceiptVoucherDetails")
                .HasKey(d => d.DetailID);

            modelBuilder.Entity<ReceiptVoucherDetail>()
                .Property(d => d.DebitCurncy).HasColumnType("decimal(18,3)");
            modelBuilder.Entity<ReceiptVoucherDetail>()
                .Property(d => d.CreditCurncy).HasColumnType("decimal(18,3)");
            modelBuilder.Entity<ReceiptVoucherDetail>()
                .Property(d => d.DebitCompCurncy).HasColumnType("decimal(18,3)");
            modelBuilder.Entity<ReceiptVoucherDetail>()
                .Property(d => d.CrediComptCurncy).HasColumnType("decimal(18,3)");

            modelBuilder.Entity<ValuationGroup>(e =>
            {
                e.ToTable("ValuationGroup", "dbo");
                e.HasKey(x => x.ValuationGroupId);
                e.Property(x => x.ValuationGroupId)
                    .HasColumnName("ValuationGroup")
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1);

                e.Property(x => x.CompanyId).IsRequired();
                e.Property(x => x.ValuationGroupCode).HasMaxLength(20).IsRequired();
                e.Property(x => x.ValuationGroupName).HasMaxLength(100).IsRequired();
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.IsActive);
                e.Property(x => x.CreatedDate);
                e.Property(x => x.ModifiedDate);
                e.Property(x => x.CreatedBy);
                e.Property(x => x.ModifiedBy);
                e.Property(x => x.CostCenterId);
            });

            // ValuationGroupAccounts
            modelBuilder.Entity<ValuationGroupAccount>(e =>
            {
                e.ToTable("ValuationGroupAccounts", "dbo");
                e.HasKey(x => x.ValuationGroupAccountsId);
                e.Property(x => x.ValuationGroupAccountsId)
                    .HasColumnName("ValuationGroupAccounts")
                    .ValueGeneratedOnAdd()
                    .UseIdentityColumn(1, 1);

                e.Property(x => x.ValuationGroup).IsRequired(); // int فقط بدون FK
                e.Property(x => x.CompanyId).IsRequired();

                e.Property(x => x.InventoryAcc).HasMaxLength(50);
                e.Property(x => x.COGSAcc).HasMaxLength(50);
                e.Property(x => x.SalesAcc).HasMaxLength(50);
                e.Property(x => x.SalesDiscountAcc).HasMaxLength(50);
                e.Property(x => x.LossAcc).HasMaxLength(50);
                e.Property(x => x.AdjustmentAcc).HasMaxLength(50);
                e.Property(x => x.EarnedDiscountAccount).HasMaxLength(50);
                e.Property(x => x.ExpenseAcc).HasMaxLength(50);
                e.Property(x => x.TaxAccPurchase).HasMaxLength(50);

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("getdate()");
            });


            modelBuilder.Entity<PaymentTerm>(e =>
            {
                e.ToTable("Payment_Terms", "dbo");

                e.HasKey(x => x.TermId).HasName("PK_Payment_Terms");

                e.Property(x => x.TermId)
                 .HasColumnName("Term_ID")
                 .ValueGeneratedOnAdd(); // Identity(1,1)

                e.Property(x => x.TermCode)
                 .HasColumnName("Term_Code")
                 .IsUnicode(false)     // varchar
                 .HasMaxLength(20)
                 .IsRequired();

                e.Property(x => x.TermName)
                 .HasColumnName("Term_Name")
                 .HasMaxLength(100)
                 .IsRequired();

                e.Property(x => x.NetDays)
                 .HasColumnName("Net_Days")
                 .IsRequired();

                e.Property(x => x.DiscountPercent)
                 .HasColumnName("Discount_Percent")
                 .HasColumnType("decimal(5,2)");

                e.Property(x => x.DiscountDays)
                 .HasColumnName("Discount_Days");

                e.Property(x => x.LateFeePercent)
                 .HasColumnName("Late_Fee_Percent")
                 .HasColumnType("decimal(5,2)");

                e.Property(x => x.IsActive)
                 .HasColumnName("Is_Active");

                // فريد على Term_Code
                e.HasIndex(x => x.TermCode)
                 .IsUnique()
                 .HasDatabaseName("UQ_Payment_Terms_Code");
            });

            // === PaymentType ===

            modelBuilder.Entity<PaymentType>(e =>
            {
                e.ToTable("Payment_Types", "dbo");

                e.HasKey(x => x.TypeId).HasName("PK_Payment_Types");

                e.Property(x => x.TypeId)
                 .HasColumnName("Type_ID")
                 .ValueGeneratedOnAdd(); // Identity(1,1)

                e.Property(x => x.Code)
                 .HasColumnName("Code")
                 .HasColumnType("char(2)")   // ثابت الطول
                 .IsFixedLength(true)
                 .IsUnicode(false)
                 .IsRequired();

                e.Property(x => x.Description)
                 .HasColumnName("Description")
                 .HasMaxLength(50);          // nvarchar(50)
            });

            // DebitCreditNotifications
            modelBuilder.Entity<DebitCreditNotification>(entity =>
            {
                entity.ToTable("DebitCreditNotifications");
                entity.HasKey(e => e.NotificationId);

                entity.Property(e => e.NotificationId).ValueGeneratedOnAdd();

                entity.Property(e => e.CompanyId).IsRequired();
                entity.Property(e => e.BranchId).IsRequired();

                // Char(1): 'D' / 'C'
                entity.Property(e => e.NotificationType)
                      .IsRequired()
                      .HasMaxLength(1)
                      .IsFixedLength(true);

                entity.Property(e => e.NotificationDate).HasColumnType("date");

                entity.Property(e => e.AccountNumber)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.CurrencyId).IsRequired();

                entity.Property(e => e.Status)      // tinyint
                      .HasColumnType("tinyint");

                entity.Property(e => e.TotalAmount) // decimal(18,4)
                      .HasColumnType("decimal(18,4)");

                entity.Property(e => e.PostingDate).HasColumnType("date");
                entity.Property(e => e.AmendmentDate).HasColumnType("date");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
                entity.Property(e => e.ModifiedAt).HasColumnType("datetime2");

                // علاقة 1-متعدد مع التفاصيل + حذف تسلسلي
                entity.HasMany(e => e.Details)
                      .WithOne()
                      .HasForeignKey(d => d.NotificationId)
                      .OnDelete(DeleteBehavior.Cascade);

                // (اختياري) فهارس مفيدة للبحث
                entity.HasIndex(e => e.BranchId);
                entity.HasIndex(e => new { e.NotificationDate, e.NotificationType });
            });

            // DebitCreditNoteDetails
            modelBuilder.Entity<DebitCreditNoteDetail>(entity =>
            {
                entity.ToTable("DebitCreditNoteDetails");
                entity.HasKey(e => e.DetailId);

                entity.Property(e => e.DetailId).ValueGeneratedOnAdd();

                entity.Property(e => e.NotificationId).IsRequired();
                entity.Property(e => e.BranchId).IsRequired();

                entity.Property(e => e.PostingDate).HasColumnType("date");

                entity.Property(e => e.Statement)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.AmountTransaction) // decimal(18,4)
                      .HasColumnType("decimal(18,4)");

                entity.Property(e => e.AmountCompany)     // decimal(18,4)
                      .HasColumnType("decimal(18,4)");

                entity.Property(e => e.ExchangeRate)      // decimal(18,6)
                      .HasColumnType("decimal(18,6)");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
            });


            // === PaymentMethod ===
            modelBuilder.Entity<PaymentMethod>(e =>
            {
                e.ToTable("Payment_Methods", "dbo");
                e.HasKey(x => x.MethodId).HasName("PK_Payment_Methods");

                e.Property(x => x.MethodId)
                 .HasColumnName("Method_ID")
                 .ValueGeneratedOnAdd();

                e.Property(x => x.MethodName)
                 .HasColumnName("Method_Name")
                 .HasMaxLength(100)
                 .IsRequired();

                e.Property(x => x.GLAccount)
                 .HasColumnName("GL_Account")
                 .HasMaxLength(50)
                 .IsRequired();

                e.Property(x => x.PaymentTypeId)
                 .HasColumnName("Payment_Type")
                 .IsRequired();

                e.Property(x => x.ProviderId)
                 .HasColumnName("ProviderId");

                e.Property(x => x.RequiresApproval)
                 .HasColumnName("Requires_Approval");

                e.Property(x => x.IsActive)
                 .HasColumnName("Is_Active");

                e.Property(x => x.SupportsSplit)
                 .HasColumnName("Supports_Split");

                // علاقة مع Payment_Types (حتى لو ما في FK في SQL، هذا يفيدنا في Include/Binding)
                e.HasOne(x => x.Type)
                 .WithMany()
                 .HasForeignKey(x => x.PaymentTypeId)
                 .HasPrincipalKey(t => t.TypeId)
                 .OnDelete(DeleteBehavior.Restrict)
                 .HasConstraintName("FK_Payment_Methods_Payment_Types"); // اسم وصفي فقط
            });

            modelBuilder.Entity<Supplier>(e =>
            {
                e.ToTable("Suppliers", "dbo");
                e.HasKey(x => x.SupplierID).HasName("PK_Suppliers");

                e.Property(x => x.SupplierID)
                 .HasColumnName("SupplierID")
                 .ValueGeneratedOnAdd();

                e.Property(x => x.SupplierCode).HasColumnName("SupplierCode").IsUnicode(false).HasMaxLength(20).IsRequired();
                e.Property(x => x.SupplierName).HasColumnName("SupplierName").HasMaxLength(200).IsRequired();
                e.Property(x => x.TaxNumber).HasColumnName("TaxNumber").IsUnicode(false).HasMaxLength(15).IsRequired();

                e.Property(x => x.AccountID).HasColumnName("AccountID").IsRequired();
                e.Property(x => x.CostCenterID).HasColumnName("CostCenterID");
                e.Property(x => x.PaymentTerms).HasColumnName("PaymentTerms");
                e.Property(x => x.CreditLimit).HasColumnName("CreditLimit").HasColumnType("decimal(18,4)");

                e.Property(x => x.ContactPerson).HasColumnName("ContactPerson").HasMaxLength(100);
                e.Property(x => x.Phone).HasColumnName("Phone").IsUnicode(false).HasMaxLength(20);
                e.Property(x => x.Email).HasColumnName("Email").IsUnicode(false).HasMaxLength(100);
                e.Property(x => x.Address).HasColumnName("Address").HasMaxLength(500);

                e.Property(x => x.IsActive).HasColumnName("IsActive");
                e.Property(x => x.CreatedDate).HasColumnName("CreatedDate");
                e.Property(x => x.ModifiedDate).HasColumnName("ModifiedDate");
                e.Property(x => x.CreatedBy).HasColumnName("CreatedBy");
                e.Property(x => x.ModifiedBy).HasColumnName("ModifiedBy");

                e.Property(x => x.Nationality).HasColumnName("Nationality").HasMaxLength(50);
                e.Property(x => x.IDNumber).HasColumnName("IDNumber").HasMaxLength(50);
                e.Property(x => x.CRNumber).HasColumnName("CRNumber").HasMaxLength(50);
                e.Property(x => x.VATNumber).HasColumnName("VATNumber").HasMaxLength(50);

                e.Property(x => x.DefaultPaymentMethodID).HasColumnName("DefaultPaymentMethodID");

                // علاقة 1..* إلى جدول الربط
                e.HasMany(x => x.PaymentMethods)
                 .WithOne()
                 .HasForeignKey(pm => pm.SupplierID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Supplier_Payment_Methods
            modelBuilder.Entity<SupplierPaymentMethod>(e =>
            {
                e.ToTable("Supplier_Payment_Methods", "dbo");
                e.HasKey(x => new { x.SupplierID, x.Method_ID })
                 .HasName("PK_Supplier_Payment_Methods");

                e.Property(x => x.SupplierID).HasColumnName("SupplierID");
                e.Property(x => x.Method_ID).HasColumnName("Method_ID");
                e.Property(x => x.Is_Default).HasColumnName("Is_Default");

                // Navigation لعرض اسم الطريقة
                e.HasOne(x => x.Method)
                 .WithMany()
                 .HasForeignKey(x => x.Method_ID)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Warehouse>()
        .HasIndex(x => new { x.CompanyId, x.WarehouseCode })
        .IsUnique();


          

            // === Print Management Configuration ===
            
            // PrintTemplates

            modelBuilder.Entity<PrintTemplate>(e =>
            {
                e.ToTable("PrintTemplates", "dbo");
                e.HasKey(x => x.TemplateId);
                e.Property(x => x.TemplateId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
                e.Property(x => x.Engine).IsRequired().HasMaxLength(20);
                e.Property(x => x.PaperSize).HasMaxLength(10);
                e.Property(x => x.Orientation).HasMaxLength(1);
                e.Property(x => x.Locale).HasMaxLength(10);
                e.Property(x => x.IsDefault).HasDefaultValue(false);
                e.Property(x => x.Active).HasDefaultValue(true);
                e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysdatetime()");
                e.Property(x => x.UpdatedAt).HasColumnType("datetime2");
                
                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.DocumentTypeId, x.Active })
                 .HasDatabaseName("IX_PrintTemplates_Doc");
            });

            // TemplateVersions
            modelBuilder.Entity<TemplateVersion>(e =>
            {
                e.ToTable("TemplateVersions", "dbo");
                e.HasKey(x => x.TemplateVersionId);
                e.Property(x => x.TemplateVersionId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.Status).IsRequired().HasMaxLength(10);
                e.Property(x => x.Notes).HasMaxLength(500);
                e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysdatetime()");
                e.Property(x => x.ActivatedAt).HasColumnType("datetime2");
                
                // السماح بنسخة واحدة Active لكل قالب
                e.HasIndex(x => x.TemplateId)
                 .IsUnique()
                 .HasFilter("[Status] = 'active'")
                 .HasDatabaseName("UX_TemplateVersions_Active");
                
                e.HasOne(x => x.Template)
                 .WithMany(x => x.Versions)
                 .HasForeignKey(x => x.TemplateId);
            });

            // TemplateContents
            modelBuilder.Entity<TemplateContent>(e =>
            {
                e.ToTable("TemplateContents", "dbo");
                e.HasKey(x => x.TemplateContentId);
                e.Property(x => x.TemplateContentId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.ContentType).IsRequired().HasMaxLength(20);
                
                e.HasOne(x => x.TemplateVersion)
                 .WithMany(x => x.Contents)
                 .HasForeignKey(x => x.TemplateVersionId);
            });

            // TemplateDataSources
            modelBuilder.Entity<TemplateDataSource>(e =>
            {
                e.ToTable("TemplateDataSources", "dbo");
                e.HasKey(x => x.DataSourceId);
                e.Property(x => x.DataSourceId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.Name).IsRequired().HasMaxLength(100);
                e.Property(x => x.SourceType).IsRequired().HasMaxLength(10);
                e.Property(x => x.SourceName).IsRequired().HasMaxLength(128);
                e.Property(x => x.IsMain).HasDefaultValue(false);
                e.Property(x => x.TimeoutSec).HasDefaultValue(30);
                
                e.HasOne(x => x.TemplateVersion)
                 .WithMany(x => x.DataSources)
                 .HasForeignKey(x => x.TemplateVersionId);
            });

            // PrintJobs
            modelBuilder.Entity<PrintJob>(e =>
            {
                e.ToTable("PrintJobs", "dbo");
                e.HasKey(x => x.JobId);
                e.Property(x => x.JobId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.DocumentNumber).HasMaxLength(60);
                e.Property(x => x.Locale).HasMaxLength(10);
                e.Property(x => x.OutputFormat)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(10)
                    .HasDefaultValue(OutputFormatType.Pdf);
                e.Property(x => x.Status).IsRequired().HasMaxLength(12).HasDefaultValue("done");
                e.Property(x => x.FileUrl).HasMaxLength(500);
                e.Property(x => x.ErrorMessage).HasMaxLength(1000);
                e.Property(x => x.Copies).HasDefaultValue(1);
                e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysdatetime()");
                e.Property(x => x.StartedAt).HasColumnType("datetime2");
                e.Property(x => x.FinishedAt).HasColumnType("datetime2");
                
                e.HasIndex(x => new { x.DocumentTypeId, x.DocumentId, x.CreatedAt })
                 .IsDescending(false, false, true)
                 .HasDatabaseName("IX_PrintJobs_Lookup");
                
                e.HasOne(x => x.TemplateVersion)
                 .WithMany(x => x.PrintJobs)
                 .HasForeignKey(x => x.TemplateVersionId);
            });

            // PrintAssets
            modelBuilder.Entity<PrintAsset>(e =>
            {
                e.ToTable("PrintAssets", "dbo");
                e.HasKey(x => x.AssetId);
                e.Property(x => x.AssetId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.Name).IsRequired().HasMaxLength(120);
                e.Property(x => x.MimeType).IsRequired().HasMaxLength(100);
                e.Property(x => x.Url).HasMaxLength(500);
                e.Property(x => x.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("sysdatetime()");
            });


            // ==== PurchaseTxn (Header) ====
            // ==== PurchaseTxn (Head) ====
            modelBuilder.Entity<PurchaseTxn>(e =>
            {
                e.ToTable("PurchaseTxn", "dbo");
                e.HasKey(x => x.TxnID).HasName("PK_PurchaseTxn");

                e.Property(x => x.TxnID).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.CompanyID).IsRequired();
                e.Property(x => x.BranchID).IsRequired();
                e.Property(x => x.SupplierID).IsRequired();
                e.Property(x => x.TxnNumber).HasMaxLength(50).IsRequired();
                e.Property(x => x.TxnType).HasColumnType("char(1)").IsRequired();
                e.Property(x => x.TxnDate).HasColumnType("date").IsRequired();
                e.Property(x => x.ExpectedDelivery).HasColumnType("date");
                e.Property(x => x.Status).HasColumnType("tinyint");
                e.Property(x => x.SubTotal).HasColumnType("decimal(18,4)");
                e.Property(x => x.TaxAmount).HasColumnType("decimal(18,4)");
                e.Property(x => x.TotalAmount).HasColumnType("decimal(18,4)");
                e.Property(x => x.Remarks).HasMaxLength(1000);
                e.Property(x => x.CreatedAt).HasColumnType("datetime2(0)").HasDefaultValueSql("sysutcdatetime()").IsRequired();
                e.Property(x => x.ModifiedAt).HasColumnType("datetime2(0)");
                e.Property(x => x.IsSynced).HasDefaultValue(false).IsRequired();

                e.HasIndex(x => new { x.CompanyID, x.BranchID, x.TxnNumber })
                 .IsUnique()
                 .HasDatabaseName("UQ_PurchaseTxn_Company_Branch_Number");

                e.HasCheckConstraint("CK_PurchaseTxn_TxnType", "[TxnType]='R' OR [TxnType]='P'");

                // العلاقة بدون Navigation في التفاصيل:
                e.HasMany(x => x.Details)
                 .WithOne()                            // لا يوجد خاصية Navigation في PurchaseTxnDetail
                 .HasForeignKey(d => d.TxnID)          // FK الحقيقي في الجدول
                 .HasConstraintName("FK_PurchaseTxnDetails_PurchaseTxn")
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ==== PurchaseTxnDetails (Lines) ====
            modelBuilder.Entity<PurchaseTxnDetail>(e =>
            {
                e.ToTable("PurchaseTxnDetails", "dbo");
                e.HasKey(x => x.DetailID).HasName("PK_PurchaseTxnDetails");

                e.Property(x => x.DetailID).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.TxnID).IsRequired();
                e.Property(x => x.CompanyID).IsRequired();
                e.Property(x => x.BranchID).IsRequired();
                e.Property(x => x.ItemID).IsRequired();
                e.Property(x => x.UnitID).IsRequired();

                e.Property(x => x.Quantity).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.TaxableAmount).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.VATRate).HasColumnType("decimal(5,2)").HasDefaultValue(0).IsRequired();
                e.Property(x => x.VATAmount).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.LineTotal).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.ReceivedQuantity).HasColumnType("decimal(18,4)");
                e.Property(x => x.IsSynced).HasDefaultValue(false).IsRequired();
            });

            // ==== StoreReceipt (Header) ====
            modelBuilder.Entity<StoreReceipt>(e =>
            {
                e.ToTable("StoreReceipts", "dbo");
                e.HasKey(x => x.ReceiptId).HasName("PK_StoreReceipts");

                e.Property(x => x.ReceiptId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.CompanyId).IsRequired();
                e.Property(x => x.BranchId).IsRequired();
                e.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
                e.Property(x => x.ReceiptDate).HasColumnType("date").IsRequired();
                e.Property(x => x.SupplierId);
                e.Property(x => x.PurchaseOrderId);
                e.Property(x => x.ReferenceNumber).HasMaxLength(50);
                e.Property(x => x.Description).HasMaxLength(500);
                e.Property(x => x.TotalAmount).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.CurrencyId).IsRequired();
                e.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)").IsRequired();
                e.Property(x => x.Status).HasColumnType("tinyint").IsRequired();
                e.Property(x => x.CreatedBy).IsRequired();
                e.Property(x => x.CreatedAt).HasColumnType("datetime2(7)");
                e.Property(x => x.ModifiedBy);
                e.Property(x => x.ModifiedAt).HasColumnType("datetime2(7)");

                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.ReceiptNumber })
                 .IsUnique()
                 .HasDatabaseName("UQ_StoreReceipts_Company_Branch_Number");

                e.HasMany(x => x.Details)
                 .WithOne(d => d.Receipt)
                 .HasForeignKey(d => d.ReceiptId)
                 .HasConstraintName("FK_StoreReceiptsDetailed_StoreReceipts")
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ==== StoreReceiptDetailed (Lines) ====
            modelBuilder.Entity<StoreReceiptDetailed>(e =>
            {
                e.ToTable("StoreReceiptsDetailed", "dbo");
                e.HasKey(x => x.DetailId).HasName("PK_StoreReceiptsDetailed");

                e.Property(x => x.DetailId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.ReceiptId).IsRequired();
                e.Property(x => x.ItemId).IsRequired();
                e.Property(x => x.UnitId).IsRequired();
                e.Property(x => x.BatchNumber).HasMaxLength(50);
                e.Property(x => x.ExpiryDate).HasColumnType("date");
                e.Property(x => x.Quantity).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.SubTotal).HasColumnType("decimal(18,4)");
                e.Property(x => x.VatRate).HasColumnType("decimal(5,2)");
                e.Property(x => x.VatAmount).HasColumnType("decimal(18,4)");
                e.Property(x => x.TotalPrice).HasColumnType("decimal(18,4)");
                e.Property(x => x.CostCenterId);
                e.Property(x => x.WarehouseId).IsRequired();
                e.Property(x => x.CurrencyId).IsRequired();
                e.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)").IsRequired();
                e.Property(x => x.DebitAccount).HasMaxLength(20);
                e.Property(x => x.CreditAccount).HasMaxLength(20);
            });

            // ItemCosts Configuration
            modelBuilder.Entity<ItemCost>(e =>
            {
                e.ToTable("ItemCosts", "dbo");
                e.HasKey(x => x.CostID);
                e.Property(x => x.CostID).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.ItemID).IsRequired();
                e.Property(x => x.CostMethod).HasMaxLength(1).HasDefaultValue("A").IsRequired();
                
                e.Property(x => x.LastCost).HasColumnType("decimal(18,4)");
                e.Property(x => x.AvgCost).HasColumnType("decimal(18,4)");
                e.Property(x => x.MinCost).HasColumnType("decimal(18,4)");
                e.Property(x => x.MaxCost).HasColumnType("decimal(18,4)");
                e.Property(x => x.StandardCost).HasColumnType("decimal(18,4)").HasDefaultValue(0).IsRequired();
                e.Property(x => x.LastPurchaseCost).HasColumnType("decimal(18,4)").HasDefaultValue(0).IsRequired();
                e.Property(x => x.MovingAverageCost).HasColumnType("decimal(18,4)").HasDefaultValue(0).IsRequired();
                e.Property(x => x.FIFOCost).HasColumnType("decimal(18,4)").HasDefaultValue(0).IsRequired();
                
                e.Property(x => x.LastPurchaseDate).HasColumnType("date");
                e.Property(x => x.LastUpdate).HasColumnType("datetime").HasDefaultValueSql("getdate()");
                
                e.Property(x => x.CompanyId);
                e.Property(x => x.BranchId);
                
                // فهرس فريد على ItemID لضمان تكلفة واحدة لكل صنف
                e.HasIndex(x => x.ItemID).IsUnique();
            });

            // ItemUnits Configuration
            modelBuilder.Entity<ItemUnit>(e =>
            {
                e.ToTable("ItemUnits", "dbo");
                e.HasKey(x => x.BarcodeID);
                e.Property(x => x.BarcodeID).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                
                e.Property(x => x.ItemID).IsRequired();
                e.Property(x => x.UnitID).IsRequired();
                e.Property(x => x.UnitBarcode).HasMaxLength(200).IsRequired();
                e.Property(x => x.BarcodeType).HasMaxLength(20);
                
                e.Property(x => x.IsPrimary);
                e.Property(x => x.IsSalesUnit);
                e.Property(x => x.PurchaseUnit);
                e.Property(x => x.IsInventoryUnit);
                
                e.Property(x => x.CreatedDate).HasColumnType("datetime");
                e.Property(x => x.CreatedBy);
                
                // علاقة مع Item
                e.HasOne(x => x.Item)
                 .WithMany(i => i.Units)
                 .HasForeignKey(x => x.ItemID)
                 .OnDelete(DeleteBehavior.Cascade);
                
                // علاقة مع Unit
                e.HasOne(x => x.Unit)
                 .WithMany()
                 .HasForeignKey(x => x.UnitID)
                 .OnDelete(DeleteBehavior.Restrict);
                
                // فهرس فريد على ItemID + UnitID لمنع التكرار
                e.HasIndex(x => new { x.ItemID, x.UnitID }).IsUnique();
            });

            // Inventory Opening Configuration
            modelBuilder.Entity<InventoryOpeningHeader>(e =>
            {
                e.ToTable("InventoryOpeningHeader", "dbo");
                e.HasKey(x => x.DocID);
                e.Property(x => x.DocID).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);

                e.Property(x => x.CompanyId).IsRequired();
                e.Property(x => x.BranchId).IsRequired();
                e.Property(x => x.DocNo).HasMaxLength(30).IsRequired();
                e.Property(x => x.DocDate).HasColumnType("date").IsRequired();
                
                e.Property(x => x.EntryMethod).HasConversion<byte>();
                e.Property(x => x.ViewMode).HasConversion<byte?>();
                e.Property(x => x.CostMethod).HasConversion<byte>().IsRequired();
                e.Property(x => x.WeightedAvgScope).HasConversion<byte?>();
                
                e.Property(x => x.UseExpiry).HasDefaultValue(false);
                e.Property(x => x.UseBatch).HasDefaultValue(false);
                e.Property(x => x.UseSerial).HasDefaultValue(false);
                e.Property(x => x.Status).HasConversion<byte>();
                
                e.Property(x => x.Notes).HasMaxLength(500);
                e.Property(x => x.CreatedAt).HasDefaultValueSql("getdate()");

                // فهرس فريد على رقم المستند داخل الشركة والفرع
                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.DocNo }).IsUnique();
                
                // علاقة مع التفاصيل
            
            });



            // ===== Permissions System =====

            // MenuForm
            modelBuilder.Entity<MenuForm>(e =>
            {
                e.ToTable("MenwFomrs");
                e.HasKey(x => x.MenuFormCode);
                e.Property(x => x.MenuFormCode);
                e.Property(x => x.MenuFormParent);
                e.Property(x => x.ProgramExecutable);
                e.Property(x => x.MenuName);
                e.Property(x => x.Visible);
                e.Property(x => x.MenuArabicName);
                e.Property(x => x.NSync);
                e.Property(x => x.DbTimestamp);

                // Self-referencing relationship for parent-child menus
                e.HasOne(x => x.Parent)
                 .WithMany(x => x.Children)
                 .HasForeignKey(x => x.MenuFormParent)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // SysRole
            modelBuilder.Entity<SysRole>(e =>
            {
                e.ToTable("SYSROLLS");
                e.HasKey(x => x.Id);

                // Configure Id as identity column
                e.Property(x => x.Id)
                 .HasColumnName("ID")
                 .ValueGeneratedOnAdd()
                 .UseIdentityColumn(1, 1);

                e.Property(x => x.Name).HasColumnName("Name").HasMaxLength(255).IsRequired();
                e.Property(x => x.RollType).HasColumnName("RollType");

                e.Property(x => x.DbTimestamp).HasColumnName("Dbtimestamp");
            });

            // MenuRole
            modelBuilder.Entity<MenuRole>(e =>
            {
                e.ToTable("MenwRolls");
                e.HasKey(x => x.Id);

                // Configure Id as identity column
                e.Property(x => x.Id)
                 .HasColumnName("ID")
                 .ValueGeneratedOnAdd()
                 .UseIdentityColumn(1, 1);

                e.Property(x => x.FormId).HasColumnName("Fromdid");
                e.Property(x => x.RoleId).HasColumnName("Rollid");
                e.Property(x => x.DbTimestamp).HasColumnName("Dbtimestamp");

                // Relationships
                e.HasOne(x => x.MenuForm)
                 .WithMany(x => x.MenuRoles)
                 .HasForeignKey(x => x.FormId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.SysRole)
                 .WithMany(x => x.MenuRoles)
                 .HasForeignKey(x => x.RoleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // UserPermission
            modelBuilder.Entity<UserPermission>(e =>
            {
                e.ToTable("UsersPermissions");
                e.HasKey(x => x.PerId);

                // Configure PerId as identity column
                e.Property(x => x.PerId)
                 .HasColumnName("PerID")
                 .ValueGeneratedOnAdd()
                 .UseIdentityColumn(1, 1);

                e.Property(x => x.BranchId).HasColumnName("BranchId");
                e.Property(x => x.CompanyId).HasColumnName("ComiId");
                e.Property(x => x.UserId).HasColumnName("UserID");
                e.Property(x => x.RoleId).HasColumnName("Rollid");
                e.Property(x => x.AllowAdd).HasColumnName("AllowAdd");
                e.Property(x => x.AllowEdit).HasColumnName("AllowEdit");
                e.Property(x => x.AllowDelete).HasColumnName("AllowDel");
                e.Property(x => x.FormId).HasColumnName("FormID");
                e.Property(x => x.AllowView).HasColumnName("AllowView");
                e.Property(x => x.AllowPost).HasColumnName("AllowPost");
                e.Property(x => x.AllowPrint).HasColumnName("AllowPrint");
                e.Property(x => x.AllowRun).HasColumnName("AllowRun");

                // Relationships
                e.HasOne(x => x.MenuForm)
                 .WithMany(x => x.UserPermissions)
                 .HasForeignKey(x => x.FormId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.SysRole)
                 .WithMany(x => x.UserPermissions)
                 .HasForeignKey(x => x.RoleId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint
                e.HasIndex(x => new { x.UserId, x.FormId, x.CompanyId, x.BranchId })
                 .IsUnique();
            });

            // Purchase AP Configuration
            modelBuilder.Entity<PurchaseAP>(e =>
            {
                e.ToTable("PurchaseAP", "dbo");
                e.HasKey(x => x.APId).HasName("PK_PurchaseAP");

                e.Property(x => x.APId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.CompanyId).IsRequired();
                e.Property(x => x.BranchId).IsRequired();
                e.Property(x => x.DocNumber).HasMaxLength(50).IsRequired();
                e.Property(x => x.DocType).HasMaxLength(2).IsRequired();
                e.Property(x => x.DocDate).HasColumnType("date").IsRequired();
                e.Property(x => x.DueDate).HasColumnType("date");
                e.Property(x => x.SupplierId).IsRequired();
                e.Property(x => x.ReferenceNumber).HasMaxLength(50);
                e.Property(x => x.CurrencyId).IsRequired();
                e.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)").IsRequired();
                e.Property(x => x.RelatedReceiptId);
                e.Property(x => x.RelatedPOId);
                e.Property(x => x.SubTotal).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.TaxAmount).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.TotalAmount).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.PriceIncludesTax).IsRequired();
                e.Property(x => x.Status).HasColumnType("tinyint").IsRequired();
                e.Property(x => x.JournalEntryId);
                e.Property(x => x.Remarks).HasMaxLength(1000);
                e.Property(x => x.CreatedBy).IsRequired();
                e.Property(x => x.CreatedAt).HasColumnType("datetime2(7)").IsRequired();
                e.Property(x => x.ModifiedBy);
                e.Property(x => x.ModifiedAt).HasColumnType("datetime2(7)");

                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.DocType, x.DocNumber })
                 .IsUnique()
                 .HasDatabaseName("UQ_PurchaseAP_Number");
                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.SupplierId, x.DocDate })
                 .HasDatabaseName("IX_PurchaseAP_Supplier");
                e.HasIndex(x => new { x.CompanyId, x.BranchId, x.RelatedReceiptId })
                 .HasDatabaseName("IX_PurchaseAP_Receipt");

                e.HasMany(x => x.Details)
                 .WithOne(d => d.PurchaseAP)
                 .HasForeignKey(d => d.APId)
                 .HasConstraintName("FK_PurchaseAPDetails_PurchaseAP")
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PurchaseAPDetail>(e =>
            {
                e.ToTable("PurchaseAPDetails", "dbo");
                e.HasKey(x => x.DetailId).HasName("PK_PurchaseAPDetails");

                e.Property(x => x.DetailId).ValueGeneratedOnAdd().UseIdentityColumn(1, 1);
                e.Property(x => x.APId).IsRequired();
                e.Property(x => x.LineNo).HasColumnName("LinNo").IsRequired();
                e.Property(x => x.ItemId).IsRequired();
                e.Property(x => x.UnitId).IsRequired();
                e.Property(x => x.Quantity).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)").IsRequired();
                e.Property(x => x.PriceIncludesTax).IsRequired();
                e.Property(x => x.VATCodeID);
                e.Property(x => x.VATRate).HasColumnType("decimal(7,4)");
                e.Property(x => x.TaxableAmount).HasColumnType("decimal(18,4)");
                e.Property(x => x.VATAmount).HasColumnType("decimal(18,4)");
                e.Property(x => x.LineTotal).HasColumnType("decimal(18,4)");
                e.Property(x => x.WarehouseId);
                e.Property(x => x.BatchID);
                e.Property(x => x.PurchaseDetailId);
                e.Property(x => x.ReceiptDetailId);
                e.Property(x => x.CostCenterId);
                e.Property(x => x.CurrencyId).IsRequired();
                e.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)").IsRequired();
                e.Property(x => x.Remarks).HasMaxLength(500);

                e.HasIndex(x => new { x.APId, x.LineNo }).HasDatabaseName("IX_PAPDetails_AP");
                e.HasIndex(x => new { x.ItemId, x.WarehouseId }).HasDatabaseName("IX_PAPDetails_Item");
                e.HasIndex(x => x.PurchaseDetailId).HasDatabaseName("IX_PAPDetails_PO");
                e.HasIndex(x => x.ReceiptDetailId).HasDatabaseName("IX_PAPDetails_RCPT");
            });

            // تكوين PosStation
            modelBuilder.Entity<Next_Future_ERP.Features.PosStations.Models.PosStation>(e =>
            {
                e.ToTable("POS_Stations");
                
                e.HasKey(x => x.PosId);
                
                e.Property(x => x.PosId).HasColumnName("POS_ID");
                e.Property(x => x.BranchId).HasColumnName("Branch_ID").IsRequired();
                e.Property(x => x.PosName).HasColumnName("POS_Name").HasMaxLength(100).IsRequired();
                e.Property(x => x.PosCode).HasColumnName("POS_Code").HasMaxLength(20).IsRequired();
                e.Property(x => x.GlCashAccount).HasColumnName("GL_Cash_Account").HasMaxLength(50).IsRequired();
                e.Property(x => x.GlSalesAccount).HasColumnName("GL_Sales_Account").HasMaxLength(50).IsRequired();
                e.Property(x => x.AssignedUser).HasColumnName("Assigned_User").IsRequired();
                e.Property(x => x.AllowedPaymentMethods).HasColumnName("Allowed_Payment_Methods");
                e.Property(x => x.UserPermissions).HasColumnName("User_Permissions");
                e.Property(x => x.IsActive).HasColumnName("Is_Active").HasDefaultValue(true);
                e.Property(x => x.CreatedDate).HasColumnName("Created_Date").HasDefaultValueSql("getdate()");
                e.Property(x => x.UpdatedDate).HasColumnName("Updated_Date");
                e.Property(x => x.CompanyId).HasColumnName("CompanyId");

                // فهارس
                e.HasIndex(x => x.PosCode).IsUnique().HasDatabaseName("IX_POS_Stations_Code");
                e.HasIndex(x => x.BranchId).HasDatabaseName("IX_POS_Stations_Branch");
                e.HasIndex(x => x.AssignedUser).HasDatabaseName("IX_POS_Stations_User");
                e.HasIndex(x => x.CompanyId).HasDatabaseName("IX_POS_Stations_Company");

                // علاقات
                e.HasOne(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.AssignedUserNavigation)
                    .WithMany()
                    .HasForeignKey(x => x.AssignedUser)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PosOperator Configuration
            modelBuilder.Entity<Next_Future_ERP.Features.PosOperators.Models.PosOperator>(e =>
            {
                e.ToTable("POS_Operators");
                
                e.HasKey(x => x.OperatorId);
                
                e.Property(x => x.OperatorId).HasColumnName("OperatorId");
                e.Property(x => x.PosId).HasColumnName("POS_ID").IsRequired();
                e.Property(x => x.UserId).HasColumnName("User_ID").IsRequired();
                e.Property(x => x.IsPrimary).HasColumnName("IsPrimary").HasDefaultValue(false);
                e.Property(x => x.StartDate).HasColumnName("StartDate").HasDefaultValueSql("getdate()");
                e.Property(x => x.EndDate).HasColumnName("EndDate");
                e.Property(x => x.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
                e.Property(x => x.CompanyId).HasColumnName("CompanyId");
                e.Property(x => x.BranchId).HasColumnName("BranchId");

                // فهارس
                e.HasIndex(x => x.PosId).HasDatabaseName("IX_POS_Operators_POS");
                e.HasIndex(x => x.UserId).HasDatabaseName("IX_POS_Operators_User");
                e.HasIndex(x => x.CompanyId).HasDatabaseName("IX_POS_Operators_Company");
                e.HasIndex(x => x.BranchId).HasDatabaseName("IX_POS_Operators_Branch");

                // علاقات
                e.HasOne(x => x.PosStation)
                    .WithMany()
                    .HasForeignKey(x => x.PosId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


        }


    }
}
