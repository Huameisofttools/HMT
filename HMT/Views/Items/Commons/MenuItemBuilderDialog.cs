using HMT.Services.Items.Commons;
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

namespace HMT.Views.Items.Commons
{
    public partial class MenuItemBuilderDialog : Form
    {
        private MenuItemBuilderParms _parms;

        public MenuItemBuilderDialog()
        {
            InitializeComponent();
        }
        public void SetParameters(MenuItemBuilderParms parms)
        {
            _parms = parms;
            comboBox1.DataSource = Enum.GetValues(_parms.MenuItemType.GetType());
            menuItemBuilderParmsBindingSource.Add(parms);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
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
