namespace HMT.HMTClass.HMTExtendAxElement
{
    partial class HMTExtendAxElementDialog
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
            this.OKBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ExtensionClassName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ExtensionType = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OKBtn
            // 
            this.OKBtn.Location = new System.Drawing.Point(276, 71);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 0;
            this.OKBtn.Text = "OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            this.OKBtn.Click += new System.EventHandler(this.OnClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Extension class name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Extension type";
            // 
            // ExtensionClassName
            // 
            this.ExtensionClassName.Location = new System.Drawing.Point(117, 9);
            this.ExtensionClassName.Name = "ExtensionClassName";
            this.ExtensionClassName.Size = new System.Drawing.Size(234, 20);
            this.ExtensionClassName.TabIndex = 4;
            this.ExtensionClassName.TextChanged += new System.EventHandler(this.ExtensionClassNameChanged);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.ExtensionType);
            this.panel1.Location = new System.Drawing.Point(117, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(234, 22);
            this.panel1.TabIndex = 5;
            // 
            // ExtensionType
            // 
            this.ExtensionType.AutoSize = true;
            this.ExtensionType.Location = new System.Drawing.Point(3, 0);
            this.ExtensionType.Name = "ExtensionType";
            this.ExtensionType.Size = new System.Drawing.Size(35, 13);
            this.ExtensionType.TabIndex = 0;
            this.ExtensionType.Text = "label3";
            // 
            // HMTExtendAxElementDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 100);
            this.Controls.Add(this.ExtensionClassName);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.OKBtn);
            this.Name = "HMTExtendAxElementDialog";
            this.Text = "Create Extension Class";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ExtensionClassName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label ExtensionType;
    }
}