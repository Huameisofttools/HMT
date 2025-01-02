namespace HMT.KernelSettings
{
    public partial class HMTKernelSettings
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ClassExtensionNameTab = new System.Windows.Forms.TabPage();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SettingsTypeComboBox = new System.Windows.Forms.ComboBox();
            this.SetDefaultButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.DescriptionBox = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.ParametersBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ValuesControl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.ClassExtensionNameTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.ClassExtensionNameTab);
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(718, 547);
            this.tabControl1.TabIndex = 21;
            // 
            // ClassExtensionNameTab
            // 
            this.ClassExtensionNameTab.Controls.Add(this.linkLabel2);
            this.ClassExtensionNameTab.Controls.Add(this.FileNameTextBox);
            this.ClassExtensionNameTab.Controls.Add(this.label4);
            this.ClassExtensionNameTab.Controls.Add(this.SettingsTypeComboBox);
            this.ClassExtensionNameTab.Controls.Add(this.SetDefaultButton);
            this.ClassExtensionNameTab.Controls.Add(this.label2);
            this.ClassExtensionNameTab.Controls.Add(this.DescriptionBox);
            this.ClassExtensionNameTab.Controls.Add(this.SaveButton);
            this.ClassExtensionNameTab.Controls.Add(this.ParametersBox);
            this.ClassExtensionNameTab.Controls.Add(this.label3);
            this.ClassExtensionNameTab.Controls.Add(this.ValuesControl);
            this.ClassExtensionNameTab.Controls.Add(this.label1);
            this.ClassExtensionNameTab.Location = new System.Drawing.Point(4, 24);
            this.ClassExtensionNameTab.Name = "ClassExtensionNameTab";
            this.ClassExtensionNameTab.Padding = new System.Windows.Forms.Padding(3);
            this.ClassExtensionNameTab.Size = new System.Drawing.Size(710, 519);
            this.ClassExtensionNameTab.TabIndex = 0;
            this.ClassExtensionNameTab.Text = "Class extension name";
            this.ClassExtensionNameTab.UseVisualStyleBackColor = true;
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(185, 18);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(295, 15);
            this.linkLabel2.TabIndex = 32;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Naming guidelines for extensions(docs.microsoft)";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // FileNameTextBox
            // 
            this.FileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileNameTextBox.Location = new System.Drawing.Point(519, 81);
            this.FileNameTextBox.Name = "FileNameTextBox";
            this.FileNameTextBox.ReadOnly = true;
            this.FileNameTextBox.Size = new System.Drawing.Size(183, 20);
            this.FileNameTextBox.TabIndex = 31;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(435, 21);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 15);
            this.label4.TabIndex = 30;
            this.label4.Text = "Default settings";
            // 
            // SettingsTypeComboBox
            // 
            this.SettingsTypeComboBox.FormattingEnabled = true;
            this.SettingsTypeComboBox.Location = new System.Drawing.Point(435, 39);
            this.SettingsTypeComboBox.Name = "SettingsTypeComboBox";
            this.SettingsTypeComboBox.Size = new System.Drawing.Size(121, 23);
            this.SettingsTypeComboBox.TabIndex = 29;
            // 
            // SetDefaultButton
            // 
            this.SetDefaultButton.Location = new System.Drawing.Point(562, 37);
            this.SetDefaultButton.Name = "SetDefaultButton";
            this.SetDefaultButton.Size = new System.Drawing.Size(112, 27);
            this.SetDefaultButton.TabIndex = 28;
            this.SetDefaultButton.Text = "Load default";
            this.SetDefaultButton.UseVisualStyleBackColor = true;
            this.SetDefaultButton.Click += new System.EventHandler(this.SetDefaultButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(211, 15);
            this.label2.TabIndex = 27;
            this.label2.Text = "Name template for class extensions";
            // 
            // DescriptionBox
            // 
            this.DescriptionBox.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.DescriptionBox.Location = new System.Drawing.Point(6, 37);
            this.DescriptionBox.Multiline = true;
            this.DescriptionBox.Name = "DescriptionBox";
            this.DescriptionBox.Size = new System.Drawing.Size(424, 67);
            this.DescriptionBox.TabIndex = 26;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(438, 78);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 27);
            this.SaveButton.TabIndex = 25;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click_1);
            // 
            // ParametersBox
            // 
            this.ParametersBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ParametersBox.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ParametersBox.Location = new System.Drawing.Point(0, 127);
            this.ParametersBox.Multiline = true;
            this.ParametersBox.Name = "ParametersBox";
            this.ParametersBox.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.ParametersBox.Size = new System.Drawing.Size(157, 380);
            this.ParametersBox.TabIndex = 24;
            this.ParametersBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ParametersBox.WordWrap = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 15);
            this.label3.TabIndex = 23;
            this.label3.Text = "Base Type";
            // 
            // ValuesControl
            // 
            this.ValuesControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ValuesControl.BackColor = System.Drawing.SystemColors.Window;
            this.ValuesControl.Location = new System.Drawing.Point(159, 127);
            this.ValuesControl.Multiline = true;
            this.ValuesControl.Name = "ValuesControl";
            this.ValuesControl.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.ValuesControl.Size = new System.Drawing.Size(543, 380);
            this.ValuesControl.TabIndex = 22;
            this.ValuesControl.WordWrap = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(156, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 15);
            this.label1.TabIndex = 21;
            this.label1.Text = "Name template";
            // 
            // HMTKernelSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 552);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Cascadia Mono", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "HMTKernelSettings";
            this.Text = "HMT - Setup";
            this.tabControl1.ResumeLayout(false);
            this.ClassExtensionNameTab.ResumeLayout(false);
            this.ClassExtensionNameTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ClassExtensionNameTab;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox DescriptionBox;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.TextBox ParametersBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ValuesControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button SetDefaultButton;
        private System.Windows.Forms.TextBox FileNameTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox SettingsTypeComboBox;
        private System.Windows.Forms.LinkLabel linkLabel2;
    }
}