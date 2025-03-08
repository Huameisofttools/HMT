using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace HMT.Kernel
{
    /// <summary>
    /// Provides methods for encrypting and decrypting SecureString instances.
    /// Willie Yao - 03/08/2025
    /// </summary>
    /// <remarks>
    /// This class uses the Data Protection API (DPAPI) to encrypt and decrypt SecureString instances.
    /// </remarks>
    public static class SecureStringHelper
    {
        /// <summary>
        /// Encrypts a SecureString instance and returns the encrypted data as a Base64-encoded string.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <param name="secureString">The SecureString instance to encrypt.</param>
        /// <returns>A Base64-encoded string representing the encrypted data.</returns>
        public static string EncryptSecureString(SecureString secureString)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(ToInsecureString(secureString)),
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser
            );
            return Convert.ToBase64String(encryptedData);
        }

        /// <summary>
        /// Decrypts a Base64-encoded string to a SecureString instance.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <param name="encryptedBase64">The Base64-encoded string representing the encrypted data.</param>
        /// <returns>A SecureString instance containing the decrypted data.</returns>
        public static SecureString DecryptToSecureString(string encryptedBase64)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
            byte[] decryptedBytes = ProtectedData.Unprotect(
                encryptedBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser
            );
            return ToSecureString(Encoding.UTF8.GetString(decryptedBytes));
        }

        /// <summary>
        /// Converts a SecureString instance to an insecure string.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <param name="secureString">The SecureString instance to convert.</param>
        /// <returns>An insecure string representing the SecureString instance.</returns>
        private static string ToInsecureString(SecureString secureString)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(secureString);
            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }

        /// <summary>
        /// Converts an insecure string to a SecureString instance.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <param name="value">The insecure string to convert.</param>
        /// <returns>A SecureString instance representing the insecure string.</returns>
        private static SecureString ToSecureString(string value)
        {
            SecureString secureString = new SecureString();
            foreach (char c in value)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            return secureString;
        }
    }
}
