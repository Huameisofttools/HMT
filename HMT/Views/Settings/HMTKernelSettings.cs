using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HMT.Services.Settings;
using Microsoft.VisualStudio.Shell;

namespace HMT.Views.Settings
{
    public partial class HMTKernelSettings : Form
    {
        private HMTKernelSettingsManager _kernelSettingsManager;

        private ExtensionTemplateDefaultScheme _extensionTemplateDefaultScheme = ExtensionTemplateDefaultScheme.Default;

        public AsyncPackage AsyncPackage;

        public HMTKernelSettings()
        {
            InitializeComponent();

            _kernelSettingsManager = new HMTKernelSettingsManager();
            _kernelSettingsManager.LoadSettings();
            RefreshFormControl();

            SettingsTypeComboBox.DataSource = Enum.GetValues(_extensionTemplateDefaultScheme.GetType());
        }

        public HMTKernelSettings(AsyncPackage asyncPackage)
        {
            InitializeComponent();

            _kernelSettingsManager = new HMTKernelSettingsManager();
            _kernelSettingsManager.LoadSettings(asyncPackage);
            AsyncPackage = asyncPackage;
            RefreshFormControl();

            SettingsTypeComboBox.DataSource = Enum.GetValues(_extensionTemplateDefaultScheme.GetType());

        }

        void RefreshFormControl()
        {
            string templateType, templateName;

            _kernelSettingsManager.InitFormControlData(out templateType, out templateName);
            ParametersBox.Text = templateType;
            ValuesControl.Text = templateName;

            DescriptionBox.Text = _kernelSettingsManager.GetDescription();

            FileNameTextBox.Text = _kernelSettingsManager.GetSettingsFilename();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            
            _kernelSettingsManager.LoadSettingsFromFormControlData(ValuesControl.Text, AsyncPackage);
        }
        private void SetDefaultButton_Click(object sender, EventArgs e)
        {
            Enum.TryParse<ExtensionTemplateDefaultScheme>(SettingsTypeComboBox.SelectedValue.ToString(), out _extensionTemplateDefaultScheme);

            _kernelSettingsManager.LoadDefaultSettings(_extensionTemplateDefaultScheme, AsyncPackage);
            RefreshFormControl();
        }

        private void SaveButton_Click_1(object sender, EventArgs e)
        {
            _kernelSettingsManager.LoadSettingsFromFormControlData(ValuesControl.Text, AsyncPackage);
            if (_kernelSettingsManager.SaveToFile())
            {
                MessageBox.Show($"Saved to file {_kernelSettingsManager.GetSettingsFilename()}", @"Save");
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = @"https://docs.microsoft.com/en-us/dynamics365/fin-ops-core/dev-itpro/extensibility/naming-guidelines-extensions";

            var si = new ProcessStartInfo(url);
            Process.Start(si);
        }
    }
}
