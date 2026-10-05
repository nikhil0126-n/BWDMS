/* ============================================================================
   BWDMS - Balaji Wafer Dealer Management System
   FILE   : Database\02_ReceiptsAndOrderSource.sql
   PURPOSE: Add the objects required by PART 12 (company stock receipts)
            and PART 13 (order sources) that were missing from 01_Schema.sql.

   WHY THIS IS A SEPARATE FILE
   ---------------------------
   01_Schema.sql created Inventory, StockTransactions and Orders, but never
   created the company-receipt tables, and Orders had no OrderSource column.
   This script closes exactly those two gaps.

   IMPORTANT
   ---------
   This script deliberately contains NO "USE <database>" statement.
   Your connection string points at a LocalDB *file*
   (AttachDbFilename=|DataDirectory|\Database1.mdf), so the database is NOT
   named "BWDMS" - it is attached under its physical file path.

   Always run this through Database\Deploy-Database.ps1, which reads
   Web.config, resolves the real database name and verifies it first.

   The script is IDEMPOTENT: it can be re-run safely.
   ============================================================================ */


/* ============================================================================
   SECTION 1 - COMPANY STOCK RECEIPTS   (PART 12)
   ----------------------------------------------------------------------------
   Header + lines for goods received from Balaji Wafers (the company).

   Posting a receipt is the only way stock enters the godown through this
   screen; it writes a StockTransactions ledger row and increases Inventory
   inside one SqlTransaction so the two can never disagree.
   ============================================================================ */

IF OBJECT_ID('dbo.CompanyStockReceipts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CompanyStockReceipts
    (
        ReceiptId        INT           IDENTITY(1,1) NOT NULL,
        DealerId         INT           NOT NULL,
        ReceiptNumber    NVARCHAR(50)  NOT NULL,
        ReceiptDate      DATE          NOT NULL,
        CompanyInvoiceNo NVARCHAR(100) NULL,
        Status           NVARCHAR(20)  NOT NULL
                         CONSTRAINT DF_CompanyStockReceipts_Status
                             DEFAULT ('Draft'),
        TotalCost        DECIMAL(18,2) NOT NULL
                         CONSTRAINT DF_CompanyStockReceipts_TotalCost
                             DEFAULT (0),
        Notes            NVARCHAR(1000) NULL,
        CreatedBy        INT           NOT NULL,
        CreatedAt        DATETIME      NOT NULL
                         CONSTRAINT DF_CompanyStockReceipts_CreatedAt
                             DEFAULT (GETDATE()),
        UpdatedBy        INT           NULL,
        UpdatedAt        DATETIME      NULL,

        CONSTRAINT PK_CompanyStockReceipts
            PRIMARY KEY CLUSTERED (ReceiptId),

        -- A receipt number is unique inside the dealer that owns it,
        -- never globally - two dealers may legitimately reuse a number.
        CONSTRAINT UQ_CompanyStockReceipts_Dealer_Number
            UNIQUE (DealerId, ReceiptNumber),

        CONSTRAINT FK_CompanyStockReceipts_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId),

        -- Only Draft receipts can be edited. Posted is terminal: the
        -- stock has already moved and the ledger already written.
        CONSTRAINT CK_CompanyStockReceipts_Status
            CHECK (Status IN ('Draft', 'Posted')),

        CONSTRAINT CK_CompanyStockReceipts_TotalCost
            CHECK (TotalCost >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_CompanyStockReceipts_Dealer
        ON dbo.CompanyStockReceipts (DealerId, ReceiptDate DESC)
        INCLUDE (ReceiptNumber, Status, TotalCost);
END
GO


IF OBJECT_ID('dbo.CompanyStockReceiptDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CompanyStockReceiptDetails
    (
        ReceiptDetailId INT           IDENTITY(1,1) NOT NULL,
        ReceiptId       INT           NOT NULL,
        ProductVariantId INT          NOT NULL,
        UnitId          INT           NULL,
        Quantity        INT           NOT NULL,
        UnitCostPrice   DECIMAL(18,2) NOT NULL,
        TotalCost       DECIMAL(18,2) NOT NULL,

        CONSTRAINT PK_CompanyStockReceiptDetails
            PRIMARY KEY CLUSTERED (ReceiptDetailId),

        CONSTRAINT FK_CompanyStockReceiptDetails_Receipt
            FOREIGN KEY (ReceiptId)
                REFERENCES dbo.CompanyStockReceipts (ReceiptId),

        CONSTRAINT FK_CompanyStockReceiptDetails_Variant
            FOREIGN KEY (ProductVariantId)
                REFERENCES dbo.ProductVariants (ProductVariantId),

        -- A variant is listed at most once per receipt; quantities for the
        -- same variant are summed before the receipt is saved.
        CONSTRAINT UQ_CompanyStockReceiptDetails_Receipt_Variant
            UNIQUE (ReceiptId, ProductVariantId),

        CONSTRAINT CK_CompanyStockReceiptDetails_Quantity
            CHECK (Quantity > 0),

        CONSTRAINT CK_CompanyStockReceiptDetails_UnitCostPrice
            CHECK (UnitCostPrice >= 0),

        CONSTRAINT CK_CompanyStockReceiptDetails_TotalCost
            CHECK (TotalCost >= 0)
    );

    CREATE NONCLUSTERED INDEX IX_CompanyStockReceiptDetails_Receipt
        ON dbo.CompanyStockReceiptDetails (ReceiptId);
END
GO


/* ============================================================================
   SECTION 2 - ORDER SOURCE   (PART 13)
   ----------------------------------------------------------------------------
   PART 13 requires three order sources:

     Beat      - salesman creates it during a shop visit
     Telephone - dealer/employee records it for the next day
     Counter   - shop places it at the dealer's warehouse

   OrderType (Standard / Order Taking) already existed and describes WHAT
   KIND of order it is; OrderSource describes WHO BROUGHT IT IN. They are
   deliberately kept as separate columns so existing rows keep working.
   ============================================================================ */

IF COL_LENGTH('dbo.Orders', 'OrderSource') IS NULL
BEGIN
    ALTER TABLE dbo.Orders
        ADD OrderSource NVARCHAR(20) NULL
            CONSTRAINT DF_Orders_OrderSource DEFAULT ('Counter');
END
GO

-- Backfill from the only information we have: orders raised by a salesman
-- on a route are beat orders; everything else came to the counter.
UPDATE dbo.Orders
SET    OrderSource = CASE
                         WHEN OrderType = 'Order Taking' THEN 'Beat'
                         ELSE 'Counter'
                     END
WHERE  OrderSource IS NULL;
GO

-- Add the check constraint only when it is missing, otherwise a re-run
-- of this script would fail.
IF NOT EXISTS
(
    SELECT 1
    FROM   sys.check_constraints
    WHERE  name = 'CK_Orders_OrderSource'
      AND   parent_object_id = OBJECT_ID('dbo.Orders')
)
BEGIN
    ALTER TABLE dbo.Orders
        ADD CONSTRAINT CK_Orders_OrderSource
            CHECK (OrderSource IN ('Beat', 'Telephone', 'Counter'));
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM   sys.indexes
    WHERE  name = 'IX_Orders_Source'
      AND   object_id = OBJECT_ID('dbo.Orders')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_Source
        ON dbo.Orders (DealerId, OrderSource, Status);
END
GO
