using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.Framework.Tools.Core;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using System;
using System.IO;
using System.Globalization;
using System.Xml.Linq;
//using System.Windows.Documents;
using System.Collections.Generic;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Commands;
using System.Xml;
using System.Runtime.Serialization;
using HMT.Kernel;
using System.Xaml;
using Microsoft.VisualStudio.Shell;

namespace HMT.HMTFunctionDemoGenerator
{
    public class HMTFunctionDemoGeneratorService
    {
        public static DTE gDTE
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;
            }
        }

        public static Project gProject;
        public VSProjectNode gActiveProjectNode;
        public IMetaModelService gMetaModelService;
        public ModelInfo gModel;
        public IDynamicsProjectService gService;
        public string gFolderPath; // The path to the custom templates files path
        public string gPreviewText;
        public string InternalPath; // The path to the system standard templates files path

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Initialize class
        /// </summary>
        public HMTFunctionDemoGeneratorService()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE service = AxServiceProvider.GetService<DTE>();
            if (service == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "No service for DTE found. The DTE must be registered as a service for using this API.", new object[0]));
            }

            gActiveProjectNode = HMTFunctionDemoGeneratorService.currentVSProject(service);

            IMetaModelProviders metaModelProviders = AxServiceProvider.GetService(typeof(IMetaModelProviders)) as IMetaModelProviders;
            gMetaModelService = metaModelProviders.CurrentMetaModelService;
            gModel = gActiveProjectNode.GetProjectsModelInfo();
            gService = AxServiceProvider.GetService<IDynamicsProjectService>();
            gFolderPath = @"C:\HMTTemplates";
            
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Get array of template types
        /// </summary>
        /// <returns>Array of template types</returns>
        public object[] getTemplateTypes()
        {
            List<string> fileNameList = new List<string>();

            object[] internalFilePathList = this.GetInternalTypes();

            // combine internal types and external types
            foreach (string filePath in internalFilePathList)
            {
                fileNameList.Add(filePath);
            }

            if (gFolderPath != "" && Directory.Exists(gFolderPath))
            {
                string[] filePathList = Directory.GetFiles(gFolderPath, "*.xml", SearchOption.AllDirectories);

                foreach (string filePath in filePathList)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    string fileName = fileInfo.Name;

                    if (fileName.EndsWith(".xml"))
                    {
                        fileName = fileName.Replace(".xml", "");
                    }

                    fileNameList.Add(fileName);
                }
            }

            fileNameList.Sort();

            return fileNameList.ToArray();
        }

        /// <summary>
        /// Willie Yao - 09/09/2024
        /// Get internal types
        /// </summary>
        /// <returns>
        /// Internal types
        /// </returns>
        public object[] GetInternalTypes()
        {
            string[] filePathList = new string[] { "Report_Classes", "Report_Classes_with_Query", "RunBaseBatch", "RunBaseBatch_with_Query", "SysOperation", "SysOperation_with_Query", "Table_Form_Entity_suit_for_one_field" };

            return filePathList;
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Get current project in Visual Studio.
        /// </summary>
        /// <returns>Current project project or null.</returns>
        public Project currentProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Array projects = gDTE.ActiveSolutionProjects as Array;

            if (projects.Length > 0)
            {
                gProject = projects.GetValue(0) as Project;
                return gProject;
            }
            return null;
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Get current VS project
        /// </summary>
        public static VSProjectNode currentVSProject(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Array array = dte.ActiveSolutionProjects as Array;
            if (array != null && array.Length > 0)
            {
                Project project = array.GetValue(0) as Project;
                if (project != null)
                {
                    return project.Object as VSProjectNode;
                }
            }
            return null;
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Update comment
        /// </summary>
        public string updateComment(string comment)
        {
            string[] lines = comment.Split(new Char[] { '\n' });

            string ret = "";
            int i = 0;

            foreach (string line in lines)
            {
                if (i > 0)
                {
                    ret += "\n";
                    ret += "    ";
                }

                ret += line;
                i++;
            }

            return ret;
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Add method to class
        /// </summary>
        public void addMethod(AxClass metaClass, string methodName, string code)
        {
            AxMethod metaMethod = new AxMethod();
            metaMethod.Name = methodName;
            metaMethod.Source = code;
            metaClass.Methods.Add(metaMethod);
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Replate placeholder with new values
        /// </summary>
        /// <param name="origString"></param>
        /// <param name="customerPrefix"></param>
        /// <param name="objectName"></param>
        /// <param name="objectLabel"></param>
        /// <param name="comment"></param>
        /// <returns>Final value</returns>
        public string formatString(string origString, string customerPrefix, string objectName, string objectLabel, string comment)
        {
            string value = origString;
            value = value.Replace("$CustomerPrefix$", customerPrefix);
            value = value.Replace("$ClassPrefix$", customerPrefix + objectName);
            value = value.Replace("$ObjectName$", objectName);
            value = value.Replace("$ObjectLabel$", objectLabel);
            value = value.Replace("$Comment$", comment);

            return value;
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Generate objects from template file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="customerPrefix"></param>
        /// <param name="objectName"></param>
        /// <param name="objectLabel"></param>
        /// <param name="comment"></param>
        public void generateObjectsFromTemplate(
            string filename, 
            string customerPrefix, 
            string objectName, 
            string objectLabel, 
            string comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // HMT added by Willie Yao on 09/09/2024 Begin
            XmlDocument xmldocumet = new XmlDocument();

            switch (filename)
            {
                case "Report_Classes":
                    xmldocumet.LoadXml(Resources.Resources.Report_Classes);
                    break;
                case "Report_Classes_with_Query":
                    xmldocumet.LoadXml(Resources.Resources.Report_Classes_with_Query);
                    break;
                case "RunBaseBatch":
                    xmldocumet.LoadXml(Resources.Resources.RunBaseBatch);
                    break;
                case "RunBaseBatch_with_Query":
                    xmldocumet.LoadXml(Resources.Resources.RunBaseBatch_with_Query);
                    break;
                case "SysOperation":
                    xmldocumet.LoadXml(Resources.Resources.SysOperation);
                    break;
                case "SysOperation_with_Query":
                    xmldocumet.LoadXml(Resources.Resources.SysOperation_with_Query);
                    break;
                case "Table_Form_Entity_suit_for_one_field":
                    xmldocumet.LoadXml(Resources.Resources.Table_Form_Entity_suit_for_one_field);
                    break;
            }
            AxHelper axHelper = new AxHelper();
            XmlNode rootNode = xmldocumet.DocumentElement;            
            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);

            foreach (XmlNode objNode in rootNode.ChildNodes)
            {
                XmlDictionaryReader xmlReader = GenerateStreamFromString(this.formatString(objNode.OuterXml, customerPrefix, objectName, objectLabel, comment));

                switch (objNode.Name)
                {
                    case "AxClass":
                        AxClass metaClass = AxClass.Deserialize(xmlReader);
                        AxMethod axMethod = new AxMethod();

                        if (gMetaModelService != null)
                        {
                            axHelper.MetaModelService.CreateClass(metaClass, saveInfo);
                            axHelper.AppendToActiveProject(metaClass);
                        }
                        break;

                    case "AxDataEntityView":
                        AxDataEntityView metaDataEntityrView = AxDataEntityView.Deserialize(xmlReader);

                        if (gMetaModelService != null)
                        {
                            axHelper.MetaModelService.CreateDataEntityView(metaDataEntityrView, saveInfo);
                            axHelper.AppendToActiveProject(metaDataEntityrView);
                        }
                        break;

                    case "AxEdt":
                        AxEdt metaEdt = AxEdt.Deserialize(xmlReader);

                        if (gMetaModelService != null)
                        {
                            axHelper.MetaModelService.CreateExtendedDataType(metaEdt, saveInfo);
                            axHelper.AppendToActiveProject(metaEdt);
                        }
                        break;

                    case "AxMenuItemDisplay":
                        AxMenuItemDisplay metaMenuItemDisplay = (AxMenuItemDisplay)new DataContractSerializer(typeof(AxMenuItemDisplay)).ReadObject(xmlReader, verifyObjectName: true);

                        if (gMetaModelService != null)
                        {
                            axHelper.MetaModelService.CreateMenuItemDisplay(metaMenuItemDisplay, saveInfo);
                            axHelper.AppendToActiveProject(metaMenuItemDisplay);
                        }
                        break;

                    case "AxForm":
                        AxForm metaForm = (AxForm)new DataContractSerializer(typeof(AxForm)).ReadObject(xmlReader, verifyObjectName: true);

                        if (gMetaModelService != null)
                        {
                            axHelper.MetaModelService.CreateForm(metaForm, saveInfo);
                            axHelper.AppendToActiveProject(metaForm);
                        }
                        break;

                    case "AxSecurityPrivilege":
                        AxSecurityPrivilege metaSecurityPrivilege = (AxSecurityPrivilege)new DataContractSerializer(typeof(AxSecurityPrivilege)).ReadObject(xmlReader, verifyObjectName: true);

                        if (gMetaModelService != null)
                        {
                            axHelper.MetaModelService.CreateSecurityPrivilege(metaSecurityPrivilege, saveInfo);
                            axHelper.AppendToActiveProject(metaSecurityPrivilege);
                        }
                        break;

                    case "AxTable":
                        AxTable metaTable = (AxTable)new DataContractSerializer(typeof(AxTable)).ReadObject(xmlReader, verifyObjectName: true);

                        if (gMetaModelService != null)
                        {
                            axHelper.MetaModelService.CreateTable(metaTable, saveInfo);
                            axHelper.AppendToActiveProject(metaTable);
                        }
                        break;
                }
            }

            //string filePath = gFolderPath + @"\" + filename + ".xml";

            //if (Directory.Exists(gFolderPath))
            //{
            //    XDocument doc = XDocument.Load(filePath);
            //    XElement templateNode = doc.Element("FunctionTemplate");
            //    foreach (XElement objNode in templateNode.Elements())
            //    {
            //        XmlDictionaryReader xmlReader = GenerateStreamFromString(this.formatString(objNode.ToString(), customerPrefix, objectName, objectLabel, comment));

            //        switch (objNode.Name.LocalName)
            //        {
            //            case "AxClass":
            //                AxClass metaClass = AxClass.Deserialize(xmlReader);
            //                AxMethod axMethod = new AxMethod();

            //                if (gMetaModelService != null)
            //                {
            //                    axHelper.MetaModelService.CreateClass(metaClass, saveInfo);
            //                    axHelper.AppendToActiveProject(metaClass);
            //                }
            //                break;

            //            case "AxDataEntityView":
            //                AxDataEntityView metaDataEntityrView = AxDataEntityView.Deserialize(xmlReader);

            //                if (gMetaModelService != null)
            //                {
            //                    axHelper.MetaModelService.CreateDataEntityView(metaDataEntityrView, saveInfo);
            //                    axHelper.AppendToActiveProject(metaDataEntityrView);
            //                }
            //                break;

            //            case "AxEdt":
            //                AxEdt metaEdt = AxEdt.Deserialize(xmlReader);

            //                if (gMetaModelService != null)
            //                {
            //                    axHelper.MetaModelService.CreateExtendedDataType(metaEdt, saveInfo);
            //                    axHelper.AppendToActiveProject(metaEdt);
            //                }
            //                break;

            //            case "AxMenuItemDisplay":
            //                AxMenuItemDisplay metaMenuItemDisplay = (AxMenuItemDisplay)new DataContractSerializer(typeof(AxMenuItemDisplay)).ReadObject(xmlReader, verifyObjectName: true);

            //                if (gMetaModelService != null)
            //                {
            //                    axHelper.MetaModelService.CreateMenuItemDisplay(metaMenuItemDisplay, saveInfo);
            //                    axHelper.AppendToActiveProject(metaMenuItemDisplay);
            //                }
            //                break;

            //            case "AxForm":
            //                AxForm metaForm = (AxForm)new DataContractSerializer(typeof(AxForm)).ReadObject(xmlReader, verifyObjectName: true);

            //                if (gMetaModelService != null)
            //                {
            //                    axHelper.MetaModelService.CreateForm(metaForm, saveInfo);
            //                    axHelper.AppendToActiveProject(metaForm);
            //                }
            //                break;

            //            case "AxSecurityPrivilege":
            //                AxSecurityPrivilege metaSecurityPrivilege = (AxSecurityPrivilege)new DataContractSerializer(typeof(AxSecurityPrivilege)).ReadObject(xmlReader, verifyObjectName: true);

            //                if (gMetaModelService != null)
            //                {
            //                    axHelper.MetaModelService.CreateSecurityPrivilege(metaSecurityPrivilege, saveInfo);
            //                    axHelper.AppendToActiveProject(metaSecurityPrivilege);
            //                }
            //                break;

            //            case "AxTable":
            //                AxTable metaTable = (AxTable)new DataContractSerializer(typeof(AxTable)).ReadObject(xmlReader, verifyObjectName: true);

            //                if (gMetaModelService != null)
            //                {
            //                    axHelper.MetaModelService.CreateTable(metaTable, saveInfo);
            //                    axHelper.AppendToActiveProject(metaTable);
            //                }
            //                break;
            //        }
            //    }
            //}            
        }

        public static XmlDictionaryReader GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            XmlDictionaryReader xmlDictReader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
            return xmlDictReader;
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Reset preview text
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>preview text</returns>
        public void resetPreviewText(string filename)
        {
            //string filePath = gFolderPath + @"\" + filename + ".xml";
            //XDocument doc = XDocument.Load(filePath);
            //XElement templateNode = doc.Element("FunctionTemplate");


            gPreviewText = "";

            //foreach (XElement objNode in templateNode.Elements())
            //{
            //    if (currentGroup != objNode.Name.LocalName)
            //    {
            //        currentGroup = objNode.Name.LocalName;
            //        gPreviewText += objNode.Name.LocalName + "\n";
            //    }

            //    string nodeXName = "Name";

            //    if (objNode.Name.NamespaceName != "")
            //    {
            //        nodeXName = "{" + objNode.Name.NamespaceName + "}" + nodeXName;
            //    }

            //    gPreviewText += "   " + objNode.Element(nodeXName).Value + "\n";
            //}

            XmlDocument xmldocumet = new XmlDocument();

            string[] filePathList = new string[] { "Report_Classes", "Report_Classes_with_Query", "RunBaseBatch", "RunBaseBatch_with_Query", "SysOperation", "SysOperation_with_Query", "Table_Form_Entity_suit_for_one_field" };

            gPreviewText = "Report_Classes" + "\n" + "Report_Classes_with_Query" + "\n" + "RunBaseBatch" + "\n" + "RunBaseBatch_with_Query" + "\n" + "SysOperation" + "\n" + "SysOperation_with_Query" + "\n" + "Table_Form_Entity_suit_for_one_field";

            //switch (filename)
            //{
            //    case "Report_Classes":
            //        MemoryStream memoryStreamReport = new MemoryStream(Resources.Resources.Report_Classes);
            //        XmlReader xmlreaderReport = new XmlTextReader(memoryStreamReport);
            //        xmldocumet.Load(xmlreaderReport);
            //        break;
            //    case "Report_Classes_with_Query":
            //        MemoryStream memoryStreamReportQuery = new MemoryStream(Resources.Resources.Report_Classes_with_Query);
            //        XmlReader xmlreaderReportQuery = new XmlTextReader(memoryStreamReportQuery);
            //        xmldocumet.Load(xmlreaderReportQuery);
            //        break;
            //    case "RunBaseBatch":
            //        MemoryStream memoryStreamRunBaseBatch = new MemoryStream(Resources.Resources.RunBaseBatch);
            //        XmlReader xmlreaderRunBaseBatch = new XmlTextReader(memoryStreamRunBaseBatch);
            //        xmldocumet.Load(xmlreaderRunBaseBatch);
            //        break;
            //    case "RunBaseBatch_with_Query":
            //        MemoryStream memoryStreamRunBaseBatchQuery = new MemoryStream(Resources.Resources.RunBaseBatch_with_Query);
            //        XmlReader xmlreaderRunBaseBatchQuery = new XmlTextReader(memoryStreamRunBaseBatchQuery);
            //        xmldocumet.Load(xmlreaderRunBaseBatchQuery);
            //        break;
            //    case "SysOperation":
            //        MemoryStream memoryStreamSysOperation = new MemoryStream(Resources.Resources.SysOperation);
            //        XmlReader xmlreaderSysOperation = new XmlTextReader(memoryStreamSysOperation);
            //        xmldocumet.Load(xmlreaderSysOperation);
            //        break;
            //    case "SysOperation_with_Query":
            //        MemoryStream memoryStreamSysOperationQuery = new MemoryStream(Resources.Resources.SysOperation_with_Query);
            //        XmlReader xmlreaderSysOperationQuery = new XmlTextReader(memoryStreamSysOperationQuery);
            //        xmldocumet.Load(xmlreaderSysOperationQuery);
            //        break;
            //    case "Table_Form_Entity_suit_for_one_field":
            //        MemoryStream memoryStreamTFEntity = new MemoryStream(Resources.Resources.Table_Form_Entity_suit_for_one_field);
            //        XmlReader xmlreaderTFEntity = new XmlTextReader(memoryStreamTFEntity);
            //        xmldocumet.Load(xmlreaderTFEntity);
            //        break;
            //}
            //AxHelper axHelper = new AxHelper();
            //XmlNodeList xmlNodeList = xmldocumet.DocumentElement.GetElementsByTagName("FunctionTemplate");
            //ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            //foreach (XmlNode objNode in xmlNodeList)
            //{
            //    if (currentGroup != objNode.Name)
            //    {
            //        currentGroup = objNode.Name;
            //        gPreviewText += objNode.Name + "\n";
            //    }

            //    string nodeXName = "Name";

            //    if (objNode.NamespaceURI != "")
            //    {
            //        nodeXName = "{" + objNode.NamespaceURI + "}" + nodeXName;
            //    }

            //    gPreviewText += "   " + objNode.Value + "\n";
            //}
        }

        /// <summary>
        /// HMTFunctionDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Update preview text
        /// </summary>
        /// <param name="customerPrefix"></param>
        /// <param name="objectName"></param>
        /// <param name="objectLabel"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public string updatePreviewText(string customerPrefix, string objectName, string objectLabel, string comment)
        {
            return this.formatString(gPreviewText, customerPrefix, objectName, objectLabel, comment);
        }
    }

}
