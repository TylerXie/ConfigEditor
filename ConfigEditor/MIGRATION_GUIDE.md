# ConfigEditor Database Migration Guide

## Overview

This guide provides step-by-step instructions for migrating the ConfigEditor database from the old hierarchical schema to the new normalized schema.

---

## Prerequisites

- SQL Server Management Studio (SSMS) or Azure Data Studio
- ConfigTest database access
- Backup of current database (recommended)

---

## Migration Steps

### Step 1: Backup Current Database (Recommended)

```bash
# Using SQL Server Management Studio
# Right-click ConfigTest database → Tasks → Back Up...
# Or use TSQL:
```

```sql
BACKUP DATABASE ConfigTest 
TO DISK = 'C:\Backups\ConfigTest_Backup_$(date).bak'
WITH NAME = 'ConfigTest Full Backup';
```

---

### Step 2: Run Migration Script

Execute the `DATABASE_MIGRATION.sql` script:

1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Open `ConfigEditor/DATABASE_MIGRATION.sql`
4. Execute the entire script
5. Verify output: "Verify the new table structure" query result

**Expected Result:**
```
AppSettingId | AppName       | ClassName     | Settings
-------------|---------------|--------|------
1            | InitialConfig | InitialConfig | {...JSON...}
```

---

### Step 3: Verify Migration

Run verification queries in SQL Server Management Studio:

```sql
-- Check new table structure
SELECT * FROM config_items;

-- Verify InitialConfig was created
SELECT * FROM config_items WHERE ClassName = 'InitialConfig';

-- Check backup table (if migration created one)
SELECT * FROM config_backup;  -- Only if old data existed
```

---

### Step 4: Update Connection String (if needed)

No changes required to `appsettings.json` unless you're using a different database:

```json
{
  "ConnectionStrings": {
    "ConfigDatabase": "Server=localhost;Database=ConfigTest;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

---

### Step 5: Rebuild and Test Application

```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project ConfigEditor/ConfigEditor.csproj

# Run unit tests
dotnet test ConfigEditor.Tests/ConfigEditor.Tests.csproj
```

**Expected Result:**
- Application loads successfully
- Loads InitialConfig from database
- "All 26 tests passed" message

---

## Old Schema Cleanup (Optional)

After verifying the new schema works correctly, you can clean up the old table:

```sql
-- OPTIONAL: Drop old config table (after full verification)
-- Keep commented until you're sure migration is successful
-- DROP TABLE config;

-- Optional: Keep backup for reference
-- SELECT * FROM config_backup;
```

**⚠️ Warning:** Do NOT drop the old table until you've verified:
- Application loads and saves data correctly
- All tests pass
- Data is accessible in new schema

---

## Rollback Plan (if needed)

If you encounter issues and need to rollback:

### Option 1: Restore from Backup

```sql
-- Restore from backup
RESTORE DATABASE ConfigTest 
FROM DISK = 'C:\Backups\ConfigTest_Backup_$(date).bak'
WITH REPLACE;
```

### Option 2: Manual Rollback

```sql
-- Drop new schema
DROP TABLE IF EXISTS config_items;

-- Keep old config table (if not dropped)
SELECT * FROM config;
```

---

## Data Migration (if moving old data)

If you have existing data in the old `config` table and want to migrate it:

### 1. Analyze Old Schema

```sql
SELECT * FROM config;
```

### 2. Transform Old Data

```sql
-- Example: Convert GeneralConfig data to individual items
-- This is custom per implementation - modify as needed

INSERT INTO config_items (AppName, ClassName, Settings)
SELECT 
    CustomerName AS AppName,
    'GeneralConfig' AS ClassName,
    CONCAT(
        '{"AppSection":',AppSection,
        ',"CustomerName":"',REPLACE(CustomerName,'"','\"'),
        '","AppVersion":"1.0"}'
    ) AS Settings
FROM config;

-- Verify
SELECT * FROM config_items;
```

### 3. Verify Data Integrity

```sql
-- Verify row counts
SELECT COUNT(*) as OldRecords FROM config;
SELECT COUNT(*) as NewRecords FROM config_items;

-- Verify sample data
SELECT TOP 1 * FROM config_items;
```

---

## Post-Migration Checklist

- [ ] Database backup created
- [ ] Migration script executed successfully
- [ ] config_items table created
- [ ] InitialConfig record present
- [ ] Old config table preserved (backup)
- [ ] Application builds without errors
- [ ] All 26 unit tests pass
- [ ] Application loads configs successfully
- [ ] Can add new config items
- [ ] Can save config items
- [ ] Can remove config items (except InitialConfig)
- [ ] Old config table dropped (optional, after verification)

---

## Troubleshooting

### Issue: "config_items table already exists"

**Solution:** Drop existing table and re-run migration

```sql
DROP TABLE IF EXISTS config_items;
DROP TABLE IF EXISTS config_backup;
-- Re-run DATABASE_MIGRATION.sql
```

---

### Issue: "InitialConfig not found" error in application

**Solution:** Ensure InitialConfig was created by migration

```sql
-- Check if InitialConfig exists
SELECT * FROM config_items WHERE ClassName = 'InitialConfig';

-- If not, create it manually
INSERT INTO config_items (AppName, ClassName, Settings)
VALUES (
    'InitialConfig',
    'InitialConfig',
    N'{"AppName":"InitialConfig","AppVersion":"1.0","AppSection":0,"Description":"Application Initial Configuration","CreatedDate":"' + CONVERT(VARCHAR(20), GETUTCDATE(), 120) + '","Environment":"Development"}'
);
```

---

### Issue: "Connection string not found" error

**Solution:** Verify appsettings.json

```json
{
  "ConnectionStrings": {
    "ConfigDatabase": "Server=localhost;Database=ConfigTest;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

---

### Issue: Old tests fail with new schema

**Solution:** All 26 existing tests should still pass because they test SerializeList/DeserializeList methods which are backward compatible. If tests fail:

1. Run: `dotnet test --verbosity detailed`
2. Check error messages
3. Verify both old and new tables exist
4. Ensure InitialConfig in database

---

## Schema Reference

### config_items table

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| AppSettingId | INT | PRIMARY KEY IDENTITY(1,1) | Unique identifier |
| AppName | NVARCHAR(100) | NOT NULL | Display name |
| ClassName | NVARCHAR(100) | NOT NULL | Type name for deserialization |
| Settings | NVARCHAR(MAX) | NOT NULL | Complete item as JSON |
| CreatedDate | DATETIME | DEFAULT GETUTCDATE() | Creation timestamp |
| ModifiedDate | DATETIME | DEFAULT GETUTCDATE() | Last modification timestamp |

### Indexes

```sql
CREATE INDEX IX_config_items_AppName ON config_items(AppName);
CREATE INDEX IX_config_items_ClassName ON config_items(ClassName);
```

---

## Sample Queries

### Get all config items

```sql
SELECT AppSettingId, AppName, ClassName, CreatedDate, ModifiedDate
FROM config_items
ORDER BY ClassName, AppName;
```

### Get specific config type

```sql
SELECT * FROM config_items
WHERE ClassName = 'DatabaseMgmtConfig'
ORDER BY AppName;
```

### Get InitialConfig

```sql
SELECT * FROM config_items
WHERE ClassName = 'InitialConfig';
```

### Count by type

```sql
SELECT ClassName, COUNT(*) as Count
FROM config_items
GROUP BY ClassName
ORDER BY ClassName;
```

### Recent changes

```sql
SELECT TOP 10 AppName, ClassName, ModifiedDate
FROM config_items
ORDER BY ModifiedDate DESC;
```

---

## Support

If you encounter issues during migration:

1. Check the troubleshooting section above
2. Review error messages in application output
3. Verify database integrity with queries above
4. Restore from backup if necessary
5. Contact your development team

---

**Document Version:** 1.0
**Last Updated:** 2024
**Status:** Ready for Production
