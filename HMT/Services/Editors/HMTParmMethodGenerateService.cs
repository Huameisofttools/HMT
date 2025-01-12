using Microsoft.Dynamics.AX.Metadata.MetaModel;
using EnvDTE;
using HMT.Kernel;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace HMT.HMTAXEditorUtils.HMTParmMethodGenerator
{
    public class HMTParmMethodGenerateService : HMTTemplate
    {
        public AxClass axClass;
        private new object obj;
        public AxClassMemberVariable axMemberVariable;

        /// <summary>
        /// AxClass method parm method
        /// </summary>
        /// <param name="_axClass">AxClass</param>
        public void parmAxClass(AxClass _axClass)
        {
            axClass = _axClass;
        }

        /// <summary>
        /// AxClassMemberVariable parm method
        /// </summary>
        /// <param name="axClassMemberVariable">AxClassMemberVariable</param>
        public void parmAxClassMemberVariable(AxClassMemberVariable axClassMemberVariable)
        {
            axMemberVariable = axClassMemberVariable;
        }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="_dte">DTE</param>
        /// <param name="_method">method</param>
        /// <param name="_AxElement">AxElement</param>
        public HMTParmMethodGenerateService(EnvDTE80.DTE2 _dte, string _method, object _AxElement = null) : base(_dte, _method, _AxElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                this.obj = HMTUtils.getAOTObjectByName(this.doc.Name);
            }
            catch
            {
            }
        }

        public override bool validate()
        {
            return true;
        }

        public override void paste()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                this.prepareParmMethod(axClass, axMemberVariable);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Willie Yao - 01/07/2024
        /// Add parm method for class
        /// </summary>
        /// <param name="classObj">AxClass</param>
        /// <param name="var">AxClassMemberVariable</param>
        private void prepareParmMethod(AxClass classObj, AxClassMemberVariable var)
        {                       
            ThreadHelper.ThrowIfNotOnUIThread();
            string name = classObj.Name;
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            this.text.EndOfDocument(false);
            for (int i = this.text.CurrentLine; i >= 1; i--)
            {
                this.text.SelectLine();
                bool flag = this.text.Text.IndexOf("}") >= 0;
                if (flag)
                {
                    this.text.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
                    this.text.NewLine(1);
                    break;
                }
                this.text.LineUp(false, 1);
            }
            int currentLine = this.text.CurrentLine;
            this.text.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
            this.text.NewLine(1);
            this.text.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
            this.text.Insert(string.Concat(new string[]
            {
                HMTTemplate.tab1,
                "public ",
                HMTUtils.getAxType(var),
                ((var.Name[0] == 'g') ? " parm" + var.Name.Substring(1) : " parm" + char.ToUpper(var.Name[0]) + var.Name.Substring(1)),
                "(",
                HMTUtils.getAxType(var),
                " _",
                ((var.Name[0] == 'g') ? char.ToLower(var.Name.Substring(1)[0]) + var.Name.Substring(2) : var.Name),
                " = ",
                var.Name,
                ")"
            }), 1);
            this.text.NewLine(1);
            this.text.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
            this.text.Insert(HMTTemplate.tab1 + "{", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
            this.text.Insert(string.Concat(new string[]
            {
                HMTTemplate.tab2,
                var.Name,
                " = _",
                ((var.Name[0] == 'g') ? char.ToLower(var.Name.Substring(1)[0]) + var.Name.Substring(2) : var.Name),
                ";"
            }), 1);
            this.text.NewLine(1);
            this.text.NewLine(1);
            this.text.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
            this.text.Insert(HMTTemplate.tab2 + "return " + var.Name + ";", 1);
            this.text.NewLine(1);
            this.text.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstColumn, false);
            this.text.Insert(HMTTemplate.tab1 + "}", 1);
            this.text.NewLine(1);
            this.text.SelectLine();
            this.text.Insert(this.text.Text.Trim(), 1);
            this.text.GotoLine(currentLine, false);
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
    }

}
