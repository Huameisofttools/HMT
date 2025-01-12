using System;
using System.Globalization;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.Dynamics.AX.Metadata.Core.Collections;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell;

namespace HMT.Kernel
{
    internal class HMTPasteText
    {
        private EnvDTE80.DTE2 dte;
        private EnvDTE.Document doc;
        private EnvDTE.TextSelection text;
        private Find findPoint;
        private string parmMethod;
        private ListBox.SelectedObjectCollection selectedItems;
        private object obj;

        public HMTPasteText(EnvDTE80.DTE2 _dte, string _method)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.dte = _dte;
            this.doc = _dte.ActiveDocument;
            this.text = (EnvDTE.TextSelection)this.doc.Selection;
            this.findPoint = _dte.ActiveDocument.DTE.Find;
            this.doc.Save("");
            bool flag = _method.Equals("Find");
            if (flag)
            {
                this.prepareFindMethod();
                this.prepareFindMethodByPrimaryIndex();
            }
            bool flag2 = _method.Equals("Exist");
            if (flag2)
            {
                this.prepareExistMethod();
            }
        }

        // Token: 0x0600007E RID: 126 RVA: 0x000041E4 File Offset: 0x000023E4
        public HMTPasteText(EnvDTE80.DTE2 _dte, string parmMethod, object _AxElement, ListBox.SelectedObjectCollection selectedItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.dte = this.dte;
            this.parmMethod = parmMethod;
            this.selectedItems = selectedItems;
            this.dte = _dte;
            this.doc = _dte.ActiveDocument;
            this.text = (EnvDTE.TextSelection)this.doc.Selection;
            this.findPoint = _dte.ActiveDocument.DTE.Find;
            bool flag = parmMethod.Equals("Parm");
            if (flag)
            {
                this.prepareParmMethods(_AxElement, selectedItems);
            }
        }

        // Token: 0x0600007F RID: 127 RVA: 0x00004270 File Offset: 0x00002470
        private void prepareParmMethods(object _AxElement, ListBox.SelectedObjectCollection selectedItems)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            AxClass classObj = null;
            bool flag = _AxElement != null;
            if (flag)
            {
                bool flag2 = _AxElement.GetType() == typeof(AxClass);
                if (flag2)
                {
                    classObj = (AxClass)_AxElement;
                }
                foreach (object element in selectedItems)
                {
                    HMTDynamicsProcessor processor = new HMTDynamicsProcessor();
                    AxClassMemberVariable var = processor.findAxClassMemberVariable(processor.getListOfMembers(classObj), element.ToString());
                    bool flag3 = var != null;
                    if (flag3)
                    {
                        this.prepareParmMethod(classObj, var);
                    }
                }
            }
        }

        private void prepareParmMethod(AxClass classObj, AxClassMemberVariable var)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string name = classObj.Name;
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            this.text.EndOfDocument(false);
            for (int i = this.text.CurrentLine; i >= 1; i--)
            {
                this.text.SelectLine();
                bool flag = this.text.Text.IndexOf("}") >= 0;
                if (flag)
                {
                    this.text.StartOfLine(0, false);
                    this.text.NewLine(1);
                    break;
                }
                this.text.LineUp(false, 1);
            }
            int commentLine = this.text.CurrentLine;
            this.text.StartOfLine(0, false);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(string.Concat(new string[]
            {
                HMTTemplate.tab1,
                "public ",
                HMTUtils.getAxType(var),
                " parm",
                char.ToUpperInvariant(var.Name[0]).ToString(),
                var.Name.Substring(1),
                "(",
                HMTUtils.getAxType(var),
                " _",
                var.Name,
                " = ",
                var.Name,
                ")"
            }), 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(HMTTemplate.tab1 + "{", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(string.Concat(new string[]
            {
                HMTTemplate.tab2,
                var.Name,
                " = _",
                var.Name,
                ";"
            }), 1);
            this.text.NewLine(1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(HMTTemplate.tab2 + "return " + var.Name + ";", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(HMTTemplate.tab1 + "}", 1);
            this.text.NewLine(1);
            this.text.SelectLine();
            this.text.Insert(this.text.Text.Trim(), 1);
            this.text.GotoLine(commentLine, false);
            this.text.Insert(HMTTemplate.tab1 + "/// <summary>", 1);
            this.text.NewLine(1);
            this.text.Insert("Standard parm method.", 1);
            this.text.NewLine(1);
            this.text.Insert("</summary>", 1);
            this.text.NewLine(1);
            this.text.Insert("<param name = \"_" + var.Name + "\">Value to set.</param>", 1);
            this.text.NewLine(1);
            this.text.Insert("<returns>Return value.</returns>", 1);
        }

        public void paste()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.doc.Save("");
        }

        private void prepareFindMethodByPrimaryIndex()
        {
            ThreadHelper.ThrowIfNotOnUIThread("prepareFindMethodByPrimaryIndex");
            AxTable table = (AxTable)HMTUtils.getAOTObjectByName(this.doc.Name);
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            this.text.EndOfDocument(false);
            for (int i = this.text.CurrentLine; i >= 1; i--)
            {
                this.text.SelectLine();
                bool flag = this.text.Text.IndexOf("}") >= 0;
                if (flag)
                {
                    this.text.StartOfLine(0, false);
                    this.text.NewLine(1);
                    break;
                }
                this.text.LineUp(false, 1);
            }
            this.text.StartOfLine(0, false);
            try
            {
                FileCodeModel fcm = this.dte.ActiveDocument.ProjectItem.FileCodeModel;
                for (int j = 1; j <= fcm.CodeElements.Count; j++)
                {
                    CodeClass c = fcm.CodeElements.Item(j) as CodeClass;
                    MessageBox.Show(c.Name);
                }
            }
            catch
            {
            }
            string inputAttributes = "";
            KeyedObjectCollection<AxTableField> list = HMTUtils.getPrimaryIndexFields(table);
            foreach (AxTableField field in list)
            {
                inputAttributes = string.Concat(new string[]
                {
                    inputAttributes,
                    HMTUtils.getAxFieldType(field),
                    " _",
                    field.Name,
                    ", "
                });
            }
            int commentLine = this.text.CurrentLine;
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(string.Concat(new string[]
            {
                "\tpublic static ",
                table.Name,
                " find(",
                inputAttributes,
                "boolean _selectForUpdate = false)"
            }), 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t{", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(string.Concat(new string[]
            {
                "\t\t",
                table.Name,
                " ",
                char.ToLowerInvariant(table.Name[0]).ToString(),
                table.Name.Substring(1),
                ";"
            }), 1);
            this.text.NewLine(1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\t" + char.ToLowerInvariant(table.Name[0]).ToString() + table.Name.Substring(1) + ".selectForUpdate(_selectForUpdate);", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\tselect firstonly " + char.ToLowerInvariant(table.Name[0]).ToString() + table.Name.Substring(1), 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(string.Concat(new string[]
            {
                "\t\t\twhere ",
                char.ToLowerInvariant(table.Name[0]).ToString(),
                table.Name.Substring(1),
                ".",
                list[0].Name,
                " == _",
                list[0].Name
            }), 1);
            for (int k = 1; k < list.Count; k++)
            {
                this.text.NewLine(1);
                this.text.StartOfLine(0, false);
                bool flag2 = k < list.Count - 1;
                if (flag2)
                {
                    this.text.Insert(string.Concat(new string[]
                    {
                        "\t\t\t\t&& ",
                        char.ToLowerInvariant(table.Name[0]).ToString(),
                        table.Name.Substring(1),
                        ".",
                        list[k].Name,
                        " == _",
                        list[k].Name
                    }), 1);
                }
                else
                {
                    this.text.Insert(string.Concat(new string[]
                    {
                        "\t\t\t\t&& ",
                        char.ToLowerInvariant(table.Name[0]).ToString(),
                        table.Name.Substring(1),
                        ".",
                        list[k].Name,
                        " == _",
                        list[k].Name,
                        ";"
                    }), 1);
                }
            }
            this.text.NewLine(1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\treturn " + char.ToLowerInvariant(table.Name[0]).ToString() + table.Name.Substring(1) + ";", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t}", 1);
            this.text.NewLine(1);
            this.text.SelectLine();
            this.text.Insert(this.text.Text.Trim(), 1);
            this.text.GotoLine(commentLine, false);
            this.text.Insert("\t/// <summary>", 1);
            this.text.NewLine(1);
            this.text.Insert("", 1);
            this.text.NewLine(1);
            this.text.Insert("/// </summary>", 1);
            foreach (AxTableField field2 in list)
            {
                this.text.NewLine(1);
                this.text.Insert("<param name = \"_" + field2.Name + "\"></param>", 1);
            }
            this.text.NewLine(1);
            this.text.Insert("<param name = \"_selectForUpdate\"></param>", 1);
            this.text.NewLine(1);
            this.text.Insert("<returns></returns>", 1);
        }

        private void prepareFindMethod()
        {
            ThreadHelper.ThrowIfNotOnUIThread("prepareFindMethod");
            string tableName = "";
            tableName = this.doc.Name.Replace("AxTable_", "");
            tableName = tableName.Replace(".xpp", "");
            AxTable table = (AxTable)HMTUtils.getAOTObjectByName(this.doc.Name);
            CodeElement objCodeCls = this.text.ActivePoint.get_CodeElement((vsCMElement)1);
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            this.text.EndOfDocument(false);
            for (int i = this.text.CurrentLine; i >= 1; i--)
            {
                this.text.SelectLine();
                bool flag = this.text.Text.IndexOf("}") >= 0;
                if (flag)
                {
                    this.text.StartOfLine(0, false);
                    this.text.NewLine(1);
                    break;
                }
                this.text.LineUp(false, 1);
            }
            this.text.StartOfLine(0, false);
            try
            {
                FileCodeModel fcm = this.dte.ActiveDocument.ProjectItem.FileCodeModel;
                for (int j = 1; j <= fcm.CodeElements.Count; j++)
                {
                    CodeClass c = fcm.CodeElements.Item(j) as CodeClass;
                    MessageBox.Show(c.Name);
                }
            }
            catch
            {
            }
            int commentLine = this.text.CurrentLine;
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\tpublic static " + table.Name + " findByRecId(RefRecId _recId, boolean _selectForUpdate = false)", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t{", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(string.Concat(new string[]
            {
                "\t\t",
                table.Name,
                " ",
                char.ToLowerInvariant(table.Name[0]).ToString(),
                table.Name.Substring(1),
                ";"
            }), 1);
            this.text.NewLine(1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\t" + char.ToLowerInvariant(table.Name[0]).ToString() + table.Name.Substring(1) + ".selectForUpdate(_selectForUpdate);", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\tselect firstonly " + char.ToLowerInvariant(tableName[0]).ToString() + table.Name.Substring(1), 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\t\twhere " + char.ToLowerInvariant(table.Name[0]).ToString() + table.Name.Substring(1) + ".RecId == _recId;", 1);
            this.text.NewLine(1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\treturn " + char.ToLowerInvariant(table.Name[0]).ToString() + table.Name.Substring(1) + ";", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t}", 1);
            this.text.NewLine(1);
            this.text.SelectLine();
            this.text.Insert(this.text.Text.Trim(), 1);
            this.text.GotoLine(commentLine, false);
            this.text.Insert("\t/// <summary>", 1);
            this.text.NewLine(1);
            this.text.Insert("", 1);
            this.text.NewLine(1);
            this.text.Insert("/// </summary>", 1);
            this.text.NewLine(1);
            this.text.Insert("<param name = \"_recId\"></param>", 1);
            this.text.NewLine(1);
            this.text.Insert("<param name = \"_selectForUpdate\"></param>", 1);
            this.text.NewLine(1);
            this.text.Insert("<returns></returns>", 1);
        }

        private void prepareExistMethod()
        {
            ThreadHelper.ThrowIfNotOnUIThread("prepareExistMethod");
            string tableName = "";
            tableName = this.doc.Name.Replace("AxTable_", "");
            tableName = tableName.Replace(".xpp", "");
            CodeElement objCodeCls = this.text.ActivePoint.get_CodeElement((vsCMElement)1);
            TextInfo info = CultureInfo.CurrentCulture.TextInfo;
            this.text.EndOfDocument(false);
            for (int i = this.text.CurrentLine; i >= 1; i--)
            {
                this.text.SelectLine();
                bool flag = this.text.Text.IndexOf("}") >= 0;
                if (flag)
                {
                    this.text.StartOfLine(0, false);
                    this.text.NewLine(1);
                    break;
                }
                this.text.LineUp(false, 1);
            }
            this.text.StartOfLine(0, false);
            try
            {
                FileCodeModel fcm = this.dte.ActiveDocument.ProjectItem.FileCodeModel;
                for (int j = 1; j <= fcm.CodeElements.Count; j++)
                {
                    CodeClass c = fcm.CodeElements.Item(j) as CodeClass;
                    MessageBox.Show(c.Name);
                }
            }
            catch
            {
            }
            int commentLine = this.text.CurrentLine;
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\tpublic static boolean exist(RefRecId _recId)", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t{", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert(string.Concat(new string[]
            {
                "\t\t",
                tableName,
                " ",
                char.ToLowerInvariant(tableName[0]).ToString(),
                tableName.Substring(1),
                ";"
            }), 1);
            this.text.NewLine(1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\treturn _recId", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\t\t&& (select firstOnly RecId from " + char.ToLowerInvariant(tableName[0]).ToString() + tableName.Substring(1), 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t\t\t\twhere " + tableName + ".RecId == _recId).RecId != 0;", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(0, false);
            this.text.Insert("\t}", 1);
            this.text.NewLine(1);
            this.text.SelectLine();
            this.text.Insert(this.text.Text.Trim(), 1);
            this.text.GotoLine(commentLine, false);
            this.text.Insert("\t/// <summary>", 1);
            this.text.NewLine(1);
            this.text.Insert("", 1);
            this.text.NewLine(1);
            this.text.Insert("/// </summary>", 1);
            this.text.NewLine(1);
            this.text.Insert("<param name = \"_recId\"></param>", 1);
            this.text.NewLine(1);
            this.text.Insert("<returns></returns>", 1);
        }

        
    }
}
