using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HMT.Kernel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.AX.Metadata.Core.Collections;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace HMT.HMTAXEditorUtils.HMTParmMethodGenerator
{
    public partial class HMTParmMethodGenerateDialog : Form
    {
        HMTParmMethodGenerateService parmMethodGenerateService;

        public HMTParmMethodGenerateDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// HMTParmMethodGenerateDialog
        /// Willie Yao - 05/25/2024
        /// The main logic for adding the parm method code block 
        /// for all member variables.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var axClassMemberVariables = parmMethodGenerateService.axClass.Members;
                var checkedItems = checkedListBox1.CheckedItems;
                var objectCollection = checkedListBox1.Items;

                if (checkedItems.Count == 0)
                {
                    MessageBox.Show("Please select at least one member variable.");
                    return;
                }


                var objectCollectionList = objectCollection.Cast<object>().ToList();
                foreach (var memberVariable in checkedItems)
                {
                    var memberName = memberVariable.ToString();
                    var index = objectCollectionList.IndexOf(memberName);

                    if (index < 0)
                    {
                        continue;
                    }

                    var methodName = formatParmMethodNameFromMemberVariable(memberName);
                    var axClassMemberVariable = axClassMemberVariables[index];
                    var parmMethodCode = parmMethodGenerateService.generateParmMethod(axClassMemberVariable.Type, axClassMemberVariable.TypeName, memberName, methodName, formatParmMethodParameterNameFromMemberVariable(memberName), false);
                    var axMethod = HMTParmMethodGenerateService.addMethod(methodName, parmMethodCode);
                    axMethod.Visibility = CompilerVisibility.Public;
                    parmMethodGenerateService.axClass.AddMethod(axMethod);
                }
                parmMethodGenerateService.updateAxClass();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }                        
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            string selectedValue = comboBox1.SelectedItem.ToString();
            bool isPrivate = false;

            if (selectedValue == "Private")
            {
                isPrivate = true;
            }

            setCheckedListBox1(isPrivate);
        }

        public void initParameters(HMTParmMethodGenerateService service)
        {
            parmMethodGenerateService = service;

            initComboBox1();
        }

        protected string getParmMethodNameFromCheckedList(bool useSelected = false)
        {
            string parmMethodName = "";
            if (useSelected)
            {
                var allSelectedVariable = checkedListBox1.SelectedItems;   
                foreach (var memberVariable in allSelectedVariable)
                {
                    parmMethodName += "parm" + memberVariable + "\n";
                }
            }
            else
            {
                var allMemberVariable = checkedListBox1.Items;
                foreach (var memberVariable in allMemberVariable)
                {
                    parmMethodName += "parm" + memberVariable + "\n";
                }
            }           

            return parmMethodName;
        }

        public void initComboBox1()
        {
            comboBox1.SelectedItem = "All";
        }

        public void setCheckedListBox1(bool isPrivate = false)
        {
            checkedListBox1.Items.Clear();

            List<AxClassMemberVariable> axClassMemberVariables = parmMethodGenerateService.axClass.Members;
            
            foreach (AxClassMemberVariable axClassMemberVariable in axClassMemberVariables)
            {
                if (axClassMemberVariable.Visibility == CompilerVisibility.Private && isPrivate)
                {
                    checkedListBox1.Items.Add(axClassMemberVariable.Name);
                }

                if ((axClassMemberVariable.Visibility == CompilerVisibility.Public
                    || axClassMemberVariable.Visibility == CompilerVisibility.Private
                    || axClassMemberVariable.Visibility == CompilerVisibility.Protected
                    || axClassMemberVariable.Visibility == CompilerVisibility.Internal) 
                    && !isPrivate)
                {
                    checkedListBox1.Items.Add(axClassMemberVariable.Name);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selectedValue = comboBox1.SelectedItem.ToString();
            bool isPrivate = false;

            if (selectedValue == "Private")
            {
                isPrivate = true;
            }

            setCheckedListBox1(isPrivate);
        }

        /// <summary>
        /// Willie Yao - 12/25/2024
        /// If a user selected the "Select All" button, all check list boxes would be checked.​
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void SelectAllBtn_Clicked(object sender, EventArgs e)
        {
            for (int start = 0; start < checkedListBox1.Items.Count; start++)
            {
                checkedListBox1.SetItemChecked(start, SelectAllField.Checked);
            }
        }

        /// <summary>
        /// Willie Yao - 12/25/2024
        /// Let <c>comboBox1</c> can resize with the window change.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void comboBox_resize(object sender, EventArgs e)
        {
            AutoControlSize.ChangeFormControlSize(this);
        }

        /// <summary>
        /// Willie Yao - 12/25/2024
        /// Format method name from variable name.
        /// </summary>
        /// <param name="memberVariableName">Member variable name</param>
        /// <returns>ParmMethodName</returns>
        private string formatParmMethodNameFromMemberVariable(string memberVariableName)
        {
            char firstChar = memberVariableName[0];

            if (firstChar == 'g')
            {
                return "parm" + memberVariableName.Substring(1);
            }
            else
            {
                return "parm" + char.ToUpper(firstChar) + memberVariableName.Substring(1);
            }
        }

        /// <summary>
        /// Willie Yao - 12/25/2024
        /// Format parameter name from variable name.
        /// </summary>
        /// <param name="memberVariableName">Member variable name</param>
        /// <returns>Parameter name</returns>
        private string formatParmMethodParameterNameFromMemberVariable(string memberVariableName)
        {
            char firstChar = memberVariableName[0];

            if (firstChar == 'g')
            {
                char secondChar = memberVariableName[1];
                string memberVariableWithout_g = memberVariableName.Substring(1);
                return char.ToLower(secondChar) + memberVariableWithout_g.Substring(1);
            }
            else
            {
                return "parm" + char.ToUpper(firstChar) + memberVariableName.Substring(1);
            }
        }
    }
}
