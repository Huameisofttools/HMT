using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace HMT.OptionsPane
{
    /// <summary>
    /// Represents the options for the HMT D365FFO tools.
    /// Willie Yao - 03/08/2025
    /// </summary>
    [ComVisible(true)]
    internal class HMTOptions : DialogPage
    {
        /// <summary>
        /// Gets or sets a value indicating whether parm methods are enabled.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <remarks>
        /// This setting controls whether parm methods are enabled in the HMT D365FFO tools.
        /// </remarks>
        [Category("Paste Class Method settings")]
        [DisplayName("Parm methods")]
        [Description("Enable or disable parm methods.")]
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

        /// <summary>
        /// Gets or sets a value indicating whether labels should be set for source code.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <remarks>
        /// This setting controls whether labels are generated for source code in the HMT D365FFO tools.
        /// </remarks>
        [Category("Label Generator")]
        [DisplayName("Set Label For Source Code")]
        [Description("Enable or disable label generation for source code.")]
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

        /// <summary>
        /// Gets or sets the prefix for extension classes.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <remarks>
        /// This setting specifies the prefix used for extension classes in the HMT D365FFO tools.
        /// </remarks>
        [Category("Create Extension")]
        [DisplayName("Extension Class Prefix")]
        [Description("Specify the prefix for extension classes.")]
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