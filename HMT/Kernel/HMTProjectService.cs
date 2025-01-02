using EnvDTE;
using EnvDTE80;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.Framework.Tools.Core;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace HMT.Kernel
{
    public class HMTProjectService
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
        public static Array gProjects;

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
        /// Willie Yao - 09/09/2024
        /// Current projects array
        /// </summary>
        /// <returns>
        /// Current projects array
        /// </returns>
        public Array CurrentProjects()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Array projects = gDTE.ActiveSolutionProjects as Array;

            return projects;
        }

        public Project currentProject(Project project)
        {
            gProject = project;

            return gProject;
        }

        /// <summary>
        /// HMT_FDD016_LabelGenerator
        /// Willie Yao - 03/27/2024
        /// Get all projects in the solution
        /// </summary>
        /// <returns>
        /// All projects in the solution
        /// </returns>
        public Array getAllProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE dte = CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;

            if (dte != null) {
                IEnumerator objects = dte.Solution.Projects.GetEnumerator();
                int count = 0;

                // initialize the gprojects array
                int i = dte.Solution.Projects.Count;
                gProjects = Array.CreateInstance(typeof(Project), i);

                while (objects.MoveNext())
                {
                    Project project = objects.Current as Project;

                    if (count < i && existsLabel(project))
                    {
                        gProjects.SetValue(project, count);
                        count++;
                    }
                }
            }            

            return gProjects;
        }

        /// <summary>
        /// Willie Yao - 08/22/2024
        /// Exists label in the project
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>True if exists label</returns>
        private bool existsLabel(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IList<Tuple<string, ProjectItem>> listOfLabelFiles;

            listOfLabelFiles = GetProjectItems(project.ProjectItems, typeof(AxLabelFile));

            if (listOfLabelFiles != null && listOfLabelFiles.Count > 0)
            {
                return true;
            }

            return false;
        }

        public IList<Tuple<string, object>> getAllElementsFromSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IList<Tuple<string, object>> allMetaElements = new List<Tuple<string, object>>();
            bool isFirstProject = true;

            foreach (Project project in gProjects)
            {
                if (project == null)
                {
                    continue;
                }

                IList<Tuple<string, object>> iMetaElements = GetMetaElements(project.ProjectItems, null);

                // Add iMetaElements to allMetaElements
                if (isFirstProject)
                {
                    allMetaElements = iMetaElements;
                    isFirstProject = false;
                }
                else
                {
                    allMetaElements = allMetaElements.Concat(iMetaElements).ToList();
                }
            }
            return allMetaElements;
        }

        public ProjectItem currentLabelNode(bool isSolutionLevel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!isSolutionLevel)
            {
                return currentLabelNode();
            }

            foreach (Project project in gProjects)
            {
                if (project == null)
                {
                    continue;
                }

                List<Type> listOfTypes = new List<Type>();
                IList<Tuple<string, ProjectItem>> listOfLabelFiles;
                Tuple<string, ProjectItem> labelItem;

                listOfLabelFiles = GetProjectItems(project.ProjectItems, typeof(AxLabelFile));

                if (listOfLabelFiles != null && listOfLabelFiles.Count > 0)
                {
                    labelItem = listOfLabelFiles.First();

                    return labelItem.Item2;
                }
            }

            return null;
        }

        public ProjectItem currentLabelNode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<Type> listOfTypes = new List<Type>();
            IList<Tuple<string, ProjectItem>> listOfLabelFiles;
            Tuple<string, ProjectItem> labelItem;

            listOfLabelFiles = GetProjectItems(gProject.ProjectItems, typeof(AxLabelFile));

            if (listOfLabelFiles != null && listOfLabelFiles.Count > 0)
            {
                labelItem = listOfLabelFiles.First();

                return labelItem.Item2;
            }

            return null;
        }


        public IList<Tuple<string, object>> getAllElements()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetMetaElements(gProject.ProjectItems, null);
        }

        public IMetaModelService currentModel()
        {
            IMetaModelProviders metaModelProviders = AxServiceProvider.GetService<IMetaModelProviders>();
            IMetaModelService metaModelService = metaModelProviders.CurrentMetaModelService;

            return metaModelService;
        }

        public static IMetaModelService model(string modelName)
        {
            IMetaModelProviders metaModelProviders = AxServiceProvider.GetService<IMetaModelProviders>();
            IMetaModelService metaModelService = metaModelProviders.GetMetaModelService(modelName);

            return metaModelService;
        }

        internal static IList<Tuple<string, ProjectItem>> GetProjectItems(ProjectItems _node, Type _filterByType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<Tuple<string, ProjectItem>> list = new List<Tuple<string, ProjectItem>>();
            foreach (ProjectItem item in _node)
            {
                if (item.ProjectItems != null)
                {
                    list.AddRange(GetProjectItems(item.ProjectItems, _filterByType));
                }
                string name = item.Name;
                string str2 = item.get_FileNames(1);
                if ((!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(str2)) && (Path.GetExtension(str2) == ".xml"))
                {
                    IMetadataReference reference;
                    Type metadataType = null;
                    if ((_filterByType != null) && DesignMetaModelService.Instance.GetMetadataReferenceFromFilePath(str2, out reference))
                    {
                        metadataType = reference.MetadataType;
                    }
                    if ((_filterByType == null) || (_filterByType == metadataType))
                    {
                        list.Add(new Tuple<string, ProjectItem>(name, item));
                    }
                }
            }
            return list;
        }


        public static IList<Tuple<string, object>> GetMetaElements(ProjectItems _node, Type _filterByType)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<Tuple<string, object>> list = new List<Tuple<string, object>>();

            foreach (ProjectItem item in _node)
            {
                if (item.ProjectItems != null)
                {
                    list.AddRange(GetMetaElements(item.ProjectItems, _filterByType));
                }
                string name = item.Name;
                string str2 = item.get_FileNames(0);

                if ((!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(str2)) && (Path.GetExtension(str2) == ".xml"))
                {
                    IMetadataReference reference;
                    Type metadataType = null;

                    if (DesignMetaModelService.Instance.GetMetadataReferenceFromFilePath(str2, out reference))
                    {
                        metadataType = reference.MetadataType;
                    }

                    if ((_filterByType == null) || (_filterByType == metadataType))
                    {
                        object element = GetMetaElement(item, metadataType, false);

                        if (element != null)
                        {
                            list.Add(new Tuple<string, object>(name, element));
                        }
                    }
                }
            }

            return list;
        }

        internal static object GetMetaElement(ProjectItem _item, Type _metadataType, Boolean _throwError = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
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
    }
}
