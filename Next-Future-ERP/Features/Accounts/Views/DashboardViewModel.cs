using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace Next_Future_ERP.Features.Accounts.Views
{
    public class DashboardViewModel : ObservableObject
    {
        public ObservableCollection<KpiItem> Kpis { get; } = new();
        public ObservableCollection<SalesBar> Sales7Days { get; } = new();
        public ObservableCollection<TopCustomer> TopCustomers { get; } = new();
        public ObservableCollection<InvoiceRow> RecentInvoices { get; } = new();

        public DashboardViewModel()
        {
            // بيانات مؤشرات
            Kpis.Add(new KpiItem("مبيعات اليوم", "↑ +12% عن الأمس", "42,350"));
            Kpis.Add(new KpiItem("فواتير غير مدفوعة", "↓ -3 فواتير", "18"));
            Kpis.Add(new KpiItem("عملاء نشطون", "↑ +5 هذا الأسبوع", "231"));
            Kpis.Add(new KpiItem("مرتجع اليوم", "ثابت", "1,250"));

            // مبيعات آخر 7 أيام (مقياس ارتفاع 20..160)
            var rnd = new Random(3);
            string[] days = { "س", "أ", "ث", "أر", "خ", "ج", "سـ" };
            for (int i = 0; i < 7; i++)
                Sales7Days.Add(new SalesBar
                {
                    Label = days[i],
                    Height = 40 + rnd.Next(0, 120) // ارتفاع عمود بسيط
                });

            // أفضل العملاء
            TopCustomers.Add(new TopCustomer("شركة النور", 98500));
            TopCustomers.Add(new TopCustomer("مؤسسة الأمانة", 74200));
            TopCustomers.Add(new TopCustomer("الروّاد للتجارة", 56800));
            TopCustomers.Add(new TopCustomer("عبدالله القحطاني", 43100));

            // فواتير حديثة
            RecentInvoices.Add(new InvoiceRow("INV-2025-0012", "شركة النور", DateTime.Today, 15230.45m, "مدفوعة"));
            RecentInvoices.Add(new InvoiceRow("INV-2025-0011", "مؤسسة الأمانة", DateTime.Today.AddDays(-1), 8200m, "قيد التحصيل"));
            RecentInvoices.Add(new InvoiceRow("INV-2025-0010", "الروّاد للتجارة", DateTime.Today.AddDays(-1), 16400.90m, "غير مدفوعة"));
            RecentInvoices.Add(new InvoiceRow("INV-2025-0009", "عبدالله القحطاني", DateTime.Today.AddDays(-2), 3100m, "مدفوعة"));
        }
    }

    public class KpiItem
    {
        public string Title { get; }
        public string Subtitle { get; }
        public string Value { get; }

        public KpiItem(string title, string subtitle, string value)
        {
            Title = title; Subtitle = subtitle; Value = value;
        }
    }

    public class SalesBar
    {
        public string Label { get; set; } = "";
        public double Height { get; set; }   // يُستخدم مباشرةً كارتفاع للعمود (بسيط)
    }

    public class TopCustomer
    {
        public string Name { get; }
        public decimal Amount { get; }
        public TopCustomer(string name, decimal amount) { Name = name; Amount = amount; }
    }

    public class InvoiceRow
    {
        public string Number { get; }
        public string Customer { get; }
        public DateTime Date { get; }
        public decimal Total { get; }
        public string Status { get; }

        public InvoiceRow(string number, string customer, DateTime date, decimal total, string status)
        {
            Number = number; Customer = customer; Date = date; Total = total; Status = status;
        }
    }
}
