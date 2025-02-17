using HMT.Services.Global;
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

namespace HMT.Views.Global
{
    public partial class EnumCreatorDialog : Form
    {
        private EnumCreatorParms _parms;

        public EnumCreatorDialog()
        {
            InitializeComponent();
        }
        public void SetParameters(EnumCreatorParms parms)
        {
            _parms = parms;
            enumCreatorParmsBindingSource.Add(parms);
        }

        private void PreviewButton_Click(object sender, EventArgs e)
        {
            PreviewTextBox.Text = _parms.GetPreviewString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                _parms.CreateEnum();

                _parms.DisplayLog();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"An exception occurred:", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }


        private void IsCreateTypeCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            if (_parms.IsCreateEnumTypeModified())
            {
                enumCreatorParmsBindingSource.ResetBindings(false);
            }
        }

        private void EnumLabelTextBox_Validated(object sender, EventArgs e)
        {
            if (_parms.EnumLabelModified())
            {
                enumCreatorParmsBindingSource.ResetBindings(false);
            }
        }
    }
}
