namespace ConfigEditor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            panel1 = new Panel();
            treeViewConfigs = new TreeView();
            panelButtons = new Panel();
            btnSave = new Button();
            btnRemoveItem = new Button();
            btnAddItem = new Button();
            panel2 = new Panel();
            labelProperties = new Label();
            panelPropertiesContainer = new Panel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            panelButtons.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(panel2);
            splitContainer1.Size = new Size(875, 562);
            splitContainer1.SplitterDistance = 306;
            splitContainer1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(treeViewConfigs);
            panel1.Controls.Add(panelButtons);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(306, 562);
            panel1.TabIndex = 0;
            // 
            // treeViewConfigs
            // 
            treeViewConfigs.Dock = DockStyle.Fill;
            treeViewConfigs.Location = new Point(0, 0);
            treeViewConfigs.Name = "treeViewConfigs";
            treeViewConfigs.Size = new Size(306, 486);
            treeViewConfigs.TabIndex = 0;
            treeViewConfigs.AfterSelect += TreeViewConfigs_AfterSelect;
            treeViewConfigs.NodeMouseDoubleClick += TreeViewConfigs_NodeMouseDoubleClick;
            // 
            // panelButtons
            // 
            panelButtons.AutoSize = true;
            panelButtons.Controls.Add(btnSave);
            panelButtons.Controls.Add(btnRemoveItem);
            panelButtons.Controls.Add(btnAddItem);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 486);
            panelButtons.Name = "panelButtons";
            panelButtons.Padding = new Padding(4, 5, 4, 5);
            panelButtons.Size = new Size(306, 76);
            panelButtons.TabIndex = 1;
            // 
            // btnSave
            // 
            btnSave.AutoSize = true;
            btnSave.BackColor = Color.LimeGreen;
            btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(9, 40);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(275, 28);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save to Database";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += BtnSave_Click;
            // 
            // btnRemoveItem
            // 
            btnRemoveItem.AutoSize = true;
            btnRemoveItem.Location = new Point(153, 9);
            btnRemoveItem.Name = "btnRemoveItem";
            btnRemoveItem.Size = new Size(131, 28);
            btnRemoveItem.TabIndex = 1;
            btnRemoveItem.Text = "Remove Item";
            btnRemoveItem.UseVisualStyleBackColor = true;
            btnRemoveItem.Click += BtnRemoveItem_Click;
            // 
            // btnAddItem
            // 
            btnAddItem.AutoSize = true;
            btnAddItem.Location = new Point(9, 9);
            btnAddItem.Name = "btnAddItem";
            btnAddItem.Size = new Size(131, 28);
            btnAddItem.TabIndex = 0;
            btnAddItem.Text = "Add Item";
            btnAddItem.UseVisualStyleBackColor = true;
            btnAddItem.Click += BtnAddItem_Click;
            // 
            // panel2
            // 
            panel2.Controls.Add(labelProperties);
            panel2.Controls.Add(panelPropertiesContainer);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Padding = new Padding(9);
            panel2.Size = new Size(565, 562);
            panel2.TabIndex = 1;
            // 
            // labelProperties
            // 
            labelProperties.AutoSize = true;
            labelProperties.Dock = DockStyle.Top;
            labelProperties.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelProperties.Location = new Point(9, 9);
            labelProperties.Name = "labelProperties";
            labelProperties.Padding = new Padding(0, 0, 0, 9);
            labelProperties.Size = new Size(88, 30);
            labelProperties.TabIndex = 0;
            labelProperties.Text = "Properties";
            // 
            // panelPropertiesContainer
            // 
            panelPropertiesContainer.AutoScroll = true;
            panelPropertiesContainer.Dock = DockStyle.Fill;
            panelPropertiesContainer.Location = new Point(9, 9);
            panelPropertiesContainer.Name = "panelPropertiesContainer";
            panelPropertiesContainer.Size = new Size(547, 544);
            panelPropertiesContainer.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(875, 562);
            Controls.Add(splitContainer1);
            Name = "Form1";
            Text = "GeneralConfig Editor";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panelButtons.ResumeLayout(false);
            panelButtons.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Panel panel1;
        private TreeView treeViewConfigs;
        private Panel panelButtons;
        private Button btnSave;
        private Button btnRemoveItem;
        private Button btnAddItem;
        private Panel panel2;
        private Label labelProperties;
        private Panel panelPropertiesContainer;
    }
}
