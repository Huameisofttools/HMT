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

namespace HMT.HMTClass.HMTExtendAxElement
{
    public partial class HMTExtendAxElementDialog : Form
    {
        HMTExtendAxElementService service;

        public HMTExtendAxElementDialog()
        {
            InitializeComponent();
        }

        public void init(HMTExtendAxElementService _service)
        {
            service = _service;
            ExtensionClassName.Text = service.ResultClassName;
            ExtensionType.Text = service.ElementType.ToString();
        }

        private void OnClick(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                service.Run();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void ExtensionClassNameChanged(object sender, EventArgs e)
        {
            if (ExtensionClassName.Text != service.ResultClassName)
                service.ResultClassName = ExtensionClassName.Text;
        }
    }
}
