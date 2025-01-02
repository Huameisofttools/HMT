using HMT.HMTAXEditorUtils.HMTParmMethodGenerator;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.Core.Collections;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Maps;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;

namespace HMT.HMTTable.HMTFindExistMethodGenerator
{
    public partial class HMTFindExistMethodGeneratorDialog : Form
    {
        HMTFindExistMethodGenerateService findExistMethodGenerateService;
        bool generateForRecId = false;

        public HMTFindExistMethodGeneratorDialog()
        {
            InitializeComponent();
        }

        public void initParameters(HMTFindExistMethodGenerateService _service)
        {
            findExistMethodGenerateService = _service;
            initComboBox1();
        }

        public void initComboBox1()
        {
            comboBox1.SelectedItem = "Other Field";

            if (comboBox1.SelectedItem.ToString() == "Other Field")
            {
                checkedListBox1.Enabled = true;
                generateForRecId = false;
                textBox1.Text = "";
                textBox2.Text = "";
                setCheckedListBox1();
            }
            else
            {
                checkedListBox1.Enabled = false;
                checkedListBox1.Items.Clear();
                textBox1.Text = "findRecId";
                textBox2.Text = "exist";
                generateForRecId = true;
            }
        }

        public void setCheckedListBox1()
        {
            checkedListBox1.Items.Clear();

            KeyedObjectCollection<AxTableField> tableFieldCollection = findExistMethodGenerateService.axTable.Fields;

            foreach (AxTableField axTableField in tableFieldCollection)
            {
                checkedListBox1.Items.Add(axTableField.Name);
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (generateForRecId)
                {
                    generateMethodForRecId();
                }
                else
                {
                    if (!generateMethodForOtherField()) return;
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void generateMethodForRecId()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var findMethodName = textBox1.Text;
            var existMethodName = textBox2.Text;

            if (!string.IsNullOrEmpty(findMethodName))
            {
                var methodString = findExistMethodGenerateService.generateFindMethod(findMethodName, "");
                var axMethod = HMTFindExistMethodGenerateService.addMethod(findMethodName, methodString);
                findExistMethodGenerateService.axTable.AddMethod(axMethod);
            }

            if (!string.IsNullOrEmpty(existMethodName))
            {
                var existString = findExistMethodGenerateService.generateExistMethod(existMethodName, "");
                var axMethod = HMTFindExistMethodGenerateService.addMethod(existMethodName, existString);
                findExistMethodGenerateService.axTable.AddMethod(axMethod);
            }

            findExistMethodGenerateService.updateAxTable();
        }

        private Boolean generateMethodForOtherField()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var axTableFields = findExistMethodGenerateService.axTable.Fields;
            var checkedItems = checkedListBox1.CheckedItems;
            var findMethodName = textBox1.Text;
            var existMethodName = textBox2.Text;

            if (checkedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one fields.");
                return false;
            }

            Dictionary<string, string> typeAndFieldMap = new Dictionary<string, string>();
            foreach (var field in checkedItems)
            {
                var memberName = field.ToString();
                var currentField = axTableFields.FirstOrDefault(f => f.Name == memberName);

                if (currentField == null)
                {
                    continue;
                }

                if (currentField.ExtendedDataType != "")
                {
                    typeAndFieldMap.Add(memberName, currentField.ExtendedDataType);
                }
                else
                {
                    int a = 41;
                    var classTypeString = currentField.GetType().ToString().Substring(a);
                    var fieldType = AxTypeHelper.getTableFieldTypeStr(classTypeString);
                    typeAndFieldMap.Add(memberName, fieldType);
                }
            }

            if (typeAndFieldMap.Count != 0)
            {
                string parameters = string.Join(", ", typeAndFieldMap.Select(item => $"{item.Value} _{item.Key}"));

                if (!string.IsNullOrEmpty(findMethodName))
                {
                    var methodString = findExistMethodGenerateService.generateFindMethod(findMethodName, parameters);
                    var axMethod = HMTFindExistMethodGenerateService.addMethod(findMethodName, methodString);
                    findExistMethodGenerateService.axTable.AddMethod(axMethod);
                }

                if (!string.IsNullOrEmpty(existMethodName))
                {
                    var existString = findExistMethodGenerateService.generateExistMethod(existMethodName, parameters);
                    var axMethod = HMTFindExistMethodGenerateService.addMethod(existMethodName, existString);
                    findExistMethodGenerateService.axTable.AddMethod(axMethod);
                }

                findExistMethodGenerateService.updateAxTable();
                return true;
            }
            return false;
        }

        private void GenerateForMofied(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Other Field")
            {
                checkedListBox1.Enabled = true;
                generateForRecId = false;
                textBox1.Text = "";
                textBox2.Text = "";
                setCheckedListBox1();
            }
            else
            {
                checkedListBox1.Enabled = false;
                checkedListBox1.Items.Clear();
                textBox1.Text = "findRecId";
                textBox2.Text = "exist";
                generateForRecId = true;
            }
        }
    }
}
