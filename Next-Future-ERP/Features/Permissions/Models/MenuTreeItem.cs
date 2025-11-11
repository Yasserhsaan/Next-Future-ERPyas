using System.Collections.ObjectModel;

namespace Next_Future_ERP.Features.Permissions.Models
{
    public class MenuTreeItem
    {
        public MenuForm MenuForm { get; set; } = null!;
        public ObservableCollection<MenuTreeItem> Children { get; set; } = new();
        public bool IsExpanded { get; set; } = false;
        public bool IsSelected { get; set; } = false;
        
        // Permission flags for the current user
        public bool CanAdd { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
        public bool CanView { get; set; } = false;
        public bool CanPost { get; set; } = false;
        public bool CanPrint { get; set; } = false;
        public bool CanRun { get; set; } = false;

        public string DisplayName => MenuForm.DisplayName;
        public bool IsVisible => MenuForm.IsVisible;
        public bool IsParent => MenuForm.IsParent;
        public string? ProgramExecutable => MenuForm.ProgramExecutable;

        public bool HasAnyPermission => CanAdd || CanEdit || CanDelete || CanView || CanPost || CanPrint || CanRun;
    }
}
