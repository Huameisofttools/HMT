using Microsoft.VisualStudio.Shell;

namespace HMT.OptionsPane
{
    /// <summary>
    /// Willie Yao - 2024/08/22
    /// The setting for HMT options.
    /// </summary>
    public class HMTOptionsUtils
    {
        /// <summary>
        /// Willie Yao - 2024/08/22
        /// Get isparm setting
        /// </summary>
        /// <param name="_package">Package</param>
        /// <returns>
        /// The setting on the option page
        /// </returns>
        public static bool getIsParmMethodActivated(AsyncPackage _package)
        {
            HMTOptions page = (HMTOptions)_package.GetDialogPage(typeof(HMTOptions));
            return page.parmtMethod;
        }

        /// <summary>
        /// Willie Yao - 2024/08/22
        /// Get isLabelForSourceCode setting
        /// </summary>
        /// <param name="_package">Package</param>
        /// <returns>
        /// The setting on the option page
        /// </returns>
        public static bool getIsLabelForSourceCode(AsyncPackage _package)
        {
            HMTOptions page = (HMTOptions)_package.GetDialogPage(typeof(HMTOptions));
            return page.SetLabelForSourceCode;
        }

        public static string getPrefix(AsyncPackage _package)
        {
            HMTOptions page = (HMTOptions)_package.GetDialogPage(typeof(HMTOptions));
            return page.ExtensionClassPrefix;
        }

        // added by Ina Wang on 03/05/2025
        // set the prefix value
        public static void setPrefix(AsyncPackage _package, string _inputPrefix)
        {
            HMTOptions page = (HMTOptions)_package.GetDialogPage(typeof(HMTOptions));
            page.ExtensionClassPrefix = _inputPrefix;
        }

        // added by Ina Wang on 03/05/2025
        // set the label for source code value
        public static void setIsLabelForSourceCode(AsyncPackage _package, bool _inputIsLabelForSourceCode)
        {
            HMTOptions page = (HMTOptions)_package.GetDialogPage(typeof(HMTOptions));
            page.SetLabelForSourceCode = _inputIsLabelForSourceCode;
        }

        // added by Ina Wang on 03/05/2025
        // set the method active value
        public static void setIsParmMethodActivated(AsyncPackage _package, bool _inputIsParmMethodActivated)
        {
            HMTOptions page = (HMTOptions)_package.GetDialogPage(typeof(HMTOptions));
            page.parmtMethod = _inputIsParmMethodActivated;
        }
    }
}
