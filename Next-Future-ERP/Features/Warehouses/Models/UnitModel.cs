using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Models
{
    public class UnitModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int UnitID { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
      
        private char _unitType; // '1': Numeric, '2': Measurable
        public char UnitType
        {
            get => _unitType;
            set
            {
                if (_unitType == value) return;
                _unitType = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnitType)));
            }
        }

        private char _unitClass; // '1': Weight, '2': Area, etc.
        public char UnitClass
        {
            get => _unitClass;
            set
            {
                if (_unitClass == value) return;
                _unitClass = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnitClass)));
            }
        }
        public int? BaseUnitID { get; set; }
        public decimal? ConversionFactor { get; set; }
        public bool? IsActive { get; set; }
        public int? DefaultPackaging { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
