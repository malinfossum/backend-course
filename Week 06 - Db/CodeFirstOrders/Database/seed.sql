-- Test data for the migration exercises. The schema comes from the migrations, not from here:
--   dotnet ef database update --project CodeFirstOrders.Api
-- Safe to run more than once.
USE CodeFirstOrdersDb;

IF NOT EXISTS (SELECT 1 FROM Customers WHERE EmailAddress = 'ada@example.com')
    INSERT INTO Customers (FullName, EmailAddress, Country, IsActive)
    VALUES ('Ada Lovelace', 'ada@example.com', 'Norway', 1);

IF NOT EXISTS (SELECT 1 FROM Customers WHERE EmailAddress = 'grace@example.com')
    INSERT INTO Customers (FullName, EmailAddress, Country, IsActive)
    VALUES ('Grace Hopper', 'grace@example.com', 'Norway', 1);

IF NOT EXISTS (SELECT 1 FROM Orders)
    INSERT INTO Orders (CustomerId, CreatedUtc, TotalAmount, Status, Note)
    VALUES (
        (SELECT TOP 1 Id FROM Customers WHERE EmailAddress = 'ada@example.com'),
        SYSUTCDATETIME(), 1299, 'Created', 'Customer called support');

SELECT * FROM Customers;
SELECT Id, CustomerId, TotalAmount, Status, Note FROM Orders;
SELECT MigrationId FROM __EFMigrationsHistory;
