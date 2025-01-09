using HMT.Kernel;
using HMT.Models;
using HMT.OptionsPane;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace HMT.Views.Global
{
    /// <summary>
    /// Willie Yao - 01/08/2025
    /// Interaction logic for HMTJsonToDataContractWindowControl.
    /// </summary>
    public partial class HMTJsonToDataContractWindowControl : UserControl
    {
        HMTJsonToDataContractToolWindowData Data;

        public HMTJsonToDataContractWindowControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Willie Yao - 01/08/2025
        /// Initializes a new instance of the <see cref="HMTJsonToDataContractWindowControl"/> class.
        /// </summary>
        public HMTJsonToDataContractWindowControl(HMTJsonToDataContractToolWindowData data) : this()
        {            
            Data = data;
        }

        /// <summary>
        /// Willie Yao - 01/08/2025
        /// LoadJsonButton click method
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void LoadJsonButton_Click(object sender, RoutedEventArgs e)
        {
            jsonTreeView.Items.Clear();  
            string jsonString = jsonInput.Text;
            if (jsonString == null) 
            { 
                return;
            }
            try
            {
                JObject jsonData = JObject.Parse(jsonString);
                PopulateTreeView(jsonData, jsonTreeView);
            }
            catch (JsonException ex)
            {
                MessageBox.Show("Error parsing JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message);
            }
        }

        /// <summary>
        /// Willie Yao - 01/08/2025
        /// Populate tree view by json data
        /// </summary>
        /// <param name="token">JToken</param>
        /// <param name="parent">ItemsControl</param>
        private void PopulateTreeView(JToken token, ItemsControl parent)
        {
            if (token is JProperty property)
            {
                bool ExistKeyNode = false;
                //  Validate whether the same items exist at the same level.
                foreach (TreeViewItem subItems in parent.Items)
                {
                    if ((string)subItems.Header == property.Name)
                    {
                        ExistKeyNode = true;
                        break;
                    }
                }

                if (!ExistKeyNode)
                {
                    var node = new TreeViewItem { Header = property.Name };
                    parent.Items.Add(node);
                    PopulateTreeView(property.Value, node);
                }                
            }
            else if (token is JObject obj)
            {
                foreach (var prop in obj.Properties())
                {
                    PopulateTreeView(prop, parent);
                }
            }
            else if (token is JArray array)
            {
                var node = new TreeViewItem { Header = "Array" };
                parent.Items.Add(node);
                foreach (var item in array)
                {
                    PopulateTreeView(item, node);
                }
            }
            else // JValue
            {
                parent.Items.Add(new TreeViewItem { Header = token.ToString() });
            }
        }

        /// <summary>
        /// Willie Yao - 01/08/2025
        /// Generate code click method
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (string.IsNullOrWhiteSpace(jsonInput.Text))
            {
                MessageBox.Show("Please enter JSON before generating X++.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(className.Text))
            {
                MessageBox.Show("Please enter Class Name before generating X++.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                JObject jsonObject = JObject.Parse(jsonInput.Text);
                GenerateXppDataContract(jsonObject, HMTOptionsUtils.getPrefix(Data.Package), className.Text);
                
                jsonInput.Clear();
                className.Clear();                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating X++: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Willie Yao - 01/08/2025
        /// Generate xpp contract class
        /// </summary>
        /// <param name="jsonObject">JObject</param>
        /// <param name="prefix">Prefix</param>
        /// <param name="className">ClassName</param>
        public static void GenerateXppDataContract(JObject jsonObject, string prefix, string className)
        {
            StringBuilder sb = new StringBuilder();
            AxHelper axHelper = new AxHelper();
            className = char.ToUpper(className[0]) + className.Substring(1);
            AxClass newClass = new AxClass() { Name = $"{prefix}{className}", IsPublic = true };
            newClass.IsPublic = true;

            sb.AppendLine("[DataContractAttribute]");
            sb.AppendLine($"class {prefix}{className}");
            sb.AppendLine("{");

            foreach (var property in jsonObject.Properties())
            { 
                if (property.Value is JArray)
                {
                    sb.AppendLine($"    List {property.Name};");
                    sb.AppendLine();
                }
                else
                {
                    string typeName = "str";
                    sb.AppendLine($"    {typeName} {property.Name};");
                    sb.AppendLine();
                }
            }

            sb.AppendLine("}");
            // Set the declaration, including the class name and variable members.
            newClass.SourceCode.Declaration = sb.ToString();
            sb.Clear();

            int order = 1;
            foreach (var property in jsonObject.Properties())
            {
                string propertyName = property.Name;
                string wavePropertyName = char.ToUpper(property.Name[0]) + propertyName.Substring(1);
                if (property.Value is JArray)
                {
                    // Assuming all items in the array are similar. Get the first one only.
                    // Distinguish JObject and JValue
                    JToken firstItem = ((JArray)property.Value)[0];
                    if (firstItem is JValue)
                    {
                        string typeName = "List";
                        sb.AppendLine($"    [DataMemberAttribute(\"{propertyName}\"), SysOperationDisplayOrder('{order}')]");
                        sb.AppendLine($"    public {typeName} parm{wavePropertyName}({typeName} _{propertyName} = {propertyName})");
                        sb.AppendLine("    {");
                        sb.AppendLine($"        {propertyName} = _{propertyName};");
                        sb.AppendLine($"        return {propertyName};");
                        sb.AppendLine("    }");
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine($"    [DataMemberAttribute(\"{propertyName}\"), DataCollection(Types::Class, classStr({prefix}{wavePropertyName}Contract)), SysOperationDisplayOrder('{order}')]");
                        sb.AppendLine($"    public List parm{wavePropertyName}(List _{propertyName} = {propertyName})");
                        sb.AppendLine("    {");
                        sb.AppendLine($"        {propertyName} = _{propertyName};");
                        sb.AppendLine($"        return {propertyName};");
                        sb.AppendLine("    }");
                        sb.AppendLine();

                        JObject subJsonObject = (JObject)firstItem;
                        GenerateXppDataContract(subJsonObject, prefix, $"{wavePropertyName}Contract"); // Recursive processing of JSON nodes
                    }                    
                }
                else
                {
                    string typeName = "str";
                    sb.AppendLine($"    [DataMemberAttribute(\"{propertyName}\"), SysOperationDisplayOrder('{order}')]");
                    sb.AppendLine($"    public {typeName} parm{wavePropertyName}({typeName} _{propertyName} = {propertyName})");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        {propertyName} = _{propertyName};");
                    sb.AppendLine($"        return {propertyName};");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }
                order++;

                newClass.AddMethod(new AxMethod()
                {
                    Name = $"parm{wavePropertyName}",
                    Source = sb.ToString()
                });

                sb.Clear();
            }

            axHelper.MetaModelService.CreateClass(newClass, axHelper.ModelSaveInfo);
            axHelper.AppendToActiveProject(newClass);
        }
    }
}