-- Create EcfrDb database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EcfrDb')
BEGIN
    CREATE DATABASE EcfrDb;
END
GO

-- Use EcfrDb database
USE EcfrDb;
GO

-- Grant access to sa user
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'sa')
BEGIN
    CREATE USER [sa] FOR LOGIN [sa];
END
GO

-- Grant db_owner role to sa user
ALTER ROLE db_owner ADD MEMBER [sa];
GO

-- Create Titles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Titles' AND xtype='U')
BEGIN
    CREATE TABLE Titles (
        Id int IDENTITY(1,1) PRIMARY KEY,
        TitleNumber int NOT NULL,
        Name nvarchar(255) NOT NULL
    );
END
GO

-- Insert sample data
IF NOT EXISTS (SELECT * FROM Titles)
BEGIN
    INSERT INTO Titles (TitleNumber, Name) VALUES 
    (42, 'Public Health'),
    (21, 'Food and Drugs'),
    (40, 'Protection of Environment');
END
GO