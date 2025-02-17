using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using System;
using System.IO;
using System.Collections.Concurrent;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace HMT.Kernel
{
    public class LocalUtils
    {
        public static DTE DTE
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;
            }
        }

        

        public static IMetaElement getNamedElementFromProjectItem(ProjectItem projectItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string name = projectItem.Name;
            string str2 = projectItem.get_FileNames(0);

            if ((!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(str2)) && (Path.GetExtension(str2) == ".xml"))
            {
                IMetadataReference  reference;
                Type                metadataType = null;

                if (DesignMetaModelService.Instance.GetMetadataReferenceFromFilePath(str2, out reference))
                {
                    metadataType = reference.MetadataType;
                }
                
                object          element     = GetMetaElement(projectItem, metadataType, false);
                var             itemTuple   = new Tuple<string, object>(name, element);
                IMetaElement    itemElement = itemTuple.Item2 as IMetaElement;

                return itemElement;
            }
            return null;
        }

        // Get the metadata element from the project item
        // Willie Yao - 2024-08-19
        internal static object GetMetaElement(ProjectItem _item, Type _metadataType, Boolean _throwError = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_metadataType == typeof(AxMenuExtension))
            {
                return DesignMetaModelService.Instance.GetMenuExtension(_item.Name);
            }

            if (_metadataType == typeof(AxReport))
            { 
                return DesignMetaModelService.Instance.GetReport(_item.Name);
            }

            if (_metadataType == typeof(AxSecurityRole))
            {
                return DesignMetaModelService.Instance.GetSecurityRole(_item.Name);
            }

            if (_metadataType == typeof(AxTable))
            {
                return DesignMetaModelService.Instance.GetTable(_item.Name);
            }

            if (_metadataType == typeof(AxTableExtension))
            {
                return DesignMetaModelService.Instance.GetTableExtension(_item.Name);
            }

            if (_metadataType == typeof(AxView))
            {
                return DesignMetaModelService.Instance.GetView(_item.Name);
            }

            if (_metadataType == typeof(AxViewExtension))
            {
                return DesignMetaModelService.Instance.GetViewExtension(_item.Name);
            }

            if (_metadataType == typeof(AxEdtString) ||
               _metadataType == typeof(AxEdtContainer) ||
               _metadataType == typeof(AxEdtDate) ||
               _metadataType == typeof(AxEdtEnum) ||
               _metadataType == typeof(AxEdtUtcDateTime) ||
               _metadataType == typeof(AxEdtGuid) ||
               _metadataType == typeof(AxEdtReal) ||
               _metadataType == typeof(AxEdtInt) ||
               _metadataType == typeof(AxEdtInt64))
            {
                return DesignMetaModelService.Instance.GetExtendedDataType(_item.Name);
            }

            if (_metadataType == typeof(AxEnum))
            {
                return DesignMetaModelService.Instance.GetEnum(_item.Name);
            }

            if (_metadataType == typeof(AxEnumExtension))
            {
                return DesignMetaModelService.Instance.GetEnumExtension(_item.Name);
            }

            if (_metadataType == typeof(AxMenuItemAction))
            {
                return DesignMetaModelService.Instance.GetMenuItemAction(_item.Name);
            }

            if (_metadataType == typeof(AxMenuItemDisplay))
            {
                return DesignMetaModelService.Instance.GetMenuItemDisplay(_item.Name);
            }

            if (_metadataType == typeof(AxMenuItemOutput))
            {
                return DesignMetaModelService.Instance.GetMenuItemOutput(_item.Name);
            }

            if (_metadataType == typeof(AxForm))
            {
                return DesignMetaModelService.Instance.GetForm(_item.Name);
            }

            if (_metadataType == typeof(AxFormExtension))
            {
                return DesignMetaModelService.Instance.GetFormExtension(_item.Name);
            }

            if (_metadataType == typeof(AxSecurityPrivilege))
            {
                return DesignMetaModelService.Instance.GetSecurityPrivilege(_item.Name);
            }

            if (_metadataType == typeof(AxSecurityDuty))
            {
                return DesignMetaModelService.Instance.GetSecurityDuty(_item.Name);
            }

            if (_metadataType == typeof(AxWorkflowHierarchyAssignmentProvider))
            {
                return DesignMetaModelService.Instance.GetSecurityDuty(_item.Name);
            }

            if (_metadataType == typeof(AxWorkflowApproval))
            {
                return DesignMetaModelService.Instance.GetWorkflowApproval(_item.Name);
            }

            if (_metadataType == typeof(AxWorkflowCategory))
            {
                return DesignMetaModelService.Instance.GetWorkflowCategory(_item.Name);
            }

            if (_metadataType == typeof(AxWorkflowTask))
            {
                return DesignMetaModelService.Instance.GetWorkflowTask(_item.Name);
            }

            if (_metadataType == typeof(AxWorkflowTemplate))
            {
                return DesignMetaModelService.Instance.GetWorkflowTemplate(_item.Name);
            }

            // v1.2
            if (_metadataType == typeof(AxDataEntityView))
            {
                return DesignMetaModelService.Instance.GetDataEntityView(_item.Name);
            }

            if (_metadataType == typeof(AxDataEntityViewExtension))
            {
                return DesignMetaModelService.Instance.GetDataEntityViewExtension(_item.Name);
            }

            if (_metadataType == typeof(AxQuerySimple))
            {
                return DesignMetaModelService.Instance.GetQuery(_item.Name);
            }

            if (_metadataType == typeof(AxQuerySimpleExtension))
            {
                return DesignMetaModelService.Instance.GetQuerySimpleExtension(_item.Name);
            }

            if (_metadataType == typeof(AxTile))
            {
                return DesignMetaModelService.Instance.GetTile(_item.Name);
            }

            // V1.3
            if (_metadataType == typeof(AxClass))
            {
                return DesignMetaModelService.Instance.GetClass(_item.Name);
            }

            if (_throwError)
            {
                throw new NotImplementedException($"The type {_metadataType.ToString()} is not implemented.");
            }
            else
            {
                return null;
            }
        }

        public IMetaModelProviders getOldTypeMetadataProviders()
        {
            return ServiceLocator.GetService(typeof(IMetaModelProviders)) as IMetaModelProviders;
        }

        public LocalUtils()
        {
            ConfigDC configDC = new ConfigDC();
        }

        public static IMetaElement getAOTObjectByName(string _name)
        {
            IMetaElement result = null;
            try
            {
                IMetaModelProviders metaModelProviders = null;
                //try
                //{
                //    metaModelProviders = new LocalUtils().getOldTypeMetadataProviders();
                //}
                //catch
                //{
                    metaModelProviders = CoreUtility.GetService<IMetaModelProviders>();
                //}
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
    }

    public class ConfigDC
    {
        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600005B RID: 91 RVA: 0x000055C9 File Offset: 0x000037C9
        // (set) Token: 0x0600005C RID: 92 RVA: 0x000055D1 File Offset: 0x000037D1
        public string RootAOT { get; set; }

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x0600005D RID: 93 RVA: 0x000055DA File Offset: 0x000037DA
        // (set) Token: 0x0600005E RID: 94 RVA: 0x000055E2 File Offset: 0x000037E2
        public bool isFindMethod { get; set; }

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600005F RID: 95 RVA: 0x000055EB File Offset: 0x000037EB
        // (set) Token: 0x06000060 RID: 96 RVA: 0x000055F3 File Offset: 0x000037F3
        public bool isExistMethod { get; set; }

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000061 RID: 97 RVA: 0x000055FC File Offset: 0x000037FC
        // (set) Token: 0x06000062 RID: 98 RVA: 0x00005604 File Offset: 0x00003804
        public bool isParmMethods { get; set; }
    }

    public static class ServiceLocator
    {
        // Token: 0x0600001E RID: 30 RVA: 0x00002844 File Offset: 0x00000A44
        public static object GetService(Type typeOfService)
        {
            if (typeOfService == null)
            {
                throw new ArgumentNullException("typeOfService");
            }
            object result;
            if (ServiceLocator.serviceMap.TryGetValue(typeOfService, out result))
            {
                return result;
            }
            return null;
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00002878 File Offset: 0x00000A78
        public static void RegisterService(Type typeOfService, object service)
        {
            if (typeOfService == null)
            {
                throw new ArgumentNullException("typeOfService");
            }
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            ServiceLocator.serviceMap.AddOrUpdate(typeOfService, service, (Type key, object oldValue) => service);
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000028D8 File Offset: 0x00000AD8
        public static void UnregisterService(Type typeOfService)
        {
            if (typeOfService == null)
            {
                throw new ArgumentNullException("typeOfService");
            }
            if (ServiceLocator.GetService(typeOfService) != null)
            {
                object obj;
                ServiceLocator.serviceMap.TryRemove(typeOfService, out obj);
            }
        }

        // Token: 0x0400000D RID: 13
        private static ConcurrentDictionary<Type, object> serviceMap = new ConcurrentDictionary<Type, object>();
    }


}
