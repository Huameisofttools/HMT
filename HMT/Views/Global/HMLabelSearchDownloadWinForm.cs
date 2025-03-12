using System;
using System.Windows.Forms;
using System.IO;
namespace HMT.Views.Global
{
    public partial class HMLabelSearchDownloadWinForm: Form
    {
        public HMLabelSearchDownloadWinForm()
        {
            InitializeComponent();
        }
        private void GenerateAndSaveFileButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter      = "exe 文件 (*.exe)|*.exe";
            saveFileDialog1.FileName    = "Search label";
            // display SaveFileDialog and get user response
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {                                        
                    // destination File Path
                    string destinationFilePath  = saveFileDialog1.FileName;
                    var    byteRes              = Resources.Resources.SearchLabel;

                    File.WriteAllBytes(destinationFilePath, byteRes);
                    System.Diagnostics.Process.Start(destinationFilePath);                                     
                    this.Close();
                    MessageBox.Show("Download successfully!", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);   
                }
                catch (Exception ex)
                {                    
                    MessageBox.Show($"An error occurred while saving the file：{ex.Message}");
                }
            }
        }
    }
}
