-- =============================================
-- Script: Add IsBlacklisted Column to Suppliers Table
-- Description: إضافة عمود القائمة السوداء إلى جدول الموردين
-- Author: Next Future ERP Team
-- Created: 2024
-- =============================================

-- التحقق من وجود العمود
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Suppliers]') AND name = 'IsBlacklisted')
BEGIN
    -- إضافة العمود
    ALTER TABLE [dbo].[Suppliers] 
    ADD [IsBlacklisted] bit NULL;
    
    PRINT 'تم إضافة عمود IsBlacklisted إلى جدول Suppliers';
    
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
    
    PRINT 'تم تعيين القيمة الافتراضية للعمود IsBlacklisted';
END
ELSE
BEGIN
    PRINT 'عمود IsBlacklisted موجود بالفعل في جدول Suppliers';
END
GO

-- إضافة تعليق على العمود
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'يحدد ما إذا كان المورد مدرج في القائمة السوداء', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'Suppliers', 
    @level2type = N'COLUMN', @level2name = N'IsBlacklisted';
GO

PRINT 'تم تنفيذ سكريبت إضافة عمود IsBlacklisted بنجاح';
GO
