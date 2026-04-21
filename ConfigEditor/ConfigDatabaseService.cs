using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ConfigEditor
{
    public class ConfigDatabaseService
    {
        private readonly string _connectionString;

        public ConfigDatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConfigDatabase") 
                ?? throw new InvalidOperationException("ConnectionString 'ConfigDatabase' not found in configuration.");
        }

        public ConfigDatabaseService() : this(CreateDefaultConfiguration())
        {
        }

        private static IConfiguration CreateDefaultConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        /// <summary>
        /// Load all config items from the database.
        /// Returns InitialConfig first, followed by all other config items.
        /// </summary>
        public async Task<List<BaseConfig>> LoadConfigItemsAsync()
        {
            var configs = new List<BaseConfig>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Query to get all config items, with InitialConfig first
                    string query = @"
                        SELECT AppSettingId, AppName, ClassName, Settings 
                        FROM config_items 
                        ORDER BY CASE WHEN ClassName = 'InitialConfig' THEN 0 ELSE 1 END, AppSettingId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int id = reader.GetInt32(0);
                                string appName = reader.GetString(1);
                                string className = reader.GetString(2);
                                string settingsJson = reader.GetString(3);

                                // Deserialize based on class name
                                BaseConfig config = DeserializeConfigItem(className, settingsJson);
                                if (config != null)
                                {
                                    configs.Add(config);
                                }
                            }
                        }
                    }
                }

                // Ensure InitialConfig exists
                if (!configs.OfType<InitialConfig>().Any())
                {
                    configs.Insert(0, new InitialConfig());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading configs from database: {ex.Message}", ex);
            }

            return configs;
        }

        /// <summary>
        /// Save a single config item to the database.
        /// If AppSettingId is 0, a new item is inserted; otherwise, existing item is updated.
        /// InitialConfig cannot be deleted.
        /// </summary>
        public async Task<int> SaveConfigItemAsync(BaseConfig config)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string settingsJson = SerializeConfigItem(config);
                    string className = config.GetType().Name;

                    string query = @"
                        IF EXISTS (SELECT 1 FROM config_items WHERE ClassName = @ClassName AND AppName = @AppName)
                            UPDATE config_items 
                            SET Settings = @Settings, ModifiedDate = GETUTCDATE()
                            WHERE ClassName = @ClassName AND AppName = @AppName
                        ELSE
                            INSERT INTO config_items (AppName, ClassName, Settings) 
                            VALUES (@AppName, @ClassName, @Settings)

                        SELECT SCOPE_IDENTITY()";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AppName", config.AppName);
                        command.Parameters.AddWithValue("@ClassName", className);
                        command.Parameters.AddWithValue("@Settings", settingsJson);

                        var result = await command.ExecuteScalarAsync();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving config item to database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete a config item from the database.
        /// InitialConfig cannot be deleted.
        /// </summary>
        public async Task DeleteConfigItemAsync(BaseConfig config)
        {
            // Prevent deletion of InitialConfig
            if (config is InitialConfig)
                throw new InvalidOperationException("InitialConfig cannot be deleted.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "DELETE FROM config_items WHERE AppName = @AppName AND ClassName = @ClassName";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AppName", config.AppName);
                        command.Parameters.AddWithValue("@ClassName", config.GetType().Name);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting config item from database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deserialize a config item from JSON based on its class name.
        /// </summary>
        private BaseConfig DeserializeConfigItem(string className, string settingsJson)
        {
            try
            {
                var type = Type.GetType($"ConfigEditor.{className}");
                if (type == null || !typeof(BaseConfig).IsAssignableFrom(type))
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Could not resolve type '{className}'");
                    return null;
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize(settingsJson, type, options) as BaseConfig;
                return config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error deserializing config item of type '{className}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Serialize a config item to JSON.
        /// </summary>
        private string SerializeConfigItem(BaseConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return JsonSerializer.Serialize(config, config.GetType(), options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Error serializing config item: {ex.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// For backward compatibility: Load configs in the old GeneralConfig format.
        /// This method will be deprecated after full UI refactoring.
        /// </summary>
        [Obsolete("Use LoadConfigItemsAsync instead. This method will be removed in a future version.")]
        public async Task<List<GeneralConfig>> LoadConfigsAsync()
        {
            var configs = new List<GeneralConfig>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT AppSection, CustomerName, DatabaseMgmtConfigs, FileMgmtConfigs, AppLoadConfigs, AppWriteConfigs FROM config ORDER BY AppSection";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var dbMgmtConfigs = DeserializeList<DatabaseMgmtConfig>(reader.GetString(2));
                                var fileMgmtConfigs = DeserializeList<FileMgmtConfig>(reader.GetString(3));

                                var config = new GeneralConfig
                                {
                                    AppSection = reader.GetInt32(0),
                                    CustomerName = reader.GetString(1),
                                    DatabaseMgmtConfigs = dbMgmtConfigs,
                                    FileMgmtConfigs = fileMgmtConfigs,
                                    AppLoadConfigs = DeserializeList<AppLoadConfig>(reader.GetString(4)),
                                    AppWriteConfigs = DeserializeList<AppWriteConfig>(reader.GetString(5))
                                };

                                configs.Add(config);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading configs from database: {ex.Message}", ex);
            }

            return configs;
        }

        [Obsolete("Use SaveConfigItemAsync instead. This method will be removed in a future version.")]
        public async Task SaveConfigsAsync(List<GeneralConfig> configs)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Delete all existing configs
                    string deleteQuery = "DELETE FROM config";
                    using (var deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        await deleteCommand.ExecuteNonQueryAsync();
                    }

                    // Insert all configs
                    foreach (var config in configs)
                    {
                        var fileMgmtJson = SerializeList(config.FileMgmtConfigs);

                        string insertQuery = @"INSERT INTO config (AppSection, CustomerName, DatabaseMgmtConfigs, FileMgmtConfigs, AppLoadConfigs, AppWriteConfigs)
                                             VALUES (@AppSection, @CustomerName, @DatabaseMgmtConfigs, @FileMgmtConfigs, @AppLoadConfigs, @AppWriteConfigs)";

                        using (var insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@AppSection", config.AppSection);
                            insertCommand.Parameters.AddWithValue("@CustomerName", config.CustomerName);
                            insertCommand.Parameters.AddWithValue("@DatabaseMgmtConfigs", SerializeList(config.DatabaseMgmtConfigs));
                            insertCommand.Parameters.AddWithValue("@FileMgmtConfigs", fileMgmtJson);
                            insertCommand.Parameters.AddWithValue("@AppLoadConfigs", SerializeList(config.AppLoadConfigs));
                            insertCommand.Parameters.AddWithValue("@AppWriteConfigs", SerializeList(config.AppWriteConfigs));

                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving configs to database: {ex.Message}", ex);
            }
        }

        public static List<T> DeserializeList<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return new List<T>();

            try
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG DeserializeList: Input JSON = {json}");

                var jsonElements = JsonSerializer.Deserialize<List<JsonElement>>(json);
                if (jsonElements == null || jsonElements.Count == 0)
                    return new List<T>();

                var result = new List<T>();

                foreach (var element in jsonElements)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Processing element: {element.GetRawText()}");

                    // Try to get the $type field to determine actual type
                    string typeName = null;
                    if (element.TryGetProperty("$type", out var typeElement))
                    {
                        typeName = typeElement.GetString();
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Found $type = {typeName}");
                    }

                    T obj = null;

                    if (!string.IsNullOrEmpty(typeName))
                    {
                        // Find the type in the same namespace
                        var assemblyType = Type.GetType($"ConfigEditor.{typeName}");
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Assembly type lookup for 'ConfigEditor.{typeName}' = {(assemblyType != null ? "Found" : "Not Found")}");

                        if (assemblyType != null && typeof(T).IsAssignableFrom(assemblyType))
                        {
                            // Deserialize as the derived type
                            System.Diagnostics.Debug.WriteLine($"DEBUG: Deserializing as derived type {assemblyType.Name}");
                            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                            obj = (T)JsonSerializer.Deserialize(element.GetRawText(), assemblyType, options);
                        }
                    }

                    // If no derived type found or failed, deserialize as base type
                    if (obj == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Deserializing as base type {typeof(T).Name}");
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        obj = JsonSerializer.Deserialize<T>(element.GetRawText(), options);
                    }

                    if (obj != null)
                        result.Add(obj);
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG DeserializeList Exception: {ex.Message}\n{ex.StackTrace}");
                return new List<T>();
            }
        }

        public static string SerializeList<T>(List<T> list) where T : class
        {
            if (list == null || list.Count == 0)
                return "[]";

            try
            {
                var jsonArray = new StringBuilder();
                jsonArray.Append("[");

                bool first = true;
                foreach (var item in list)
                {
                    if (item == null)
                        continue;

                    if (!first)
                        jsonArray.Append(",");
                    first = false;

                    // Serialize item to JSON string with ALL properties included
                    var options = new JsonSerializerOptions 
                    { 
                        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                        WriteIndented = false,
                        PropertyNameCaseInsensitive = false,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    // Use the actual runtime type to ensure all properties (including derived type properties) are serialized
                    var json = JsonSerializer.Serialize(item, item.GetType(), options);

                    System.Diagnostics.Debug.WriteLine($"DEBUG SerializeList: Serialized {item.GetType().Name} to JSON: {json}");

                    // Parse the JSON and inject $type field
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;

                        // Start building object with properties
                        jsonArray.Append("{");

                        // Add $type first
                        jsonArray.Append($"\"$type\":\"{item.GetType().Name}\"");

                        // Add all properties
                        foreach (var prop in root.EnumerateObject())
                        {
                            jsonArray.Append(",");
                            jsonArray.Append($"\"{prop.Name}\":");
                            jsonArray.Append(prop.Value.GetRawText());
                        }

                        jsonArray.Append("}");
                    }
                }

                jsonArray.Append("]");
                var result = jsonArray.ToString();
                System.Diagnostics.Debug.WriteLine($"DEBUG SerializeList: Final JSON array: {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG SerializeList Exception: {ex.Message}\n{ex.StackTrace}");
                return "[]";
            }
        }
    }
}
