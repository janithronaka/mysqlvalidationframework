namespace Server_Code_Genarator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnGenarate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbSchema = new System.Windows.Forms.ComboBox();
            this.tblRelation = new System.Windows.Forms.DataGridView();
            this.RmbOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cmbTable = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnViewCode = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.colFieldName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFieldType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFieldSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMandatory = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colPrimaryKey = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colUniqueField = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCustomizedType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colGetMethod = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCheckInTable = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.tblRelation)).BeginInit();
            this.RmbOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGenarate
            // 
            this.btnGenarate.Location = new System.Drawing.Point(803, 351);
            this.btnGenarate.Name = "btnGenarate";
            this.btnGenarate.Size = new System.Drawing.Size(99, 30);
            this.btnGenarate.TabIndex = 0;
            this.btnGenarate.Text = "Genarate Code";
            this.btnGenarate.UseVisualStyleBackColor = true;
            this.btnGenarate.Click += new System.EventHandler(this.btnGenarate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Database Name:";
            // 
            // cmbSchema
            // 
            this.cmbSchema.FormattingEnabled = true;
            this.cmbSchema.Location = new System.Drawing.Point(105, 16);
            this.cmbSchema.Name = "cmbSchema";
            this.cmbSchema.Size = new System.Drawing.Size(348, 21);
            this.cmbSchema.TabIndex = 2;
            this.cmbSchema.SelectedIndexChanged += new System.EventHandler(this.cmbSchema_SelectedIndexChanged);
            // 
            // tblRelation
            // 
            this.tblRelation.AllowUserToAddRows = false;
            this.tblRelation.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.tblRelation.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFieldName,
            this.colFieldType,
            this.colFieldSize,
            this.colMandatory,
            this.colPrimaryKey,
            this.colUniqueField,
            this.colCustomizedType,
            this.colGetMethod,
            this.colCheckInTable});
            this.tblRelation.ContextMenuStrip = this.RmbOptions;
            this.tblRelation.Location = new System.Drawing.Point(12, 85);
            this.tblRelation.Name = "tblRelation";
            this.tblRelation.ShowCellErrors = false;
            this.tblRelation.ShowRowErrors = false;
            this.tblRelation.Size = new System.Drawing.Size(890, 260);
            this.tblRelation.TabIndex = 3;
            this.tblRelation.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tblRelation_CellClick);
            this.tblRelation.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.tblRelation_CellValidated);
            this.tblRelation.Click += new System.EventHandler(this.tblRelation_Click);
            // 
            // RmbOptions
            // 
            this.RmbOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.RmbOptions.Name = "RmbOptions";
            this.RmbOptions.Size = new System.Drawing.Size(211, 26);
            this.RmbOptions.Click += new System.EventHandler(this.RmbOptions_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(210, 22);
            this.toolStripMenuItem1.Text = "Set Foreign Key Reference";
            // 
            // cmbTable
            // 
            this.cmbTable.FormattingEnabled = true;
            this.cmbTable.Location = new System.Drawing.Point(105, 43);
            this.cmbTable.Name = "cmbTable";
            this.cmbTable.Size = new System.Drawing.Size(348, 21);
            this.cmbTable.TabIndex = 5;
            this.cmbTable.SelectedIndexChanged += new System.EventHandler(this.cmbTable_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Table Name:";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(698, 351);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(99, 30);
            this.btnReset.TabIndex = 6;
            this.btnReset.Text = "Reset Data";
            this.btnReset.UseVisualStyleBackColor = true;
            // 
            // btnViewCode
            // 
            this.btnViewCode.Location = new System.Drawing.Point(593, 351);
            this.btnViewCode.Name = "btnViewCode";
            this.btnViewCode.Size = new System.Drawing.Size(99, 30);
            this.btnViewCode.TabIndex = 7;
            this.btnViewCode.Text = "View Code";
            this.btnViewCode.UseVisualStyleBackColor = true;
            this.btnViewCode.Click += new System.EventHandler(this.btnViewCode_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(488, 351);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 30);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Exit";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // colFieldName
            // 
            this.colFieldName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colFieldName.HeaderText = "Field Name";
            this.colFieldName.Name = "colFieldName";
            this.colFieldName.ReadOnly = true;
            this.colFieldName.Width = 85;
            // 
            // colFieldType
            // 
            this.colFieldType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colFieldType.HeaderText = "Field Type";
            this.colFieldType.Name = "colFieldType";
            this.colFieldType.ReadOnly = true;
            this.colFieldType.Width = 81;
            // 
            // colFieldSize
            // 
            this.colFieldSize.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colFieldSize.HeaderText = "Field Size";
            this.colFieldSize.Name = "colFieldSize";
            this.colFieldSize.ReadOnly = true;
            this.colFieldSize.Width = 77;
            // 
            // colMandatory
            // 
            this.colMandatory.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colMandatory.HeaderText = "Mandetory Field";
            this.colMandatory.Name = "colMandatory";
            this.colMandatory.Width = 88;
            // 
            // colPrimaryKey
            // 
            this.colPrimaryKey.HeaderText = "Primary Key";
            this.colPrimaryKey.Name = "colPrimaryKey";
            this.colPrimaryKey.ReadOnly = true;
            // 
            // colUniqueField
            // 
            this.colUniqueField.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colUniqueField.HeaderText = "Unique Field";
            this.colUniqueField.Name = "colUniqueField";
            this.colUniqueField.Width = 72;
            // 
            // colCustomizedType
            // 
            this.colCustomizedType.HeaderText = "Customized Field Type";
            this.colCustomizedType.Items.AddRange(new object[] {
            "TEXT",
            "NUMERIC",
            "DATE TIME",
            "CURRENCY"});
            this.colCustomizedType.Name = "colCustomizedType";
            this.colCustomizedType.ReadOnly = true;
            // 
            // colGetMethod
            // 
            this.colGetMethod.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colGetMethod.HeaderText = "Get Method";
            this.colGetMethod.Name = "colGetMethod";
            this.colGetMethod.Width = 69;
            // 
            // colCheckInTable
            // 
            this.colCheckInTable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colCheckInTable.HeaderText = "Referenced Table";
            this.colCheckInTable.Name = "colCheckInTable";
            this.colCheckInTable.ReadOnly = true;
            this.colCheckInTable.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colCheckInTable.Width = 99;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 387);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnViewCode);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.cmbTable);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tblRelation);
            this.Controls.Add(this.cmbSchema);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGenarate);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Server Code Genarator";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tblRelation)).EndInit();
            this.RmbOptions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnGenarate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSchema;
        private System.Windows.Forms.DataGridView tblRelation;
        private System.Windows.Forms.ComboBox cmbTable;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnViewCode;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ContextMenuStrip RmbOptions;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFieldName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFieldType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFieldSize;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colMandatory;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colPrimaryKey;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colUniqueField;
        private System.Windows.Forms.DataGridViewComboBoxColumn colCustomizedType;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetMethod;
        private System.Windows.Forms.DataGridViewButtonColumn colCheckInTable;
    }
}

