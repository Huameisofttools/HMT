using Microsoft.VisualStudio.Shell;

namespace HMT.HMTFunctionDemoGenerator
{
    partial class HMTFunctionDemoGeneratorDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        
        public AsyncPackage package;

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
        private void InitializeComponent(AsyncPackage _package = null)
        {
            gGeneratorService = new HMTFunctionDemoGeneratorService();

            if (_package != null)
            {
                package = _package;
            }

            this.DialogLabel = new System.Windows.Forms.Label();
            this.Function = new System.Windows.Forms.ComboBox();
            this.FunctionLabel = new System.Windows.Forms.Label();
            this.CustomerPrefixLabel = new System.Windows.Forms.Label();
            this.CustomerPrefix = new System.Windows.Forms.TextBox();
            this.ObjectLabelLabel = new System.Windows.Forms.Label();
            this.ObjectLabel = new System.Windows.Forms.TextBox();
            this.ObjectNamePrefixLabel = new System.Windows.Forms.Label();
            this.ObjectNamePrefix = new System.Windows.Forms.TextBox();
            this.ObjectNamePreview = new System.Windows.Forms.RichTextBox();
            this.ObjectNamePrev = new System.Windows.Forms.Label();
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
            this.DialogLabel.Text = "HMT - Function Demo Generator";
            // 
            // FunctionLabel
            // 
            this.FunctionLabel.AutoSize = true;
            this.FunctionLabel.Location = new System.Drawing.Point(13, 50);
            this.FunctionLabel.Name = "FunctionLabel";
            this.FunctionLabel.Size = new System.Drawing.Size(55, 13);
            this.FunctionLabel.TabIndex = 1;
            this.FunctionLabel.Text = "Function type";
            // 
            // Function
            // 
            this.Function.FormattingEnabled = true;
            this.Function.Items.AddRange(gGeneratorService.getTemplateTypes());
            this.Function.Location = new System.Drawing.Point(13, 70);
            this.Function.Name = "Function";
            this.Function.Size = new System.Drawing.Size(300, 20);
            this.Function.TabIndex = 2;

            // initialize Function combobox
            this.Function.SelectedIndex = 0;

            this.Function.SelectedValueChanged += new System.EventHandler(this.resetObjectNames);
            //
            // Customer prefix label
            //
            this.CustomerPrefixLabel.AutoSize = true;
            this.CustomerPrefixLabel.Location = new System.Drawing.Point(13, 100);
            this.CustomerPrefixLabel.Name = "CustomerPrefixLabel";
            this.CustomerPrefixLabel.Size = new System.Drawing.Size(55, 13);
            this.CustomerPrefixLabel.TabIndex = 3;
            this.CustomerPrefixLabel.Text = "Prefix";
            // 
            // CustomerPrefix
            // 
            if (package != null)
            {
                this.CustomerPrefix.Text = OptionsPane.HMTOptionsUtils.getPrefix(package);
            }
            this.CustomerPrefix.Location = new System.Drawing.Point(13, 120);
            this.CustomerPrefix.Name = "CustomerPrefix";
            this.CustomerPrefix.Size = new System.Drawing.Size(55, 21);
            this.CustomerPrefix.TabIndex = 4;
            this.CustomerPrefix.TextChanged += new System.EventHandler(this.updateObjectNames);
            // 
            // ObjectNamePrefixLabel
            // 
            this.ObjectNamePrefixLabel.AutoSize = true;
            this.ObjectNamePrefixLabel.Location = new System.Drawing.Point(60, 100);
            this.ObjectNamePrefixLabel.Name = "ObjectNamePrefixLabel";
            this.ObjectNamePrefixLabel.Size = new System.Drawing.Size(55, 13);
            this.ObjectNamePrefixLabel.TabIndex = 5;
            this.ObjectNamePrefixLabel.Text = "Object name";
            // 
            // ObjectNamePrefix
            // 
            this.ObjectNamePrefix.Location = new System.Drawing.Point(60, 120);
            this.ObjectNamePrefix.Name = "ObjectNamePrefix";
            this.ObjectNamePrefix.Size = new System.Drawing.Size(253, 20);
            this.ObjectNamePrefix.TabIndex = 6;
            this.ObjectNamePrefix.TextChanged += new System.EventHandler(this.updateObjectNames);
            //
            // Object label label
            //
            this.ObjectLabelLabel.AutoSize = true;
            this.ObjectLabelLabel.Location = new System.Drawing.Point(13, 150);
            this.ObjectLabelLabel.Name = "ObjectLabelLabel";
            this.ObjectLabelLabel.Size = new System.Drawing.Size(89, 13);
            this.ObjectLabelLabel.TabIndex = 7;
            this.ObjectLabelLabel.Text = "Object label (Optional)";
            // 
            // Object label
            // 
            this.ObjectLabel.Location = new System.Drawing.Point(13, 170);
            this.ObjectLabel.Name = "ObjectLabel";
            this.ObjectLabel.Size = new System.Drawing.Size(300, 20);
            this.ObjectLabel.TabIndex = 8;
            this.ObjectLabel.TextChanged += new System.EventHandler(this.updateObjectNames);
            // 
            // CommentLabel
            // 
            this.CommentLabel.AutoSize = true;
            this.CommentLabel.Location = new System.Drawing.Point(13, 200);
            this.CommentLabel.Name = "CommentLabel";
            this.CommentLabel.Size = new System.Drawing.Size(51, 13);
            this.CommentLabel.TabIndex = 9;
            this.CommentLabel.Text = "Comment";
            // 
            // Comment
            // 
            this.Comment.Location = new System.Drawing.Point(13, 230);
            this.Comment.Name = "Comment";
            this.Comment.Size = new System.Drawing.Size(300, 60);
            this.Comment.TabIndex = 10;
            this.Comment.Text = "/// HM_D365_Addin_ClassDemoGenerator\n/// [Developer] - [MM/DD/YYYY]";
            // 
            // ObjectNamePrev
            // 
            this.ObjectNamePrev.AutoSize = true;
            this.ObjectNamePrev.Location = new System.Drawing.Point(13, 300);
            this.ObjectNamePrev.Name = "ObjectNamePrev";
            this.ObjectNamePrev.Size = new System.Drawing.Size(112, 13);
            this.ObjectNamePrev.TabIndex = 11;
            this.ObjectNamePrev.Text = "Object name(s) preview";
            // 
            // 
            // ObjectNamePreview
            // 
            this.ObjectNamePreview.Location = new System.Drawing.Point(13, 320);
            this.ObjectNamePreview.Name = "ObjectNamePreview";
            this.ObjectNamePreview.ReadOnly = true;
            this.ObjectNamePreview.Size = new System.Drawing.Size(300, 100);
            this.ObjectNamePreview.TabIndex = 12;
            this.ObjectNamePreview.Text = "";
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(50, 430);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 13;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(200, 430);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 14;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // HMTBatchJobGenerateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 460);
            this.Controls.Add(this.Comment);
            this.Controls.Add(this.CommentLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ObjectNamePrev);
            this.Controls.Add(this.ObjectNamePreview);
            this.Controls.Add(this.ObjectNamePrefix);
            this.Controls.Add(this.ObjectNamePrefixLabel);
            this.Controls.Add(this.FunctionLabel);
            this.Controls.Add(this.Function);
            this.Controls.Add(this.CustomerPrefixLabel);
            this.Controls.Add(this.CustomerPrefix);
            this.Controls.Add(this.ObjectLabelLabel);
            this.Controls.Add(this.ObjectLabel);
            this.Controls.Add(this.DialogLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HMTFunctionDemoGeneratorDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Huamei Tools";
            this.ResumeLayout(false);
            this.PerformLayout();
            this.resetObjectNames(null, null);
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

        private void resetObjectNames(object sender, System.EventArgs e)
        {
            var selectedValue = this.Function.SelectedItem.ToString();

            gGeneratorService.resetPreviewText(selectedValue);
            this.updateObjectNames(sender, e);
        }
        private void updateObjectNames(object sender, System.EventArgs e)
        {
            var CustomerPref = this.CustomerPrefix.Text;
            var ObjectNamePref = this.ObjectNamePrefix.Text;
            var ObjectLabel = this.ObjectLabel.Text;
            var commentText = this.Comment.Text;

            this.ObjectNamePreview.Text = gGeneratorService.updatePreviewText(CustomerPref, ObjectNamePref, ObjectLabel, commentText);
        }

        private System.Windows.Forms.Label DialogLabel;
        public System.Windows.Forms.ComboBox Function;
        private System.Windows.Forms.Label FunctionLabel;
        public System.Windows.Forms.TextBox CustomerPrefix;
        private System.Windows.Forms.Label CustomerPrefixLabel;
        public System.Windows.Forms.TextBox ObjectLabel;
        private System.Windows.Forms.Label ObjectLabelLabel;
        private System.Windows.Forms.Label ObjectNamePrefixLabel;
        public System.Windows.Forms.TextBox ObjectNamePrefix;
        private System.Windows.Forms.RichTextBox ObjectNamePreview;
        private System.Windows.Forms.Label ObjectNamePrev;
        private System.Windows.Forms.Button OKButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label CommentLabel;
        public System.Windows.Forms.RichTextBox Comment;
        public HMTFunctionDemoGeneratorService gGeneratorService;
    }
}