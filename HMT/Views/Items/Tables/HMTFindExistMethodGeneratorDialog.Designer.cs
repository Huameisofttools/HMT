namespace HMT.HMTTable.HMTFindExistMethodGenerator
{
    partial class HMTFindExistMethodGeneratorDialog
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.FindMethodName = new System.Windows.Forms.Label();
            this.ExistMethodName = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OKBtn = new System.Windows.Forms.Button();
            //this.RefreshBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "RecId",
            "Other Field"});
            this.comboBox1.Location = new System.Drawing.Point(16, 19);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.TextChanged += new System.EventHandler(this.GenerateForMofied);
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(16, 46);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(121, 124);
            this.checkedListBox1.TabIndex = 1;
            // 
            // FindMethodName
            // 
            this.FindMethodName.AutoSize = true;
            this.FindMethodName.Location = new System.Drawing.Point(143, 46);
            this.FindMethodName.Name = "FindMethodName";
            this.FindMethodName.Size = new System.Drawing.Size(97, 13);
            this.FindMethodName.TabIndex = 2;
            this.FindMethodName.Text = "Find Method Name";
            // 
            // ExistMethodName
            // 
            this.ExistMethodName.AutoSize = true;
            this.ExistMethodName.Location = new System.Drawing.Point(143, 76);
            this.ExistMethodName.Name = "ExistMethodName";
            this.ExistMethodName.Size = new System.Drawing.Size(99, 13);
            this.ExistMethodName.TabIndex = 3;
            this.ExistMethodName.Text = "Exist Method Name";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(248, 43);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 4;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(248, 73);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Generate For";
            // 
            // OKBtn
            // 
            this.OKBtn.Location = new System.Drawing.Point(273, 144);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 7;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
            // 
            // RefreshBtn
            // 
            //this.RefreshBtn.Location = new System.Drawing.Point(273, 17);
            //this.RefreshBtn.Name = "RefreshBtn";
            //this.RefreshBtn.Size = new System.Drawing.Size(75, 23);
            //this.RefreshBtn.TabIndex = 8;
            //this.RefreshBtn.Text = "Refresh";
            //this.RefreshBtn.UseVisualStyleBackColor = true;
            // 
            // HMTFindExistMethodGeneratorDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(363, 179);
            //this.Controls.Add(this.RefreshBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.ExistMethodName);
            this.Controls.Add(this.FindMethodName);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.comboBox1);
            this.Name = "HMTFindExistMethodGeneratorDialog";
            this.Text = "Generate find and exist method";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Label FindMethodName;
        private System.Windows.Forms.Label ExistMethodName;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OKBtn;
        //private System.Windows.Forms.Button RefreshBtn;
    }
}