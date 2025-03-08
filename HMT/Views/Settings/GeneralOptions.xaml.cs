using HMT.OptionsPane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HMT.Views.Settings
{
    /// <summary>
    /// Interaction logic for GeneralOptions.xaml
    /// Willie Yao - 03/08/2025
    /// </summary>
    public partial class GeneralOptions : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralOptions"/> class.
        /// Willie Yao - 03/08/2025
        /// </summary>
        public GeneralOptions()
        {
            InitializeComponent();

            ModelComboBox.Items.Add("deepseek-V3");
        }

        /// <summary>
        /// Gets or sets the general options page.
        /// Willie Yao - 03/08/2025
        /// </summary>
        internal GeneralOptionPage generalOptionsPage;

        /// <summary>
        /// Gets or sets the password.
        /// Willie Yao - 03/08/2025
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Initializes the GeneralOptions control with saved settings.
        /// Willie Yao - 03/08/2025
        /// </summary>
        public void Initialize()
        {
            PasswordField.Password = General.Instance.EncryptedSecureString;
            ModelComboBox.SelectedItem = General.Instance.AiModelName;
            General.Instance.Save();
        }

        /// <summary>
        /// Handles the PasswordChanged event of the PasswordBox control.
        /// Updates the encrypted secure string and AI key in the general settings.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = ((PasswordBox)sender).Password;
            General.Instance.EncryptedSecureString = Password;
            General.Instance.Save();

            SecureString secureString = new SecureString();
            foreach (char c in Password)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();

            General.Instance.AiKey = secureString;
            General.Instance.Save();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ModelComboBox control.
        /// Updates the AI model name in the general settings.
        /// Willie Yao - 03/08/2025
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ModelComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string selectedModel = ModelComboBox.SelectedItem.ToString();
            General.Instance.AiModelName = selectedModel;
            General.Instance.Save();
        }
    }
}

