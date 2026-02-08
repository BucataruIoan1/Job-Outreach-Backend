IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'JobOutreach')
BEGIN
    CREATE DATABASE JobOutreach;
END
GO

USE JobOutreach;
GO

CREATE TABLE Company (
    Id INT IDENTITY PRIMARY KEY,
    CompanyName NVARCHAR(255) NOT NULL,
    CompanyPhone NVARCHAR(50) NOT NULL,
    CompanyEmail NVARCHAR(255) NOT NULL,
    IsContacted BIT NOT NULL DEFAULT 0,
    CompanyUrl NVARCHAR(255) NOT NULL
);

CREATE TABLE Position (
    Id INT IDENTITY PRIMARY KEY,
    PositionName NVARCHAR(100) NOT NULL
);

CREATE TABLE Country (
    Id INT IDENTITY PRIMARY KEY,
    CountryName NVARCHAR(100) NOT NULL,
    CountryCode NVARCHAR(10) NOT NULL
);

CREATE TABLE Email (
    Id INT IDENTITY PRIMARY KEY,
    Subject NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Attachment NVARCHAR(255),
    SentBy NVARCHAR(255) NOT NULL,
    CountryId INT NULL,
    PositionId INT NULL,
    CONSTRAINT FK_Email_Country FOREIGN KEY (CountryId) REFERENCES Country(Id),
    CONSTRAINT FK_Email_Position FOREIGN KEY (PositionId) REFERENCES Position(Id)
);

CREATE TABLE CompanyPosition (
    CompanyId INT NOT NULL,
    PositionId INT NOT NULL,
    CONSTRAINT PK_CompanyPosition PRIMARY KEY (CompanyId, PositionId),
    CONSTRAINT FK_CP_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CP_Position FOREIGN KEY (PositionId) REFERENCES Position(Id) ON DELETE CASCADE
);

CREATE TABLE EmailSent (
    Id INT IDENTITY PRIMARY KEY,
    EmailId INT NOT NULL,
    CompanyId INT NOT NULL,
    PositionId INT NOT NULL,
    SentToEmail NVARCHAR(255) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_EmailSent_Email FOREIGN KEY (EmailId) REFERENCES Email(Id),
    CONSTRAINT FK_EmailSent_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id),
    CONSTRAINT FK_EmailSent_Position FOREIGN KEY (PositionId) REFERENCES Position(Id)
);
