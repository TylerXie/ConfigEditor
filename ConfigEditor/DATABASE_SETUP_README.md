# ConfigEditor Database Setup Guide

## Prerequisites

- SQL Server (LocalDB or Express Edition) running on localhost
- Windows Authentication enabled
- SQL Server Management Studio (SSMS) or Visual Studio

## Database Setup

### Option 1: Using SQL Server Management Studio (SSMS)

1. Open SQL Server Management Studio
2. Connect to your local SQL Server instance (Server: localhost, Authentication: Windows)
3. Open the `DATABASE_SETUP.sql` file located in the project root
4. Execute the script (Ctrl+E or F5)

This will:
- Create the `ConfigTest` database (if it doesn't exist)
- Create the `config` table with the following schema:
  - **AppSection** (INT, Primary Key) - Unique identifier for each configuration
  - **CustomerName** (NVARCHAR(100)) - Customer name
  - **DatabaseMgmtConfigs** (NVARCHAR(MAX)) - JSON array of database configurations
  - **FileMgmtConfigs** (NVARCHAR(MAX)) - JSON array of file management configurations
  - **AppLoadConfigs** (NVARCHAR(MAX)) - JSON array of application load configurations
  - **AppWriteConfigs** (NVARCHAR(MAX)) - JSON array of application write configurations
- Insert sample data with 2 empty configurations

### Option 2: Using T-SQL Command Prompt

```sql
sqlcmd -S localhost -E -i DATABASE_SETUP.sql
```

### Option 3: Using Visual Studio

1. Open Server Explorer (View → Server Explorer)
2. Right-click on Data Connections → Add Connection
3. Server name: `localhost`
4. Authentication: `Windows Authentication`
5. Connect to database: Select or create `ConfigTest`
6. Once connected, right-click on the connection and select "New Query"
7. Copy and paste the contents of `DATABASE_SETUP.sql`
8. Execute the query

## Connection String

The application uses the following connection string:
```
Server=localhost;Database=ConfigTest;Integrated Security=true;TrustServerCertificate=true;
```

- **Server**: localhost (or your SQL Server instance name)
- **Database**: ConfigTest (the database name you created)
- **Integrated Security**: true (uses Windows Authentication)
- **TrustServerCertificate**: true (allows connections without certificate validation)

## Modifying the Connection String

If your SQL Server is on a different machine or uses a different instance name, edit the `_connectionString` in `ConfigDatabaseService.cs`:

```csharp
private readonly string _connectionString = "Server=YOUR_SERVER;Database=ConfigTest;Integrated Security=true;TrustServerCertificate=true;";
```

## Verifying the Setup

After running the setup script, verify the table was created:

```sql
USE ConfigTest;
SELECT * FROM config;
```

You should see 2 rows with AppSection 1 and 2, and empty JSON arrays for the config columns.

## Application Usage

1. **Load Configs**: On startup, the application automatically loads all configs from the database
2. **Edit Configs**: Modify configurations in the UI
3. **Save Changes**: Click the green "Save to Database" button to persist changes to the database
4. **Add/Remove Items**: Use the "Add Item" and "Remove Item" buttons to manage configurations and collections

## Troubleshooting

### Error: "Connection failed"
- Ensure SQL Server is running
- Check the server name in the connection string
- Verify Windows Authentication is enabled
- Run `sqlcmd -S localhost -E` to test connectivity

### Error: "Database 'ConfigTest' does not exist"
- Run the DATABASE_SETUP.sql script
- Check the database name in the connection string

### Error: "Login failed for user"
- Verify you're using Windows Authentication
- Ensure your Windows account has SQL Server login rights

### Error: "No data found on startup"
- The application will load sample data if the database is empty or unavailable
- Check the Application log for database connection errors

## JSON Format

The configurations are stored as JSON arrays in the database. Example:

```json
[
  {
    "AppName": "DatabaseApp1",
    "AppVersion": "1.0",
    "LoadNumber": 0,
    "SaveNumber": 0,
    "UpdateNumber": 0,
    "LoadTime": "2024-01-01T00:00:00",
    "SaveTime": "2024-01-01T00:00:00",
    "FileAction": 0,
    "FileName": "data.csv",
    "FilePath": "C:\\Data\\"
  }
]
```

All modifications are automatically serialized to JSON when saving to the database.
