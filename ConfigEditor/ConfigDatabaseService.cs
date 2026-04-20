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

                                // Debug output
                                if (fileMgmtConfigs.Count > 0)
                                {
                                    var first = fileMgmtConfigs[0];
                                    System.Diagnostics.Debug.WriteLine($"DEBUG: Loaded FileMgmtConfig - Type: {first.GetType().Name}, FileName: {first.FileName}");
                                    if (first is CSVFileMgmtConfig csv)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  CodeColumnName: {csv.CodeColumnName}, NameColumnName: {csv.NameColumnName}");
                                    }
                                }

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
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Saving FileMgmtConfigs JSON: {fileMgmtJson}");

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
                        PropertyNameCaseInsensitive = false,  // Important: preserve property names as they are in JSON
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase  // Use camelCase for consistency
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
