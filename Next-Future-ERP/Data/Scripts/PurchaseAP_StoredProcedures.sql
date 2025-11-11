-- =============================================
-- إجراءات فواتير المشتريات والمرتجعات (PI/PR)
-- حسب الدليل التنفيذي المفصل
-- الإصدار: 1.1 — التاريخ: 2025-09-13 (UTC)
-- =============================================

-- =============================================
-- 1. إجراء التحقق من صحة البيانات
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_Validate]
    @APId BIGINT = NULL,
    @CompanyId INT,
    @BranchId INT,
    @DocType CHAR(2),
    @DocNumber NVARCHAR(50),
    @SupplierId INT,
    @ReferenceNumber NVARCHAR(50) = NULL,
    @DocDate DATE,
    @Details NVARCHAR(MAX) -- JSON array of details
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsValid BIT = 1;
    DECLARE @ErrorMessage NVARCHAR(MAX) = '';
    
    -- 1. التحقق من وجود المورد وحسابه
    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE SupplierID = @SupplierId AND AccountID IS NOT NULL)
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = @ErrorMessage + 'المورّد غير موجود أو بدون حساب محاسبي;';
    END
    
    -- 2. التحقق من عدم تكرار رقم المستند الداخلي
    IF EXISTS (SELECT 1 FROM PurchaseAP 
               WHERE CompanyId = @CompanyId 
                 AND BranchId = @BranchId 
                 AND DocType = @DocType 
                 AND DocNumber = @DocNumber 
                 AND (@APId IS NULL OR APId != @APId))
    BEGIN
        SET @IsValid = 0;
        SET @ErrorMessage = @ErrorMessage + 'رقم المستند الداخلي مكرر;';
    END
    
    -- 3. التحقق من عدم تكرار رقم فاتورة المورد (للفواتير فقط)
    IF @DocType = 'PI' AND @ReferenceNumber IS NOT NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM PurchaseAP 
                   WHERE CompanyId = @CompanyId 
                     AND SupplierId = @SupplierId 
                     AND ReferenceNumber = @ReferenceNumber 
                     AND (@APId IS NULL OR APId != @APId))
        BEGIN
            SET @IsValid = 0;
            SET @ErrorMessage = @ErrorMessage + 'رقم فاتورة المورد مكرر;';
        END
    END
    
    -- 4. التحقق من التفاصيل (JSON parsing)
    -- TODO: إضافة تحليل JSON للتفاصيل
    
    -- إرجاع النتيجة
    SELECT @IsValid AS IsValid, @ErrorMessage AS ErrorMessage;
END
GO

-- =============================================
-- 2. إجراء حساب الضرائب والمجاميع
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_CalculateTotals]
    @APId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @SubTotal DECIMAL(18,4) = 0;
    DECLARE @TaxAmount DECIMAL(18,4) = 0;
    DECLARE @TotalAmount DECIMAL(18,4) = 0;
    
    -- حساب المجاميع لكل سطر
    SELECT 
        @SubTotal = @SubTotal + ISNULL(TaxableAmount, 0),
        @TaxAmount = @TaxAmount + ISNULL(VATAmount, 0),
        @TotalAmount = @TotalAmount + ISNULL(LineTotal, 0)
    FROM PurchaseAPDetails 
    WHERE APId = @APId;
    
    -- تحديث الرأس
    UPDATE PurchaseAP 
    SET SubTotal = @SubTotal,
        TaxAmount = @TaxAmount,
        TotalAmount = @TotalAmount
    WHERE APId = @APId;
    
    -- إرجاع المجاميع
    SELECT @SubTotal AS SubTotal, @TaxAmount AS TaxAmount, @TotalAmount AS TotalAmount;
END
GO

-- =============================================
-- 3. إجراء الترحيل المحاسبي (السياسة ب)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_Post]
    @APId BIGINT,
    @UserId INT,
    @JournalEntryId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- التحقق من حالة المستند
        IF NOT EXISTS (SELECT 1 FROM PurchaseAP WHERE APId = @APId AND Status = 1)
        BEGIN
            RAISERROR('المستند غير موجود أو غير محفوظ', 16, 1);
            RETURN;
        END
        
        -- قفل المستند لمنع التعديل المتزامن
        UPDATE PurchaseAP 
        SET Status = 2 -- Posted
        WHERE APId = @APId AND Status = 1;
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('فشل في تحديث حالة المستند', 16, 1);
            RETURN;
        END
        
        -- إنشاء القيد المحاسبي
        EXEC [dbo].[sp_PurchaseAP_CreateJournalEntry] @APId, @UserId, @JournalEntryId OUTPUT;
        
        -- تحديث المستند برقم القيد
        UPDATE PurchaseAP 
        SET JournalEntryId = @JournalEntryId,
            ModifiedBy = @UserId,
            ModifiedAt = GETUTCDATE()
        WHERE APId = @APId;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- 4. إجراء إنشاء القيد المحاسبي
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_CreateJournalEntry]
    @APId BIGINT,
    @UserId INT,
    @JournalEntryId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DocType CHAR(2);
    DECLARE @SupplierId INT;
    DECLARE @TotalAmount DECIMAL(18,4);
    DECLARE @SupplierAccountId INT;
    DECLARE @GRNIAccountId INT = 1001; -- TODO: من إعدادات الشركة
    
    -- الحصول على بيانات المستند
    SELECT @DocType = DocType, @SupplierId = SupplierId, @TotalAmount = TotalAmount
    FROM PurchaseAP 
    WHERE APId = @APId;
    
    -- الحصول على حساب المورد
    SELECT @SupplierAccountId = AccountID 
    FROM Suppliers 
    WHERE SupplierID = @SupplierId;
    
    -- إنشاء القيد المحاسبي
    INSERT INTO GeneralJournalEntries (
        CompanyId, BranchId, EntryDate, Reference, 
        Description, Status, CreatedBy, CreatedAt
    )
    VALUES (
        1, 1, GETDATE(), 
        (SELECT DocNumber FROM PurchaseAP WHERE APId = @APId),
        CASE WHEN @DocType = 'PI' THEN 'فاتورة مشتريات' ELSE 'مرتجع مشتريات' END,
        1, @UserId, GETUTCDATE()
    );
    
    SET @JournalEntryId = SCOPE_IDENTITY();
    
    -- إضافة تفاصيل القيد
    IF @DocType = 'PI'
    BEGIN
        -- فاتورة مشتريات: مدين GRNI، دائن المورد
        INSERT INTO GeneralJournalDetails (JournalEntryId, AccountId, Debit, Credit, Description)
        VALUES (@JournalEntryId, @GRNIAccountId, @TotalAmount, 0, 'فاتورة مشتريات');
        
        INSERT INTO GeneralJournalDetails (JournalEntryId, AccountId, Debit, Credit, Description)
        VALUES (@JournalEntryId, @SupplierAccountId, 0, @TotalAmount, 'فاتورة مشتريات');
    END
    ELSE
    BEGIN
        -- مرتجع مشتريات: مدين المورد، دائن GRNI
        INSERT INTO GeneralJournalDetails (JournalEntryId, AccountId, Debit, Credit, Description)
        VALUES (@JournalEntryId, @SupplierAccountId, @TotalAmount, 0, 'مرتجع مشتريات');
        
        INSERT INTO GeneralJournalDetails (JournalEntryId, AccountId, Debit, Credit, Description)
        VALUES (@JournalEntryId, @GRNIAccountId, 0, @TotalAmount, 'مرتجع مشتريات');
    END
END
GO

-- =============================================
-- 5. إجراء عكس الترحيل
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_Unpost]
    @APId BIGINT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @JournalEntryId BIGINT;
        
        -- الحصول على رقم القيد
        SELECT @JournalEntryId = JournalEntryId 
        FROM PurchaseAP 
        WHERE APId = @APId AND Status = 2;
        
        IF @JournalEntryId IS NULL
        BEGIN
            RAISERROR('المستند غير مرحل', 16, 1);
            RETURN;
        END
        
        -- عكس القيد المحاسبي
        EXEC [dbo].[sp_PurchaseAP_ReverseJournalEntry] @JournalEntryId;
        
        -- تحديث حالة المستند
        UPDATE PurchaseAP 
        SET Status = 1, -- Saved
            JournalEntryId = NULL,
            ModifiedBy = @UserId,
            ModifiedAt = GETUTCDATE()
        WHERE APId = @APId;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- =============================================
-- 6. إجراء عكس القيد المحاسبي
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_ReverseJournalEntry]
    @JournalEntryId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- إنشاء قيد عكسي
    DECLARE @ReverseEntryId BIGINT;
    
    INSERT INTO GeneralJournalEntries (
        CompanyId, BranchId, EntryDate, Reference, 
        Description, Status, CreatedBy, CreatedAt
    )
    SELECT 
        CompanyId, BranchId, EntryDate, 
        Reference + ' (عكسي)',
        Description + ' - عكس',
        1, CreatedBy, GETUTCDATE()
    FROM GeneralJournalEntries 
    WHERE JournalEntryId = @JournalEntryId;
    
    SET @ReverseEntryId = SCOPE_IDENTITY();
    
    -- إضافة تفاصيل القيد العكسي (تبديل المدين والدائن)
    INSERT INTO GeneralJournalDetails (JournalEntryId, AccountId, Debit, Credit, Description)
    SELECT 
        @ReverseEntryId, AccountId, Credit, Debit, 
        Description + ' - عكس'
    FROM GeneralJournalDetails 
    WHERE JournalEntryId = @JournalEntryId;
END
GO

-- =============================================
-- 7. إجراء الحصول على الكمية المتبقية من سند الاستلام
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_GetRemainingQuantity]
    @ReceiptDetailId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ReceivedQty DECIMAL(18,4);
    DECLARE @InvoicedQty DECIMAL(18,4);
    DECLARE @RemainingQty DECIMAL(18,4);
    
    -- الحصول على الكمية المستلمة
    SELECT @ReceivedQty = Quantity 
    FROM StoreReceiptsDetailed 
    WHERE DetailId = @ReceiptDetailId;
    
    -- حساب الكمية المفوترة
    SELECT @InvoicedQty = ISNULL(SUM(Quantity), 0)
    FROM PurchaseAPDetails 
    WHERE ReceiptDetailId = @ReceiptDetailId;
    
    SET @RemainingQty = @ReceivedQty - @InvoicedQty;
    
    SELECT 
        @ReceivedQty AS ReceivedQuantity,
        @InvoicedQty AS InvoicedQuantity,
        @RemainingQty AS RemainingQuantity;
END
GO

-- =============================================
-- 8. إجراء توليد رقم المستند التالي
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_GenerateNextNumber]
    @CompanyId INT,
    @BranchId INT,
    @DocType CHAR(2),
    @NextNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Year INT = YEAR(GETDATE());
    DECLARE @Month INT = MONTH(GETDATE());
    DECLARE @Prefix NVARCHAR(10);
    DECLARE @LastSequence INT = 0;
    
    -- تحديد البادئة حسب نوع المستند
    SET @Prefix = CASE @DocType 
        WHEN 'PI' THEN 'PI'
        WHEN 'PR' THEN 'PR'
        ELSE 'AP'
    END;
    
    -- الحصول على آخر رقم
    SELECT @LastSequence = ISNULL(MAX(
        CASE 
            WHEN LEN(DocNumber) > 10 
            THEN CAST(RIGHT(DocNumber, 5) AS INT)
            ELSE 0
        END
    ), 0)
    FROM PurchaseAP 
    WHERE CompanyId = @CompanyId 
      AND BranchId = @BranchId 
      AND DocType = @DocType
      AND DocNumber LIKE @Prefix + '-' + CAST(@Year AS VARCHAR) + '-' + CAST(@Month AS VARCHAR) + '-%';
    
    -- توليد الرقم التالي
    SET @NextNumber = @Prefix + '-' + CAST(@Year AS VARCHAR) + '-' + CAST(@Month AS VARCHAR) + '-' + RIGHT('00000' + CAST(@LastSequence + 1 AS VARCHAR), 5);
END
GO

-- =============================================
-- 9. إجراء الحصول على فواتير المورد
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_GetBySupplier]
    @SupplierId INT,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @DocType CHAR(2) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.APId,
        p.DocNumber,
        p.DocType,
        p.DocDate,
        p.ReferenceNumber,
        p.SubTotal,
        p.TaxAmount,
        p.TotalAmount,
        p.Status,
        p.Remarks,
        s.SupplierName
    FROM PurchaseAP p
    INNER JOIN Suppliers s ON p.SupplierId = s.SupplierID
    WHERE p.SupplierId = @SupplierId
      AND (@FromDate IS NULL OR p.DocDate >= @FromDate)
      AND (@ToDate IS NULL OR p.DocDate <= @ToDate)
      AND (@DocType IS NULL OR p.DocType = @DocType)
    ORDER BY p.DocDate DESC, p.APId DESC;
END
GO

-- =============================================
-- 10. إجراء الحصول على تقرير المطابقة
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[sp_PurchaseAP_MatchingReport]
    @CompanyId INT,
    @FromDate DATE,
    @ToDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.DocNumber,
        p.DocType,
        p.DocDate,
        s.SupplierName,
        p.RelatedReceiptId,
        sr.ReceiptNumber,
        p.RelatedPOId,
        po.TxnNumber AS PONumber,
        p.SubTotal,
        p.TaxAmount,
        p.TotalAmount,
        p.Status,
        CASE 
            WHEN p.RelatedReceiptId IS NOT NULL AND p.RelatedPOId IS NOT NULL THEN 'مطابق كامل'
            WHEN p.RelatedReceiptId IS NOT NULL THEN 'مطابق جزئي (استلام)'
            WHEN p.RelatedPOId IS NOT NULL THEN 'مطابق جزئي (أمر شراء)'
            ELSE 'غير مطابق'
        END AS MatchingStatus
    FROM PurchaseAP p
    INNER JOIN Suppliers s ON p.SupplierId = s.SupplierID
    LEFT JOIN StoreReceipts sr ON p.RelatedReceiptId = sr.ReceiptId
    LEFT JOIN PurchaseTxn po ON p.RelatedPOId = po.TxnID
    WHERE p.CompanyId = @CompanyId
      AND p.DocDate BETWEEN @FromDate AND @ToDate
    ORDER BY p.DocDate DESC;
END
GO

-- =============================================
-- إنشاء الفهارس المطلوبة
-- =============================================

-- فهرس فريدة رقم المستند
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_PurchaseAP_Number')
BEGIN
    CREATE UNIQUE INDEX UQ_PurchaseAP_Number 
    ON PurchaseAP (CompanyId, BranchId, DocType, DocNumber);
END

-- فهرس المورد والتاريخ
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PurchaseAP_Supplier')
BEGIN
    CREATE INDEX IX_PurchaseAP_Supplier 
    ON PurchaseAP (CompanyId, BranchId, SupplierId, DocDate);
END

-- فهرس سند الاستلام
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PurchaseAP_Receipt')
BEGIN
    CREATE INDEX IX_PurchaseAP_Receipt 
    ON PurchaseAP (CompanyId, BranchId, RelatedReceiptId);
END

-- فهرس فريدة رقم فاتورة المورد (اختياري)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_PurchaseAP_SupplierRef')
BEGIN
    CREATE UNIQUE INDEX UQ_PurchaseAP_SupplierRef
    ON PurchaseAP (CompanyId, SupplierId, ReferenceNumber)
    WHERE DocType = 'PI' AND ReferenceNumber IS NOT NULL;
END

-- فهارس التفاصيل
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PAPDetails_AP')
BEGIN
    CREATE INDEX IX_PAPDetails_AP 
    ON PurchaseAPDetails (APId, LineNo);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PAPDetails_Item')
BEGIN
    CREATE INDEX IX_PAPDetails_Item 
    ON PurchaseAPDetails (ItemId, WarehouseId);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PAPDetails_PO')
BEGIN
    CREATE INDEX IX_PAPDetails_PO 
    ON PurchaseAPDetails (PurchaseDetailId);
END

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PAPDetails_RCPT')
BEGIN
    CREATE INDEX IX_PAPDetails_RCPT 
    ON PurchaseAPDetails (ReceiptDetailId);
END

PRINT 'تم إنشاء إجراءات فواتير المشتريات والمرتجعات بنجاح';
