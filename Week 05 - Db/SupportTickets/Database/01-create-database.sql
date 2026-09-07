-- SupportTicketDb — week 5, Entity Framework Database First.
-- Re-runnable: the database is dropped first, so a broken run costs nothing.

USE master;
GO

IF DB_ID('SupportTicketDb') IS NOT NULL
BEGIN
    ALTER DATABASE SupportTicketDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE SupportTicketDb;
END
GO

CREATE DATABASE SupportTicketDb;
GO

USE SupportTicketDb;
GO

CREATE TABLE Customers
(
    Id    INT IDENTITY(1,1) PRIMARY KEY,
    Name  NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE
);
GO

CREATE TABLE Tickets
(
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId  INT NOT NULL,
    Title       NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Status      NVARCHAR(30) NOT NULL,
    Priority    INT NOT NULL,
    CreatedUtc  DATETIME2 NOT NULL,

    CONSTRAINT FK_Tickets_Customers
        FOREIGN KEY (CustomerId) REFERENCES Customers(Id),

    CONSTRAINT CK_Tickets_Priority
        CHECK (Priority BETWEEN 1 AND 5)
);
GO

INSERT INTO Customers (Name, Email)
VALUES
    ('Ada Lovelace',      'ada@example.com'),
    ('Grace Hopper',      'grace@example.com'),
    ('Alan Turing',       'alan@example.com'),
    ('Margaret Hamilton', 'margaret@example.com'),
    ('Donald Knuth',      'donald@example.com');
GO

INSERT INTO Tickets (CustomerId, Title, Description, Status, Priority, CreatedUtc)
VALUES
    (1, 'Cannot log in',          'Login fails after entering password.',        'Open',   5, DATEADD(DAY,  -7, SYSUTCDATETIME())),
    (1, 'Wrong invoice',          'The amount on the invoice seems incorrect.',  'Closed', 2, DATEADD(DAY, -15, SYSUTCDATETIME())),
    (2, 'Application is slow',    'Pages take several seconds to load.',         'Open',   4, DATEADD(DAY,  -3, SYSUTCDATETIME())),
    (2, 'Cannot change email',    'Saving a new email address gives an error.',  'Open',   3, DATEADD(DAY,  -2, SYSUTCDATETIME())),
    (3, 'Missing data',           'Some records are missing from the report.',   'Closed', 5, DATEADD(DAY, -20, SYSUTCDATETIME())),
    (3, 'Export does not work',   'CSV export produces an empty file.',          'Open',   4, DATEADD(DAY,  -1, SYSUTCDATETIME())),
    (4, 'Typo in profile',        'There is a spelling error in the profile page.', 'Open', 1, DATEADD(HOUR, -8, SYSUTCDATETIME())),
    (4, 'Password reset',         'Reset email never arrives.',                  'Closed', 3, DATEADD(DAY,  -6, SYSUTCDATETIME())),
    (1, 'Mobile layout broken',   'Some buttons overlap on a small screen.',     'Open',   2, DATEADD(HOUR, -4, SYSUTCDATETIME())),
    (2, 'Duplicate notification', 'The same notification arrives twice.',        'Open',   2, DATEADD(HOUR, -2, SYSUTCDATETIME()));
GO

SELECT COUNT(*) AS Customers FROM Customers;
SELECT COUNT(*) AS Tickets   FROM Tickets;
GO
