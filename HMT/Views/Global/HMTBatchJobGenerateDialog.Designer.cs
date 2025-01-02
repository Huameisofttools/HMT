namespace HMT.HMTBatchJobTemplateGenerator
{
    partial class HMTBatchJobGenerateDialog
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
            this.DialogLabel = new System.Windows.Forms.Label();
            this.ClassType = new System.Windows.Forms.ComboBox();
            this.ClassTypeLabel = new System.Windows.Forms.Label();
            this.ClassNamePrefixLabel = new System.Windows.Forms.Label();
            this.ClassNamePrefix = new System.Windows.Forms.TextBox();
            this.ClassNamePreview = new System.Windows.Forms.RichTextBox();
            this.ClassNamePrev = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.CommentLabel = new System.Windows.Forms.Label();
            this.Comment = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // DialogLabel
            // 
            this.DialogLabel.AutoSize = true;
            this.DialogLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DialogLabel.Location = new System.Drawing.Point(10, 10);
            this.DialogLabel.Name = "DialogLabel";
            this.DialogLabel.Size = new System.Drawing.Size(258, 20);
            this.DialogLabel.TabIndex = 0;
            this.DialogLabel.Text = "HMT - Batch Job Generator";
            // 
            // ClassType
            // 
            this.ClassType.FormattingEnabled = true;
            this.ClassType.Items.AddRange(new object[] {
            "RunBaseBatch",
            "RunBaseBatch with Query",
            "SysOperation",
            "SysOperation with Query"});
            this.ClassType.Location = new System.Drawing.Point(13, 70);
            this.ClassType.Name = "ClassType";
            this.ClassType.Size = new System.Drawing.Size(300, 21);
            this.ClassType.TabIndex = 1;

            // initialize classtype combobox
            this.ClassType.SelectedIndex = 0;

            this.ClassType.SelectedValueChanged += new System.EventHandler(this.resetClassNames);
            // 
            // ClassTypeLabel
            // 
            this.ClassTypeLabel.AutoSize = true;
            this.ClassTypeLabel.Location = new System.Drawing.Point(13, 50);
            this.ClassTypeLabel.Name = "ClassTypeLabel";
            this.ClassTypeLabel.Size = new System.Drawing.Size(55, 13);
            this.ClassTypeLabel.TabIndex = 2;
            this.ClassTypeLabel.Text = "Class type";
            // 
            // ClassNamePrefixLabel
            // 
            this.ClassNamePrefixLabel.AutoSize = true;
            this.ClassNamePrefixLabel.Location = new System.Drawing.Point(13, 100);
            this.ClassNamePrefixLabel.Name = "ClassNamePrefixLabel";
            this.ClassNamePrefixLabel.Size = new System.Drawing.Size(89, 13);
            this.ClassNamePrefixLabel.TabIndex = 3;
            this.ClassNamePrefixLabel.Text = "Class name prefix";
            // 
            // ClassNamePrefix
            // 
            this.ClassNamePrefix.Location = new System.Drawing.Point(13, 120);
            this.ClassNamePrefix.Name = "ClassNamePrefix";
            this.ClassNamePrefix.Size = new System.Drawing.Size(300, 20);
            this.ClassNamePrefix.TabIndex = 4;
            this.ClassNamePrefix.TextChanged += new System.EventHandler(this.resetClassNames);
            // 
            // ClassNamePreview
            // 
            this.ClassNamePreview.Location = new System.Drawing.Point(13, 260);
            this.ClassNamePreview.Name = "ClassNamePreview";
            this.ClassNamePreview.ReadOnly = true;
            this.ClassNamePreview.Size = new System.Drawing.Size(300, 100);
            this.ClassNamePreview.TabIndex = 5;
            this.ClassNamePreview.Text = "";
            // 
            // ClassNamePrev
            // 
            this.ClassNamePrev.AutoSize = true;
            this.ClassNamePrev.Location = new System.Drawing.Point(13, 240);
            this.ClassNamePrev.Name = "ClassNamePrev";
            this.ClassNamePrev.Size = new System.Drawing.Size(112, 13);
            this.ClassNamePrev.TabIndex = 6;
            this.ClassNamePrev.Text = "Class name(s) preview";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(50, 370);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 7;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(200, 370);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 8;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // CommentLabel
            // 
            this.CommentLabel.AutoSize = true;
            this.CommentLabel.Location = new System.Drawing.Point(13, 150);
            this.CommentLabel.Name = "CommentLabel";
            this.CommentLabel.Size = new System.Drawing.Size(51, 13);
            this.CommentLabel.TabIndex = 9;
            this.CommentLabel.Text = "Comment";
            // 
            // Comment
            // 
            this.Comment.Location = new System.Drawing.Point(13, 170);
            this.Comment.Name = "Comment";
            this.Comment.Size = new System.Drawing.Size(300, 60);
            this.Comment.TabIndex = 10;
            this.Comment.Text = "/// HM_D365_Addin_ClassDemoGenerator\n/// [Developer] - [MM/DD/YYYY]";
            // 
            // HMTBatchJobGenerateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 403);
            this.Controls.Add(this.Comment);
            this.Controls.Add(this.CommentLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ClassNamePrev);
            this.Controls.Add(this.ClassNamePreview);
            this.Controls.Add(this.ClassNamePrefix);
            this.Controls.Add(this.ClassNamePrefixLabel);
            this.Controls.Add(this.ClassTypeLabel);
            this.Controls.Add(this.ClassType);
            this.Controls.Add(this.DialogLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HMTBatchJobGenerateDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Huamei Tools";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private void CancelButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void OKButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void resetClassNames(object sender, System.EventArgs e)
        {
            var selectedValue = this.ClassType.SelectedItem.ToString();
            var classNamePref = this.ClassNamePrefix.Text;
            string ret = "";
            const string newline = "\n";

            if (!string.IsNullOrEmpty(selectedValue) && !string.IsNullOrEmpty(classNamePref))
            {
                switch (selectedValue)
                {
                    case "RunBaseBatch":
                    case "RunBaseBatch with Query":
                        ret += classNamePref + "Batch";
                        break;

                    case "SysOperation":
                    case "SysOperation with Query":
                        ret += classNamePref + "Contract" + newline;
                        ret += classNamePref + "Controller" + newline;
                        ret += classNamePref + "Service" + newline;
                        ret += classNamePref + "UIBuilder";
                        break;
                }
            }

            this.ClassNamePreview.Text = ret;
        }

        private System.Windows.Forms.Label DialogLabel;
        public System.Windows.Forms.ComboBox ClassType;
        private System.Windows.Forms.Label ClassTypeLabel;
        private System.Windows.Forms.Label ClassNamePrefixLabel;
        public System.Windows.Forms.TextBox ClassNamePrefix;
        private System.Windows.Forms.RichTextBox ClassNamePreview;
        private System.Windows.Forms.Label ClassNamePrev;
        private System.Windows.Forms.Button OKButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label CommentLabel;
        public System.Windows.Forms.RichTextBox Comment;
    }
}