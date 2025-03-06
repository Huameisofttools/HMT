using System;
using System.ComponentModel;
using System.Text;
using Microsoft.VisualStudio.Shell;
using HMT.Views.Settings;
using System.Windows.Forms;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using System.Security.Cryptography;
using System.Windows.Forms.Integration;

namespace HMT.OptionsPane
{
    internal class HMTCoplotOptions : DialogPage
    {
        private HMTCopilotOptionPageControl _page;

        protected override IWin32Window Window
        {
            get
            {
                if (_page == null)
                {
                    _page = new HMTCopilotOptionPageControl();
                }

                // 创建 ElementHost 并将 WPF 控件作为其子元素
                ElementHost elementHost = new ElementHost
                {
                    Child = _page,
                    Dock = DockStyle.Fill
                };

                // 创建一个 Windows Forms 容器来承载 ElementHost
                Form form = new Form
                {
                    Text = "Options",
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };
                form.Controls.Add(elementHost);

                return form;
            }
        }

        public string Password { get; set; }

        // 集合名称
        string collectionName = "HMT";

        // 属性名称
        string propertyName = "ApiKey";

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();

            // 获取 Visual Studio 的设置存储
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var writableSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            // 检查集合是否存在
            if (writableSettingsStore.CollectionExists(collectionName))
            {
                // 检查属性是否存在
                if (writableSettingsStore.PropertyExists(collectionName, propertyName))
                {
                    // 加载加密的密码
                    string encryptedPassword = writableSettingsStore.GetString(collectionName, propertyName);
                    Password = Decrypt(encryptedPassword);
                }
            }
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            // 获取 Visual Studio 的设置存储
            var settingsManager = new ShellSettingsManager(ServiceProvider.GlobalProvider);
            var writableSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

            // 创建集合（如果不存在）
            if (!writableSettingsStore.CollectionExists(collectionName))
            {
                writableSettingsStore.CreateCollection(collectionName);
            }

            // 保存加密的密码
            string encryptedPassword = Encrypt(Password);
            writableSettingsStore.SetString(collectionName, propertyName, encryptedPassword);
        }

        // 简单的加密方法（仅用于示例，生产环境中应使用更安全的方法）
        private string Encrypt(string plainText)
        {
            byte[] data = Encoding.Unicode.GetBytes(plainText);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        // 简单的解密方法
        private string Decrypt(string encryptedText)
        {
            byte[] data = Convert.FromBase64String(encryptedText);
            byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.Unicode.GetString(decrypted);
        }
    }
}
