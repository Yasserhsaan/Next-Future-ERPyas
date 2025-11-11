using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Inventory.Models
{
    /// <summary>
    /// طريقة إدخال بيانات الجرد الافتتاحي
    /// </summary>
    public enum EntryMethod : byte
    {
        Manual = 1,     // يدوي - المستخدم يُدخل البيانات يدوياً
        Auto = 2        // تلقائي - النظام ينزّل قائمة الأصناف/المخازن
    }

    /// <summary>
    /// طريقة عرض البيانات في الشاشة
    /// </summary>
    public enum ViewMode : byte
    {
        ByItem = 1,         // حسب الصنف - الصنف يتكرر بعدد المخازن
        ByWarehouse = 2     // حسب المخزن - تعرض أصناف كل مخزن على حدة
    }

    /// <summary>
    /// طريقة احتساب التكاليف
    /// </summary>
    public enum CostMethod : byte
    {
        WeightedAverage = 1,    // المتوسط المرجح
        FIFO = 2               // الوارد أولاً الصادر أولاً
    }

    /// <summary>
    /// نطاق المتوسط المرجح
    /// </summary>
    public enum WeightedAvgScope : byte
    {
        ByItem = 1,             // حسب الصنف
        ByItemWarehouse = 2     // حسب الصنف والمخزن
    }

    /// <summary>
    /// حالة مستند الجرد الافتتاحي
    /// </summary>
    public enum InventoryOpeningStatus : byte
    {
        Draft = 1,      // مسودة - يمكن التعديل
        Approved = 2    // معتمد - لا يمكن التعديل
    }
}
