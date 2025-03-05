using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;

// Ina Wang on 03/05/2025
// This class is used to download XML file
namespace HMT.Views.Global
{
    public partial class HMTXMLFileDownloadWinForm : Form
    {
        private XmlDocument xmlContent;

        public HMTXMLFileDownloadWinForm(XmlDocument xml)
        {
            InitializeComponent();
            xmlContent = xml;
        }

        private void GenerateAndSaveFileButton_Click(object sender, EventArgs e)
        {
            // set SaveFileDialog properties
            saveFileDialog1.Filter = "XML 文件 (*.xml)|*.xml";
            //saveFileDialog1.FileName = "options.xml";

            // display SaveFileDialog and get user response
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // get the file path
                    string filePathLoc = saveFileDialog1.FileName;

                    FilePath.Text = filePathLoc;
                    // dowmload the XML content to the file
                    xmlContent.Save(filePathLoc);
                    //this.DialogResult = DialogResult.OK;
                    this.Close();
                    MessageBox.Show("XML file download successfully！", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    // 若写入文件时出现异常，显示错误信息
                    MessageBox.Show($"An error occurred while saving the file：{ex.Message}");
                }
            }
        }
    }
}
