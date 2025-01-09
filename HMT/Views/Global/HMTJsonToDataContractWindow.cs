using HMT.Models;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace HMT.Views.Global
{
    /// <summary>
    /// Willie Yao - 01/08/2025
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("e5775bcc-3ede-465c-876b-73cd0aba9cfb")]
    public class HMTJsonToDataContractWindow : ToolWindowPane
    {
        /// <summary>
        /// Willie Yao - 01/08/2025
        /// Initializes a new instance of the <see cref="HMTJsonToDataContractWindow"/> class.
        /// </summary>
        public HMTJsonToDataContractWindow(HMTJsonToDataContractToolWindowData data) : base(null)
        {
            this.Caption = "JSON To Data Contract";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new HMTJsonToDataContractWindowControl(data);
        }
    }
}
