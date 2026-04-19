using System;
using System.Reflection;

namespace ConfigEditor
{
    public partial class Form1 : Form
    {
        private List<GeneralConfig> _generalConfigs;
        private object _selectedObject;
        private Dictionary<string, Type> _fileMgmtConfigTypes;
        private ConfigDatabaseService _databaseService;
        private const string GeneralConfigNodeTag = "GeneralConfig";
        private const string CollectionNodeTag = "Collection";
        private const string ItemNodeTag = "Item";

        public Form1()
        {
            InitializeComponent();
            _databaseService = new ConfigDatabaseService();
            InitializeFileMgmtConfigTypes();
            LoadConfigsFromDatabaseAsync();
        }

        private void InitializeFileMgmtConfigTypes()
        {
            _fileMgmtConfigTypes = new Dictionary<string, Type>();
            var assembly = Assembly.GetExecutingAssembly();
            var fileMgmtConfigBaseType = typeof(FileMgmtConfig);

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && fileMgmtConfigBaseType.IsAssignableFrom(type))
                {
                    _fileMgmtConfigTypes[type.Name] = type;
                }
            }
        }

        private async void LoadConfigsFromDatabaseAsync()
        {
            try
            {
                _generalConfigs = await _databaseService.LoadConfigsAsync();

                if (_generalConfigs == null || _generalConfigs.Count == 0)
                {
                    LoadSampleData();
                }

                BuildTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configs from database:\n{ex.Message}\n\nLoading sample data instead.", "Database Error");
                LoadSampleData();
                BuildTreeView();
            }
        }

        private void LoadSampleData()
        {
            _generalConfigs = new List<GeneralConfig>
            {
                new GeneralConfig
                {
                    AppSection = 1,
                    CustomerName = "Sample Customer 1",
                    DatabaseMgmtConfigs = new List<DatabaseMgmtConfig>
                    {
                        new DatabaseMgmtConfig { AppName = "DB App 1", AppVersion = "1.0" }
                    },
                    FileMgmtConfigs = new List<FileMgmtConfig>
                    {
                        new TextFileMgmtConfig
                        {
                            AppName = "Text File 1",
                            AppVersion = "1.0",
                            FileAction = FileMgmtAction.Load,
                            LoadTime = DateTime.Now,
                            CodeStart = 0,
                            CodeEnd = 15,
                            NameLength = 50
                        }
                    },
                    AppLoadConfigs = new List<AppLoadConfig>(),
                    AppWriteConfigs = new List<AppWriteConfig>()
                },
                new GeneralConfig
                {
                    AppSection = 2,
                    CustomerName = "Sample Customer 2",
                    DatabaseMgmtConfigs = new List<DatabaseMgmtConfig>(),
                    FileMgmtConfigs = new List<FileMgmtConfig>(),
                    AppLoadConfigs = new List<AppLoadConfig>(),
                    AppWriteConfigs = new List<AppWriteConfig>()
                }
            };
        }

        private void BuildTreeView()
        {
            treeViewConfigs.Nodes.Clear();

            // Add all GeneralConfig items to the tree
            for (int i = 0; i < _generalConfigs.Count; i++)
            {
                var config = _generalConfigs[i];
                var configNode = new TreeNode($"GeneralConfig_{i + 1} - {config.CustomerName}")
                {
                    Tag = new NodeData(GeneralConfigNodeTag, config, typeof(GeneralConfig))
                };

                // Add DatabaseMgmtConfigs collection
                AddCollectionNode(configNode, "DatabaseMgmtConfigs", config.DatabaseMgmtConfigs, typeof(DatabaseMgmtConfig));

                // Add FileMgmtConfigs collection
                AddCollectionNode(configNode, "FileMgmtConfigs", config.FileMgmtConfigs, typeof(FileMgmtConfig));

                    // Add AppLoadConfigs collection
                    AddCollectionNode(configNode, "AppLoadConfigs", config.AppLoadConfigs, typeof(AppLoadConfig));

                    // Add AppWriteConfigs collection
                    AddCollectionNode(configNode, "AppWriteConfigs", config.AppWriteConfigs, typeof(AppWriteConfig));

                    treeViewConfigs.Nodes.Add(configNode);
                    configNode.Expand();
                }
            }

        private void AddCollectionNode(TreeNode parentNode, string collectionName, System.Collections.IList collection, Type itemType)
        {
            var collectionNode = new TreeNode($"{collectionName} ({collection.Count})")
            {
                Tag = new NodeData(CollectionNodeTag, collection, itemType)
            };

            if (collection.Count > 0)
            {
                foreach (var item in collection)
                {
                    var itemNode = new TreeNode(GetNodeDisplayName(item))
                    {
                        Tag = new NodeData(ItemNodeTag, item, itemType)
                    };
                    collectionNode.Nodes.Add(itemNode);
                }
            }

            parentNode.Nodes.Add(collectionNode);
        }

        private string GetNodeDisplayName(object obj)
        {
            if (obj is BaseConfig bc)
            {
                return $"{obj.GetType().Name} - {bc.AppName}";
            }
            return obj.GetType().Name;
        }

        private void TreeViewConfigs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is not NodeData nodeData)
                return;

            _selectedObject = nodeData.Data;

            // Display properties for GeneralConfig items and collection items, but not collection nodes
            if (nodeData.NodeType == ItemNodeTag || (nodeData.NodeType == GeneralConfigNodeTag && nodeData.Data is GeneralConfig))
            {
                DisplayProperties(nodeData.Data);
            }
            else
            {
                panelPropertiesContainer.Controls.Clear();
            }
        }

        private void TreeViewConfigs_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is NodeData nodeData && nodeData.NodeType == ItemNodeTag)
            {
                // Item nodes are displayed in properties panel
                DisplayProperties(nodeData.Data);
            }
        }

        private void DisplayProperties(object obj)
        {
            panelPropertiesContainer.Controls.Clear();

            if (obj == null)
                return;

            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0)
                .Where(p => !(p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                .ToList();

            int yPosition = 10;

            foreach (var prop in properties)
            {
                try
                {
                    var label = new Label
                    {
                        Text = prop.Name,
                        Location = new Point(10, yPosition),
                        AutoSize = true,
                        Font = new Font(Font, FontStyle.Bold)
                    };
                    panelPropertiesContainer.Controls.Add(label);
                    yPosition += 25;

                    Control editor = CreatePropertyEditor(obj, prop);
                    editor.Location = new Point(10, yPosition);
                    editor.Width = 400;
                    panelPropertiesContainer.Controls.Add(editor);
                    yPosition += 35;
                }
                catch (Exception ex)
                {
                    // Skip properties that cause errors
                }
            }
        }

        private Control CreatePropertyEditor(object obj, PropertyInfo prop)
        {
            var propType = prop.PropertyType;
            var currentValue = prop.GetValue(obj);

            if (propType.IsEnum)
            {
                var comboBox = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Height = 25
                };

                foreach (var enumValue in Enum.GetValues(propType))
                {
                    comboBox.Items.Add(enumValue);
                }

                comboBox.SelectedItem = currentValue;
                comboBox.SelectedIndexChanged += (s, e) =>
                {
                    prop.SetValue(obj, comboBox.SelectedItem);
                    RefreshTreeView();
                };

                return comboBox;
            }

            if (propType == typeof(DateTime))
            {
                var dateTimePicker = new DateTimePicker
                {
                    Height = 25,
                    Value = (DateTime)(currentValue ?? DateTime.Now)
                };

                dateTimePicker.ValueChanged += (s, e) =>
                {
                    prop.SetValue(obj, dateTimePicker.Value);
                };

                return dateTimePicker;
            }

            if (propType == typeof(int))
            {
                var numericUpDown = new NumericUpDown
                {
                    Height = 25,
                    Value = (int)(currentValue ?? 0)
                };

                numericUpDown.ValueChanged += (s, e) =>
                {
                    prop.SetValue(obj, (int)numericUpDown.Value);
                };

                return numericUpDown;
            }

            if (propType == typeof(string))
            {
                var textBox = new TextBox
                {
                    Height = 25,
                    Text = (string)(currentValue ?? "")
                };

                textBox.TextChanged += (s, e) =>
                {
                    prop.SetValue(obj, textBox.Text);
                };

                return textBox;
            }

            // Default: TextBox for unknown types
            return new TextBox
            {
                Height = 25,
                Text = currentValue?.ToString() ?? ""
            };
        }

        private void BtnAddItem_Click(object sender, EventArgs e)
        {
            if (treeViewConfigs.SelectedNode == null)
            {
                MessageBox.Show("Please select an item or collection to add to.");
                return;
            }

            var selectedNode = treeViewConfigs.SelectedNode;
            if (selectedNode.Tag is not NodeData nodeData)
                return;

            // If a GeneralConfig is selected (top-level node with no parent)
            if (selectedNode.Parent == null && nodeData.NodeType == GeneralConfigNodeTag)
            {
                AddNewGeneralConfig();
                return;
            }

            // If adding to a collection
            if (nodeData.NodeType == CollectionNodeTag)
            {
                var collection = nodeData.Data as System.Collections.IList;
                if (collection == null)
                    return;

                // For FileMgmtConfigs, show dialog to choose subtype
                if (nodeData.ItemType == typeof(FileMgmtConfig))
                {
                    ShowFileMgmtConfigTypeSelection(selectedNode, collection);
                }
                else
                {
                    AddItemToCollection(selectedNode, collection, nodeData.ItemType);
                }
            }
            else
            {
                MessageBox.Show("Please select a collection or GeneralConfig to add items to.");
            }
        }

        private void AddNewGeneralConfig()
        {
            var newConfig = new GeneralConfig
            {
                AppSection = _generalConfigs.Count + 1,
                CustomerName = $"Customer_{_generalConfigs.Count + 1}",
                DatabaseMgmtConfigs = new List<DatabaseMgmtConfig>(),
                FileMgmtConfigs = new List<FileMgmtConfig>(),
                AppLoadConfigs = new List<AppLoadConfig>(),
                AppWriteConfigs = new List<AppWriteConfig>()
            };

            _generalConfigs.Add(newConfig);
            BuildTreeView();
        }

        private void ShowFileMgmtConfigTypeSelection(TreeNode parentNode, System.Collections.IList collection)
        {
            var form = new Form
            {
                Text = "Select FileMgmtConfig Type",
                Width = 300,
                Height = 200,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 120
            };

            foreach (var typeName in _fileMgmtConfigTypes.Keys.OrderBy(x => x))
            {
                listBox.Items.Add(typeName);
            }

            var btnOK = new Button
            {
                Text = "OK",
                Dock = DockStyle.Bottom,
                Height = 30,
                DialogResult = DialogResult.OK
            };

            btnOK.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    var selectedTypeName = listBox.SelectedItem.ToString();
                    if (_fileMgmtConfigTypes.TryGetValue(selectedTypeName, out var selectedType))
                    {
                        AddItemToCollection(parentNode, collection, selectedType);
                    }
                }
                form.Close();
            };

            form.Controls.Add(btnOK);
            form.Controls.Add(listBox);
            form.ShowDialog(this);
        }

        private void AddItemToCollection(TreeNode parentNode, System.Collections.IList collection, Type itemType)
        {
            var newItem = Activator.CreateInstance(itemType);
            if (newItem is BaseConfig bc)
            {
                bc.AppName = $"{itemType.Name}_{collection.Count + 1}";
            }

            collection.Add(newItem);

            var itemNode = new TreeNode(GetNodeDisplayName(newItem))
            {
                Tag = new NodeData(ItemNodeTag, newItem, itemType)
            };

            parentNode.Nodes.Add(itemNode);
            parentNode.Text = $"{parentNode.Text.Split(' ')[0]} ({collection.Count})";
            treeViewConfigs.SelectedNode = parentNode;
            parentNode.EnsureVisible();
            treeViewConfigs.Focus();
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (treeViewConfigs.SelectedNode == null)
            {
                MessageBox.Show("Please select an item to remove.");
                return;
            }

            var selectedNode = treeViewConfigs.SelectedNode;
            if (selectedNode.Tag is not NodeData nodeData)
                return;

            // If removing a GeneralConfig item (top-level node with no parent)
            if (selectedNode.Parent == null && nodeData.NodeType == GeneralConfigNodeTag)
            {
                var config = nodeData.Data as GeneralConfig;
                if (config != null && _generalConfigs.Contains(config))
                {
                    _generalConfigs.Remove(config);
                    BuildTreeView();
                    panelPropertiesContainer.Controls.Clear();
                    return;
                }
            }

            // If removing a collection item
            if (nodeData.NodeType == ItemNodeTag)
            {
                var parentNode = selectedNode.Parent;
                if (parentNode?.Tag is not NodeData parentNodeData || parentNodeData.NodeType != CollectionNodeTag)
                    return;

                var collection = parentNodeData.Data as System.Collections.IList;
                if (collection == null)
                    return;

                collection.Remove(nodeData.Data);
                selectedNode.Remove();
                parentNode.Text = $"{parentNode.Text.Split(' ')[0]} ({collection.Count})";

                panelPropertiesContainer.Controls.Clear();
                RefreshTreeView();
                return;
            }

            MessageBox.Show("Please select an item to remove.");
        }

        private void RefreshTreeView()
        {
            var selectedNode = treeViewConfigs.SelectedNode;
            var selectedData = selectedNode?.Tag;

            // Refresh the count in parent collection nodes
            if (selectedNode?.Parent?.Tag is NodeData parentNodeData && parentNodeData.NodeType == CollectionNodeTag)
            {
                var collection = parentNodeData.Data as System.Collections.IList;
                if (collection != null)
                {
                    var collectionName = selectedNode.Parent.Text.Split(' ')[0];
                    selectedNode.Parent.Text = $"{collectionName} ({collection.Count})";
                }
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                btnSave.Enabled = false;
                btnSave.Text = "Saving...";

                // Debug: Check if FileMgmtConfigs have properties set
                if (_generalConfigs.Count > 0 && _generalConfigs[0].FileMgmtConfigs.Count > 0)
                {
                    var firstConfig = _generalConfigs[0].FileMgmtConfigs[0];
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Before Save - Type: {firstConfig.GetType().Name}, FileName: {firstConfig.FileName}");
                    if (firstConfig is CSVFileMgmtConfig csv)
                    {
                        System.Diagnostics.Debug.WriteLine($"  CodeColumnName: {csv.CodeColumnName}, NameColumnName: {csv.NameColumnName}");
                    }
                }

                await _databaseService.SaveConfigsAsync(_generalConfigs);

                MessageBox.Show("Configurations saved successfully to the database!", "Success");
                btnSave.Text = "Save to Database";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configurations to database:\n{ex.Message}", "Error");
                btnSave.Text = "Save to Database";
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }
    }
}
