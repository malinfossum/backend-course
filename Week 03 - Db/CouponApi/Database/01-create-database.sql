-- Week 3: the database behind CouponApi.Api.
-- Run once against a local SQL Server instance:
--   sqlcmd -S localhost -E -C -b -i Database/01-create-database.sql

IF DB_ID('CouponDb') IS NULL
BEGIN
    CREATE DATABASE CouponDb;
END
GO

USE CouponDb;
GO

IF OBJECT_ID('dbo.Coupons', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Coupons;
END
GO

-- Every rule here is one the service also enforces. The service gives the user
-- a readable explanation; the table is what holds when the write arrives from
-- somewhere else entirely - Rider's database window, a script, another service.
CREATE TABLE Coupons
(
    Id            INT           IDENTITY(1,1) PRIMARY KEY,
    Code          NVARCHAR(50)  NOT NULL,
    Description   NVARCHAR(200) NOT NULL,
    RemainingUses INT           NOT NULL,
    IsActive      BIT           NOT NULL,

    -- Named rather than written inline on the column: an inline UNIQUE makes
    -- SQL Server invent a name like UQ__Coupons__A25C5AA7C414D884, and the
    -- random tail is no help when an error has to say which rule broke.
    CONSTRAINT UQ_Coupons_Code UNIQUE (Code),

    -- The one rule C# cannot be trusted with alone: "used up" must never turn
    -- into a negative balance, no matter who does the UPDATE.
    CONSTRAINT CK_Coupons_RemainingUses CHECK (RemainingUses >= 0)
);
GO

-- Id is deliberately absent: IDENTITY hands it out, so the database decides.
INSERT INTO Coupons (Code, Description, RemainingUses, IsActive)
VALUES
    ('SUMMER26', '20 % rabatt',     3,  1),
    ('WELCOME',  'Velkomstrabatt', 10,  1),
    ('OLD2025',  'Gammel kampanje', 5,  0);
GO

SELECT * FROM Coupons;
GO
