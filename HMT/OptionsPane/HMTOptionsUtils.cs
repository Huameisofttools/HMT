using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;
using System;
using System.Security;
using HMT.Kernel;

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

        /// <summary>
        /// Get the AI api key
        /// Willie Yao - 2024/08/22
        /// </summary>
        /// <param name="_package">AsyncPackage</param>
        /// <returns>
        /// The AI api key
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// ArgumentNullException
        /// </exception>
        public static string getAIApiKey(AsyncPackage _package)
        {
            HMTOptionsProvider.GeneralOptions page = (HMTOptionsProvider.GeneralOptions)_package.GetDialogPage(typeof(HMTOptionsProvider.GeneralOptions));
            General general = (General)page.AutomationObject;
            string encryptedSecureString = general.EncryptedSecureString;
            SecureString aiKey = SecureStringHelper.DecryptToSecureString(encryptedSecureString);

            if (aiKey == null)
                throw new ArgumentNullException(nameof(aiKey));

            IntPtr bstr = IntPtr.Zero;
            try
            {
                bstr = Marshal.SecureStringToBSTR(aiKey);

                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                if (bstr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(bstr); 
            }
        }
    }
}
