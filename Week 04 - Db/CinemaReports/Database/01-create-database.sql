-- Week 4: the cinema reporting database.
-- Run once against a local SQL Server instance:
--   sqlcmd -S localhost -E -C -b -i "Database/01-create-database.sql"
--
-- The schema is handed out with the task; the point of this week is not designing
-- it but asking questions of it. Four tables, two one-to-many chains meeting in
-- Reservations: a movie has many screenings, a screening has many reservations,
-- and a customer has many reservations.

IF DB_ID('CinemaReportsDb') IS NULL
BEGIN
    CREATE DATABASE CinemaReportsDb;
END
GO

USE CinemaReportsDb;
GO

-- Children first: every foreign key below points at a table dropped after it.
IF OBJECT_ID('dbo.Reservations', 'U') IS NOT NULL DROP TABLE dbo.Reservations;
IF OBJECT_ID('dbo.Screenings', 'U')   IS NOT NULL DROP TABLE dbo.Screenings;
IF OBJECT_ID('dbo.Customers', 'U')    IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.Movies', 'U')       IS NOT NULL DROP TABLE dbo.Movies;
GO

CREATE TABLE Movies
(
    Id              INT           IDENTITY(1,1) PRIMARY KEY,
    Title           NVARCHAR(200) NOT NULL,
    DurationMinutes INT           NOT NULL,

    CONSTRAINT CK_Movies_Duration
        CHECK (DurationMinutes > 0)
);
GO

CREATE TABLE Screenings
(
    Id             INT           IDENTITY(1,1) PRIMARY KEY,
    MovieId        INT           NOT NULL,
    StartsAt       DATETIME2     NOT NULL,
    Auditorium     NVARCHAR(50)  NOT NULL,
    NumberOfSeats  INT           NOT NULL,
    TicketPrice    DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_Screenings_Movies
        FOREIGN KEY (MovieId)
        REFERENCES Movies(Id),

    CONSTRAINT CK_Screenings_NumberOfSeats
        CHECK (NumberOfSeats > 0),

    CONSTRAINT CK_Screenings_TicketPrice
        CHECK (TicketPrice >= 0)
);
GO

CREATE TABLE Customers
(
    Id    INT           IDENTITY(1,1) PRIMARY KEY,
    Name  NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE
);
GO

-- CustomerId is nullable on purpose: a reservation can belong to a registered
-- customer, or to a guest who never made an account. For a guest the name is
-- written straight onto the reservation instead. That is why one query cannot
-- just read Customers.Name - it has to cope with both shapes.
CREATE TABLE Reservations
(
    Id           INT           IDENTITY(1,1) PRIMARY KEY,
    ScreeningId  INT           NOT NULL,
    CustomerId   INT           NULL,
    CustomerName NVARCHAR(200) NULL,
    SeatNumber   INT           NOT NULL,
    ReservedUtc  DATETIME2     NOT NULL,

    CONSTRAINT FK_Reservations_Screenings
        FOREIGN KEY (ScreeningId)
        REFERENCES Screenings(Id),

    CONSTRAINT FK_Reservations_Customers
        FOREIGN KEY (CustomerId)
        REFERENCES Customers(Id)
);
GO
