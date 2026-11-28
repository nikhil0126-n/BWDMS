/*
 * BWDMS - Collections Table Creation Script
 * Run this script against your database to add payment collection support
 */

-- ============================================================
-- COLLECTIONS TABLE
-- Stores payments collected from shops by salesmen
-- ============================================================
CREATE TABLE Collections (
    CollectionId        INT IDENTITY(1,1) PRIMARY KEY,
    ShopId              INT NOT NULL,
    SalesmanId          INT NOT NULL,
    DealerId            INT NOT NULL,          -- denormalized for dealer reports
    CollectionDate      DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    CollectionTime      TIME NOT NULL DEFAULT CAST(GETDATE() AS TIME),
    Amount              DECIMAL(18,2) NOT NULL,
    PaymentMode         NVARCHAR(20) NOT NULL DEFAULT 'Cash',  -- Cash, UPI, Card, Cheque, Bank Transfer
    ReferenceNumber     NVARCHAR(100) NULL,                     -- cheque no, UPI ref, etc.
    Notes               NVARCHAR(500) NULL,
    CreatedAt           DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt           DATETIME NULL,

    CONSTRAINT FK_Collections_Shop
        FOREIGN KEY (ShopId) REFERENCES Shops(ShopId),
    CONSTRAINT FK_Collections_Salesman
        FOREIGN KEY (SalesmanId) REFERENCES Users(UserId),
    CONSTRAINT FK_Collections_Dealer
        FOREIGN KEY (DealerId) REFERENCES Users(UserId),
    CONSTRAINT CK_Collections_Amount_Positive
        CHECK (Amount > 0),
    CONSTRAINT CK_Collections_PaymentMode
        CHECK (PaymentMode IN ('Cash', 'UPI', 'Card', 'Cheque', 'Bank Transfer'))
);

-- Indexes for common query patterns
CREATE INDEX IX_Collections_Shop_Date
    ON Collections (ShopId, CollectionDate DESC);

CREATE INDEX IX_Collections_Salesman_Date
    ON Collections (SalesmanId, CollectionDate DESC);

CREATE INDEX IX_Collections_Dealer_Date
    ON Collections (DealerId, CollectionDate DESC);

CREATE INDEX IX_Collections_CollectionDate
    ON Collections (CollectionDate DESC);


-- ============================================================
-- HELPER VIEW: Shop Outstanding Balance
-- Calculates real-time outstanding: OpeningBalance + TotalSales - TotalCollections
-- ============================================================
CREATE VIEW vw_ShopOutstanding AS
SELECT
    s.ShopId,
    s.ShopCode,
    s.ShopName,
    s.OwnerName,
    s.Phone,
    s.VillageId,
    s.RouteId,
    s.RouteScheduleId,
    s.OpeningBalance,
    s.CreditLimit,
    s.DealerId,
    ISNULL(Sales.TotalSales, 0) AS TotalSales,
    ISNULL(Coll.TotalCollections, 0) AS TotalCollections,
    (s.OpeningBalance + ISNULL(Sales.TotalSales, 0) - ISNULL(Coll.TotalCollections, 0)) AS OutstandingBalance,
    CASE
        WHEN (s.OpeningBalance + ISNULL(Sales.TotalSales, 0) - ISNULL(Coll.TotalCollections, 0)) > ISNULL(s.CreditLimit, 999999999)
        THEN 1 ELSE 0
    END AS OverCreditLimit
FROM Shops s
OUTER APPLY (
    SELECT SUM(o.GrandTotal) AS TotalSales
    FROM Orders o
    WHERE o.ShopId = s.ShopId
      AND o.Status NOT IN ('Cancelled', 'Returned')
) Sales
OUTER APPLY (
    SELECT SUM(c.Amount) AS TotalCollections
    FROM Collections c
    WHERE c.ShopId = s.ShopId
) Coll
WHERE s.IsActive = 1;


-- ============================================================
-- SAMPLE DATA (Optional - for testing)
-- ============================================================
/*
INSERT INTO Collections (ShopId, SalesmanId, DealerId, CollectionDate, CollectionTime, Amount, PaymentMode, ReferenceNumber, Notes)
VALUES
    (1, 5, 1, CAST(GETDATE() AS DATE), CAST(GETDATE() AS TIME), 5000.00, 'Cash', NULL, 'Part payment'),
    (1, 5, 1, DATEADD(DAY, -7, CAST(GETDATE() AS DATE)), CAST(GETDATE() AS TIME), 3000.00, 'UPI', 'UPI123456789', 'Full settlement'),
    (2, 5, 1, CAST(GETDATE() AS DATE), CAST(GETDATE() AS TIME), 10000.00, 'Card', 'CARD987654321', NULL);
*/