// DocumentTypesViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Next_Future_ERP.Features.Accounts.Services;
using Next_Future_ERP.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Next_Future_ERP.Features.Accounts.ViewModels
{
    public class DocumentTypesViewModel : ObservableObject, IDisposable
    {
        private readonly DocumentTypeService _service;
        private bool _disposed = false;

        public ObservableCollection<DocumentType> DocumentTypes { get; } = new();

        private DocumentType _selectedDocumentType;
        public DocumentType SelectedDocumentType
        {
            get => _selectedDocumentType;
            set => SetProperty(ref _selectedDocumentType, value);
        }

        private DocumentType _formDocumentType = new();
        public DocumentType FormDocumentType
        {
            get => _formDocumentType;
            set => SetProperty(ref _formDocumentType, value);
        }

        private bool _isFormVisible;
        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => SetProperty(ref _isFormVisible, value);
        }

        private bool _isEditMode = false;

        public ICommand StartAddCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public DocumentTypesViewModel()
        {
            _service = new DocumentTypeService();
            StartAddCommand = new RelayCommand(StartAdd);
            StartEditCommand = new RelayCommand(StartEdit);
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);

            LoadDocumentTypes();
        }

        private async void LoadDocumentTypes()
        {
            try
            {
                DocumentTypes.Clear();
                var list = await _service.GetAllAsync();
                foreach (var documentType in list)
                    DocumentTypes.Add(documentType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ في تحميل أنواع المستندات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartAdd()
        {
            FormDocumentType = new DocumentType
            {
                IsActive = true,
                IsSystem = false,
                CreatedAt = DateTime.Now
            };
            IsFormVisible = true;
            _isEditMode = false;
        }

        private void StartEdit()
        {
            if (SelectedDocumentType == null) return;

            if (SelectedDocumentType.IsSystem)
            {
                MessageBox.Show("لا يمكن تعديل أنواع المستندات النظامية", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FormDocumentType = new DocumentType
            {
                DocumentTypeId = SelectedDocumentType.DocumentTypeId,
                DocumentCode = SelectedDocumentType.DocumentCode,
                DocumentNameAr = SelectedDocumentType.DocumentNameAr,
                DocumentNameEn = SelectedDocumentType.DocumentNameEn,
                IsActive = SelectedDocumentType.IsActive,
                ModuleId = SelectedDocumentType.ModuleId,
                SequencePrefix = SelectedDocumentType.SequencePrefix,
                IsSystem = SelectedDocumentType.IsSystem,
                CreatedAt = SelectedDocumentType.CreatedAt,
                CreatedBy = SelectedDocumentType.CreatedBy,
                ModifiedAt = DateTime.Now,
                ModifiedBy = 1 // يجب الحصول من النظام
            };
            _isEditMode = true;
            IsFormVisible = true;
        }

        private async void Save()
        {
            try
            {
                // التحقق من القيم المطلوبة
                if (string.IsNullOrWhiteSpace(FormDocumentType.DocumentCode))
                {
                    MessageBox.Show("رمز المستند مطلوب", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(FormDocumentType.DocumentNameAr))
                {
                    MessageBox.Show("اسم المستند بالعربية مطلوب", "حقل إجباري", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _service.SaveAsync(FormDocumentType);
                IsFormVisible = false;
                LoadDocumentTypes();
                MessageBox.Show("تم حفظ نوع المستند بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الحفظ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            IsFormVisible = false;
        }

        private async void Delete()
        {
            if (SelectedDocumentType != null)
            {
                if (SelectedDocumentType.IsSystem)
                {
                    MessageBox.Show("لا يمكن حذف أنواع المستندات النظامية", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("هل أنت متأكد من حذف هذا النوع؟", "تأكيد الحذف",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _service.DeleteAsync(SelectedDocumentType.DocumentTypeId);
                        LoadDocumentTypes();
                        MessageBox.Show("تم حذف نوع المستند بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _service?.Dispose();
                _disposed = true;
            }
        }

        ~DocumentTypesViewModel()
        {
            Dispose(false);
        }
    }
}