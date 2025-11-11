using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.StoreSetting.Models
{
    public enum ValuationMethod
    {
        AverageCost = 1, // متوسط التكلفة
        Standard = 2,    // معياري
        LIFO = 3,
        FIFO = 4
    }

    // لطريقة عرض التاريخ
    public enum DateDisplayMode
    {
        Gregorian = 1,
        Hijri = 2,
        Custom = 3
    }

    // لأنماط التسلسل (بالشرح: يسمح بالتعديل/لا يسمح/يدوي لكل)
    public enum SequencePolicy
    {
        Editable = 1, // يسمح بالتعديل
        Locked = 2,   // لا يسمح بالتعديل
        ManualPerDoc = 3 // يدوي لكل مستند
    }
}
