// Path: Models/TaxpayerType.cs
namespace Next_Future_ERP.Models
{
    public enum TaxpayerType : byte
    {
        Unknown = 0,
        Individual = 1,    // فرد
        Establishment = 2, // منشأة
        Company = 3,       // شركة
        NonResident = 4
    }
}
