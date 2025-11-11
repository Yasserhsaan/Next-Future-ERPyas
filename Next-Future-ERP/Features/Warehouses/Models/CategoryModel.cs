using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Next_Future_ERP.Features.Warehouses.Models
{
    public class CategoryModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Key]
        public int CategoryID { get; set; }
        
        private string _categoryCode = string.Empty;
        public string CategoryCode
        {
            get => _categoryCode;
            set
            {
                if (_categoryCode == value) return;
                _categoryCode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CategoryCode)));
            }
        }

        private string _categoryName = string.Empty;
        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (_categoryName == value) return;
                _categoryName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CategoryName)));
            }
        }

        private int? _parentCategoryID;
        public int? ParentCategoryID
        {
            get => _parentCategoryID;
            set
            {
                if (_parentCategoryID == value) return;
                _parentCategoryID = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentCategoryID)));
            }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
    }
} 