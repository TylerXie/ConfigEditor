-- SQL Server Setup Script for ConfigEditor
-- Create the ConfigTest database and config table

-- Create database (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ConfigTest')
BEGIN
    CREATE DATABASE ConfigTest;
END
GO

USE ConfigTest;
GO

-- Create the config table
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'config')
BEGIN
    CREATE TABLE config (
        AppSection INT PRIMARY KEY,
        CustomerName NVARCHAR(100) NOT NULL,
        DatabaseMgmtConfigs NVARCHAR(MAX) NOT NULL DEFAULT '[]',
        FileMgmtConfigs NVARCHAR(MAX) NOT NULL DEFAULT '[]',
        AppLoadConfigs NVARCHAR(MAX) NOT NULL DEFAULT '[]',
        AppWriteConfigs NVARCHAR(MAX) NOT NULL DEFAULT '[]'
    );
END
GO

-- Insert sample data
IF NOT EXISTS (SELECT * FROM config)
BEGIN
    INSERT INTO config (AppSection, CustomerName, DatabaseMgmtConfigs, FileMgmtConfigs, AppLoadConfigs, AppWriteConfigs)
    VALUES 
        (1, 'Sample Customer 1', '[]', '[]', '[]', '[]'),
        (2, 'Sample Customer 2', '[]', '[]', '[]', '[]');
END
GO

-- Verify the table
SELECT * FROM config;
