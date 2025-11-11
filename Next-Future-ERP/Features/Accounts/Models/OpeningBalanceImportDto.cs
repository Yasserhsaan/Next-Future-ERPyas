using System;
using System.Collections.Generic;

namespace Next_Future_ERP.Features.Accounts.Models
{
    public class OpeningBalanceImportDto
    {
        public OpeningBalanceBatchDto Batch { get; set; } = new();
        public List<OpeningBalanceLineDto> Lines { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool IsValid => Errors.Count == 0;
    }

    public class OpeningBalanceBatchDto
    {
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public short FiscalYear { get; set; }
        public DateTime DocDate { get; set; }
        public string? DocNo { get; set; }
        public string? Description { get; set; }
        public bool PostAfterImport { get; set; }
    }

    public class OpeningBalanceLineDto
    {
        public int LineNo { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public int? CostCenterId { get; set; }
        public int TransactionCurrencyId { get; set; }
        public decimal TransactionDebit { get; set; }
        public decimal TransactionCredit { get; set; }
        public decimal ExchangeRate { get; set; } = 1;
        public string? Note { get; set; }

        // للتحقق من صحة البيانات
        public List<string> ValidationErrors { get; set; } = new();
        public bool IsValid => ValidationErrors.Count == 0;
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? BatchId { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int ProcessedLines { get; set; }
        public bool WasPosted { get; set; }
    }
}
