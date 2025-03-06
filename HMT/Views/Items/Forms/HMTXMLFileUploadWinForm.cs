using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace HMT.Views.Items.Forms
{
    public partial class HMTXMLFileUploadWinForm : Form
    {
        public string UploadedXmlContent { get; private set; }

        public HMTXMLFileUploadWinForm()
        {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            // create a OpenFileDialog 
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // set a file filter，only can upload XML file
            openFileDialog.Filter = "XML 文件 (*.xml)|*.xml";

            // show the OpenFileDialog
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // get user selected file path
                string filePath = openFileDialog.FileName;
                txtFilePath.Text = filePath;

                try
                {
                    // read XML content from file
                    string xmlContent = File.ReadAllText(filePath);
                    rtbXmlContent.Text = xmlContent;

                    // parse XML file
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(filePath);
                    UploadedXmlContent = File.ReadAllText(filePath);
                    MessageBox.Show("XML file upload successfully！", "Tips", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    // 捕获并显示异常信息
                    MessageBox.Show($"Throw an error when impot XML file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
