using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ConfigEditor
{
    public partial class Form1 : Form
    {
        private List<BaseConfig> _configItems;
        private object _selectedObject;
        private Dictionary<string, Type> _configTypes;
        private ConfigDatabaseService _databaseService;
        private const string ConfigItemNodeTag = "ConfigItem";

        public Form1()
        {
            InitializeComponent();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            _databaseService = new ConfigDatabaseService(configuration);
            InitializeConfigTypes();
            LoadConfigsFromDatabaseAsync();
        }

        /// <summary>
        /// Initialize all config types that can be added to the list.
        /// Excludes InitialConfig from the list of addable types since it's always present.
        /// </summary>
        private void InitializeConfigTypes()
        {
            _configTypes = new Dictionary<string, Type>();
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                // Include all BaseConfig-derived types except InitialConfig and abstract types
                if (type.IsClass && !type.IsAbstract && typeof(BaseConfig).IsAssignableFrom(type) && type != typeof(InitialConfig))
                {
                    _configTypes[type.Name] = type;
                }
            }
        }

        private async void LoadConfigsFromDatabaseAsync()
        {
            try
            {
                _configItems = await _databaseService.LoadConfigItemsAsync();

                if (_configItems == null || _configItems.Count == 0)
                {
                    _configItems = new List<BaseConfig> { new InitialConfig() };
                }

                BuildListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configs from database:\n{ex.Message}\n\nLoading default config instead.", "Database Error");
                _configItems = new List<BaseConfig> { new InitialConfig() };
                BuildListView();
            }
        }

        private void BuildListView()
        {
            treeViewConfigs.Nodes.Clear();

            // Add all config items to the tree view as a flat list
            for (int i = 0; i < _configItems.Count; i++)
            {
                var config = _configItems[i];
                bool isInitialConfig = config is InitialConfig;
                string displayName = $"{config.GetType().Name} - {config.AppName}";

                if (isInitialConfig)
                {
                    displayName = "📌 " + displayName + " (System)";
                }

                var configNode = new TreeNode(displayName)
                {
                    Tag = new NodeData(ConfigItemNodeTag, config, config.GetType())
                };

                treeViewConfigs.Nodes.Add(configNode);
            }

            // Expand all nodes for better visibility
            treeViewConfigs.ExpandAll();
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

            // Display properties for selected config item
            if (nodeData.NodeType == ConfigItemNodeTag)
            {
                DisplayProperties(nodeData.Data);
            }
            else
            {
                panelPropertiesContainer.Controls.Clear();
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
                    System.Diagnostics.Debug.WriteLine($"Error displaying property {prop.Name}: {ex.Message}");
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
                    Value = (int)(currentValue ?? 0),
                    Minimum = int.MinValue,
                    Maximum = int.MaxValue
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
                    Text = (string)(currentValue ?? ""),
                    Multiline = false
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
            ShowConfigTypeSelectionDialog();
        }

        private void ShowConfigTypeSelectionDialog()
        {
            var form = new Form
            {
                Text = "Add New Configuration Item",
                Width = 350,
                Height = 300,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = "Select a configuration type to add:",
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(10)
            };

            var listBox = new ListBox
            {
                Dock = DockStyle.Top,
                Height = 150
            };

            foreach (var typeName in _configTypes.Keys.OrderBy(x => x))
            {
                listBox.Items.Add(typeName);
            }

            if (listBox.Items.Count > 0)
                listBox.SelectedIndex = 0;

            var btnOK = new Button
            {
                Text = "Add",
                Dock = DockStyle.Bottom,
                Height = 30,
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Dock = DockStyle.Bottom,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };

            btnOK.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    var selectedTypeName = listBox.SelectedItem.ToString();
                    if (_configTypes.TryGetValue(selectedTypeName, out var selectedType))
                    {
                        AddNewConfigItem(selectedType);
                    }
                }
                form.Close();
            };

            form.Controls.Add(btnCancel);
            form.Controls.Add(btnOK);
            form.Controls.Add(listBox);
            form.Controls.Add(label);
            form.ShowDialog(this);
        }

        private void AddNewConfigItem(Type configType)
        {
            try
            {
                var newItem = Activator.CreateInstance(configType) as BaseConfig;
                if (newItem != null)
                {
                    newItem.AppName = $"{configType.Name}_{_configItems.Count}";
                    newItem.AppVersion = "1.0";
                    _configItems.Add(newItem);
                }

                BuildListView();

                // Select the newly added item
                if (treeViewConfigs.Nodes.Count > 0)
                {
                    treeViewConfigs.SelectedNode = treeViewConfigs.Nodes[treeViewConfigs.Nodes.Count - 1];
                }

                MessageBox.Show($"New {configType.Name} added successfully. Configure its properties and save.", "Item Added");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating new config item: {ex.Message}", "Error");
            }
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (treeViewConfigs.SelectedNode == null)
            {
                MessageBox.Show("Please select a configuration item to remove.");
                return;
            }

            var selectedNode = treeViewConfigs.SelectedNode;
            if (selectedNode.Tag is not NodeData nodeData || nodeData.NodeType != ConfigItemNodeTag)
            {
                MessageBox.Show("Please select a configuration item to remove.");
                return;
            }

            var config = nodeData.Data as BaseConfig;
            if (config == null)
                return;

            // Prevent deletion of InitialConfig
            if (config is InitialConfig)
            {
                MessageBox.Show("The InitialConfig item is system-managed and cannot be removed.", "Cannot Delete");
                return;
            }

            // Confirm deletion
            var result = MessageBox.Show(
                $"Are you sure you want to remove '{config.AppName}'?",
                "Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _configItems.Remove(config);
                BuildListView();
                panelPropertiesContainer.Controls.Clear();
                MessageBox.Show("Item removed. Remember to save your changes to the database.", "Item Removed");
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                btnSave.Enabled = false;
                btnSave.Text = "Saving...";

                // Save each config item individually
                foreach (var config in _configItems)
                {
                    await _databaseService.SaveConfigItemAsync(config);
                }

                MessageBox.Show("All configurations saved successfully to the database!", "Success");
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
