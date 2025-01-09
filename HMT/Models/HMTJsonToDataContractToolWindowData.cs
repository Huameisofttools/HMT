using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;

namespace HMT.Models
{
    [DisplayName("HMT JsonToDataContractTool Window Data")]
    public class HMTJsonToDataContractToolWindowData
    {
        [DisplayName("DTE Instance")]
        [Category("General")]
        [Description("The DTE Instance")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public DTE DTE { get; set; }

        [DisplayName("Async Package")]
        [Category("General")]
        [Description("The Package")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AsyncPackage Package { get; set; }

        [DisplayName("Text Box")]
        [Category("General")]
        [Description("The TextBox")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public TextBox TextBox { get; set; }
    }
}
