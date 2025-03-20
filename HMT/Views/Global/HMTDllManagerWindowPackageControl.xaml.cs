namespace HMT.Views.Global
{
    using Microsoft.Win32;
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for HMTDllManagerWindowPackageControl.
    /// </summary>
    public partial class HMTDllManagerWindowPackageControl : UserControl
    {
        private string TargetPath = string.Empty;

        public ObservableCollection<DllFileInfo> DllFiles { get; } = new ObservableCollection<DllFileInfo>();

        public HMTDllManagerWindowPackageControl()
        {
            this.InitializeComponent();
            DataContext = this;
            LoadExistingDlls();
            TargetPath = FindExtensionFolder();
        }

        private string FindExtensionFolder()
        {
            string environmentVariable = Environment.GetEnvironmentVariable("DynamicsVSTools");
            if (string.IsNullOrEmpty(environmentVariable))
            {
                throw new ApplicationException("Could not find D365FO tools in Windows registry.");
            }
            return Path.Combine(environmentVariable, "AddinExtensions");
        }

        private void LoadExistingDlls()
        {
            if (!Directory.Exists(TargetPath)) return;

            DllFiles.Clear();
            foreach (var file in Directory.GetFiles(TargetPath, "*.dll"))
            {
                DllFiles.Add(new DllFileInfo(file));
            }
        }

        private void ImportDll_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "DLL files|*.dll"
            };

            if (dialog.ShowDialog() == true)
            {
                Directory.CreateDirectory(TargetPath);
                foreach (var file in dialog.FileNames)
                {
                    try
                    {
                        var assembly = Assembly.ReflectionOnlyLoadFrom(file);
                        var references = assembly.GetReferencedAssemblies();
                        if (references.Any(r => r.Name == "Contoso"))
                        {
                            throw new InvalidOperationException("Importing DLLs with Contoso dependency is prohibited");
                        }

                        var dest = Path.Combine(TargetPath, Path.GetFileName(file));
                        File.Copy(file, dest, overwrite: true);
                        DllFiles.Add(new DllFileInfo(dest));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Import failed [{Path.GetFileName(file)}]:\n{ex.Message}");
                    }
                }
            }
        }

        // Property View
        private void ContextMenu_Properties_Click(object sender, RoutedEventArgs e)
        {
            if (DllList.SelectedItem is DllFileInfo selected)
            {
                var info = new FileInfo(selected.FullPath);
                MessageBox.Show($"File size: {info.Length / 1024}KB\n" +
                              $"Last modified: {info.LastWriteTime}");
            }
        }

        // Reload
        private void ContextMenu_Reload_Click(object sender, RoutedEventArgs e)
        {
            LoadExistingDlls();
        }

        // Open containing folder
        private void ContextMenu_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (DllList.SelectedItem is DllFileInfo selected)
            {
                Process.Start("explorer.exe", $"/select,\"{selected.FullPath}\"");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.Trim();
            var view = CollectionViewSource.GetDefaultView(DllFiles);

            view.Filter = item =>
            {
                if (string.IsNullOrEmpty(searchText)) return true;
                var dll = item as DllFileInfo;
                return dll?.FileName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
            };
        }

        private void DeleteDll_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (!(button.Tag is string filePath)) return;

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    foreach (var item in DllFiles)
                    {
                        if (item.FullPath == filePath)
                        {
                            DllFiles.Remove(item);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Deletion failed: {ex.Message}");
            }
        }
    }

    public class DllFileInfo
    {
        public string Version { get; }
        public string FileName { get; }
        public string FullPath { get; }

        public DllFileInfo(string path)
        {
            FullPath = path;
            FileName = Path.GetFileName(path);

            try
            {
                Version = FileVersionInfo.GetVersionInfo(path).FileVersion ?? "Unknown";
            }
            catch
            {
                Version = "Invalid";
            }
        }
    }
}