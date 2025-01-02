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

namespace HMT.HMTForm.PrivilegeBuilder
{
    public partial class SecurityPrivilegeBuilderDialog : Form
    {
        private SecurityPrivilegeBuilderParms _parms;

        public SecurityPrivilegeBuilderDialog()
        {
            InitializeComponent();
        }
        public void SetParameters(SecurityPrivilegeBuilderParms parms)
        {
            AccessLevelComboBox.DataSource = Enum.GetValues(parms.AccessLevel.GetType());
            MenuItemTypeComboBox.DataSource = Enum.GetValues(parms.MenuItemType.GetType());

            _parms = parms;

            securityPrivilegeBuilderParmsBindingSource.Add(parms);

            securityPrivilegeBuilderParmsBindingSource.ResetBindings(false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
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

        private void AccessLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_parms != null)
            {
                _parms.GenerateNames();
                securityPrivilegeBuilderParmsBindingSource.ResetBindings(false);
            }
        }
    }
}
