using HMT.Services.Items.Forms;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Forms;

namespace HMT.Views.Items.Forms
{
    public partial class FormBuilderDialog : Form
    {
        private FormBuilderParms _parms;

        public FormBuilderDialog()
        {
            InitializeComponent();
        }
        public void SetParameters(FormBuilderParms parms)
        {
            _parms = parms;
            formBuilderParmsBindingSource.Add(parms);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _parms.TemplateType = (FormTemplateType)Enum.Parse(typeof(FormTemplateType), tabControl1.SelectedTab.Tag.ToString());
                _parms.Run();

                _parms.DisplayLog();

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
