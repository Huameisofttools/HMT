using System.ComponentModel;
using Community.VisualStudio.Toolkit;
using System.Runtime.InteropServices;
using System.Security;
using HMT.Kernel;

namespace HMT.OptionsPane
{
    internal partial class HMTOptionsProvider
    {
        /// <summary>
        /// Represents the general options page for the HMT D365FFO tools.
        /// Willie Yao - 03/08/2025
        /// </summary>
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    /// <summary>
    /// Represents the general options for the HMT D365FFO tools.
    /// Willie Yao - 03/08/2025
    /// </summary>
    public class General : BaseOptionModel<General>
    {
        /// <summary>
        /// Gets or sets the AI key as a SecureString.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <remarks>
        /// The AI key is stored in an encrypted format and decrypted when accessed.
        /// </remarks>
        [Category("HMT D365FFO tools")]
        [DisplayName("AI key")]
        [Description("The AI key used for authentication.")]
        public SecureString AiKey
        {
            get
            {
                if (!string.IsNullOrEmpty(EncryptedSecureString))
                {
                    this.aiKey = SecureStringHelper.DecryptToSecureString(EncryptedSecureString);
                }
                return this.aiKey;
            }
            set
            {
                this.aiKey = value;
                EncryptedSecureString = SecureStringHelper.EncryptSecureString(value);
            }
        }

        /// <summary>
        /// Gets or sets the encrypted SecureString.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <remarks>
        /// This property stores the encrypted version of the AI key.
        /// </remarks>
        [Category("HMT D365FFO tools")]
        [DisplayName("Encrypted Secure String")]
        [Description("The encrypted version of the AI key.")]
        public string encryptedSecureString
        {
            get
            {
                return this.EncryptedSecureString;
            }
            set
            {
                this.EncryptedSecureString = value;
            }
        }

        /// <summary>
        /// Gets or sets the AI model name.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <remarks>
        /// The name of the AI model used in the HMT D365FFO tools.
        /// </remarks>
        [Category("HMT D365FFO tools")]
        [DisplayName("Ai Model Name")]
        [Description("The name of the AI model used in the HMT D365FFO tools.")]
        public string AiModelName
        {
            get
            {
                return this.aiModelName;
            }
            set
            {
                this.aiModelName = value;
            }
        }

        private SecureString aiKey;
        public string EncryptedSecureString;
        public string aiModelName;
    }
}

