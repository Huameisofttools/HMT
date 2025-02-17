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
    public partial class HMTFunctionDemoGeneratorDialog : Form
    {
        public HMTFunctionDemoGeneratorDialog(AsyncPackage package)
        {
            InitializeComponent(package);
        }
    }
}
