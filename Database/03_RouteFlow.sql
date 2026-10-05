/* ============================================================================
   BWDMS - Balaji Wafers Dealer Management System
   FILE   : Database\03_RouteFlow.sql
   PURPOSE: Make the database express the real route-day flow:

              load stock for a ROUTE -> salesman sells from the VEHICLE ->
              end-of-day count -> verify against the orders -> route complete

            This script adds the three things the flow needs that the previous
            schema could not express:

              1. VehicleStock.RouteScheduleId  - whose load is this?
              2. a vehicle can only be on ONE route per weekday ("if free")
              3. a village may not be attached to the same route twice

   IMPORTANT
   ---------
   No "USE <database>" statement, exactly like the other scripts: the database
   is attached by file path, not by name. Run through Deploy-Database.ps1.

   The script is IDEMPOTENT.
   ============================================================================ */


/* Required by the filtered indexes further down: CREATE INDEX fails unless
   QUOTED_IDENTIFIER is ON. Set once here so every batch in this file gets it. */
SET QUOTED_IDENTIFIER ON;
GO


/* ============================================================================
   SECTION 1 - A VEHICLE LOAD BELONGS TO A ROUTE SCHEDULE
   ----------------------------------------------------------------------------
   Before this change VehicleStock was keyed on (DealerId, VehicleId, StockDate,
   ProductVariantId). That cannot answer "how much did THIS route load?", which
   is the first question of the day. Two routes running on the same day with
   different vehicles were fine, but the moment one vehicle covered two routes
   (or the same vehicle reloaded for a second schedule) the rows collided.

   RouteScheduleId is nullable so existing rows stay valid.
   ============================================================================ */

IF COL_LENGTH('dbo.VehicleStock', 'RouteScheduleId') IS NULL
BEGIN
    ALTER TABLE dbo.VehicleStock
        ADD RouteScheduleId INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM   sys.foreign_keys
    WHERE  name = 'FK_VS_RouteSchedule'
)
BEGIN
    ALTER TABLE dbo.VehicleStock
        ADD CONSTRAINT FK_VS_RouteSchedule
            FOREIGN KEY (RouteScheduleId)
                REFERENCES dbo.RouteSchedules (RouteScheduleId);
END
GO

-- Drop the old 4-column uniqueness and replace it with one that includes the
-- route, so the same vehicle can carry a separate load for each route it runs.
IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE  name = 'UQ_VehicleStock_Day'
      AND  object_id = OBJECT_ID('dbo.VehicleStock')
)
BEGIN
    DROP INDEX UQ_VehicleStock_Day ON dbo.VehicleStock;
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE  name = 'UQ_VehicleStock_Route_Day'
      AND  object_id = OBJECT_ID('dbo.VehicleStock')
)
BEGIN
    CREATE UNIQUE INDEX UQ_VehicleStock_Route_Day
        ON dbo.VehicleStock
           (DealerId, VehicleId, RouteScheduleId, StockDate, ProductVariantId);
END
GO

-- Backfill: attribute any existing load to the active schedule that uses this
-- vehicle on that weekday. Runs after StockDate is known.
UPDATE v
SET    v.RouteScheduleId = s.RouteScheduleId
FROM   dbo.VehicleStock v
INNER JOIN dbo.RouteSchedules s
        ON s.VehicleId = v.VehicleId
       AND s.DayOfWeek = DATENAME(WEEKDAY, v.StockDate)
WHERE  v.RouteScheduleId IS NULL
  AND  s.IsActive = 1
  AND  NOT EXISTS
       (
           SELECT 1
           FROM   dbo.VehicleStock x
           WHERE  x.VehicleId = v.VehicleId
             AND  x.StockDate = v.StockDate
             AND  x.ProductVariantId = v.ProductVariantId
             AND  x.VehicleStockId <> v.VehicleStockId
       );
GO

-- Per-route queries are now the common case.
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE  name = 'IX_VehicleStock_Route'
      AND  object_id = OBJECT_ID('dbo.VehicleStock')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_VehicleStock_Route
        ON dbo.VehicleStock (DealerId, RouteScheduleId, StockDate);
END
GO


/* ============================================================================
   SECTION 2 - A VEHICLE CAN ONLY BE ON ONE ROUTE PER DAY
   ----------------------------------------------------------------------------
   "One vehicle is assigned to one route IF FREE." A vehicle physically cannot
   run two routes on the same weekday, so the database must say so instead of
   leaving it to the operator to notice.

   A filtered UNIQUE index gives exactly that rule:
     - only ACTIVE schedules are considered,
     - only schedules that actually named a vehicle,
     - VehicleId is already dealer-scoped (Vehicles belongs to one dealer), so
       two dealers may each use GJ-03 on Monday without colliding.
   ============================================================================ */

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE  name = 'UQ_RouteSchedules_Vehicle_Weekday'
      AND  object_id = OBJECT_ID('dbo.RouteSchedules')
)
BEGIN
    CREATE UNIQUE INDEX UQ_RouteSchedules_Vehicle_Weekday
        ON dbo.RouteSchedules (VehicleId, DayOfWeek)
        WHERE VehicleId IS NOT NULL AND IsActive = 1;
END
GO


/* ============================================================================
   SECTION 3 - A VILLAGE MAY NOT BE ATTACHED TO THE SAME ROUTE TWICE
   ----------------------------------------------------------------------------
   A village is ALLOWED to sit on several different routes (that is a real
   business case), but repeating the same (route, village) pair is a data-entry
   mistake and would double the village in the route's list.
   ============================================================================ */

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE  name = 'UQ_RouteVillages_Route_Village'
      AND  object_id = OBJECT_ID('dbo.RouteVillages')
)
BEGIN
    CREATE UNIQUE INDEX UQ_RouteVillages_Route_Village
        ON dbo.RouteVillages (RouteId, VillageId);
END
GO


/* ============================================================================
   SECTION 4 - END-OF-DAY VERIFICATION IS TIED TO A ROUTE
   ----------------------------------------------------------------------------
   The count is meaningless without knowing which route it closes out, so the
   reconciliation header records the schedule and the day it belongs to.
   ============================================================================ */

IF COL_LENGTH('dbo.VehicleReconciliations', 'RouteScheduleId') IS NULL
BEGIN
    ALTER TABLE dbo.VehicleReconciliations
        ADD RouteScheduleId INT NULL;
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM   sys.foreign_keys
    WHERE  name = 'FK_VR_RouteSchedule'
)
BEGIN
    ALTER TABLE dbo.VehicleReconciliations
        ADD CONSTRAINT FK_VR_RouteSchedule
            FOREIGN KEY (RouteScheduleId)
                REFERENCES dbo.RouteSchedules (RouteScheduleId);
END
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE  name = 'IX_VehicleReconciliations_Route'
      AND  object_id = OBJECT_ID('dbo.VehicleReconciliations')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_VehicleReconciliations_Route
        ON dbo.VehicleReconciliations (DealerId, RouteScheduleId, ReconciliationDate);
END
GO