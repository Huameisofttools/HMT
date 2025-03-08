using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static HMT.OptionsPane.HMTOptionsProvider;
using System.Windows;
using HMT.Views.Settings;

namespace HMT.OptionsPane
{
    /// <summary>
    /// Represents the general options page for the HMT D365FFO tools.
    /// Willie Yao - 03/08/2025
    /// </summary>
    [Guid("A1E9AD78-AA94-4EA3-855E-E9D738110FBD")]
    [ComVisible(true)]
    public class GeneralOptionPage : UIElementDialogPage
    {
        /// <summary>
        /// Gets the child element to display.
        /// Willie Yao - 03/08/2025
        /// </summary>
        protected override UIElement Child
        {
            get
            {
                HMT.Views.Settings.GeneralOptions page = new HMT.Views.Settings.GeneralOptions
                {
                    generalOptionsPage = this
                };
                page.Initialize();
                return page;
            }
        }
    }
}
