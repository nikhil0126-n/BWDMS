/* ============================================================================
   BWDMS - Balaji Wafers Dealer Management System
   FILE   : Database\04_RouteDayAndPricing.sql
   PURPOSE: Close the last gaps:

              1. ROUTE-DAY CLOSURE
                 A route day is "complete" once its vehicle has been counted
                 and the count approved. Until then the day is open and
                 stock may still be loaded and bills still raised.

              2. OFFLINE SYNC IDEMPOTENCY
                 Orders.ClientOrderId already exists; it needs to be UNIQUE
                 so a retried upload can never create a second order.

              3. PRICE HISTORY
                 UQ_ProductPrices_Option allowed only ONE row per unit option,
                 which made EffectiveFrom meaningless. A catalogue option can
                 now carry several prices over time, and lookups take the row
                 in force on the order date.

   No "USE <database>" statement. Run through Deploy-Database.ps1.
   The script is IDEMPOTENT.
   ============================================================================ */

SET QUOTED_IDENTIFIER ON;
GO


/* ============================================================================
   SECTION 1 - ROUTE-DAY CLOSURE
   ============================================================================ */

IF COL_LENGTH('dbo.RouteSchedules', 'StockDate') IS NULL
BEGIN
    ALTER TABLE dbo.RouteSchedules
        ADD StockDate DATE NULL;
END
GO

IF COL_LENGTH('dbo.RouteSchedules', 'IsClosed') IS NULL
BEGIN
    ALTER TABLE dbo.RouteSchedules
        ADD IsClosed BIT NOT NULL
            CONSTRAINT DF_RouteSchedules_IsClosed DEFAULT (0);
END
GO

IF COL_LENGTH('dbo.RouteSchedules', 'ClosedAt') IS NULL
BEGIN
    ALTER TABLE dbo.RouteSchedules
        ADD ClosedAt DATETIME NULL;
END
GO

IF COL_LENGTH('dbo.RouteSchedules', 'ClosedBy') IS NULL
BEGIN
    ALTER TABLE dbo.RouteSchedules
        ADD ClosedBy INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM   sys.foreign_keys
    WHERE  name = 'FK_RouteSchedules_ClosedBy'
)
BEGIN
    ALTER TABLE dbo.RouteSchedules
        ADD CONSTRAINT FK_RouteSchedules_ClosedBy
            FOREIGN KEY (ClosedBy)
                REFERENCES dbo.Users (UserId);
END
GO

-- Backfill: a day that already has an APPROVED reconciliation is closed.
UPDATE rs
SET    rs.IsClosed = 1,
       rs.ClosedAt = ISNULL(rs.ClosedAt, vr.ApprovedAt),
       rs.ClosedBy = ISNULL(rs.ClosedBy, vr.ApprovedBy)
FROM   dbo.RouteSchedules rs
INNER JOIN dbo.VehicleReconciliations vr
        ON vr.RouteScheduleId = rs.RouteScheduleId
       AND vr.IsApproved = 1
WHERE  rs.IsClosed = 0;
GO


/* ============================================================================
   SECTION 2 - OFFLINE SYNC IDEMPOTENCY
   ----------------------------------------------------------------------------
   The salesman generates the operation id in the browser. The unique index is
   what makes a retry safe: the same upload twice inserts once.
   ============================================================================ */

IF NOT EXISTS
(
    SELECT 1
    FROM   sys.indexes
    WHERE  name = 'UQ_Orders_ClientOrderId'
      AND  object_id = OBJECT_ID('dbo.Orders')
)
BEGIN
    CREATE UNIQUE INDEX UQ_Orders_ClientOrderId
        ON dbo.Orders (ClientOrderId)
        WHERE ClientOrderId IS NOT NULL;
END
GO


/* ============================================================================
   SECTION 3 - PRICE HISTORY
   ----------------------------------------------------------------------------
   Before: one row per unit option, so EffectiveFrom was decorative.
   After : one row per (unit option, effective date), and lookups use the row
           in force on a given date.
   ============================================================================ */

IF EXISTS
(
    SELECT 1
    FROM   sys.indexes
    WHERE  name = 'UQ_ProductPrices_Option'
      AND  object_id = OBJECT_ID('dbo.ProductPrices')
)
BEGIN
    DROP INDEX UQ_ProductPrices_Option ON dbo.ProductPrices;
END
GO

-- Rows that were saved without a date get one so the unique key is usable.
UPDATE dbo.ProductPrices
SET    EffectiveFrom = CAST('1900-01-01' AS DATE)
WHERE  EffectiveFrom IS NULL;
GO

IF EXISTS
(
    SELECT 1
    FROM   sys.check_constraints
    WHERE  name = 'CK_ProductPrices_EffectiveFrom'
      AND  parent_object_id = OBJECT_ID('dbo.ProductPrices')
)
BEGIN
    ALTER TABLE dbo.ProductPrices
        DROP CONSTRAINT CK_ProductPrices_EffectiveFrom;
END
GO

ALTER TABLE dbo.ProductPrices
    ADD CONSTRAINT CK_ProductPrices_EffectiveFrom
        CHECK (EffectiveFrom IS NOT NULL);
GO

IF NOT EXISTS
(
    SELECT 1
    FROM   sys.indexes
    WHERE  name = 'UQ_ProductPrices_Option_From'
      AND  object_id = OBJECT_ID('dbo.ProductPrices')
)
BEGIN
    CREATE UNIQUE INDEX UQ_ProductPrices_Option_From
        ON dbo.ProductPrices (ProductOptionId, EffectiveFrom);
END
GO