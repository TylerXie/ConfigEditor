-- SQL Server Migration Script for ConfigEditor
-- Migrates from GeneralConfig/role-based schema to flat config items schema

USE ConfigTest;
GO

-- Create new config_items table with the new schema
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'config_items')
BEGIN
    CREATE TABLE config_items (
        AppSettingId INT PRIMARY KEY IDENTITY(1,1),
        AppName NVARCHAR(100) NOT NULL,
        ClassName NVARCHAR(100) NOT NULL,
        Settings NVARCHAR(MAX) NOT NULL,
        CreatedDate DATETIME DEFAULT GETUTCDATE(),
        ModifiedDate DATETIME DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_config_items_AppName ON config_items(AppName);
    CREATE INDEX IX_config_items_ClassName ON config_items(ClassName);
END
GO

-- Migrate data from old config table to new config_items table (if old table exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'config')
BEGIN
    -- Backup old table before migration
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'config_backup')
    BEGIN
        SELECT * INTO config_backup FROM config;
    END

    -- Insert InitialConfig item
    INSERT INTO config_items (AppName, ClassName, Settings)
    VALUES (
        'InitialConfig',
        'InitialConfig',
        N'{"AppName":"InitialConfig","AppVersion":"1.0","AppSection":0,"Description":"Application Initial Configuration","CreatedDate":"' + CONVERT(VARCHAR(20), GETUTCDATE(), 120) + '","Environment":"Development"}'
    );

    -- Optional: Migrate existing data from old schema
    -- This is commented out as the old schema doesn't map directly to new schema
    -- Uncomment if you have migration logic for GeneralConfig data

END
GO

-- Drop old config table after migration is verified
-- UNCOMMENT AFTER VERIFICATION:
-- IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'config')
-- BEGIN
--     DROP TABLE config;
-- END
-- GO

-- Verify the new table structure
SELECT * FROM config_items;
GO
