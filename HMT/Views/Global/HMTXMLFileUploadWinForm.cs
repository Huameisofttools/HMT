using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;


namespace HMT.Views.Global
{
    /// <summary>
    /// ina wang on 03/05/2025
    /// this class is used to upload XML file
    /// </summary>
    public partial class HMTXMLFileUploadWinForm : Form
    {
        public string UploadedXmlContent { get; private set; }

        public HMTXMLFileUploadWinForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Ina Wang on 03/05/2025
        /// this method is used to select the XML file to upload, click the button to select the file
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        private void btnSelectFile_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Set a file filter to only allow XML files
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";

                // Show the OpenFileDialog
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the user-selected file path
                    string filePath = openFileDialog.FileName;
                    txtFilePath.Text = filePath;

                    try
                    {
                        // Read and display XML content from the file
                        string xmlContent = File.ReadAllText(filePath);
                        rtbXmlContent.Text = xmlContent;

                        // Parse the XML file to ensure it is valid
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlContent);
                        UploadedXmlContent = xmlContent;

                        MessageBox.Show("XML file uploaded successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    catch (XmlException xmlEx)
                    {
                        // Capture and display XML-specific exception information
                        MessageBox.Show($"Error parsing XML file: {xmlEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        // Capture and display general exception information
                        MessageBox.Show($"Error uploading XML file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

    }
}
