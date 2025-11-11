# ميزة القائمة السوداء للموردين

## نظرة عامة
تم إضافة ميزة "القائمة السوداء" للموردين لمنع إنشاء أوامر الشراء للموردين غير المرغوب فيهم.

## التحديثات المطبقة

### 1. تحديث نموذج البيانات
- **الملف**: `Features/Suppliers/Models/Supplier.cs`
- **التحديث**: إضافة خاصية `IsBlacklisted` من نوع `bool?`

```csharp
public bool? IsBlacklisted { get; set; }
```

### 2. تحديث واجهة المستخدم - شاشة تعديل المورد
- **الملف**: `Features/Suppliers/Views/SupplierEditWindow.xaml`
- **التحديث**: إضافة CheckBox للقائمة السوداء بجانب "نشط"

```xml
<StackPanel Orientation="Horizontal">
    <CheckBox IsChecked="{Binding Model.IsBlacklisted}" />
    <TextBlock Margin="10,0,0,0" VerticalAlignment="Center" Text="القائمة السوداء" Foreground="#DC2626"/>
</StackPanel>
```

### 3. تحديث قائمة الموردين
- **الملف**: `Features/Suppliers/Views/SuppliersListView.xaml`
- **التحديث**: إضافة عمود "القائمة السوداء" في DataGrid

```xml
<DataGridCheckBoxColumn Header="القائمة السوداء" Binding="{Binding IsBlacklisted}" Width="120">
    <DataGridCheckBoxColumn.ElementStyle>
        <Style TargetType="CheckBox">
            <Setter Property="IsEnabled" Value="False"/>
            <Setter Property="Foreground" Value="#DC2626"/>
        </Style>
    </DataGridCheckBoxColumn.ElementStyle>
</DataGridCheckBoxColumn>
```

### 4. تحديث شاشة أوامر الشراء
- **الملف**: `Features/Purchases/ViewModels/PurchaseEditViewModel.cs`
- **التحديث**: إضافة منطق التحقق من القائمة السوداء

```csharp
[ObservableProperty] private bool isSupplierBlacklisted;

private async Task CheckSupplierBlacklistStatus()
{
    if (Model.SupplierID > 0)
    {
        var supplier = await _suppliers.GetByIdAsync(Model.SupplierID);
        IsSupplierBlacklisted = supplier?.IsBlacklisted == true;
        
        if (IsSupplierBlacklisted)
        {
            MessageBox.Show("تحذير: المورد المحدد مدرج في القائمة السوداء!", "تحذير", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
```

### 5. تحديث واجهة أوامر الشراء
- **الملف**: `Features/Purchases/Views/PurchaseEditWindow.xaml`
- **التحديث**: إضافة مؤشر بصري للقائمة السوداء

```xml
<Border Background="#FEF2F2" BorderBrush="#FECACA" BorderThickness="1" 
        CornerRadius="4" Padding="6,2" Margin="8,0,0,0"
        Visibility="{Binding IsSupplierBlacklisted, Converter={StaticResource BoolToVisibility}}">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="⚠️" FontSize="12" Margin="0,0,4,0"/>
        <TextBlock Text="القائمة السوداء" Foreground="#DC2626" FontSize="10" FontWeight="SemiBold"/>
    </StackPanel>
</Border>
```

### 6. تحديث Business Rules Engine
- **الملف**: `Features/Purchases/Services/PurchaseBusinessRulesEngine.cs`
- **التحديث**: إضافة قاعدة عمل للتحقق من القائمة السوداء

```csharp
case "SUPPLIER_BLACKLIST":
    if (data is PurchaseTxn order3)
    {
        var isBlacklisted = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.SupplierID == order3.SupplierID)
            .Select(s => s.IsBlacklisted)
            .FirstOrDefaultAsync();

        if (isBlacklisted == true)
        {
            result.AddViolation("SUPPLIER_BLACKLIST", "المورد مدرج في القائمة السوداء");
        }
    }
    break;
```

## سكريبت قاعدة البيانات

### إضافة العمود إلى جدول الموردين
```sql
-- التحقق من وجود العمود
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Suppliers]') AND name = 'IsBlacklisted')
BEGIN
    -- إضافة العمود
    ALTER TABLE [dbo].[Suppliers] 
    ADD [IsBlacklisted] bit NULL;
    
    -- تعيين القيمة الافتراضية
    UPDATE [dbo].[Suppliers] 
    SET [IsBlacklisted] = 0 
    WHERE [IsBlacklisted] IS NULL;
    
    -- جعل العمود NOT NULL مع القيمة الافتراضية
    ALTER TABLE [dbo].[Suppliers] 
    ALTER COLUMN [IsBlacklisted] bit NOT NULL;
    
    -- إضافة القيمة الافتراضية
    ALTER TABLE [dbo].[Suppliers] 
    ADD CONSTRAINT [DF_Suppliers_IsBlacklisted] 
    DEFAULT (0) FOR [IsBlacklisted];
END
```

## كيفية الاستخدام

### 1. إضافة مورد إلى القائمة السوداء
1. افتح شاشة الموردين
2. اختر المورد المراد إضافته للقائمة السوداء
3. اضغط "تعديل"
4. في تبويب "بيانات المورد"، فعّل مربع "القائمة السوداء"
5. احفظ التغييرات

### 2. التحقق من القائمة السوداء في أوامر الشراء
- عند إنشاء أمر شراء جديد، سيظهر تحذير إذا كان المورد في القائمة السوداء
- سيظهر مؤشر بصري أحمر بجانب اسم المورد
- يمكن للمستخدم المتابعة أو تغيير المورد

### 3. عرض الموردين في القائمة السوداء
- في قائمة الموردين، العمود "القائمة السوداء" سيظهر ✓ للموردين المدرجين
- يمكن فلترة القائمة لعرض الموردين في القائمة السوداء فقط

## الفوائد

1. **منع الأخطاء**: منع إنشاء أوامر الشراء للموردين غير المرغوب فيهم
2. **تحذيرات واضحة**: إشعارات بصرية ونصية واضحة
3. **سهولة الإدارة**: إمكانية إضافة/إزالة الموردين من القائمة بسهولة
4. **تتبع شامل**: إمكانية رؤية جميع الموردين في القائمة السوداء

## التطوير المستقبلي

- إضافة تقرير للموردين في القائمة السوداء
- إضافة أسباب إدراج المورد في القائمة السوداء
- إضافة صلاحيات للتحكم في من يمكنه إضافة/إزالة الموردين من القائمة
- إضافة إشعارات تلقائية عند محاولة استخدام مورد في القائمة السوداء
