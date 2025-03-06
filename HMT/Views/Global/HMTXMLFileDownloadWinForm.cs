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


namespace HMT.Views.Global
{
    /// <summary>
    /// ina wang on 03/05/2025
    /// This class is used to download XML file
    /// </summary>
    public partial class HMTXMLFileDownloadWinForm : Form
    {
        private XmlDocument xmlContent;

        public HMTXMLFileDownloadWinForm(XmlDocument xml)
        {
            InitializeComponent();
            xmlContent = xml;
        }

        /// <summary>
        /// Ina Wang on 03/05/2025
        /// save the XML content to the file and download it
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        private void GenerateAndSaveFileButton_Click(object sender, EventArgs e)
        {
            // Set SaveFileDialog properties
            saveFileDialog1.Filter = "XML Files (*.xml)|*.xml";

            // Display SaveFileDialog and get user response
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePathLoc = saveFileDialog1.FileName;
                FilePath.Text = filePathLoc;

                try
                {
                    // Save the XML content to the file
                    xmlContent.Save(filePathLoc);
                    MessageBox.Show("XML file downloaded successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    // Display an error message if an exception occurs during file writing
                    MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

    }
}
