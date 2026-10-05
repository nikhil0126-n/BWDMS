/* ============================================================================
   BWDMS - Balaji Wafer Dealer Management System
   FILE   : Database\00_CreateDatabase.sql
   PURPOSE: PART 6 / PART 18 deliverable #1 - the database creation script,
            plus the instructions for applying the scripts in the correct
            order.

   WHY THIS FILE LOOKS UNUSUAL
   ---------------------------
   Most BWDMS tutorials start with "CREATE DATABASE BWDMS" and then
   "USE BWDMS".  THAT IS WRONG FOR THIS PROJECT and is exactly the mistake
   PART 6 warns about:

       "Do not use an incorrect USE statement for a database that has not
        been created."

   Web.config connects like this:

       Data Source=(LocalDB)\MSSQLLocalDB;
       AttachDbFilename=|DataDirectory|\Database1.mdf;
       Integrated Security=True

   There is NO database named "BWDMS".  The database is attached under its
   physical file path, and the connection is ALREADY attached to it when
   this script runs.  A "USE BWDMS" here would fail with
   "Cannot open database BWDMS because it does not exist", and a bare
   "CREATE DATABASE BWDMS" would quietly create a second, empty, unused
   database that nothing points at.

   So this script supports BOTH deployment shapes explicitly:

     PATH A - FILE ATTACHED (default, what the project ships with)
              Nothing to create.  Database1.mdf is the database.  The script
              only verifies the attachment and prints the facts.

     PATH B - NAMED DATABASE (recommended for a real IIS deployment)
              Flip @CreateNamedDb to 1 and re-run once; then point
              Web.config at Initial Catalog=BWDMS.  The CREATE DATABASE is
              guarded by DB_ID() so a re-run is harmless.

   ORDER OF SCRIPTS (PART 18, deliverable 5)
   -----------------------------------------
     00_CreateDatabase.sql          database creation / attachment check
     01_Schema.sql                  tables + columns + foreign keys +
                                    check constraints + indexes + demo seed
                                    + reporting views   (idempotent)
     02_ReceiptsAndOrderSource.sql  company receipts + Orders.OrderSource
                                    (idempotent)

   Deploy-Database.ps1 runs every *.sql file in this folder in filename
   order against the connection string read from Web.config, so the order
   above is applied automatically.

     powershell -ExecutionPolicy Bypass -File Database\Deploy-Database.ps1
     powershell -ExecutionPolicy Bypass -File Database\Deploy-Database.ps1 -WhatIf

   NOTE ON THE "SPLIT SCRIPTS" REQUIREMENT
   ---------------------------------------
   PART 18 suggests separate table / constraints / seed scripts.  In this
   schema the UNIQUE indexes and the table-level CHECK constraints are
   interleaved with their CREATE TABLE statements (a unique index must be
   created after its table and before rows exist), so splitting them into
   separate files would produce scripts that cannot run independently -
   which PART 18 also forbids.  They are therefore kept together inside
   01_Schema.sql and numbered so the required order is still explicit.
   ============================================================================ */


/* ============================================================================
   PATH B SWITCH - leave at 0 for the shipped, file-attached deployment.
   ============================================================================ */

DECLARE @CreateNamedDb bit;
SET @CreateNamedDb = 0;          -- set to 1 to create [BWDMS] (PATH B)

IF @CreateNamedDb = 1 AND DB_ID(N'BWDMS') IS NULL
BEGIN
    -- EXEC keeps the CREATE DATABASE out of the parse-time batch so the
    -- guarded statement above stays valid on a connection that has no
    -- permission to create databases until you deliberately flip the flag.
    EXEC (N'CREATE DATABASE [BWDMS]');
    PRINT 'Created named database [BWDMS]. Now set Web.config connection string to Initial Catalog=BWDMS';
END
GO


/* ============================================================================
   VERIFICATION - works for both paths, never modifies anything.
   ============================================================================ */

DECLARE @attached nvarchar(256) = DB_NAME();

SELECT
    AttachedDatabase = @attached,
    DatabaseId       = DB_ID(),
    Collation        = CONVERT(nvarchar(128), DATABASEPROPERTYEX(@attached, 'Collation')),
    ServerProduct    = CONVERT(nvarchar(128), SERVERPROPERTY('ProductVersion'));

-- Physical file locations, so the operator can confirm which .mdf is live.
SELECT
    LogicalName  = name,
    PhysicalName = physical_name,
    StateDesc    = state_desc
FROM   sys.master_files
WHERE  database_id = DB_ID();

-- PATH A is the default: confirm the connection really is a file attachment.
IF EXISTS
(
    SELECT 1
    FROM   sys.master_files
    WHERE  database_id = DB_ID()
      AND  physical_name LIKE N'%.mdf'
)
    PRINT 'PATH A confirmed: the connection is attached to a LocalDB .mdf file. No CREATE DATABASE is required.';
ELSE
    PRINT 'Named database detected - this is PATH B. Ensure Web.config uses Initial Catalog=BWDMS.';


/* ============================================================================
   OBJECT COUNTS - the starting point for the post-deploy check that
   Deploy-Database.ps1 re-runs after 01 and 02 have been applied.
   ============================================================================ */

SELECT
    Tables      = (SELECT COUNT(*) FROM sys.tables),
    Views       = (SELECT COUNT(*) FROM sys.views),
    ForeignKeys = (SELECT COUNT(*) FROM sys.foreign_keys),
    CheckCks    = (SELECT COUNT(*) FROM sys.check_constraints),
    Indexes     = (SELECT COUNT(*) FROM sys.indexes WHERE is_hypothetical = 0);
