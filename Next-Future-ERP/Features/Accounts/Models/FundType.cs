using System;
using System.Globalization;
using System.Windows.Data;
using Next_Future_ERP.Features.Accounts.Models;

namespace Next_Future_ERP.Models


{
    public enum FundType : byte
    {
        ReceiptOnly = 0,
        PaymentOnly = 1,
        Both = 2
    }
}
