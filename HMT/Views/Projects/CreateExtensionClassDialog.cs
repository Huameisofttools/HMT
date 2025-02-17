using HMT.Services.Projects;
using HMT.Views.Settings;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Forms;

namespace HMT.Views.Projects
{
    public partial class CreateExtensionClassDialog : Form
    {
        private CreateExtensionClassParms _parms;
        public AsyncPackage AsyncPackage;

        public CreateExtensionClassDialog(AsyncPackage asyncPackage = null)
        {
            InitializeComponent();

            if (asyncPackage != null)
            {
                AsyncPackage = asyncPackage;
            }
        }

        public void SetParameters(CreateExtensionClassParms parms)
        {
            comboBox1.DataSource = Enum.GetValues(parms.ClassModeType.GetType());
            ElementTypeComboBox.DataSource = Enum.GetValues(parms.ElementType.GetType());

            _parms = parms;
            _parms.CalcResultName();

            createExtensionClassParmsBindingSource.Add(_parms);

            //UpdateResult();
        }


        private void RestoreNameButton_Click(object sender, EventArgs e)
        {
            _parms.CalcResultName();
            createExtensionClassParmsBindingSource.ResetBindings(false);
        }


        private void CreateClassButton_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (_parms.Run())
                {
                    _parms.DisplayLog();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"An exception occurred:", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrefixTextBox_Validated(object sender, EventArgs e)
        {
            if (_parms != null && _parms.PrefixModified())
            {
                createExtensionClassParmsBindingSource.ResetBindings(false);
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //return;
            if (_parms != null && _parms.ClassTypeModified())
            {
                createExtensionClassParmsBindingSource.ResetBindings(false);
            }
        }

        private void SetupNameButton_Click(object sender, EventArgs e)
        {
            HMTKernelSettings f = new HMTKernelSettings(AsyncPackage);
            f.ShowDialog();

            _parms.InitFromSettings();

            _parms.CalcResultName();
            createExtensionClassParmsBindingSource.ResetBindings(false);


        }
    }
}
