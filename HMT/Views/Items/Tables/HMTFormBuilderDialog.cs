using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMT.HMTFormGenerator
{
    public partial class HMTFormBuilderDialog : Form
    {
        private HMTFormService _service;

        public HMTFormBuilderDialog()
        {
            InitializeComponent();
        }

        public void SetParameters(HMTFormService service)
        {
            _service = service;
            formBuilderParmsBindingSource.Add(service);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _service.TemplateType = (FormTemplateType)Enum.Parse(typeof(FormTemplateType), tabControl1.SelectedTab.Tag.ToString());
                _service.Run();
                _service.DisplayLog();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"An exception occurred:", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
}
