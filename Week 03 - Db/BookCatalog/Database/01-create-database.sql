-- Week 3: the database behind BookCatalog.Api.
-- Run once against a local SQL Server instance:
--   sqlcmd -S localhost -E -C -b -i Database/01-create-database.sql

IF DB_ID('BookCatalogDb') IS NULL
BEGIN
    CREATE DATABASE BookCatalogDb;
END
GO

USE BookCatalogDb;
GO

IF OBJECT_ID('dbo.Books', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Books;
END
GO

CREATE TABLE Books
(
    Id          INT           PRIMARY KEY,
    Title       NVARCHAR(200) NOT NULL,
    Author      NVARCHAR(150) NOT NULL,
    [Year]      INT           NOT NULL,
    IsAvailable BIT           NOT NULL
);
GO

-- Same six books as data/books.json, so the two repositories can be compared
-- against identical data.
INSERT INTO Books (Id, Title, Author, [Year], IsAvailable)
VALUES
    (1, 'The Hobbit',               'J.R.R. Tolkien',   1937, 1),
    (2, 'Clean Code',               'Robert C. Martin', 2008, 1),
    (3, 'The Pragmatic Programmer', 'David Thomas',     1999, 0),
    (4, 'Algorithms',               'Ada',              2024, 1),
    (5, 'Backend Fundamentals',     'Ada',              2026, 1),
    (6, 'Domain-Driven Design',     'Eric Evans',       2003, 0);
GO

SELECT * FROM Books;
GO
