using System;
using System.Windows.Forms;

namespace HMT.Kernel
{
    public abstract class HMTTemplate
    {
        public static string tab1 = "    ";

        public static string tab2 = "        ";

        public static string tab3 = "            ";

        public static string tab4 = "                ";

        public static string tab5 = "                    ";

        public static string space = " ";

        public static string classStr = "class";

        public static string openFigural = "{";

        public static string closeFigural = "}";

        public static string openScope = "(";

        public static string closeScope = ")";

        public static string returnStr = "return";

        public static string extends = "extends";

        public static string comma = ",";

        public static string dotComma = ";";

        protected EnvDTE80.DTE2 dte;

        protected EnvDTE.Document doc;

        protected EnvDTE.TextSelection text;

        protected string method;

        protected ListBox.SelectedObjectCollection selectedItems;

        protected object obj;

        protected IServiceProvider provider = null;
        public HMTTemplate(EnvDTE80.DTE2 _dte, string _method, object _AxElement = null, ListBox.SelectedObjectCollection _selectedItems = null)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            this.method = _method;
            this.selectedItems = _selectedItems;
            this.dte = _dte;
            bool flag = this.dte != null;
            if (flag)
            {
                this.doc = this.dte.ActiveDocument;
            }
            bool flag2 = this.doc != null;
            if (flag2)
            {
                try
                {
                    this.text = (EnvDTE.TextSelection)this.doc.Selection;
                }
                catch
                {
                }
            }
        }

        public HMTTemplate(EnvDTE80.DTE2 _dte)
        {
            this.dte = _dte;
        }

        public abstract bool validate();

        public void run()
        {
            bool flag = this.validate();
            if (flag)
            {
                this.paste();
            }
        }

        public abstract void paste();

        public void setProvider(IServiceProvider _provider)
        {
            this.provider = _provider;
        }
    }
}
