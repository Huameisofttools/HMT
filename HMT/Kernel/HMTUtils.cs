using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using EnvDTE;
using EnvDTE80;
using Microsoft.Dynamics.AX.Metadata.Core.Collections;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.Framework.Tools.Configuration;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Classes;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.Win32;

namespace HMT.Kernel
{
    public class HMTUtils
    {
        private const string root = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Dynamics\\AX\\Development";
        public ConfigDC dc = new ConfigDC();
        public string defaultConfigFile = "";
        public static IMetaModelProviders MetaModelProviders;
        public static IMetaModelService MetaModelService;

        public HMTUtils()
        {
            ConfigDC configDC = new ConfigDC();
        }

        public static WritableSettingsStore getStore(SVsServiceProvider vsServiceProvider)
        {
            ShellSettingsManager shellSettingsManager = new ShellSettingsManager(vsServiceProvider);
            return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        public string AXconfigDirectory()
        {
            RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            registryKey = registryKey.OpenSubKey("SOFTWARE");
            registryKey = registryKey.OpenSubKey("Microsoft");
            registryKey = registryKey.OpenSubKey("Dynamics");
            registryKey = registryKey.OpenSubKey("AX");
            registryKey = registryKey.OpenSubKey("Development");
            this.defaultConfigFile = (string)registryKey.GetValue("DefaultConfig");
            return this.defaultConfigFile;
        }

        public static DTE DTE
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;
            }
        }

        public static DTE2 DTE2
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE2;
            }
        }

        public void parseConfig()
        {
            XmlDocument xmlDocument = new XmlDocument();
            this.dc = new ConfigDC();
            try
            {
                xmlDocument.Load(this.defaultConfigFile);
                foreach (object obj in xmlDocument.DocumentElement.ChildNodes)
                {
                    XmlNode xmlNode = (XmlNode)obj;
                    bool flag = xmlNode.Name == "CompiledILFolder";
                    if (flag)
                    {
                        IEnumerator enumerator2 = xmlNode.ChildNodes.GetEnumerator();
                        
                        if (enumerator2.MoveNext())
                        {
                            XmlNode xmlNode2 = (XmlNode)enumerator2.Current;
                            this.dc.RootAOT = xmlNode2.Value;
                        }
                        
                    }
                }
            }
            catch
            {
            }
        }

        public static VSProjectNode GetActiveProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Array array = HMTUtils.DTE.ActiveSolutionProjects as Array;
            bool flag = array != null;
            if (flag)
            {
                bool flag2 = array.Length > 0;
                if (flag2)
                {
                    Project project = array.GetValue(0) as Project;
                    return project.Object as VSProjectNode;
                }
            }
            return null;
        }

        public static VSProjectFileNode GetActiveProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ProjectItem projectItem = HMTUtils.DTE.ActiveDocument.ProjectItem;
            ProjectItem projectItem2 = projectItem;
            return ((projectItem2 != null) ? projectItem2.Object : null) as VSProjectFileNode;
        }

        public static VSProjectNode GetActiveProjectNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Array array = HMTUtils.DTE2.ActiveSolutionProjects as Array;
            bool flag = array != null && array.Length > 0;
            VSProjectNode result;
            if (flag)
            {
                Project project = array.GetValue(0) as Project;
                Project project2 = project;
                result = (((project2 != null) ? project2.Object : null) as VSProjectNode);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public static ModelSaveInfo GetModel()
        {
            ModelInfo projectsModelInfo = HMTUtils.GetActiveProjectNode().GetProjectsModelInfo(false);
            return new ModelSaveInfo
            {
                Id = projectsModelInfo.Id,
                Layer = projectsModelInfo.Layer
            };
        }

        public static IMetaElement getAOTClassByName(string _name)
        {
            IMetaElement result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                AxClass @class = metaModelService.GetClass(_name);
                result = @class;
            }
            catch
            {
            }
            return result;
        }

        public static string getAOTTableNameByName(string _name)
        {
            string text = "";
            IMetaElement aottableByName = HMTUtils.getAOTTableByName(_name);
            bool flag = aottableByName.GetType() == typeof(AxTable);
            if (flag)
            {
                text = ((AxTable)aottableByName).Name;
            }
            bool flag2 = aottableByName.GetType() == typeof(AxView);
            if (flag2)
            {
                text = ((AxView)aottableByName).Name;
            }
            bool flag3 = aottableByName.GetType() == typeof(AxViewExtension);
            if (flag3)
            {
                text = ((AxViewExtension)aottableByName).Name;
                text = text.Remove(text.IndexOf('.'));
            }
            bool flag4 = aottableByName.GetType() == typeof(AxDataEntityView);
            if (flag4)
            {
                text = ((AxDataEntityView)aottableByName).Name;
            }
            bool flag5 = aottableByName.GetType() == typeof(AxTableExtension);
            if (flag5)
            {
                text = ((AxTableExtension)aottableByName).Name;
                text = text.Remove(text.IndexOf('.'));
            }
            return text;
        }

        public static AxTable getAOTBaseTableByName(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                AxTable table = metaModelService.GetTable(_name);
                bool flag = table == null;
                if (flag)
                {
                    table = metaModelService.GetTable(HMTUtils.getAOTTableNameByName(_name));
                }
                return table;
            }
            catch
            {
            }
            return null;
        }

        public static AxView getAOTBaseViewByName(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                AxView view = metaModelService.GetView(_name);
                bool flag = view == null;
                if (flag)
                {
                    view = metaModelService.GetView(HMTUtils.getAOTTableNameByName(_name));
                }
                return view;
            }
            catch
            {
            }
            return null;
        }

        public static List<AxTableExtension> getAOTBaseTableExtensionsByName(string _name)
        {
            List<AxTableExtension> list = new List<AxTableExtension>();
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                foreach (string text in metaModelService.GetTableExtensionNames())
                {
                    bool flag = text.IndexOf(_name + ".") == 0;
                    if (flag)
                    {
                        list.Add(metaModelService.GetTableExtension(text));
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        public static IMetaElement getAOTTableByName(string _name)
        {
            IMetaElement result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                IMetaElement metaElement = metaModelService.GetTable(_name);
                bool flag = metaElement != null;
                if (flag)
                {
                    result = metaElement;
                }
                metaElement = metaModelService.GetTableExtension(_name);
                bool flag2 = metaElement != null;
                if (flag2)
                {
                    result = metaElement;
                }
                metaElement = metaModelService.GetDataEntityView(_name);
                bool flag3 = metaElement != null;
                if (flag3)
                {
                    result = metaElement;
                }
                metaElement = metaModelService.GetView(_name);
                bool flag4 = metaElement != null;
                if (flag4)
                {
                    result = metaElement;
                }
                metaElement = metaModelService.GetViewExtension(_name);
                bool flag5 = metaElement != null;
                if (flag5)
                {
                    result = metaElement;
                }
            }
            catch
            {
            }
            return result;
        }

        public static AxEdt getEDTByName(string _name)
        {
            AxEdt result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                result = metaModelService.GetExtendedDataType(_name);
            }
            catch
            {
            }
            return result;
        }

        public static AxEnum getEnumByName(string _name)
        {
            AxEnum result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                result = metaModelService.GetEnum(_name);
            }
            catch
            {
            }
            return result;
        }

        public static AxQuery getQueryByName(string _name)
        {
            AxQuery result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                result = metaModelService.GetQuery(_name);
            }
            catch
            {
            }
            return result;
        }

        public static List<string> getBaseAOTEDTByBaseType(string _name = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte = CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE2;
            List<string> result = new List<string>();
            //ToolWindows toolWindows = dte.ToolWindows;
            //if (HMTUtils.<> o__26.<> p__0 == null)
            //{
            //    HMTUtils.<> o__26.<> p__0 = CallSite<Func<CallSite, object, ApplicationExplorerControlAutomation>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(ApplicationExplorerControlAutomation), typeof(HMTUtils)));
            //}
            //ApplicationExplorerControlAutomation obj = HMTUtils.<> o__26.<> p__0.Target(HMTUtils.<> o__26.<> p__0, toolWindows.GetToolWindow("Application Explorer"));
            //ApplicationExplorerControl applicationExplorerControl = (ApplicationExplorerControl)obj.GetField_AllAccess("control");
            //ApplicationExplorerDataBinder dataBinder = applicationExplorerControl.DataBinder;
            //NodeHolderList allNodeHolders = dataBinder.AllNodeHolders;
            return result;
        }

        public static List<string> getAOTEDTByBaseType(string _name = "")
        {
            List<string> list = new List<string>();
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                bool flag = _name != null && metaModelService != null;
                if (flag)
                {
                    bool flag2 = _name.Equals("str");
                    if (flag2)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtString));
                    }
                    bool flag3 = _name.Equals("int");
                    if (flag3)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtInt));
                    }
                    bool flag4 = _name.Equals("guid");
                    if (flag4)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtGuid));
                    }
                    bool flag5 = _name.Equals("int64");
                    if (flag5)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtInt64));
                    }
                    bool flag6 = _name.Equals("real");
                    if (flag6)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtReal));
                    }
                    bool flag7 = _name.Equals("Date");
                    if (flag7)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtDate));
                    }
                    bool flag8 = _name.Equals("container");
                    if (flag8)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtContainer));
                    }
                    bool flag9 = _name.Equals("enum");
                    if (flag9)
                    {
                        list = (List<string>)metaModelService.GetEnumNames();
                        list.AddRange(metaModelService.GetExtendedDataTypeNames(typeof(AxEdtEnum)));
                    }
                    bool flag10 = _name.Equals("utcDateTime");
                    if (flag10)
                    {
                        list = (List<string>)metaModelService.GetExtendedDataTypeNames(typeof(AxEdtUtcDateTime));
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        public static List<string> getAOTQueriesArray()
        {
            List<string> result = new List<string>();
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                result = (List<string>)metaModelService.GetQueryNames(typeof(AxQuerySimple));
            }
            catch
            {
            }
            return result;
        }

        public IMetaModelProviders getOldTypeMetadataProviders()
        {
            return CoreUtility.GetService<IMetaModelProviders>();
            //return ServiceLocator.GetService(typeof(IMetaModelProviders)) as IMetaModelProviders;
        }

        public static IMetaElement getAOTObjectByName(string _name)
        {
            IMetaElement result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService metaModelService = (metaModelProviders != null) ? metaModelProviders.CurrentMetaModelService : null;
                bool flag = _name.IndexOf("AxTable_") >= 0;
                if (flag)
                {
                    string text = _name.Replace("AxTable_", "");
                    text = text.Replace(".xpp", "");
                    AxTable table = metaModelService.GetTable(text);
                    result = table;
                }
                bool flag2 = _name.IndexOf("AxClass_") >= 0;
                if (flag2)
                {
                    string text = _name.Replace("AxClass_", "");
                    text = text.Replace(".xpp", "");
                    AxClass @class = metaModelService.GetClass(text);
                    result = @class;
                }
                bool flag3 = _name.IndexOf("AxForm_") >= 0;
                if (flag3)
                {
                    string text = _name.Replace("AxForm_", "");
                    text = text.Replace(".xpp", "");
                    AxForm form = metaModelService.GetForm(text);
                    result = form;
                }
                bool flag4 = _name.IndexOf("AxView_") >= 0;
                if (flag4)
                {
                    string text = _name.Replace("AxView_", "");
                    text = text.Replace(".xpp", "");
                    AxView view = metaModelService.GetView(text);
                    result = view;
                }
                bool flag5 = _name.IndexOf("AxDataEntityView_") >= 0;
                if (flag5)
                {
                    string text = _name.Replace("AxDataEntityView_", "");
                    text = text.Replace(".xpp", "");
                    AxDataEntityView dataEntityView = metaModelService.GetDataEntityView(text);
                    result = dataEntityView;
                }
                bool flag6 = _name.IndexOf("AxAggregateDataEntity_") >= 0;
                if (flag6)
                {
                    string text = _name.Replace("AxAggregateDataEntity_", "");
                    text = text.Replace(".xpp", "");
                    AxAggregateDataEntity aggregateDataEntity = metaModelService.GetAggregateDataEntity(text);
                    result = aggregateDataEntity;
                }
                bool flag7 = _name.IndexOf("AxMap_") >= 0;
                if (flag7)
                {
                    string text = _name.Replace("AxMap_", "");
                    text = text.Replace(".xpp", "");
                    AxMap map = metaModelService.GetMap(text);
                    result = map;
                }
            }
            catch
            {
            }
            return result;
        }

        public static IList<string> getAOTMenuItemsDisplayFromProject()
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                return currentMetaModelService.GetMenuItemDisplayNames();
            }
            catch
            {
            }
            return null;
        }

        public static List<string> getAOTMenuItemsFromProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<string> list = new List<string>();
            try
            {
                Project project = null;
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                Array array = HMTUtils.DTE.ActiveSolutionProjects as Array;
                bool flag = array != null;
                if (flag)
                {
                    bool flag2 = array.Length > 0;
                    if (flag2)
                    {
                        project = (array.GetValue(0) as Project);
                    }
                }
                foreach (object obj in project.ProjectItems)
                {
                    ProjectItem projectItem = obj as ProjectItem;
                    list.Add(projectItem.Name);
                    HMTUtils.projectItems(projectItem, list, null);
                }
            }
            catch
            {
            }
            return list;
        }

        public static List<string> getAOTFromProject(Type _type = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<string> list = new List<string>();
            try
            {
                Project project = null;
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                Array array = HMTUtils.DTE.ActiveSolutionProjects as Array;
                bool flag = array != null;
                if (flag)
                {
                    bool flag2 = array.Length > 0;
                    if (flag2)
                    {
                        project = (array.GetValue(0) as Project);
                    }
                }
                foreach (object obj in project.ProjectItems)
                {
                    ProjectItem projectItem = obj as ProjectItem;
                    bool flag3 = _type != null;
                    if (flag3)
                    {
                        bool flag4 = HMTUtils.getMetedataType(projectItem).Equals(_type.Name);
                        if (flag4)
                        {
                            list.Add(projectItem.Name);
                        }
                    }
                    else
                    {
                        list.Add(projectItem.Name);
                    }
                    HMTUtils.projectItems(projectItem, list, _type);
                }
            }
            catch
            {
            }
            return list;
        }

        public static string getMetedataType(ProjectItem _item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool flag = _item.Object is VSProjectFileNode;
            string result="";
            //if (flag)
            //{
            //    if (HMTUtils.<> o__34.<> p__0 == null)
            //    {
            //        HMTUtils.<> o__34.<> p__0 = CallSite<Func<CallSite, object, VSProjectFileNode>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(VSProjectFileNode), typeof(HMTUtils)));
            //    }
            //    VSProjectFileNode vsprojectFileNode = HMTUtils.<> o__34.<> p__0.Target(HMTUtils.<> o__34.<> p__0, _item.Object);
            //    result = vsprojectFileNode.MetadataReference.MetadataType.Name;
            //}
            //else
            //{
            //    result = "";
            //}
            return result;
        }

        public static List<string> projectItems(ProjectItem _item, List<string> _l, Type _type = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool flag = _item.ProjectItems != null;
            if (flag)
            {
                foreach (object obj in _item.ProjectItems)
                {
                    ProjectItem projectItem = obj as ProjectItem;
                    bool flag2 = _type != null;
                    if (flag2)
                    {
                        bool flag3 = HMTUtils.getMetedataType(projectItem).Equals(_type.Name);
                        if (flag3)
                        {
                            _l.Add(projectItem.Name);
                        }
                    }
                    else
                    {
                        _l.Add(projectItem.Name);
                    }
                    HMTUtils.projectItems(projectItem, _l, null);
                }
            }
            return _l;
        }

        public static bool checkPrivilege(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                AxMenuItem axMenuItem = HMTUtils.getAOTMenuItemAction(_name);
                bool flag = axMenuItem != null;
                if (flag)
                {
                    return false;
                }
                axMenuItem = HMTUtils.getAOTMenuItemDisplay(_name);
                bool flag2 = axMenuItem != null;
                if (flag2)
                {
                    return false;
                }
                axMenuItem = HMTUtils.getAOTMenuItemOutput(_name);
                bool flag3 = axMenuItem != null;
                if (flag3)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static void updateDuty(AxSecurityDuty _duty)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                ModelInfoCollection securityDutyModelInfo = currentMetaModelService.GetSecurityDutyModelInfo(_duty.Name);
                ModelInfo modelReference = securityDutyModelInfo[0];
                ModelSaveInfo saveInfo = new ModelSaveInfo(modelReference);
                currentMetaModelService.UpdateSecurityDuty(_duty, saveInfo);
            }
            catch
            {
            }
        }

        public static void updateRole(AxSecurityRole _role)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                ModelInfoCollection securityRoleModelInfo = currentMetaModelService.GetSecurityRoleModelInfo(_role.Name);
                ModelInfo modelReference = securityRoleModelInfo[0];
                ModelSaveInfo saveInfo = new ModelSaveInfo(modelReference);
                currentMetaModelService.UpdateSecurityRole(_role, saveInfo);
            }
            catch
            {
            }
        }

        public static AxSecurityPrivilege createPrivilege(AxMenuItem _menuItem, string _name, AccessGrant _grant)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                EntryPointType objectType = EntryPointType.None;
                ModelInfoCollection modelInfoCollection = null;
                bool flag = _menuItem.GetType().Equals(typeof(AxMenuItemAction));
                if (flag)
                {
                    modelInfoCollection = currentMetaModelService.GetMenuItemActionModelInfo(((AxMenuItemAction)_menuItem).Name);
                    objectType = EntryPointType.MenuItemAction;
                }
                bool flag2 = _menuItem.GetType().Equals(typeof(AxMenuItemDisplay));
                if (flag2)
                {
                    modelInfoCollection = currentMetaModelService.GetMenuItemDisplayModelInfo(((AxMenuItemDisplay)_menuItem).Name);
                    objectType = EntryPointType.MenuItemDisplay;
                }
                bool flag3 = _menuItem.GetType().Equals(typeof(AxMenuItemOutput));
                if (flag3)
                {
                    modelInfoCollection = currentMetaModelService.GetMenuItemOutputModelInfo(((AxMenuItemOutput)_menuItem).Name);
                    objectType = EntryPointType.MenuItemOutput;
                }
                ModelInfo modelReference = modelInfoCollection[0];
                AxSecurityPrivilege axSecurityPrivilege = new AxSecurityPrivilege();
                ModelSaveInfo saveInfo = new ModelSaveInfo(modelReference);
                AxSecurityEntryPointReference axSecurityEntryPointReference = new AxSecurityEntryPointReference();
                axSecurityEntryPointReference.ObjectType = objectType;
                axSecurityEntryPointReference.ObjectName = _menuItem.Name;
                axSecurityEntryPointReference.Grant = _grant;
                axSecurityEntryPointReference.Name = _menuItem.Name;
                axSecurityPrivilege.Label = _menuItem.Label;
                axSecurityPrivilege.Name = _name;
                axSecurityPrivilege.EntryPoints.Add(axSecurityEntryPointReference);
                currentMetaModelService.CreateSecurityPrivilege(axSecurityPrivilege, saveInfo);
                return axSecurityPrivilege;
            }
            catch
            {
            }
            return null;
        }

        public static AxMenuItemAction createActionMenuItem(IMetaElement _currentElement, string _name, string _label, string _enumTypeParameter, string _enumParameter, MenuItemObjectType _objectType, string _object, string _parameters)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                AxMenuItemAction axMenuItemAction = new AxMenuItemAction();
                axMenuItemAction.Name = _name;
                axMenuItemAction.Label = _label;
                axMenuItemAction.EnumTypeParameter = _enumTypeParameter;
                axMenuItemAction.EnumParameter = _enumParameter;
                axMenuItemAction.ObjectType = _objectType;
                axMenuItemAction.Object = _object;
                axMenuItemAction.Parameters = _parameters;
                ModelInfoCollection modelInfoCollection = null;
                bool flag = _currentElement is AxClass;
                if (flag)
                {
                    modelInfoCollection = currentMetaModelService.GetClassModelInfo(((AxClass)_currentElement).Name);
                }
                bool flag2 = _currentElement is AxForm;
                if (flag2)
                {
                    modelInfoCollection = currentMetaModelService.GetFormModelInfo(((AxForm)_currentElement).Name);
                }
                ModelInfo modelReference = modelInfoCollection[0];
                ModelSaveInfo saveInfo = new ModelSaveInfo(modelReference);
                currentMetaModelService.CreateMenuItemAction(axMenuItemAction, saveInfo);
                return axMenuItemAction;
            }
            catch
            {
            }
            return null;
        }

        public static AxMenuItem getAOTMenuItem(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                AxMenuItem axMenuItem = HMTUtils.getAOTMenuItemAction(_name);
                bool flag = axMenuItem != null;
                if (flag)
                {
                    return axMenuItem;
                }
                axMenuItem = HMTUtils.getAOTMenuItemDisplay(_name);
                bool flag2 = axMenuItem != null;
                if (flag2)
                {
                    return axMenuItem;
                }
                axMenuItem = HMTUtils.getAOTMenuItemOutput(_name);
                bool flag3 = axMenuItem != null;
                if (flag3)
                {
                    return axMenuItem;
                }
            }
            catch
            {
            }
            return null;
        }

        public static AxMenuItemAction getAOTMenuItemAction(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                return currentMetaModelService.GetMenuItemAction(_name);
            }
            catch
            {
            }
            return null;
        }

        public static AxMenuItemDisplay getAOTMenuItemDisplay(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                return currentMetaModelService.GetMenuItemDisplay(_name);
            }
            catch
            {
            }
            return null;
        }

        public static AxMenuItemOutput getAOTMenuItemOutput(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                return currentMetaModelService.GetMenuItemOutput(_name);
            }
            catch
            {
            }
            return null;
        }

        public static AxSecurityRole getAOTSecurityRole(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                return currentMetaModelService.GetSecurityRole(_name);
            }
            catch
            {
            }
            return null;
        }

        public static AxSecurityDuty getAOTSecurityDuty(string _name)
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                return currentMetaModelService.GetSecurityDuty(_name);
            }
            catch
            {
            }
            return null;
        }

        public static IList<string> getAOTMenuItemsOutputFromProject()
        {
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                return currentMetaModelService.GetMenuItemOutputNames();
            }
            catch
            {
            }
            return null;
        }

        public static IMetaElement getAOTMenuItemByName(string _name = "")
        {
            return null;
        }

        public static AxLabelFile getLabelFileFromProject(DTE _dte, string _name = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            AxLabelFile result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                ModelInfoCollection modelInfoCollection = null;
                bool flag = _dte.ActiveDocument != null;
                if (flag)
                {
                    IMetaElement aotobjectByName = HMTUtils.getAOTObjectByName(_dte.ActiveDocument.Name);
                    bool flag2 = aotobjectByName.GetType().Equals(typeof(AxTable));
                    if (flag2)
                    {
                        modelInfoCollection = currentMetaModelService.GetTableModelInfo(((AxTable)aotobjectByName).Name);
                    }
                    bool flag3 = aotobjectByName.GetType().Equals(typeof(AxClass));
                    if (flag3)
                    {
                        modelInfoCollection = currentMetaModelService.GetClassModelInfo(((AxClass)aotobjectByName).Name);
                    }
                    bool flag4 = aotobjectByName.GetType().Equals(typeof(AxForm));
                    if (flag4)
                    {
                        modelInfoCollection = currentMetaModelService.GetFormModelInfo(((AxForm)aotobjectByName).Name);
                    }
                    bool flag5 = aotobjectByName.GetType().Equals(typeof(AxView));
                    if (flag5)
                    {
                        modelInfoCollection = currentMetaModelService.GetViewModelInfo(((AxView)aotobjectByName).Name);
                    }
                    bool flag6 = modelInfoCollection != null;
                    if (flag6)
                    {
                        result = HMTUtils.GetLabelFile(metaModelProviders, currentMetaModelService, modelInfoCollection);
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public static string getClassFilePath(AxClass _class)
        {
            string result = "";
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                ModelInfoCollection classModelInfo = currentMetaModelService.GetClassModelInfo(_class.Name);
                ModelInfo modelInfo = classModelInfo[0];
                DevelopmentConfiguration currentConfiguration = ConfigurationHelper.CurrentConfiguration;
                result = string.Concat(new string[]
                {
                    currentConfiguration.ModelStoreFolder,
                    "\\",
                    modelInfo.Module,
                    "\\",
                    modelInfo.Name,
                    "\\AxClass\\",
                    _class.Name,
                    ".xml"
                });
            }
            catch
            {
            }
            return result;
        }

        public static DevelopmentConfiguration getConfiguration()
        {
            return ConfigurationHelper.CurrentConfiguration;
        }

        public static string getClassTMPFilePath(AxClass _class)
        {
            string result = "";
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                ModelInfoCollection classModelInfo = currentMetaModelService.GetClassModelInfo(_class.Name);
                ModelInfo modelInfo = classModelInfo[0];
                DevelopmentConfiguration currentConfiguration = ConfigurationHelper.CurrentConfiguration;
                result = string.Concat(new string[]
                {
                    currentConfiguration.InstallationDirectory,
                    "\\XppSource\\",
                    modelInfo.Name,
                    "\\AxClass_",
                    _class.Name,
                    ".xpp"
                });
            }
            catch
            {
            }
            return result;
        }

        private void AddMethodToFile(ClassItem c)
        {
            try
            {
                AxClass axClass = null;
                axClass.AddMethod(this.BuildMethod());
            }
            catch
            {
            }
        }

        public static void addItemToProject(string _elementName, Type _type)
        {
            HMTUtils.GetActiveProject().AddModelElementsToProject(new List<MetadataReference>
            {
                new MetadataReference(_elementName, _type)
            });
        }

        private AxMethod BuildMethod()
        {
            return new AxMethod
            {
                Name = "myNewMethod",
                ReturnType = new AxMethodReturnType()
            };
        }

        public static List<AxLabelFile> getLabelFilesFromProject(DTE _dte, string _name = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<AxLabelFile> result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                ModelInfoCollection modelInfoCollection = null;
                bool flag = _dte.ActiveDocument != null;
                if (flag)
                {
                    IMetaElement aotobjectByName = HMTUtils.getAOTObjectByName(_dte.ActiveDocument.Name);
                    bool flag2 = aotobjectByName.GetType().Equals(typeof(AxTable));
                    if (flag2)
                    {
                        modelInfoCollection = currentMetaModelService.GetTableModelInfo(((AxTable)aotobjectByName).Name);
                    }
                    bool flag3 = aotobjectByName.GetType().Equals(typeof(AxClass));
                    if (flag3)
                    {
                        modelInfoCollection = currentMetaModelService.GetClassModelInfo(((AxClass)aotobjectByName).Name);
                    }
                    bool flag4 = aotobjectByName.GetType().Equals(typeof(AxForm));
                    if (flag4)
                    {
                        modelInfoCollection = currentMetaModelService.GetFormModelInfo(((AxForm)aotobjectByName).Name);
                    }
                    bool flag5 = aotobjectByName.GetType().Equals(typeof(AxView));
                    if (flag5)
                    {
                        modelInfoCollection = currentMetaModelService.GetViewModelInfo(((AxView)aotobjectByName).Name);
                    }
                    bool flag6 = modelInfoCollection != null;
                    if (flag6)
                    {
                        result = HMTUtils.GetLabelFiles(metaModelProviders, currentMetaModelService, modelInfoCollection);
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        public static List<AxLabelFile> getLabelFilesFromProjectV3(DTE _dte, string _name = "")
        {
            List<AxLabelFile> result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                VSProjectNode activeProject = HMTUtils.GetActiveProject();
                ModelInfo projectsModelInfo = activeProject.GetProjectsModelInfo(false);
                bool flag = projectsModelInfo != null;
                if (flag)
                {
                    result = HMTUtils.GetLabelFilesV2(metaModelProviders, currentMetaModelService, projectsModelInfo);
                }
            }
            catch
            {
            }
            return result;
        }

        public static List<AxLabelFile> getLabelFilesFromProjectV2(DTE _dte, IMetaElement _metaElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<AxLabelFile> result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                try
                {
                    metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
                }
                catch
                {
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                }
                IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
                ModelInfoCollection modelInfoCollection = null;
                bool flag = _dte.ActiveDocument != null;
                if (flag)
                {
                    bool flag2 = _metaElement.GetType().Equals(typeof(AxTable));
                    if (flag2)
                    {
                        modelInfoCollection = currentMetaModelService.GetTableModelInfo(((AxTable)_metaElement).Name);
                    }
                    bool flag3 = _metaElement.GetType().Equals(typeof(AxClass));
                    if (flag3)
                    {
                        modelInfoCollection = currentMetaModelService.GetClassModelInfo(((AxClass)_metaElement).Name);
                    }
                    bool flag4 = _metaElement.GetType().Equals(typeof(AxForm));
                    if (flag4)
                    {
                        modelInfoCollection = currentMetaModelService.GetFormModelInfo(((AxForm)_metaElement).Name);
                    }
                    bool flag5 = _metaElement.GetType().Equals(typeof(AxView));
                    if (flag5)
                    {
                        modelInfoCollection = currentMetaModelService.GetViewModelInfo(((AxView)_metaElement).Name);
                    }
                    bool flag6 = _metaElement.GetType().Equals(typeof(AxSecurityDuty));
                    if (flag6)
                    {
                        modelInfoCollection = currentMetaModelService.GetSecurityDutyModelInfo(((AxSecurityDuty)_metaElement).Name);
                    }
                    bool flag7 = _metaElement.GetType().Equals(typeof(AxSecurityPrivilege));
                    if (flag7)
                    {
                        modelInfoCollection = currentMetaModelService.GetSecurityPrivilegeModelInfo(((AxSecurityPrivilege)_metaElement).Name);
                    }
                    bool flag8 = _metaElement.GetType().Equals(typeof(AxSecurityRole));
                    if (flag8)
                    {
                        modelInfoCollection = currentMetaModelService.GetSecurityRoleModelInfo(((AxSecurityRole)_metaElement).Name);
                    }
                    bool flag9 = modelInfoCollection != null;
                    if (flag9)
                    {
                        result = HMTUtils.GetLabelFiles(metaModelProviders, currentMetaModelService, modelInfoCollection);
                    }
                }
            }
            catch
            {
            }
            return result;
        }

        private static AxLabelFile GetLabelFile(IMetaModelProviders metaModelProviders, IMetaModelService metaModelService, ModelInfoCollection modelInfoCollection)
        {
            AxLabelFile result = null;
            ModelInfo modelInfo = modelInfoCollection[0];
            ModelLoadInfo loadInfo = new ModelLoadInfo
            {
                Id = modelInfo.Id,
                Layer = modelInfo.Layer
            };
            IList<string> list = metaModelProviders.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(modelInfo.Name);
            try
            {
                result = metaModelService.GetLabelFile(list[0], loadInfo);
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public static List<AxLabelFile> getAllLabelFilesV2()
        {
            List<AxLabelFile> list = new List<AxLabelFile>();
            IMetaModelProviders metaModelProviders = null;
            try
            {
                metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
            }
            catch
            {
                metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
            }
            IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
            IList<string> labelFileNames = currentMetaModelService.GetLabelFileNames();
            foreach (string labeFileElementName in labelFileNames)
            {
                AxLabelFile labelFile = currentMetaModelService.GetLabelFile(labeFileElementName);
                list.Add(labelFile);
            }
            return list;
        }

        public static object[] getAllLabelFiles()
        {
            List<AxLabelFile> list = new List<AxLabelFile>();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            IMetaModelProviders metaModelProviders = null;
            try
            {
                metaModelProviders = new HMTUtils().getOldTypeMetadataProviders();
            }
            catch
            {
                metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
            }
            IMetaModelService currentMetaModelService = metaModelProviders.CurrentMetaModelService;
            IList<string> labelFileNames = currentMetaModelService.GetLabelFileNames();
            foreach (string labeFileElementName in labelFileNames)
            {
                AxLabelFile labelFile = currentMetaModelService.GetLabelFile(labeFileElementName);
                list.Add(labelFile);
                dictionary.Add(labelFile.LabelContentFileName, labelFile.LocalPath());
            }
            return new object[]
            {
                list,
                dictionary
            };
        }

        private static List<AxLabelFile> GetLabelFiles(IMetaModelProviders metaModelProviders, IMetaModelService metaModelService, ModelInfoCollection modelInfoCollection)
        {
            List<AxLabelFile> list = null;
            ModelInfo modelInfo = modelInfoCollection[0];
            ModelLoadInfo loadInfo = new ModelLoadInfo
            {
                Id = modelInfo.Id,
                Layer = modelInfo.Layer
            };
            IList<string> list2 = metaModelProviders.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(modelInfo.Name);
            try
            {
                bool flag = list2.Count > 0;
                if (flag)
                {
                    list = new List<AxLabelFile>();
                    foreach (string labelFileElementName in list2)
                    {
                        AxLabelFile labelFile = metaModelService.GetLabelFile(labelFileElementName, loadInfo);
                        list.Add(labelFile);
                    }
                }
            }
            catch
            {
                list = null;
            }
            return list;
        }

        private static List<AxLabelFile> GetLabelFilesV2(IMetaModelProviders metaModelProviders, IMetaModelService metaModelService, ModelInfo _modelInfo)
        {
            List<AxLabelFile> list = null;
            ModelLoadInfo loadInfo = new ModelLoadInfo
            {
                Id = _modelInfo.Id,
                Layer = _modelInfo.Layer
            };
            IList<string> list2 = metaModelProviders.CurrentMetadataProvider.LabelFiles.ListObjectsForModel(_modelInfo.Name);
            try
            {
                bool flag = list2.Count > 0;
                if (flag)
                {
                    list = new List<AxLabelFile>();
                    foreach (string labelFileElementName in list2)
                    {
                        AxLabelFile labelFile = metaModelService.GetLabelFile(labelFileElementName, loadInfo);
                        list.Add(labelFile);
                    }
                }
            }
            catch
            {
                list = null;
            }
            return list;
        }

        public static bool isMethodExist(string _objectName, string _methodName)
        {
            bool result = false;
            IMetaElement aotobjectByName = HMTUtils.getAOTObjectByName(_objectName);
            bool flag = aotobjectByName.GetType() == typeof(AxClass);
            if (flag)
            {
                AxClass axClass = (AxClass)aotobjectByName;
                bool flag2 = axClass != null;
                if (flag2)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axClass.Methods, _methodName);
                }
            }
            bool flag3 = aotobjectByName.GetType() == typeof(AxTable);
            if (flag3)
            {
                AxTable axTable = (AxTable)aotobjectByName;
                bool flag4 = axTable != null;
                if (flag4)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axTable.Methods, _methodName);
                }
            }
            bool flag5 = aotobjectByName.GetType() == typeof(AxForm);
            if (flag5)
            {
                AxForm axForm = (AxForm)aotobjectByName;
                bool flag6 = axForm != null;
                if (flag6)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axForm.Methods, _methodName);
                }
            }
            bool flag7 = aotobjectByName.GetType() == typeof(AxView);
            if (flag7)
            {
                AxView axView = (AxView)aotobjectByName;
                bool flag8 = axView != null;
                if (flag8)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axView.Methods, _methodName);
                }
            }
            bool flag9 = aotobjectByName.GetType() == typeof(AxDataEntityView);
            if (flag9)
            {
                AxDataEntityView axDataEntityView = (AxDataEntityView)aotobjectByName;
                bool flag10 = axDataEntityView != null;
                if (flag10)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axDataEntityView.Methods, _methodName);
                }
            }
            bool flag11 = aotobjectByName.GetType() == typeof(AxAggregateDataEntity);
            if (flag11)
            {
                AxAggregateDataEntity axAggregateDataEntity = (AxAggregateDataEntity)aotobjectByName;
                bool flag12 = axAggregateDataEntity != null;
                if (flag12)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axAggregateDataEntity.Methods, _methodName);
                }
            }
            bool flag13 = aotobjectByName.GetType() == typeof(AxMap);
            if (flag13)
            {
                AxMap axMap = (AxMap)aotobjectByName;
                bool flag14 = axMap != null;
                if (flag14)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axMap.Methods, _methodName);
                }
            }
            return result;
        }

        public static bool isMethodExist(IMetaElement _object, string _methodName)
        {
            bool result = false;
            bool flag = _object.GetType() == typeof(AxClass);
            if (flag)
            {
                AxClass axClass = (AxClass)_object;
                bool flag2 = axClass != null;
                if (flag2)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axClass.Methods, _methodName);
                }
            }
            bool flag3 = _object.GetType() == typeof(AxTable);
            if (flag3)
            {
                AxTable axTable = (AxTable)_object;
                bool flag4 = axTable != null;
                if (flag4)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axTable.Methods, _methodName);
                }
            }
            bool flag5 = _object.GetType() == typeof(AxForm);
            if (flag5)
            {
                AxForm axForm = (AxForm)_object;
                bool flag6 = axForm != null;
                if (flag6)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axForm.Methods, _methodName);
                }
            }
            bool flag7 = _object.GetType() == typeof(AxView);
            if (flag7)
            {
                AxView axView = (AxView)_object;
                bool flag8 = axView != null;
                if (flag8)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axView.Methods, _methodName);
                }
            }
            bool flag9 = _object.GetType() == typeof(AxDataEntityView);
            if (flag9)
            {
                AxDataEntityView axDataEntityView = (AxDataEntityView)_object;
                bool flag10 = axDataEntityView != null;
                if (flag10)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axDataEntityView.Methods, _methodName);
                }
            }
            bool flag11 = _object.GetType() == typeof(AxAggregateDataEntity);
            if (flag11)
            {
                AxAggregateDataEntity axAggregateDataEntity = (AxAggregateDataEntity)_object;
                bool flag12 = axAggregateDataEntity != null;
                if (flag12)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axAggregateDataEntity.Methods, _methodName);
                }
            }
            bool flag13 = _object.GetType() == typeof(AxMap);
            if (flag13)
            {
                AxMap axMap = (AxMap)_object;
                bool flag14 = axMap != null;
                if (flag14)
                {
                    result = HMTUtils.isMethodExistIncapsulate(axMap.Methods, _methodName);
                }
            }
            return result;
        }

        public static KeyedObjectCollection<AxTableField> getPrimaryIndexFields(AxTable _table)
        {
            KeyedObjectCollection<AxTableField> result = new KeyedObjectCollection<AxTableField>();
            bool flag = !_table.PrimaryIndex.Equals("SurrogateKey");
            if (flag)
            {
                foreach (AxTableIndex axTableIndex in _table.Indexes)
                {
                    bool flag2 = axTableIndex.Name.Equals(_table.PrimaryIndex);
                    if (flag2)
                    {
                        result = HMTUtils.getAxIndexFields(_table, axTableIndex.Fields);
                        break;
                    }
                }
            }
            return result;
        }

        public static KeyedObjectCollection<AxTableField> getAlternateIndexFields(AxTable _table)
        {
            KeyedObjectCollection<AxTableField> result = new KeyedObjectCollection<AxTableField>();
            foreach (AxTableIndex axTableIndex in _table.Indexes)
            {
                bool flag = axTableIndex.AlternateKey == NoYes.Yes;
                if (flag)
                {
                    result = HMTUtils.getAxIndexFields(_table, axTableIndex.Fields);
                    break;
                }
            }
            return result;
        }

        public static KeyedObjectCollection<AxTableField> getSeparateIndexFields(AxTable _table)
        {
            KeyedObjectCollection<AxTableField> result = new KeyedObjectCollection<AxTableField>();
            bool flag = !_table.PrimaryIndex.Equals("SurrogateKey");
            if (flag)
            {
                foreach (AxTableIndex axTableIndex in _table.Indexes)
                {
                    bool flag2 = axTableIndex.Name.Equals(_table.PrimaryIndex);
                    if (flag2)
                    {
                        result = HMTUtils.getAxIndexFields(_table, axTableIndex.Fields);
                        break;
                    }
                }
            }
            else
            {
                foreach (AxTableIndex axTableIndex2 in _table.Indexes)
                {
                    bool flag3 = axTableIndex2.AllowDuplicates == NoYes.No && axTableIndex2.AlternateKey == NoYes.Yes;
                    if (flag3)
                    {
                        result = HMTUtils.getAxIndexFields(_table, axTableIndex2.Fields);
                        break;
                    }
                }
            }
            return result;
        }

        public static KeyedObjectCollection<AxTableField> getAxIndexFields(AxTable _table, KeyedObjectCollection<AxTableIndexField> _list)
        {
            KeyedObjectCollection<AxTableField> keyedObjectCollection = new KeyedObjectCollection<AxTableField>();
            foreach (AxTableIndexField axTableIndexField in _list)
            {
                AxTableField fieldByName = HMTUtils.getFieldByName(_table, axTableIndexField.DataField);
                bool flag = fieldByName != null;
                if (flag)
                {
                    keyedObjectCollection.Add(fieldByName);
                }
            }
            return keyedObjectCollection;
        }

        public static AxTableField getFieldByName(AxTable _table, string _name)
        {
            AxTableField result = null;
            foreach (AxTableField axTableField in _table.Fields)
            {
                bool flag = axTableField.Name.Equals(_name);
                if (flag)
                {
                    result = axTableField;
                    break;
                }
            }
            return result;
        }

        private static bool isMethodExistIncapsulate(KeyedObjectCollection<AxMethod> list, string _methodName)
        {
            bool result = false;
            foreach (AxMethod axMethod in list)
            {
                bool flag = axMethod.Name.Equals(_methodName);
                if (flag)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static List<string> getBaseTypes()
        {
            return new List<string>
            {
                "boolean",
                "int",
                "real",
                "str",
                "container"
            };
        }

        public static string getAxType(AxClassMemberVariable _var)
        {
            switch (_var.Type)
            {
                case CompilerBaseType.Void:
                    return "void";
                case CompilerBaseType.String:
                    return "str";
                case CompilerBaseType.Int32:
                    return "int";
                case CompilerBaseType.Int64:
                    return "int64";
                case CompilerBaseType.Enum:
                    return _var.TypeName;
                case CompilerBaseType.Time:
                    return "TimeOfDay";
                case CompilerBaseType.DateTime:
                    return "utcDateTime";
                case CompilerBaseType.ExtendedDataType:
                    return _var.TypeName;
                case CompilerBaseType.AnyType:
                    return "anytype";
                case CompilerBaseType.Guid:
                    return "guid";
                case CompilerBaseType.Real:
                    return "real";
                case CompilerBaseType.Date:
                    return "Date";
                case CompilerBaseType.Container:
                    return "container";
                case CompilerBaseType.Class:
                    return _var.TypeName;
                case CompilerBaseType.Record:
                    return _var.TypeName;
                case CompilerBaseType.FormElementType:
                    return "anytype";
            }
            return _var.TypeName;
        }

        public static string getAxType(CompilerBaseType _type, string _moreType = "")
        {
            bool flag = _moreType != "";
            if (flag)
            {
                _moreType = "object";
            }
            switch (_type)
            {
                case CompilerBaseType.Void:
                    return "void";
                case CompilerBaseType.String:
                    return "str";
                case CompilerBaseType.Int32:
                    return "int";
                case CompilerBaseType.Int64:
                    return "int64";
                case CompilerBaseType.Enum:
                    return _moreType;
                case CompilerBaseType.Time:
                    return "TimeOfDay";
                case CompilerBaseType.DateTime:
                    return "utcDateTime";
                case CompilerBaseType.ExtendedDataType:
                    return _moreType;
                case CompilerBaseType.AnyType:
                    return "anytype";
                case CompilerBaseType.Guid:
                    return "guid";
                case CompilerBaseType.Real:
                    return "real";
                case CompilerBaseType.Date:
                    return "Date";
                case CompilerBaseType.Container:
                    return "container";
                case CompilerBaseType.Class:
                    return _moreType;
                case CompilerBaseType.Record:
                    return "Common";
                case CompilerBaseType.FormElementType:
                    return "anytype";
            }
            return _moreType;
        }

        public static string getAxFieldType(AxTableField _field)
        {
            string result = "";
            bool flag = _field.ExtendedDataType == "";
            if (flag)
            {
                bool flag2 = _field.GetType() == typeof(AxTableFieldInt);
                if (flag2)
                {
                    result = "int";
                }
                bool flag3 = _field.GetType() == typeof(AxTableFieldString);
                if (flag3)
                {
                    result = "str";
                }
                bool flag4 = _field.GetType() == typeof(AxTableFieldReal);
                if (flag4)
                {
                    result = "real";
                }
                bool flag5 = _field.GetType() == typeof(AxTableFieldDate);
                if (flag5)
                {
                    result = "Date";
                }
                bool flag6 = _field.GetType() == typeof(AxTableFieldInt64);
                if (flag6)
                {
                    result = "int64";
                }
                bool flag7 = _field.GetType() == typeof(AxTableFieldContainer);
                if (flag7)
                {
                    result = "container";
                }
                bool flag8 = _field.GetType() == typeof(AxTableFieldGuid);
                if (flag8)
                {
                    result = "guid";
                }
                bool flag9 = _field.GetType() == typeof(AxTableFieldTime);
                if (flag9)
                {
                    result = "TimeOfDay";
                }
                bool flag10 = _field.GetType() == typeof(AxTableFieldUtcDateTime);
                if (flag10)
                {
                    result = "utcDateTime";
                }
            }
            else
            {
                result = _field.ExtendedDataType;
            }
            return result;
        }

        public static AxClassMemberVariable findAxClassMemberVariable(List<AxClassMemberVariable> _list, string _name)
        {
            AxClassMemberVariable result = null;
            foreach (AxClassMemberVariable axClassMemberVariable in _list)
            {
                bool flag = axClassMemberVariable.Name.Equals(_name);
                if (flag)
                {
                    result = axClassMemberVariable;
                    break;
                }
            }
            return result;
        }

        public static List<AxClassMemberVariable> getListOfMembers(object _obj)
        {
            List<AxClassMemberVariable> result = null;
            bool flag = _obj != null;
            if (flag)
            {
                bool flag2 = _obj.GetType() == typeof(AxClass);
                if (flag2)
                {
                    AxClass axClass = (AxClass)_obj;
                    result = axClass.Members;
                }
            }
            return result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
