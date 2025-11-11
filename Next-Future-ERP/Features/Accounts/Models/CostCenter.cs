namespace Next_Future_ERP.Models
{
    public class CostCenter
    {
        public int CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string LinkedAccounts { get; set; }
        public string Classification { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
