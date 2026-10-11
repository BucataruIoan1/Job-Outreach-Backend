-- One-time script: aduce o baza JobOutreach creata cu vechiul Create-Database-Script-SQL.sql
-- la schema generata de migratia EF Core 20261011140249_InitialCreate si o marcheaza ca aplicata.
-- Pentru o baza noua NU rulezi acest script, ci: dotnet ef database update
-- Scriptul e idempotent: daca migratia e deja inregistrata, nu face nimic.

USE JobOutreach;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.__EFMigrationsHistory') IS NULL
BEGIN
    CREATE TABLE dbo.[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[__EFMigrationsHistory] WHERE [MigrationId] = N'20261011140249_InitialCreate')
BEGIN
    -- 1. Primary keys: nume generate de SQL Server -> conventia EF (PK_<Tabel>)
    DECLARE @table sysname, @oldName nvarchar(300), @newName sysname;
    DECLARE pk_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT t.name, N'dbo.' + kc.name, N'PK_' + t.name
        FROM sys.key_constraints kc
        JOIN sys.tables t ON t.object_id = kc.parent_object_id
        WHERE kc.type = 'PK'
          AND t.name IN (N'Company', N'Country', N'Email', N'Position')
          AND kc.name <> N'PK_' + t.name;
    OPEN pk_cursor;
    FETCH NEXT FROM pk_cursor INTO @table, @oldName, @newName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC sp_rename @oldName, @newName, N'OBJECT';
        FETCH NEXT FROM pk_cursor INTO @table, @oldName, @newName;
    END;
    CLOSE pk_cursor;
    DEALLOCATE pk_cursor;

    -- 2. Foreign keys: conventia EF (FK_<Tabel>_<Principal>_<Coloana>)
    EXEC sp_rename N'dbo.FK_CompanyPosition_Company',  N'FK_CompanyPosition_Company_CompanyId',   N'OBJECT';
    EXEC sp_rename N'dbo.FK_CompanyPosition_Position', N'FK_CompanyPosition_Position_PositionId', N'OBJECT';
    EXEC sp_rename N'dbo.FK_EmailSent_Company',        N'FK_EmailSent_Company_CompanyId',         N'OBJECT';
    EXEC sp_rename N'dbo.FK_EmailSent_Email',          N'FK_EmailSent_Email_EmailId',             N'OBJECT';
    EXEC sp_rename N'dbo.FK_EmailSent_Position',       N'FK_EmailSent_Position_PositionId',       N'OBJECT';
    EXEC sp_rename N'dbo.FK_Email_Country',            N'FK_Email_Country_CountryId',             N'OBJECT';
    EXEC sp_rename N'dbo.FK_Email_Position',           N'FK_Email_Position_PositionId',           N'OBJECT';

    -- 3. Coloane care in model sunt obligatorii (verificat: nu exista valori NULL)
    UPDATE dbo.Email SET Attachment = N'' WHERE Attachment IS NULL;
    ALTER TABLE dbo.Email ALTER COLUMN Attachment nvarchar(255) NOT NULL;

    -- CountryId intra in FK_Email_Country_CountryId, deci FK-ul trebuie scos temporar
    ALTER TABLE dbo.Email DROP CONSTRAINT FK_Email_Country_CountryId;
    ALTER TABLE dbo.Email ALTER COLUMN CountryId int NOT NULL;
    ALTER TABLE dbo.Email WITH CHECK ADD CONSTRAINT FK_Email_Country_CountryId
        FOREIGN KEY (CountryId) REFERENCES dbo.Country (Id);

    -- 4. Indexuri pe foreign keys (EF le creeaza implicit)
    CREATE INDEX IX_CompanyPosition_PositionId ON dbo.CompanyPosition (PositionId);
    CREATE INDEX IX_Email_CountryId            ON dbo.Email (CountryId);
    CREATE INDEX IX_Email_PositionId           ON dbo.Email (PositionId);
    CREATE INDEX IX_EmailSent_CompanyId        ON dbo.EmailSent (CompanyId);
    CREATE INDEX IX_EmailSent_EmailId          ON dbo.EmailSent (EmailId);
    CREATE INDEX IX_EmailSent_PositionId       ON dbo.EmailSent (PositionId);

    -- 5. Marcheaza migratia ca aplicata
    INSERT INTO dbo.[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011140249_InitialCreate', N'9.0.0');
END;

COMMIT TRANSACTION;
GO
