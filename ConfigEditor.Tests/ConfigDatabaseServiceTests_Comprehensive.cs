using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ConfigEditor.Tests
{
    [TestClass]
    public class SerializeListFileMgmtConfigTests
    {
        [TestMethod]
        public void SerializeList_WithEmptyList_ReturnsEmptyJsonArray()
        {
            var list = new List<FileMgmtConfig>();
            var result = ConfigDatabaseService.SerializeList(list);
            Assert.AreEqual("[]", result);
        }

        [TestMethod]
        public void SerializeList_WithNullList_ReturnsEmptyJsonArray()
        {
            List<FileMgmtConfig> list = null;
            var result = ConfigDatabaseService.SerializeList(list);
            Assert.AreEqual("[]", result);
        }

        [TestMethod]
        public void SerializeList_WithSingleFileMgmtConfig_SerializesCorrectly()
        {
            var config = new TextFileMgmtConfig
            {
                AppName = "TestApp",
                CodeStart = 35,
                CodeEnd = 77,
                NameLength = 85,
                Name = "TestConfig",
                AppVersion = "1.0",
                LoadNumber = 5,
                SaveNumber = 3,
                UpdateNumber = 2,
                LoadTime = new DateTime(2024, 1, 15, 10, 30, 0),
                SaveTime = new DateTime(2024, 1, 15, 11, 0, 0),
                FileAction = FileMgmtAction.Load,
                FileName = "test.csv",
                FilePath = "C:\\data\\test.csv"
            };
            var list = new List<FileMgmtConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.StartsWith("["));
            Assert.IsTrue(result.EndsWith("]"));
            Assert.IsTrue(result.Contains("\"$type\":\"TextFileMgmtConfig\""));
            Assert.IsTrue(result.Contains("77"));
            Assert.IsTrue(result.Contains("85"));
            Assert.IsTrue(result.Contains("35"));
        }

        [TestMethod]
        public void SerializeList_WithMultipleFileMgmtConfigs_SerializesAllItems()
        {
            var config1 = new FileMgmtConfig
            {
                AppName = "TestApp1",
                AppVersion = "1.0",
                FileName = "file1.csv",
                FilePath = "C:\\data\\file1.csv"
            };
            var config2 = new FileMgmtConfig
            {
                AppName = "TestApp2",
                AppVersion = "2.0",
                FileName = "file2.csv",
                FilePath = "C:\\data\\file2.csv"
            };
            var list = new List<FileMgmtConfig> { config1, config2 };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("\"$type\":\"FileMgmtConfig\""));
            var openBraces = System.Text.RegularExpressions.Regex.Matches(result, "{").Count;
            var closeBraces = System.Text.RegularExpressions.Regex.Matches(result, "}").Count;
            Assert.AreEqual(2, openBraces);
            Assert.AreEqual(2, closeBraces);
        }

        [TestMethod]
        public void SerializeList_WithListContainingNulls_SkipsNullItems()
        {
            var config = new FileMgmtConfig
            {
                AppName = "TestApp",
                FileName = "test.csv",
                FilePath = "C:\\data\\test.csv"
            };
            var list = new List<FileMgmtConfig> { null, config, null };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            var openBraces = System.Text.RegularExpressions.Regex.Matches(result, "{").Count;
            Assert.AreEqual(1, openBraces);
        }

        [TestMethod]
        public void SerializeList_ProducesValidJsonArray()
        {
            var config = new FileMgmtConfig
            {
                AppName = "TestApp",
                AppVersion = "1.0",
                LoadNumber = 1,
                FileName = "test.csv",
                FilePath = "C:\\test.csv"
            };
            var list = new List<FileMgmtConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsNotNull(result);
            try
            {
                var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(result);
                Assert.IsNotNull(jsonArray);
                Assert.AreEqual(1, jsonArray.Length);
            }
            catch (JsonException ex)
            {
                Assert.Fail($"Serialized output is not valid JSON: {ex.Message}. Output: {result}");
            }
        }

        [TestMethod]
        public void SerializeList_IncludesAllProperties()
        {
            var config = new FileMgmtConfig
            {
                AppName = "TestApp",
                AppVersion = "1.0",
                LoadNumber = 5,
                SaveNumber = 3,
                UpdateNumber = 2,
                FileAction = FileMgmtAction.Save,
                FileName = "data.csv",
                FilePath = "C:\\files\\data.csv"
            };
            var list = new List<FileMgmtConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsTrue(result.Contains("\"appName\"") || result.Contains("\"AppName\""));
            Assert.IsTrue(result.Contains("\"appVersion\"") || result.Contains("\"AppVersion\""));
            Assert.IsTrue(result.Contains("\"loadNumber\"") || result.Contains("\"LoadNumber\""));
            Assert.IsTrue(result.Contains("\"saveNumber\"") || result.Contains("\"SaveNumber\""));
            Assert.IsTrue(result.Contains("\"updateNumber\"") || result.Contains("\"UpdateNumber\""));
            Assert.IsTrue(result.Contains("\"fileName\"") || result.Contains("\"FileName\""));
            Assert.IsTrue(result.Contains("\"filePath\"") || result.Contains("\"FilePath\""));
        }
    }

    [TestClass]
    public class SerializeListCSVFileMgmtConfigTests
    {
        [TestMethod]
        public void SerializeList_WithCSVFileMgmtConfig_SerializesCorrectly()
        {
            var config = new CSVFileMgmtConfig
            {
                AppName = "CSVApp",
                AppVersion = "1.0",
                FileName = "data.csv",
                FilePath = "C:\\data\\data.csv",
                RowNumber = 10,
                CodeColumnName = "Code",
                NameColumnName = "Name",
                DescriptionColumnName = "Description"
            };
            var list = new List<CSVFileMgmtConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("\"$type\":\"CSVFileMgmtConfig\""));
            Assert.IsTrue(result.Contains("\"rowNumber\""));
            Assert.IsTrue(result.Contains("\"codeColumnName\""));
            Assert.IsTrue(result.Contains("\"nameColumnName\""));
            Assert.IsTrue(result.Contains("\"descriptionColumnName\""));
        }

        [TestMethod]
        public void SerializeList_WithMultipleCSVConfigs_SerializesAll()
        {
            var config1 = new CSVFileMgmtConfig
            {
                AppName = "App1",
                FileName = "file1.csv",
                FilePath = "C:\\file1.csv",
                CodeColumnName = "Code"
            };
            var config2 = new CSVFileMgmtConfig
            {
                AppName = "App2",
                FileName = "file2.csv",
                FilePath = "C:\\file2.csv",
                CodeColumnName = "ID"
            };
            var list = new List<CSVFileMgmtConfig> { config1, config2 };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsTrue(result.Contains("\"$type\":\"CSVFileMgmtConfig\""));
            var openBraces = System.Text.RegularExpressions.Regex.Matches(result, "{").Count;
            Assert.AreEqual(2, openBraces);
        }
    }

    [TestClass]
    public class SerializeListTextFileMgmtConfigTests
    {
        [TestMethod]
        public void SerializeList_WithTextFileMgmtConfig_SerializesCorrectly()
        {
            var config = new TextFileMgmtConfig
            {
                AppName = "TextApp",
                AppVersion = "1.0",
                FileName = "data.txt",
                FilePath = "C:\\data\\data.txt",
                CodeStart = 0,
                CodeEnd = 10,
                NameLength = 50,
                Name = "TextConfig",
                DescriptionLength = 100
            };
            var list = new List<TextFileMgmtConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("\"$type\":\"TextFileMgmtConfig\""));
            Assert.IsTrue(result.Contains("\"codeStart\""));
            Assert.IsTrue(result.Contains("\"codeEnd\""));
            Assert.IsTrue(result.Contains("\"nameLength\""));
            Assert.IsTrue(result.Contains("\"name\""));
        }
    }

    [TestClass]
    public class SerializeListDatabaseMgmtConfigTests
    {
        [TestMethod]
        public void SerializeList_WithDatabaseMgmtConfig_SerializesCorrectly()
        {
            var config = new DatabaseMgmtConfig
            {
                AppName = "DatabaseApp",
                AppVersion = "1.0"
            };
            var list = new List<DatabaseMgmtConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("\"$type\":\"DatabaseMgmtConfig\""));
            Assert.IsTrue(result.Contains("\"appName\""));
            Assert.IsTrue(result.Contains("\"appVersion\""));
        }

        [TestMethod]
        public void SerializeList_WithEmptyDatabaseMgmtConfig_SerializesCorrectly()
        {
            var config = new DatabaseMgmtConfig();
            var list = new List<DatabaseMgmtConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("\"$type\":\"DatabaseMgmtConfig\""));
        }
    }

    [TestClass]
    public class SerializeListAppLoadConfigTests
    {
        [TestMethod]
        public void SerializeList_WithAppLoadConfig_SerializesCorrectly()
        {
            var config = new AppLoadConfig
            {
                AppName = "LoadApp",
                AppVersion = "2.0"
            };
            var list = new List<AppLoadConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("\"$type\":\"AppLoadConfig\""));
        }

        [TestMethod]
        public void SerializeList_WithMultipleAppLoadConfigs_SerializesAll()
        {
            var list = new List<AppLoadConfig>
            {
                new AppLoadConfig { AppName = "App1", AppVersion = "1.0" },
                new AppLoadConfig { AppName = "App2", AppVersion = "2.0" },
                new AppLoadConfig { AppName = "App3", AppVersion = "3.0" }
            };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsTrue(result.Contains("\"$type\":\"AppLoadConfig\""));
            var openBraces = System.Text.RegularExpressions.Regex.Matches(result, "{").Count;
            Assert.AreEqual(3, openBraces);
        }
    }

    [TestClass]
    public class SerializeListAppWriteConfigTests
    {
        [TestMethod]
        public void SerializeList_WithAppWriteConfig_SerializesCorrectly()
        {
            var config = new AppWriteConfig
            {
                AppName = "WriteApp",
                AppVersion = "1.5"
            };
            var list = new List<AppWriteConfig> { config };

            var result = ConfigDatabaseService.SerializeList(list);

            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("\"$type\":\"AppWriteConfig\""));
        }
    }

    [TestClass]
    public class DeserializeListFileMgmtConfigTests
    {
        [TestMethod]
        public void DeserializeList_WithValidJsonString_DeserializesCorrectly()
        {
            var json = "[{\"$type\":\"FileMgmtConfig\",\"appName\":\"TestApp\",\"appVersion\":\"1.0\",\"loadNumber\":5,\"saveNumber\":3,\"updateNumber\":2,\"loadTime\":\"2024-01-15T10:30:00\",\"saveTime\":\"2024-01-15T11:00:00\",\"fileAction\":0,\"fileName\":\"test.csv\",\"filePath\":\"C:\\\\data\\\\test.csv\"}]";

            var result = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("TestApp", result[0].AppName);
            Assert.AreEqual("test.csv", result[0].FileName);
        }

        [TestMethod]
        public void DeserializeList_WithEmptyJsonArray_ReturnsEmptyList()
        {
            var json = "[]";

            var result = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DeserializeList_WithNullString_ReturnsEmptyList()
        {
            var result = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(null);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DeserializeList_WithEmptyString_ReturnsEmptyList()
        {
            var result = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(string.Empty);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }

    [TestClass]
    public class DeserializeListCSVFileMgmtConfigTests
    {
        [TestMethod]
        public void DeserializeList_WithCSVFileMgmtConfigJson_DeserializesAsDerivedType()
        {
            var json = "[{\"$type\":\"CSVFileMgmtConfig\",\"appName\":\"CSVApp\",\"appVersion\":\"1.0\",\"fileName\":\"data.csv\",\"filePath\":\"C:\\\\data\\\\data.csv\",\"rowNumber\":10,\"codeColumnName\":\"Code\",\"nameColumnName\":\"Name\",\"descriptionColumnName\":\"Description\",\"loadNumber\":0,\"saveNumber\":0,\"updateNumber\":0,\"loadTime\":\"1970-01-01T00:00:00\",\"saveTime\":\"1970-01-01T00:00:00\",\"fileAction\":0}]";

            var result = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(CSVFileMgmtConfig));
            var csvConfig = result[0] as CSVFileMgmtConfig;
            Assert.AreEqual("Code", csvConfig.CodeColumnName);
            Assert.AreEqual(10, csvConfig.RowNumber);
        }
    }

    [TestClass]
    public class DeserializeListDatabaseMgmtConfigTests
    {
        [TestMethod]
        public void DeserializeList_WithDatabaseMgmtConfigJson_DeserializesCorrectly()
        {
            var json = "[{\"$type\":\"DatabaseMgmtConfig\",\"appName\":\"DatabaseApp\",\"appVersion\":\"1.0\"}]";

            var result = ConfigDatabaseService.DeserializeList<DatabaseMgmtConfig>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("DatabaseApp", result[0].AppName);
        }
    }

    [TestClass]
    public class DeserializeListAppLoadConfigTests
    {
        [TestMethod]
        public void DeserializeList_WithAppLoadConfigJson_DeserializesCorrectly()
        {
            var json = "[{\"$type\":\"AppLoadConfig\",\"appName\":\"LoadApp\",\"appVersion\":\"2.0\"}]";

            var result = ConfigDatabaseService.DeserializeList<AppLoadConfig>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("LoadApp", result[0].AppName);
        }
    }

    [TestClass]
    public class DeserializeListAppWriteConfigTests
    {
        [TestMethod]
        public void DeserializeList_WithAppWriteConfigJson_DeserializesCorrectly()
        {
            var json = "[{\"$type\":\"AppWriteConfig\",\"appName\":\"WriteApp\",\"appVersion\":\"1.5\"}]";

            var result = ConfigDatabaseService.DeserializeList<AppWriteConfig>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("WriteApp", result[0].AppName);
        }
    }

    [TestClass]
    public class RoundTripSerializationTests
    {
        [TestMethod]
        public void RoundTrip_FileMgmtConfig_SerializeAndDeserialize()
        {
            var originalConfig = new FileMgmtConfig
            {
                AppName = "RoundTripApp",
                AppVersion = "1.0",
                LoadNumber = 5,
                FileName = "roundtrip.csv",
                FilePath = "C:\\roundtrip.csv"
            };
            var originalList = new List<FileMgmtConfig> { originalConfig };

            var serialized = ConfigDatabaseService.SerializeList(originalList);
            var deserialized = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(serialized);

            Assert.AreEqual(1, deserialized.Count);
            Assert.AreEqual("RoundTripApp", deserialized[0].AppName);
            Assert.AreEqual("roundtrip.csv", deserialized[0].FileName);
            Assert.AreEqual(5, deserialized[0].LoadNumber);
        }

        [TestMethod]
        public void RoundTrip_CSVFileMgmtConfig_SerializeAndDeserialize()
        {
            var originalConfig = new CSVFileMgmtConfig
            {
                AppName = "CSVRoundTrip",
                FileName = "csv_roundtrip.csv",
                FilePath = "C:\\csv_roundtrip.csv",
                CodeColumnName = "ProductCode",
                NameColumnName = "ProductName",
                RowNumber = 15
            };
            var originalList = new List<CSVFileMgmtConfig> { originalConfig };

            var serialized = ConfigDatabaseService.SerializeList(originalList);
            var deserialized = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(serialized);

            Assert.AreEqual(1, deserialized.Count);
            Assert.IsInstanceOfType(deserialized[0], typeof(CSVFileMgmtConfig));
            var csvConfig = deserialized[0] as CSVFileMgmtConfig;
            Assert.AreEqual("ProductCode", csvConfig.CodeColumnName);
            Assert.AreEqual(15, csvConfig.RowNumber);
        }

        [TestMethod]
        public void RoundTrip_TextFileMgmtConfig_SerializeAndDeserialize()
        {
            var originalConfig = new TextFileMgmtConfig
            {
                AppName = "TextRoundTrip",
                FileName = "text_roundtrip.txt",
                FilePath = "C:\\text_roundtrip.txt",
                CodeStart = 0,
                CodeEnd = 10,
                NameLength = 50,
                Name = "TextData"
            };
            var originalList = new List<TextFileMgmtConfig> { originalConfig };

            var serialized = ConfigDatabaseService.SerializeList(originalList);
            var deserialized = ConfigDatabaseService.DeserializeList<FileMgmtConfig>(serialized);

            Assert.AreEqual(1, deserialized.Count);
            Assert.IsInstanceOfType(deserialized[0], typeof(TextFileMgmtConfig));
            var textConfig = deserialized[0] as TextFileMgmtConfig;
            Assert.AreEqual("TextData", textConfig.Name);
            Assert.AreEqual(10, textConfig.CodeEnd);
        }
    }
}
