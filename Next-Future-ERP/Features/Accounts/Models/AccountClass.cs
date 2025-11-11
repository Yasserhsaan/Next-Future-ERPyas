// Models/AccountClass.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public partial class AccountClass : ObservableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [ObservableProperty] private int accountClassId;

    [ObservableProperty] private string accountClassAname = string.Empty;
    [ObservableProperty] private string accountClassEname = string.Empty;

    // المخزّن في الجدول (CategoryKey)
 
    [ObservableProperty] private string categoryKey = string.Empty;

    // للعرض فقط في الجدول
    private string? _categoryNameArDisplay;

    [NotMapped]
    public string? CategoryNameArDisplay
    {
        get => _categoryNameArDisplay;
        set => SetProperty(ref _categoryNameArDisplay, value); // من ObservableObject
    }

}
