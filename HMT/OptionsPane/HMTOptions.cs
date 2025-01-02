using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace HMT.OptionsPane
{
    internal class HMTOptions : DialogPage
    {
        [Category("Paste Class Method settings")]
        [DisplayName("Parm methods")]
        [Description("Parm methods")]
        public bool parmtMethod
        {
            get
            {
                return this.isParm;
            }
            set
            {
                this.isParm = value;
            }
        }

        [Category("Label Generator")]
        [DisplayName("Set Label For Source Code")]
        [Description("Set Label For Source Code")]
        public bool SetLabelForSourceCode
        {
            get
            {
                return this.isLabelForSourceCode;
            }
            set
            {
                this.isLabelForSourceCode = value;
            }
        }

        [Category("Create Extension")]
        [DisplayName("Extension Class Prefix")]
        [Description("Extension Class Prefix")]
        public string ExtensionClassPrefix
        {
            get
            {
                return this.extensionClassPrefix;
            }
            set
            {
                this.extensionClassPrefix = value;
            }
        }

        private bool isParm = true;
        private bool isLabelForSourceCode = true;
        private string extensionClassPrefix = "HMT";
    }
}
