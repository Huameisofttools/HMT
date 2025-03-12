using System.Drawing;

namespace HMT.Views.Global
{
    partial class HMLabelSearchDownloadWinForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
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
            this.GenerateAndSaveFileButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.FilePath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // GenerateAndSaveFileButton
            // 
            this.GenerateAndSaveFileButton.Location = new System.Drawing.Point(195, 61);
            this.GenerateAndSaveFileButton.Name = "GenerateAndSaveFileButton";
            this.GenerateAndSaveFileButton.Size = new System.Drawing.Size(122, 20);
            this.GenerateAndSaveFileButton.TabIndex = 0;
            this.GenerateAndSaveFileButton.Text = "Download Search label.exe file";
            this.GenerateAndSaveFileButton.UseVisualStyleBackColor = true;
            this.GenerateAndSaveFileButton.Click += new System.EventHandler(this.GenerateAndSaveFileButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(369, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Please select the                                  to download the Search label.e" +
    "xe file!";
            // 
            // FilePath
            // 
            this.FilePath.Location = new System.Drawing.Point(16, 62);
            this.FilePath.Name = "FilePath";
            this.FilePath.ReadOnly = true;
            this.FilePath.Size = new System.Drawing.Size(135, 20);
            this.FilePath.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Red;
            this.label2.Location = new System.Drawing.Point(97, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = " AOS directory path";
            // 
            // HMLabelSearchDownloadWinForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 116);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FilePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.GenerateAndSaveFileButton);
            this.Name = "HMLabelSearchDownloadWinForm";
            this.Text = "Download Search label.exe file";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GenerateAndSaveFileButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FilePath;
        private System.Windows.Forms.Label label2;
    }
}