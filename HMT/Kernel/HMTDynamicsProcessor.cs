using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace HMT.Kernel
{
    internal class HMTDynamicsProcessor
    {
        //private List<ClassDC> classList;

        //public List<ClassDC> getStructure()
        //{
        //    return this.classList;
        //}

        public void processClassXml(string _path)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(_path);
                foreach (object obj in doc.DocumentElement.ChildNodes)
                {
                    XmlNode node = (XmlNode)obj;
                }
            }
            catch
            {
            }
        }

        internal List<AxClassMemberVariable> getListOfMembers(object _obj)
        {
            List<AxClassMemberVariable> retList = null;
            bool flag = _obj != null;
            if (flag)
            {
                bool flag2 = _obj.GetType() == typeof(AxClass);
                if (flag2)
                {
                    AxClass classObj = (AxClass)_obj;
                    retList = classObj.Members;
                }
            }
            return retList;
        }

        internal AxClassMemberVariable findAxClassMemberVariable(List<AxClassMemberVariable> _list, string _name)
        {
            AxClassMemberVariable ret = null;
            foreach (AxClassMemberVariable var in _list)
            {
                bool flag = var.Name.Equals(_name);
                if (flag)
                {
                    ret = var;
                    break;
                }
            }
            return ret;
        }

        
    }
}
