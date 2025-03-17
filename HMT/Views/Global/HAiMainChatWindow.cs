using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using HMT.Views.Global;

namespace HMT.Views.Global
{
    /// <summary>
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
    [Guid("0a2ad24f-73bd-45aa-b76b-931e876da011")]
    public class HAiMainChatWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HAiMainChatWindow"/> class.
        /// </summary>
        public HAiMainChatWindow() : base(null)
        {
            this.Caption = "Huamei Copilot Chat";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new HAiMainChatWindowControl();
        }
    }
}
