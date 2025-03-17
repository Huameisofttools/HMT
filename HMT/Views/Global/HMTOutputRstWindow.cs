using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HMT.Views.Global
{
    [Guid("CFC3D02A-20A4-4E19-8342-411E2D63F122")]
    public class HMTOutputRstWindow : ToolWindowPane
    {
        public HMTOutputRstWindow() : base(null)
        {
            this.Caption = "HMT Output Rst";
            this.Content = new HMTOutputRst();
        }
    }
}
