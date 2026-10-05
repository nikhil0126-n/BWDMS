/* ============================================================================
   BWDMS - Balaji Wafers Dealer Management System
   FILE   : Database\01_Schema.sql
   PURPOSE: Create every table needed by Phases 1-10.

   IMPORTANT
   ---------
   This script deliberately contains NO "USE <database>" statement.
   Your connection string points at a LocalDB *file*
   (AttachDbFilename=|DataDirectory|\Database1.mdf), so the database is NOT
   named "BWDMS" - it is attached under its physical file path.

   Always run this through Database\Deploy-Database.ps1, which reads
   Web.config, resolves the real database name and verifies it first.

   The script is IDEMPOTENT: it can be re-run safely. It only creates what
   is missing and only adds columns that do not already exist.
   ============================================================================ */


/* ============================================================================
   SECTION 1 - EXISTING TABLES (create only if missing)
   ============================================================================ */

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FullName     NVARCHAR(200)     NOT NULL,
        Email        NVARCHAR(300)     NOT NULL,
        Phone        NVARCHAR(40)      NULL,
        PasswordHash NVARCHAR(512)     NOT NULL,
        Role         NVARCHAR(40)      NOT NULL,
        IsActive     BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        CreatedAt    DATETIME          NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (GETDATE())
    );

    CREATE UNIQUE INDEX UQ_Users_Email ON dbo.Users (Email);
END


IF OBJECT_ID('dbo.Villages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Villages
    (
        VillageId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Villages PRIMARY KEY,
        VillageName NVARCHAR(300)     NOT NULL,
        Taluka      NVARCHAR(200)     NULL,
        District    NVARCHAR(200)     NULL,
        Pincode     NVARCHAR(20)      NULL,
        IsActive    BIT               NOT NULL CONSTRAINT DF_Villages_IsActive DEFAULT (1),
        CreatedAt   DATETIME          NOT NULL CONSTRAINT DF_Villages_CreatedAt DEFAULT (GETDATE()),
        UpdatedAt   DATETIME          NULL
    );

    CREATE INDEX IX_Villages_VillageName ON dbo.Villages (VillageName);
END


IF OBJECT_ID('dbo.Routes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Routes
    (
        RouteId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Routes PRIMARY KEY,
        DealerId             INT               NOT NULL,
        RouteName            NVARCHAR(200)     NOT NULL,
        RouteCode            NVARCHAR(100)     NULL,
        RouteType            NVARCHAR(100)     NULL,
        DayOfWeek            NVARCHAR(40)      NULL,
        OrderDispatchDays    NVARCHAR(400)     NULL,
        PreferredSalesmanId  INT               NULL,
        PreferredDriverId    INT               NULL,
        DefaultVehicleId     INT               NULL,
        IsActive             BIT               NOT NULL CONSTRAINT DF_Routes_IsActive DEFAULT (1),
        CreatedBy            INT               NULL,
        CreatedAt            DATETIME          NOT NULL CONSTRAINT DF_Routes_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy            INT               NULL,
        UpdatedAt            DATETIME          NULL
    );

    CREATE INDEX IX_Routes_DealerId ON dbo.Routes (DealerId);
    CREATE UNIQUE INDEX UQ_Routes_Dealer_Code
        ON dbo.Routes (DealerId, RouteCode)
        WHERE RouteCode IS NOT NULL;
END


IF OBJECT_ID('dbo.Vehicles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehicles
    (
        VehicleId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vehicles PRIMARY KEY,
        DealerId      INT               NULL,
        VehicleNumber NVARCHAR(100)     NOT NULL,
        VehicleName   NVARCHAR(200)     NULL,
        VehicleType   NVARCHAR(100)     NULL,
        IsActive      BIT               NOT NULL CONSTRAINT DF_Vehicles_IsActive DEFAULT (1),
        CreatedBy     INT               NULL,
        CreatedAt     DATETIME          NOT NULL CONSTRAINT DF_Vehicles_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy     INT               NULL,
        UpdatedAt     DATETIME          NULL
    );

    CREATE UNIQUE INDEX UQ_Vehicles_VehicleNumber ON dbo.Vehicles (VehicleNumber);
END


IF OBJECT_ID('dbo.RouteSchedules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RouteSchedules
    (
        RouteScheduleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RouteSchedules PRIMARY KEY,
        RouteId         INT               NOT NULL,
        DayOfWeek       NVARCHAR(40)      NOT NULL,
        VehicleId       INT               NULL,
        SalesmanId      INT               NULL,
        DriverId        INT               NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_RouteSchedules_IsActive DEFAULT (1),
        CreatedBy       INT               NULL,
        CreatedAt       DATETIME          NOT NULL CONSTRAINT DF_RouteSchedules_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy       INT               NULL,
        UpdatedAt       DATETIME          NULL
    );

    CREATE INDEX IX_RouteSchedules_RouteId   ON dbo.RouteSchedules (RouteId);
    CREATE INDEX IX_RouteSchedules_DayOfWeek ON dbo.RouteSchedules (DayOfWeek);
END


IF OBJECT_ID('dbo.RouteVillages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RouteVillages
    (
        RouteVillageId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RouteVillages PRIMARY KEY,
        RouteId        INT               NOT NULL,
        VillageId      INT               NOT NULL,
        IsActive       BIT               NOT NULL CONSTRAINT DF_RouteVillages_IsActive DEFAULT (1),
        CreatedBy      INT               NULL,
        CreatedAt      DATETIME          NOT NULL CONSTRAINT DF_RouteVillages_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy      INT               NULL,
        UpdatedAt      DATETIME          NULL
    );

    CREATE INDEX IX_RouteVillages_RouteId   ON dbo.RouteVillages (RouteId);
    CREATE INDEX IX_RouteVillages_VillageId ON dbo.RouteVillages (VillageId);
END


IF OBJECT_ID('dbo.RouteVillageSchedules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RouteVillageSchedules
    (
        RouteVillageScheduleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RouteVillageSchedules PRIMARY KEY,
        RouteScheduleId        INT               NOT NULL,
        RouteVillageId         INT               NOT NULL,
        VisitSequence          INT               NOT NULL CONSTRAINT DF_RVS_VisitSequence DEFAULT (1),
        IsActive               BIT               NOT NULL CONSTRAINT DF_RVS_IsActive DEFAULT (1),
        CreatedBy              INT               NULL,
        CreatedAt              DATETIME          NOT NULL CONSTRAINT DF_RVS_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy              INT               NULL,
        UpdatedAt              DATETIME          NULL
    );

    CREATE INDEX IX_RVS_ScheduleId ON dbo.RouteVillageSchedules (RouteScheduleId);
END


IF OBJECT_ID('dbo.WeeklyRoutePlans', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WeeklyRoutePlans
    (
        WeeklyRoutePlanId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WeeklyRoutePlans PRIMARY KEY,
        DealerId          INT               NOT NULL,
        RouteId           INT               NOT NULL,
        WeekStartDate     DATE              NOT NULL,
        WeekEndDate       DATE              NOT NULL,
        IsActive          BIT               NOT NULL CONSTRAINT DF_WRP_IsActive DEFAULT (1),
        CreatedAt         DATETIME          NOT NULL CONSTRAINT DF_WRP_CreatedAt DEFAULT (GETDATE()),
        UpdatedAt         DATETIME          NULL
    );

    CREATE INDEX IX_WeeklyRoutePlans_DealerId ON dbo.WeeklyRoutePlans (DealerId);
    CREATE INDEX IX_WeeklyRoutePlans_Week     ON dbo.WeeklyRoutePlans (WeekStartDate, WeekEndDate);
END


IF OBJECT_ID('dbo.WeeklyRoutePlanVillages', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WeeklyRoutePlanVillages
    (
        WeeklyRoutePlanVillageId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WeeklyRoutePlanVillages PRIMARY KEY,
        WeeklyRoutePlanId        INT               NOT NULL,
        VillageId                INT               NOT NULL,
        VisitSequence            INT               NOT NULL CONSTRAINT DF_WRPV_VisitSequence DEFAULT (1),
        CreatedAt                DATETIME          NOT NULL CONSTRAINT DF_WRPV_CreatedAt DEFAULT (GETDATE())
    );

    CREATE INDEX IX_WeeklyRoutePlanVillages_Plan ON dbo.WeeklyRoutePlanVillages (WeeklyRoutePlanId);
END


GO

/* ============================================================================
   SECTION 2 - ADD MISSING COLUMNS TO EXISTING TABLES
   Multi-tenant rule: every dealer-owned row must carry a DealerId so that a
   Dealer can never read another dealer's records.

   NOTE: this section must be its own batch (GO above). SQL Server resolves
   column names when it compiles a batch, so a column added by ALTER TABLE is
   not visible to statements later in the *same* batch.
   ============================================================================ */

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Users', 'DealerId') IS NULL
    ALTER TABLE dbo.Users ADD DealerId INT NULL;

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Users', 'UpdatedAt') IS NULL
    ALTER TABLE dbo.Users ADD UpdatedAt DATETIME NULL;

IF OBJECT_ID('dbo.Vehicles', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Vehicles', 'DealerId') IS NULL
    ALTER TABLE dbo.Vehicles ADD DealerId INT NULL;

IF OBJECT_ID('dbo.RouteVillageSchedules', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.RouteVillageSchedules', 'UpdatedBy') IS NULL
    ALTER TABLE dbo.RouteVillageSchedules ADD UpdatedBy INT NULL;

IF OBJECT_ID('dbo.RouteVillageSchedules', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.RouteVillageSchedules', 'UpdatedAt') IS NULL
    ALTER TABLE dbo.RouteVillageSchedules ADD UpdatedAt DATETIME NULL;

IF OBJECT_ID('dbo.RouteVillages', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.RouteVillages', 'VisitSequence') IS NULL
    ALTER TABLE dbo.RouteVillages ADD VisitSequence INT NOT NULL
        CONSTRAINT DF_RouteVillages_VisitSequence DEFAULT (1);

/* --- Routes: full field set required by the route spec ---------- */

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Routes', 'RouteType') IS NULL
    ALTER TABLE dbo.Routes ADD RouteType NVARCHAR(100) NULL;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Routes', 'OrderDispatchDays') IS NULL
    ALTER TABLE dbo.Routes ADD OrderDispatchDays NVARCHAR(400) NULL;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Routes', 'PreferredSalesmanId') IS NULL
    ALTER TABLE dbo.Routes ADD PreferredSalesmanId INT NULL;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Routes', 'PreferredDriverId') IS NULL
    ALTER TABLE dbo.Routes ADD PreferredDriverId INT NULL;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Routes', 'DefaultVehicleId') IS NULL
    ALTER TABLE dbo.Routes ADD DefaultVehicleId INT NULL;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Routes', 'CreatedBy') IS NULL
    ALTER TABLE dbo.Routes ADD CreatedBy INT NULL;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.Routes', 'UpdatedBy') IS NULL
    ALTER TABLE dbo.Routes ADD UpdatedBy INT NULL;

/* Route Number must be unique inside a dealer (nullable, so filtered). */
IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = 'UQ_Routes_Dealer_Code'
                     AND object_id = OBJECT_ID('dbo.Routes'))
    CREATE UNIQUE INDEX UQ_Routes_Dealer_Code
        ON dbo.Routes (DealerId, RouteCode)
        WHERE RouteCode IS NOT NULL;

GO

/* ----------------------------------------------------------------------------
   Back-fill DealerId on a single-dealer installation.
   A Dealer user owns their own record (DealerId = UserId).
   Salesmen and Drivers belong to whichever dealer exists.

   Separate batch: the columns added above only become visible to the
   compiler once the ALTER TABLE batch has completed.
   ---------------------------------------------------------------------------- */

DECLARE @DealerCount INT =
    (SELECT COUNT(*) FROM dbo.Users WHERE Role = 'Dealer');

IF @DealerCount = 1
BEGIN
    DECLARE @DealerId INT =
        (SELECT MIN(UserId) FROM dbo.Users WHERE Role = 'Dealer');

    UPDATE dbo.Users
       SET DealerId = @DealerId
     WHERE Role IN ('Salesman', 'Driver')
       AND DealerId IS NULL;

    IF OBJECT_ID('dbo.Vehicles', 'U') IS NOT NULL
        UPDATE dbo.Vehicles
           SET DealerId = @DealerId
         WHERE DealerId IS NULL;
END

UPDATE dbo.Users
   SET DealerId = UserId
 WHERE Role = 'Dealer'
   AND (DealerId IS NULL OR DealerId <> UserId);

GO

/* ============================================================================
   SECTION 3 - PHASE 4 : PRODUCT CATALOG AND PRICING
   ============================================================================ */

IF OBJECT_ID('dbo.ProductCategories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductCategories
    (
        CategoryId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductCategories PRIMARY KEY,
        CategoryName NVARCHAR(150)     NOT NULL,
        Description  NVARCHAR(500)     NULL,
        SortOrder    INT               NOT NULL CONSTRAINT DF_PC_SortOrder DEFAULT (0),
        IsActive     BIT               NOT NULL CONSTRAINT DF_PC_IsActive DEFAULT (1),
        CreatedBy    INT               NULL,
        CreatedAt    DATETIME          NOT NULL CONSTRAINT DF_PC_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy    INT               NULL,
        UpdatedAt    DATETIME          NULL
    );

    CREATE UNIQUE INDEX UQ_ProductCategories_Name
        ON dbo.ProductCategories (CategoryName);
END


IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        ProductId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        CategoryId  INT               NOT NULL,
        ProductCode NVARCHAR(100)     NULL,
        ProductName NVARCHAR(250)     NOT NULL,
        Description NVARCHAR(500)     NULL,
        IsActive    BIT               NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT (1),
        CreatedBy   INT               NULL,
        CreatedAt   DATETIME          NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy   INT               NULL,
        UpdatedAt   DATETIME          NULL
    );

    CREATE INDEX IX_Products_CategoryId ON dbo.Products (CategoryId);
END


IF OBJECT_ID('dbo.ProductVariants', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductVariants
    (
        ProductVariantId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductVariants PRIMARY KEY,
        ProductId        INT               NOT NULL,
        VariantCode      NVARCHAR(100)     NULL,
        VariantName      NVARCHAR(250)     NOT NULL,
        PacketWeight     DECIMAL(18,3)     NULL,
        WeightUnit       NVARCHAR(20)      NULL,   -- g / kg / ml / l
        SortOrder        INT               NOT NULL CONSTRAINT DF_PV_SortOrder DEFAULT (0),
        IsActive         BIT               NOT NULL CONSTRAINT DF_PV_IsActive DEFAULT (1),
        CreatedBy        INT               NULL,
        CreatedAt        DATETIME          NOT NULL CONSTRAINT DF_PV_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy        INT               NULL,
        UpdatedAt        DATETIME          NULL
    );

    CREATE INDEX IX_ProductVariants_ProductId ON dbo.ProductVariants (ProductId);
END


IF OBJECT_ID('dbo.SellingUnits', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SellingUnits
    (
        UnitId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SellingUnits PRIMARY KEY,
        UnitCode NVARCHAR(20)      NOT NULL,
        UnitName NVARCHAR(100)     NOT NULL,
        SortOrder INT              NOT NULL CONSTRAINT DF_SU_SortOrder DEFAULT (0),
        IsActive BIT               NOT NULL CONSTRAINT DF_SU_IsActive DEFAULT (1),
        CreatedAt DATETIME         NOT NULL CONSTRAINT DF_SU_CreatedAt DEFAULT (GETDATE())
    );

    CREATE UNIQUE INDEX UQ_SellingUnits_Code ON dbo.SellingUnits (UnitCode);
END


IF OBJECT_ID('dbo.ProductUnitOptions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductUnitOptions
    (
        ProductOptionId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductUnitOptions PRIMARY KEY,
        ProductVariantId INT               NOT NULL,
        UnitId           INT               NOT NULL,
        PacketsPerUnit   INT               NOT NULL CONSTRAINT DF_POU_Packets DEFAULT (1),
        IsActive         BIT               NOT NULL CONSTRAINT DF_POU_IsActive DEFAULT (1),
        CreatedBy        INT               NULL,
        CreatedAt        DATETIME          NOT NULL CONSTRAINT DF_POU_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy        INT               NULL,
        UpdatedAt        DATETIME          NULL
    );

    CREATE INDEX IX_POU_VariantId ON dbo.ProductUnitOptions (ProductVariantId);

    CREATE UNIQUE INDEX UQ_POU_Variant_Unit
        ON dbo.ProductUnitOptions (ProductVariantId, UnitId);
END


IF OBJECT_ID('dbo.ProductPrices', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProductPrices
    (
        PriceId              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductPrices PRIMARY KEY,
        ProductOptionId      INT               NOT NULL,
        PrintedPrice         DECIMAL(18,2)     NOT NULL,  -- MRP printed on pack
        DealerPurchasePrice  DECIMAL(18,2)     NOT NULL,  -- what the dealer pays
        ShopSellingPrice     DECIMAL(18,2)     NOT NULL,  -- what the shop pays
        EffectiveFrom        DATE              NOT NULL CONSTRAINT DF_PP_EffectiveFrom DEFAULT (GETDATE()),
        IsActive             BIT               NOT NULL CONSTRAINT DF_PP_IsActive DEFAULT (1),
        CreatedBy            INT               NULL,
        CreatedAt            DATETIME          NOT NULL CONSTRAINT DF_PP_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy            INT               NULL,
        UpdatedAt            DATETIME          NULL
    );

    CREATE UNIQUE INDEX UQ_ProductPrices_Option ON dbo.ProductPrices (ProductOptionId);
END


/* ============================================================================
   SECTION 4 - PHASE 6 : SHOPS
   ============================================================================ */

IF OBJECT_ID('dbo.Shops', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Shops
    (
        ShopId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Shops PRIMARY KEY,
        DealerId       INT               NOT NULL,
        ShopCode       NVARCHAR(100)     NULL,
        ShopName       NVARCHAR(250)     NOT NULL,
        OwnerName      NVARCHAR(200)     NULL,
        Phone          NVARCHAR(40)      NULL,
        Email          NVARCHAR(300)     NULL,
        Address        NVARCHAR(500)     NULL,
        VillageId      INT               NOT NULL,
        RouteId        INT               NULL,
        RouteScheduleId INT              NULL,
        OpeningBalance DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Shops_OpeningBalance DEFAULT (0),
        CreditLimit    DECIMAL(18,2)     NULL,
        IsActive       BIT               NOT NULL CONSTRAINT DF_Shops_IsActive DEFAULT (1),
        CreatedBy      INT               NULL,
        CreatedAt      DATETIME          NOT NULL CONSTRAINT DF_Shops_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy      INT               NULL,
        UpdatedAt      DATETIME          NULL
    );

    CREATE INDEX IX_Shops_DealerId  ON dbo.Shops (DealerId);
    CREATE INDEX IX_Shops_VillageId ON dbo.Shops (VillageId);
    CREATE INDEX IX_Shops_RouteId   ON dbo.Shops (RouteId);
END


/* ============================================================================
   SECTION 5 - PHASE 5 : INVENTORY AND STOCK
   Stock rule used throughout BWDMS
   ---------------------------------
   Warehouse inventory is always held in BASE PACKETS.
   Entry screens may accept a Box / Patti quantity; the application converts
   it to packets using ProductUnitOptions.PacketsPerUnit before saving.

   Warehouse movement
       Opening / Purchase ............. IN   (+)
       Vehicle Load ................... OUT  (-)
       Vehicle Return ................ IN   (+)
       Damaged / Expired .............. OUT  (-)
       Adjustment ..................... +/- 

   A sale does NOT touch warehouse stock: the goods already left the
   warehouse when they were loaded onto the vehicle.
   ============================================================================ */

IF OBJECT_ID('dbo.Inventory', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inventory
    (
        InventoryId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Inventory PRIMARY KEY,
        DealerId         INT               NOT NULL,
        ProductVariantId INT               NOT NULL,
        QuantityPackets  INT               NOT NULL CONSTRAINT DF_Inv_Qty DEFAULT (0),
        ReorderLevel     INT               NULL,
        UpdatedBy        INT               NULL,
        UpdatedAt        DATETIME          NOT NULL CONSTRAINT DF_Inv_UpdatedAt DEFAULT (GETDATE())
    );

    CREATE UNIQUE INDEX UQ_Inventory_Dealer_Variant
        ON dbo.Inventory (DealerId, ProductVariantId);
END


IF OBJECT_ID('dbo.StockTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockTransactions
    (
        StockTransactionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockTransactions PRIMARY KEY,
        DealerId           INT               NOT NULL,
        TransactionType    NVARCHAR(40)      NOT NULL,
            /* Opening, Purchase, VehicleLoad, VehicleReturn,
               Damaged, Expired, ReturnToCompany, Adjustment */
        TransactionDate    DATETIME          NOT NULL CONSTRAINT DF_ST_Date DEFAULT (GETDATE()),
        ReferenceNo        NVARCHAR(100)     NULL,
        Remarks            NVARCHAR(500)     NULL,
        CreatedBy          INT               NULL,
        CreatedAt          DATETIME          NOT NULL CONSTRAINT DF_ST_CreatedAt DEFAULT (GETDATE())
    );

    CREATE INDEX IX_StockTransactions_Dealer ON dbo.StockTransactions (DealerId, TransactionDate);
END


IF OBJECT_ID('dbo.StockTransactionDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockTransactionDetails
    (
        StockTransactionDetailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockTransactionDetails PRIMARY KEY,
        StockTransactionId       INT               NOT NULL,
        ProductVariantId         INT               NOT NULL,
        UnitId                   INT               NOT NULL,
        Quantity                 INT               NOT NULL,          -- as entered
        QuantityPackets          INT               NOT NULL,          -- normalised
        UnitCostPrice            DECIMAL(18,2)     NULL
    );

    CREATE INDEX IX_STD_TransactionId ON dbo.StockTransactionDetails (StockTransactionId);
END


IF OBJECT_ID('dbo.VehicleStock', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleStock
    (
        VehicleStockId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VehicleStock PRIMARY KEY,
        DealerId               INT               NOT NULL,
        VehicleId              INT               NOT NULL,
        StockDate              DATE              NOT NULL CONSTRAINT DF_VS_Date DEFAULT (GETDATE()),
        ProductVariantId       INT               NOT NULL,
        LoadedPackets          INT               NOT NULL CONSTRAINT DF_VS_Loaded DEFAULT (0),
        SoldPackets            INT               NOT NULL CONSTRAINT DF_VS_Sold DEFAULT (0),
        ReturnedPackets        INT               NOT NULL CONSTRAINT DF_VS_Returned DEFAULT (0),
        DamagedPackets         INT               NOT NULL CONSTRAINT DF_VS_Damaged DEFAULT (0),
        ActualClosingPackets   INT               NULL,   -- counted at reconciliation
        SalesmanId             INT               NULL,
        DriverId               INT               NULL,
        IsReconciled           BIT               NOT NULL CONSTRAINT DF_VS_Reconciled DEFAULT (0),
        CreatedBy              INT               NULL,
        CreatedAt              DATETIME          NOT NULL CONSTRAINT DF_VS_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy              INT               NULL,
        UpdatedAt              DATETIME          NULL
    );

    CREATE UNIQUE INDEX UQ_VehicleStock_Day
        ON dbo.VehicleStock (DealerId, VehicleId, StockDate, ProductVariantId);

    CREATE INDEX IX_VehicleStock_Vehicle ON dbo.VehicleStock (VehicleId, StockDate);
END


/* ============================================================================
   SECTION 6 - PHASE 7 : ORDERS
   ============================================================================ */

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        OrderId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        DealerId         INT               NOT NULL,
        OrderNumber      NVARCHAR(50)      NOT NULL,
        ShopId           INT               NULL,
        OrderType        NVARCHAR(30)      NOT NULL CONSTRAINT DF_Orders_Type DEFAULT ('Beat'),
            /* Beat, Telephone, Counter */
        OrderDate        DATE              NOT NULL CONSTRAINT DF_Orders_Date DEFAULT (GETDATE()),
        DeliveryDate     DATE              NULL,
        RouteScheduleId  INT               NULL,
        SalesmanId       INT               NULL,
        Status           NVARCHAR(30)      NOT NULL CONSTRAINT DF_Orders_Status DEFAULT ('Pending'),
            /* Pending, Approved, Prepared, Dispatched, Delivered,
               Cancelled, Returned */
        SubTotal         DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Orders_SubTotal DEFAULT (0),
        Discount         DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Orders_Discount DEFAULT (0),
        GrandTotal       DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Orders_GrandTotal DEFAULT (0),
        Remarks          NVARCHAR(500)     NULL,
        ClientOrderId    NVARCHAR(64)      NULL,   -- offline capture, used for sync
        IsSynced         BIT               NOT NULL CONSTRAINT DF_Orders_Synced DEFAULT (1),
        CreatedBy        INT               NULL,
        CreatedAt        DATETIME          NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy        INT               NULL,
        UpdatedAt        DATETIME          NULL
    );

    CREATE INDEX IX_Orders_Dealer  ON dbo.Orders (DealerId, OrderDate);
    CREATE INDEX IX_Orders_Shop    ON dbo.Orders (ShopId);
    CREATE INDEX IX_Orders_Status  ON dbo.Orders (Status);
    CREATE UNIQUE INDEX UQ_Orders_Dealer_Number ON dbo.Orders (DealerId, OrderNumber);
END


IF OBJECT_ID('dbo.OrderDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderDetails
    (
        OrderDetailId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderDetails PRIMARY KEY,
        OrderId          INT               NOT NULL,
        ProductVariantId INT               NOT NULL,
        UnitId           INT               NOT NULL,
        Quantity         INT               NOT NULL,
        QuantityPackets  INT               NOT NULL CONSTRAINT DF_OD_Packets DEFAULT (0),
        UnitPrice        DECIMAL(18,2)     NOT NULL CONSTRAINT DF_OD_Price DEFAULT (0),
        LineTotal        DECIMAL(18,2)     NOT NULL CONSTRAINT DF_OD_Total DEFAULT (0)
    );

    CREATE INDEX IX_OrderDetails_OrderId ON dbo.OrderDetails (OrderId);
END


/* ============================================================================
   SECTION 7 - PHASE 8 : DISPATCH, RETURNS, RECONCILIATION, EXPENSES
   ============================================================================ */

IF OBJECT_ID('dbo.Dispatches', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Dispatches
    (
        DispatchId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Dispatches PRIMARY KEY,
        DealerId         INT               NOT NULL,
        DispatchNumber   NVARCHAR(50)      NOT NULL,
        DispatchDate     DATE              NOT NULL CONSTRAINT DF_Disp_Date DEFAULT (GETDATE()),
        VehicleId        INT               NULL,
        SalesmanId       INT               NULL,
        DriverId         INT               NULL,
        RouteScheduleId  INT               NULL,
        Status           NVARCHAR(30)      NOT NULL CONSTRAINT DF_Disp_Status DEFAULT ('Loading'),
            /* Loading, Dispatched, Completed, Cancelled */
        Remarks          NVARCHAR(500)     NULL,
        CreatedBy        INT               NULL,
        CreatedAt        DATETIME          NOT NULL CONSTRAINT DF_Disp_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy        INT               NULL,
        UpdatedAt        DATETIME          NULL
    );

    CREATE INDEX IX_Dispatches_Dealer ON dbo.Dispatches (DealerId, DispatchDate);
    CREATE UNIQUE INDEX UQ_Dispatches_Number ON dbo.Dispatches (DealerId, DispatchNumber);
END


IF OBJECT_ID('dbo.DispatchDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DispatchDetails
    (
        DispatchDetailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DispatchDetails PRIMARY KEY,
        DispatchId       INT               NOT NULL,
        OrderId          INT               NULL,
        ProductVariantId INT               NOT NULL,
        UnitId           INT               NOT NULL,
        Quantity         INT               NOT NULL,
        QuantityPackets  INT               NOT NULL CONSTRAINT DF_DD_Packets DEFAULT (0)
    );

    CREATE INDEX IX_DispatchDetails_DispatchId ON dbo.DispatchDetails (DispatchId);
    CREATE INDEX IX_DispatchDetails_OrderId    ON dbo.DispatchDetails (OrderId);
END


IF OBJECT_ID('dbo.SalesReturns', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesReturns
    (
        SalesReturnId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesReturns PRIMARY KEY,
        DealerId      INT               NOT NULL,
        ReturnNumber  NVARCHAR(50)      NOT NULL,
        OrderId       INT               NULL,
        ShopId        INT               NULL,
        ReturnDate    DATE              NOT NULL CONSTRAINT DF_SR_Date DEFAULT (GETDATE()),
        Reason        NVARCHAR(500)     NULL,
        Status        NVARCHAR(30)      NOT NULL CONSTRAINT DF_SR_Status DEFAULT ('Pending'),
            /* Pending, Approved, Rejected, Received */
        CreatedBy     INT               NULL,
        CreatedAt     DATETIME          NOT NULL CONSTRAINT DF_SR_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy     INT               NULL,
        UpdatedAt     DATETIME          NULL
    );

    CREATE INDEX IX_SalesReturns_Dealer ON dbo.SalesReturns (DealerId, ReturnDate);
    CREATE UNIQUE INDEX UQ_SalesReturns_Number ON dbo.SalesReturns (DealerId, ReturnNumber);
END


IF OBJECT_ID('dbo.SalesReturnDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesReturnDetails
    (
        SalesReturnDetailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesReturnDetails PRIMARY KEY,
        SalesReturnId       INT               NOT NULL,
        ProductVariantId    INT               NOT NULL,
        UnitId              INT               NOT NULL,
        Quantity            INT               NOT NULL,
        QuantityPackets     INT               NOT NULL CONSTRAINT DF_SRD_Packets DEFAULT (0),
        Condition           NVARCHAR(30)      NOT NULL CONSTRAINT DF_SRD_Condition DEFAULT ('Good')
            /* Good, Damaged, Expired */
    );

    CREATE INDEX IX_SalesReturnDetails_ReturnId ON dbo.SalesReturnDetails (SalesReturnId);
END


IF OBJECT_ID('dbo.VehicleReconciliations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleReconciliations
    (
        VehicleReconciliationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VehicleReconciliations PRIMARY KEY,
        DealerId                INT               NOT NULL,
        VehicleId               INT               NOT NULL,
        ReconciliationDate      DATE              NOT NULL CONSTRAINT DF_VR_Date DEFAULT (GETDATE()),
        SalesmanId              INT               NULL,
        DriverId                INT               NULL,
        Remarks                 NVARCHAR(500)     NULL,
        IsApproved              BIT               NOT NULL CONSTRAINT DF_VR_Approved DEFAULT (0),
        ApprovedBy              INT               NULL,
        ApprovedAt              DATETIME          NULL,
        CreatedBy               INT               NULL,
        CreatedAt               DATETIME          NOT NULL CONSTRAINT DF_VR_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy               INT               NULL,
        UpdatedAt               DATETIME          NULL
    );

    CREATE UNIQUE INDEX UQ_VehicleReconciliations_Day
        ON dbo.VehicleReconciliations (DealerId, VehicleId, ReconciliationDate);
END


IF OBJECT_ID('dbo.VehicleReconciliationDetails', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleReconciliationDetails
    (
        VehicleReconciliationDetailId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_VRD PRIMARY KEY,
        VehicleReconciliationId       INT               NOT NULL,
        ProductVariantId              INT               NOT NULL,
        ExpectedPackets               INT               NOT NULL CONSTRAINT DF_VRD_Expected DEFAULT (0),
        ActualPackets                 INT               NOT NULL CONSTRAINT DF_VRD_Actual DEFAULT (0),
        VariancePackets               INT               NOT NULL CONSTRAINT DF_VRD_Variance DEFAULT (0),
        Remarks                       NVARCHAR(300)     NULL
    );

    CREATE INDEX IX_VRD_ReconciliationId
        ON dbo.VehicleReconciliationDetails (VehicleReconciliationId);
END


IF OBJECT_ID('dbo.Expenses', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expenses
    (
        ExpenseId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Expenses PRIMARY KEY,
        DealerId        INT               NOT NULL,
        ExpenseDate     DATE              NOT NULL CONSTRAINT DF_Exp_Date DEFAULT (GETDATE()),
        ExpenseHead     NVARCHAR(150)     NOT NULL,   -- Fuel, Loading, Salary, Rent ...
        Description     NVARCHAR(500)     NULL,
        Amount          DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Exp_Amount DEFAULT (0),
        VehicleId       INT               NULL,
        RouteScheduleId INT               NULL,
        IsActive        BIT               NOT NULL CONSTRAINT DF_Exp_IsActive DEFAULT (1),
        CreatedBy       INT               NULL,
        CreatedAt       DATETIME          NOT NULL CONSTRAINT DF_Exp_CreatedAt DEFAULT (GETDATE()),
        UpdatedBy       INT               NULL,
        UpdatedAt       DATETIME          NULL
    );

    CREATE INDEX IX_Expenses_Dealer ON dbo.Expenses (DealerId, ExpenseDate);
END


/* ============================================================================
   SECTION 8 - PHASE 10 : AUDIT
   ============================================================================ */

IF OBJECT_ID('dbo.AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        AuditLogId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
        UserId     INT               NULL,
        Role       NVARCHAR(40)      NULL,
        Action     NVARCHAR(50)      NOT NULL,
        TableName  NVARCHAR(120)     NULL,
        RecordId   INT               NULL,
        Details    NVARCHAR(1000)    NULL,
        CreatedAt  DATETIME          NOT NULL CONSTRAINT DF_AL_CreatedAt DEFAULT (GETDATE())
    );

    CREATE INDEX IX_AuditLog_CreatedAt ON dbo.AuditLog (CreatedAt);
END


GO

/* ============================================================================
   SECTION 9 - FOREIGN KEYS (created only when absent)

   A GO before this section is required: an ALTER TABLE ... ADD CONSTRAINT
   cannot see a table created or altered in the same batch.
   ============================================================================ */

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Routes')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.Routes WITH CHECK
        ADD CONSTRAINT FK_Routes_Users_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.RouteSchedules')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Routes')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'RouteId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'RouteId')
    ALTER TABLE dbo.RouteSchedules WITH CHECK
        ADD CONSTRAINT FK_RouteSchedules_Routes
            FOREIGN KEY (RouteId) REFERENCES dbo.Routes (RouteId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.RouteSchedules')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Vehicles')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'VehicleId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'VehicleId')
    ALTER TABLE dbo.RouteSchedules WITH CHECK
        ADD CONSTRAINT FK_RouteSchedules_Vehicles
            FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicles (VehicleId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.RouteVillages')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Routes')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'RouteId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'RouteId')
    ALTER TABLE dbo.RouteVillages WITH CHECK
        ADD CONSTRAINT FK_RouteVillages_Routes
            FOREIGN KEY (RouteId) REFERENCES dbo.Routes (RouteId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.RouteVillages')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Villages')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'VillageId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'VillageId')
    ALTER TABLE dbo.RouteVillages WITH CHECK
        ADD CONSTRAINT FK_RouteVillages_Villages
            FOREIGN KEY (VillageId) REFERENCES dbo.Villages (VillageId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.RouteVillageSchedules')
        AND fk.referenced_object_id = OBJECT_ID('dbo.RouteSchedules')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'RouteScheduleId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'RouteScheduleId')
    ALTER TABLE dbo.RouteVillageSchedules WITH CHECK
        ADD CONSTRAINT FK_RVS_RouteSchedules
            FOREIGN KEY (RouteScheduleId) REFERENCES dbo.RouteSchedules (RouteScheduleId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.RouteVillageSchedules')
        AND fk.referenced_object_id = OBJECT_ID('dbo.RouteVillages')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'RouteVillageId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'RouteVillageId')
    ALTER TABLE dbo.RouteVillageSchedules WITH CHECK
        ADD CONSTRAINT FK_RVS_RouteVillages
            FOREIGN KEY (RouteVillageId) REFERENCES dbo.RouteVillages (RouteVillageId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Products')
        AND fk.referenced_object_id = OBJECT_ID('dbo.ProductCategories')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'CategoryId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'CategoryId')
    ALTER TABLE dbo.Products WITH CHECK
        ADD CONSTRAINT FK_Products_Category
            FOREIGN KEY (CategoryId) REFERENCES dbo.ProductCategories (CategoryId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.ProductVariants')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Products')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'ProductId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'ProductId')
    ALTER TABLE dbo.ProductVariants WITH CHECK
        ADD CONSTRAINT FK_ProductVariants_Product
            FOREIGN KEY (ProductId) REFERENCES dbo.Products (ProductId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.ProductUnitOptions')
        AND fk.referenced_object_id = OBJECT_ID('dbo.ProductVariants')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'ProductVariantId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'ProductVariantId')
    ALTER TABLE dbo.ProductUnitOptions WITH CHECK
        ADD CONSTRAINT FK_POU_Variant
            FOREIGN KEY (ProductVariantId) REFERENCES dbo.ProductVariants (ProductVariantId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.ProductUnitOptions')
        AND fk.referenced_object_id = OBJECT_ID('dbo.SellingUnits')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'UnitId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UnitId')
    ALTER TABLE dbo.ProductUnitOptions WITH CHECK
        ADD CONSTRAINT FK_POU_Unit
            FOREIGN KEY (UnitId) REFERENCES dbo.SellingUnits (UnitId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.ProductPrices')
        AND fk.referenced_object_id = OBJECT_ID('dbo.ProductUnitOptions')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'ProductOptionId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'ProductOptionId')
    ALTER TABLE dbo.ProductPrices WITH CHECK
        ADD CONSTRAINT FK_ProductPrices_Option
            FOREIGN KEY (ProductOptionId) REFERENCES dbo.ProductUnitOptions (ProductOptionId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Shops')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.Shops WITH CHECK
        ADD CONSTRAINT FK_Shops_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Shops')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Villages')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'VillageId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'VillageId')
    ALTER TABLE dbo.Shops WITH CHECK
        ADD CONSTRAINT FK_Shops_Village
            FOREIGN KEY (VillageId) REFERENCES dbo.Villages (VillageId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Inventory')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.Inventory WITH CHECK
        ADD CONSTRAINT FK_Inventory_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Inventory')
        AND fk.referenced_object_id = OBJECT_ID('dbo.ProductVariants')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'ProductVariantId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'ProductVariantId')
    ALTER TABLE dbo.Inventory WITH CHECK
        ADD CONSTRAINT FK_Inventory_Variant
            FOREIGN KEY (ProductVariantId) REFERENCES dbo.ProductVariants (ProductVariantId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.StockTransactions')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.StockTransactions WITH CHECK
        ADD CONSTRAINT FK_ST_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.StockTransactionDetails')
        AND fk.referenced_object_id = OBJECT_ID('dbo.StockTransactions')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'StockTransactionId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'StockTransactionId')
    ALTER TABLE dbo.StockTransactionDetails WITH CHECK
        ADD CONSTRAINT FK_STD_Transaction
            FOREIGN KEY (StockTransactionId)
            REFERENCES dbo.StockTransactions (StockTransactionId)
            ON DELETE CASCADE;

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.VehicleStock')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.VehicleStock WITH CHECK
        ADD CONSTRAINT FK_VS_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.VehicleStock')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Vehicles')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'VehicleId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'VehicleId')
    ALTER TABLE dbo.VehicleStock WITH CHECK
        ADD CONSTRAINT FK_VS_Vehicle
            FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicles (VehicleId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Orders')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.Orders WITH CHECK
        ADD CONSTRAINT FK_Orders_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Orders')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Shops')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'ShopId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'ShopId')
    ALTER TABLE dbo.Orders WITH CHECK
        ADD CONSTRAINT FK_Orders_Shop
            FOREIGN KEY (ShopId) REFERENCES dbo.Shops (ShopId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.OrderDetails')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Orders')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'OrderId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'OrderId')
    ALTER TABLE dbo.OrderDetails WITH CHECK
        ADD CONSTRAINT FK_OrderDetails_Order
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders (OrderId)
            ON DELETE CASCADE;

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Dispatches')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.Dispatches WITH CHECK
        ADD CONSTRAINT FK_Dispatches_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.SalesReturns')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.SalesReturns WITH CHECK
        ADD CONSTRAINT FK_SalesReturns_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.SalesReturns')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Orders')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'OrderId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'OrderId')
    ALTER TABLE dbo.SalesReturns WITH CHECK
        ADD CONSTRAINT FK_SalesReturns_Order
            FOREIGN KEY (OrderId) REFERENCES dbo.Orders (OrderId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.SalesReturns')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Shops')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'ShopId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'ShopId')
    ALTER TABLE dbo.SalesReturns WITH CHECK
        ADD CONSTRAINT FK_SalesReturns_Shop
            FOREIGN KEY (ShopId) REFERENCES dbo.Shops (ShopId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.SalesReturnDetails')
        AND fk.referenced_object_id = OBJECT_ID('dbo.SalesReturns')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'SalesReturnId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'SalesReturnId')
    ALTER TABLE dbo.SalesReturnDetails WITH CHECK
        ADD CONSTRAINT FK_SRD_Return
            FOREIGN KEY (SalesReturnId)
            REFERENCES dbo.SalesReturns (SalesReturnId)
            ON DELETE CASCADE;

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.VehicleReconciliations')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.VehicleReconciliations WITH CHECK
        ADD CONSTRAINT FK_VR_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.VehicleReconciliations')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Vehicles')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'VehicleId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'VehicleId')
    ALTER TABLE dbo.VehicleReconciliations WITH CHECK
        ADD CONSTRAINT FK_VR_Vehicle
            FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicles (VehicleId);

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.VehicleReconciliationDetails')
        AND fk.referenced_object_id = OBJECT_ID('dbo.VehicleReconciliations')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'VehicleReconciliationId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'VehicleReconciliationId')
    ALTER TABLE dbo.VehicleReconciliationDetails WITH CHECK
        ADD CONSTRAINT FK_VRD_Reconciliation
            FOREIGN KEY (VehicleReconciliationId)
            REFERENCES dbo.VehicleReconciliations (VehicleReconciliationId)
            ON DELETE CASCADE;

IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc
            ON fk.object_id = fkc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID('dbo.Expenses')
        AND fk.referenced_object_id = OBJECT_ID('dbo.Users')
        AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = 'DealerId'
        AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'UserId')
    ALTER TABLE dbo.Expenses WITH CHECK
        ADD CONSTRAINT FK_Expenses_Dealer
            FOREIGN KEY (DealerId) REFERENCES dbo.Users (UserId);


/* ============================================================================
   SECTION 9b - CHECK CONSTRAINTS (created only when absent)
   ============================================================================
   These two constraints already existed in the live database but were
   missing from this script, so a rebuilt database would have silently
   lost them. Definitions below are copied verbatim from sys.check_constraints.

   CK_Users_Role          -> no Driver login role may ever be created.
   CK_RouteSchedules_Day  -> a schedule must name a real weekday.
   ============================================================================ */

IF NOT EXISTS (
        SELECT 1
        FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.Users')
        AND name = 'CK_Users_Role')
    ALTER TABLE dbo.Users WITH CHECK
        ADD CONSTRAINT CK_Users_Role
            CHECK
            (
                   [Role] = 'Salesman'
                OR [Role] = 'Dealer'
                OR [Role] = 'Admin'
            );


IF NOT EXISTS (
        SELECT 1
        FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.RouteSchedules')
        AND name = 'CK_RouteSchedules_DayOfWeek')
    ALTER TABLE dbo.RouteSchedules WITH CHECK
        ADD CONSTRAINT CK_RouteSchedules_DayOfWeek
            CHECK
            (
                   [DayOfWeek] = 'Monday'
                OR [DayOfWeek] = 'Tuesday'
                OR [DayOfWeek] = 'Wednesday'
                OR [DayOfWeek] = 'Thursday'
                OR [DayOfWeek] = 'Friday'
                OR [DayOfWeek] = 'Saturday'
                OR [DayOfWeek] = 'Sunday'
            );


/* ============================================================================
   SECTION 10 - SEED DATA
   ============================================================================ */

/* --- Selling units: Packet is the base unit (PacketsPerUnit = 1) --------- */

IF OBJECT_ID('dbo.SellingUnits', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.SellingUnits)
BEGIN
    INSERT INTO dbo.SellingUnits (UnitCode, UnitName, SortOrder, IsActive)
    VALUES ('PKT', 'Packet', 1, 1),
           ('PTI', 'Patti',  2, 1),
           ('BOX', 'Box',    3, 1);
END


/* --- The four categories named in the project specification -------------- */

IF OBJECT_ID('dbo.ProductCategories', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.ProductCategories)
BEGIN
    INSERT INTO dbo.ProductCategories (CategoryName, Description, SortOrder, IsActive)
    VALUES ('Wafers',  'Potato wafers and chips',                 1, 1),
           ('Namkeen', 'Namkeen, bhujia and mixture',             2, 1),
           ('Sev',     'Sev and gathiya products',                3, 1),
           ('Fryums',  'Fryums and extruded snack products',      4, 1);
END


/* ============================================================================
   SECTION 11 - REPORTING VIEWS  (Phase 9)
   ============================================================================ */

EXEC ('CREATE OR ALTER VIEW dbo.vw_CurrentStock AS
SELECT
    i.DealerId,
    i.ProductVariantId,
    p.ProductName,
    v.VariantName,
    v.PacketWeight,
    v.WeightUnit,
    c.CategoryName,
    i.QuantityPackets,
    i.ReorderLevel,
    i.UpdatedAt
FROM dbo.Inventory i
INNER JOIN dbo.ProductVariants v ON v.ProductVariantId = i.ProductVariantId
INNER JOIN dbo.Products p        ON p.ProductId        = v.ProductId
INNER JOIN dbo.ProductCategories c ON c.CategoryId     = p.CategoryId;');


EXEC ('CREATE OR ALTER VIEW dbo.vw_SalesByDate AS
SELECT
    o.DealerId,
    o.OrderDate,
    o.OrderType,
    o.Status,
    o.ShopId,
    s.ShopName,
    s.VillageId,
    v.VillageName,
    s.RouteId,
    r.RouteName,
    o.SalesmanId,
    u.FullName AS SalesmanName,
    od.ProductVariantId,
    od.UnitId,
    od.Quantity,
    od.QuantityPackets,
    od.LineTotal
FROM dbo.Orders o
LEFT JOIN dbo.Shops s       ON s.ShopId       = o.ShopId
LEFT JOIN dbo.Villages v    ON v.VillageId    = s.VillageId
LEFT JOIN dbo.Routes r      ON r.RouteId      = s.RouteId
LEFT JOIN dbo.Users u       ON u.UserId       = o.SalesmanId
INNER JOIN dbo.OrderDetails od ON od.OrderId  = o.OrderId;');


EXEC ('CREATE OR ALTER VIEW dbo.vw_VehicleStockStatus AS
SELECT
    vs.DealerId,
    vs.VehicleId,
    vh.VehicleNumber,
    vs.StockDate,
    vs.ProductVariantId,
    p.ProductName,
    v.VariantName,
    vs.LoadedPackets,
    vs.SoldPackets,
    vs.ReturnedPackets,
    vs.DamagedPackets,
    (vs.LoadedPackets - vs.SoldPackets - vs.ReturnedPackets - vs.DamagedPackets)
        AS ExpectedClosingPackets,
    vs.ActualClosingPackets,
    CASE
        WHEN vs.ActualClosingPackets IS NULL THEN NULL
        ELSE vs.ActualClosingPackets
             - (vs.LoadedPackets - vs.SoldPackets - vs.ReturnedPackets - vs.DamagedPackets)
    END AS VariancePackets,
    vs.IsReconciled
FROM dbo.VehicleStock vs
LEFT JOIN dbo.Vehicles vh       ON vh.VehicleId       = vs.VehicleId
INNER JOIN dbo.ProductVariants v ON v.ProductVariantId = vs.ProductVariantId
INNER JOIN dbo.Products p        ON p.ProductId        = v.ProductId;');


PRINT 'BWDMS schema script completed successfully.';
