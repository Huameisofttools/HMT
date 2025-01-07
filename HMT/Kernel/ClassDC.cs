using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMT.Kernel
{
    internal class ClassDC
    {
        internal bool exists(ClassDC.VarDC _varDC)
        {
            bool ret = false;
            foreach (ClassDC.VarDC dc in this.vars)
            {
                bool flag = dc.name == _varDC.name;
                if (flag)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public string name;
        public string displayName;
        public string secureModificator;
        public List<ClassDC.VarDC> vars;
        public bool isEnabled;

        public class VarDC
        {
            public string name;
            public string displayName;
            public string type;
            public string baseType;
            public ClassDC classVar;
            public string secureModificator;
            public bool isEnabled;
        }
    }
}
