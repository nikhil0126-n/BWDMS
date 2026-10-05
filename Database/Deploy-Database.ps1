<#
==============================================================================
 BWDMS - Database deployment script
------------------------------------------------------------------------------
 WHY THIS EXISTS
 ---------------
 Web.config connects to a LocalDB *file*:

     AttachDbFilename=|DataDirectory|\Database1.mdf

 so the database is NOT named "BWDMS". It is attached under its own physical
 file path. A script that starts with "USE BWDMS" therefore fails with
 "Cannot open database ... because it does not exist".

 This script:
   1. Reads the real connection string out of Web.config
   2. Resolves |DataDirectory| to BWDMS\App_Data
   3. Opens a connection to that exact database
   4. Prints the database name it actually selected  <- verification step
   5. Runs every .sql file in this folder, in filename order
   6. Re-verifies the object counts afterwards

 USAGE
 -----
   powershell -ExecutionPolicy Bypass -File Database\Deploy-Database.ps1
   powershell -ExecutionPolicy Bypass -File Database\Deploy-Database.ps1 -WhatIf
==============================================================================#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    # Override the Web.config connection string if you ever need to.
    [string]$ConnectionString,

    # Skip the post-deploy verification query.
    [switch]$SkipVerify
)

$ErrorActionPreference = 'Stop'

# --- locate project root (this script lives in <root>\Database) -------------
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir   = Split-Path -Parent $scriptDir
$webConfig = Join-Path $rootDir 'BWDMS\Web.config'
$sqlDir    = $scriptDir

if (-not (Test-Path $webConfig)) {
    throw "Web.config not found at: $webConfig"
}

Write-Host ''
Write-Host '======================================================' -ForegroundColor Cyan
Write-Host ' BWDMS - Database deployment' -ForegroundColor Cyan
Write-Host '======================================================' -ForegroundColor Cyan

# --- 1 + 2. read the connection string --------------------------------------
if (-not $ConnectionString) {
    [xml]$cfg = Get-Content $webConfig
    $node = $cfg.configuration.connectionStrings.add |
            Where-Object { $_.name -eq 'BWDMSConnection' }

    if (-not $node) {
        throw "Connection string 'BWDMSConnection' is missing from Web.config"
    }

    $ConnectionString = $node.connectionString
}

Write-Host "[1/6] Connection string read from Web.config"
Write-Host "      $($ConnectionString)" -ForegroundColor DarkGray

# Resolve |DataDirectory| exactly like System.Web does.
$dataDirectory = Join-Path $rootDir 'BWDMS\App_Data'
$cs = $ConnectionString -replace '\|DataDirectory\|', $dataDirectory

# --- parse out server + database (works for AttachDbFilename and Initial Catalog)
$server   = $null
$fileName = $null
$catalog  = $null

foreach ($part in $cs.Split(';')) {
    $p = $part.Trim()
    if ($p -match '^(Data Source|Server)\s*=\s*(.+)$')   { $server   = $Matches[2].Trim() }
    if ($p -match '^AttachDbFilename\s*=\s*(.+)$')       { $fileName = $Matches[1].Trim().Trim('"') }
    if ($p -match '^(Initial Catalog|Database)\s*=\s*(.+)$') { $catalog = $Matches[2].Trim() }
}

if (-not $server) { $server = '(localdb)\MSSQLLocalDB' }
if (-not $fileName -and -not $catalog) {
    throw "Could not determine which database the connection string points at."
}

Write-Host "[2/6] Server        : $server"

# --- 3. open the connection with System.Data.SqlClient ----------------------
Add-Type -AssemblyName System.Data

if (-not $ConnectionString.Contains('Initial Catalog') -and $fileName) {
    # AttachDbFilename is handled by the provider itself, so pass it through
    # untouched - it resolves |DataDirectory| server-side only if we already
    # replaced it, so give it the absolute path we computed.
    $cs = $cs -replace 'AttachDbFilename\s*=\s*', 'AttachDbFilename='
}

$conn = New-Object System.Data.SqlClient.SqlConnection $cs

try {
    $conn.Open()
}
catch {
    Write-Host ''
    Write-Host 'Could not open the database.' -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ''
    Write-Host 'If the file is in use, stop Visual Studio / IIS Express first,' -ForegroundColor Yellow
    Write-Host 'or run:  sqllocaldb stop MSSQLLocalDB && sqllocaldb start MSSQLLocalDB' -ForegroundColor Yellow
    exit 1
}

# --- 4. VERIFY the selected database ----------------------------------------
$dbName = $conn.Database

if ($fileName) {
    $phys = Split-Path -Leaf $fileName
    Write-Host "[3/6] Database file : $phys"
}

Write-Host "[3/6] VERIFIED selected database = [$dbName]" -ForegroundColor Green

$check = $conn.CreateCommand()
$check.CommandText = 'SELECT COUNT(*) FROM sys.tables'
$tableCount = [int]$check.ExecuteScalar()
Write-Host "      existing tables: $tableCount"

# --- 5. run the scripts ------------------------------------------------------
$scripts = Get-ChildItem -Path $sqlDir -Filter '*.sql' | Sort-Object Name

if (-not $scripts) {
    Write-Host 'No .sql files found in' $sqlDir -ForegroundColor Yellow
    $conn.Close()
    exit 0
}

Write-Host "[4/6] Found $($scripts.Count) script(s):"
$scripts | ForEach-Object { Write-Host "        $($_.Name)" -ForegroundColor DarkGray }

if (-not $PSCmdlet.ShouldProcess($dbName, 'Apply BWDMS SQL scripts')) {
    $conn.Close()
    return
}

Write-Host "[5/6] Applying scripts to [$dbName] ..."

foreach ($file in $scripts) {
    $sql = Get-Content -Path $file.FullName -Raw

    # Belt and braces: refuse any script that tries to switch databases.
    if ($sql -match '(?im)^\s*USE\s+\[') {
        throw "$($file.Name) contains a USE statement. Remove it - this script targets the verified database directly."
    }

    # Split on GO batch separators. SQL Server resolves column and table
    # names when it compiles a batch, so ALTER TABLE ... ADD COLUMN and the
    # statements that use that column must run in different batches.
    # A GO is a client-side separator, never part of the T-SQL text.
    $batches = [regex]::Split($sql, '(?im)^\s*GO\s*(?:\d+)?\s*$') |
               ForEach-Object { $_.Trim() } |
               Where-Object { $_ -ne '' }

    $batchIndex = 0

    foreach ($batch in $batches) {
        $batchIndex++

        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $batch

        try {
            $null = $cmd.ExecuteNonQuery()
        }
        catch {
            Write-Host ''
            Write-Host "FAILED in $($file.Name) - batch $batchIndex of $($batches.Count)" -ForegroundColor Red
            Write-Host $_.Exception.Message -ForegroundColor Red

            # Show the first line of the failing batch to make it findable.
            $firstLine = ($batch -split "`n")[0].Trim()
            Write-Host "First line: $firstLine" -ForegroundColor Yellow

            $conn.Close()
            exit 1
        }
    }

    Write-Host "      OK  $($file.Name)  ($batchIndex batch(es))" -ForegroundColor Green
}

# --- 6. verify afterwards ----------------------------------------------------
if (-not $SkipVerify) {
    Write-Host '[6/6] Post-deploy verification ...'

    $verify = $conn.CreateCommand()
    $verify.CommandText = @'
SELECT
    (SELECT COUNT(*) FROM sys.tables)                              AS TablesTotal,
    (SELECT COUNT(*) FROM sys.views)                               AS ViewsTotal,
    (SELECT COUNT(*) FROM sys.foreign_keys)                        AS ForeignKeys,
    (SELECT COUNT(*) FROM ProductCategories)                       AS Categories,
    (SELECT COUNT(*) FROM SellingUnits)                            AS SellingUnits,
    (SELECT COUNT(*) FROM Users WHERE Role = 'Dealer')            AS Dealers;
'@

    $reader = $verify.ExecuteReader()
    while ($reader.Read()) {
        Write-Host "      Tables          : $($reader['TablesTotal'])" -ForegroundColor Green
        Write-Host "      Views           : $($reader['ViewsTotal'])" -ForegroundColor Green
        Write-Host "      Foreign keys    : $($reader['ForeignKeys'])" -ForegroundColor Green
        Write-Host "      Product categories: $($reader['Categories'])" -ForegroundColor Green
        Write-Host "      Selling units   : $($reader['SellingUnits'])" -ForegroundColor Green
        Write-Host "      Dealers         : $($reader['Dealers'])" -ForegroundColor Green
    }
    $reader.Close()
}

$conn.Close()

Write-Host ''
Write-Host 'Deployment completed.' -ForegroundColor Cyan
Write-Host ''
