using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.AX.Metadata.Core.Collections;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Forms;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Tables;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Views;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.BaseTypes;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Menus;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.DataEntityViews;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Security;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Workflows;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Presentation;
using Microsoft.Dynamics.Framework.Tools.Labels;
using EnvDTE;
using HMT.Kernel;
using EnvDTE80;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Reports;
using Microsoft.VisualStudio.Shell;

namespace HMT.HMTLabelGenerator
{
    abstract public class HMLabelService
    {
        public LabelManager labelManager;
        public IMetaModelService metaModelService;
        public bool generateForCode;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initialize class
        /// </summary>
        public HMLabelService()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HMTProjectService projectService = new HMTProjectService();
            Project projectNode = projectService.currentProject();
            ProjectItem labelFileNode = projectService.currentLabelNode() as ProjectItem;
            AxLabelFile labelFile = projectService.currentModel().GetLabelFile(labelFileNode.Name);

            labelManager = new LabelManager(labelFile.LabelFileId, labelFile, projectNode.Name);
            metaModelService = projectService.currentModel();
            generateForCode = false;
        }

        public HMLabelService(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HMTProjectService projectService = new HMTProjectService();
            Project projectNode = projectService.currentProject(project);
            ProjectItem labelFileNode = projectService.currentLabelNode() as ProjectItem;
            AxLabelFile labelFile = projectService.currentModel().GetLabelFile(labelFileNode.Name);

            labelManager = new LabelManager(labelFile.LabelFileId, labelFile, project.Name);
            metaModelService = projectService.currentModel();
            generateForCode = false;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the core logic for named element
        /// </summary>
        abstract public void run();

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the core logic for meta element
        /// </summary>
        abstract public void runAX();

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 10/23/2020
        /// Process labels in code
        /// </summary>
        public string processLabelsInCode(string sourceCode)
        {
            string ret = sourceCode;
            int startPosition = 0;

            while (startPosition >= 0 && startPosition < ret.Length)
            {
                startPosition = ret.IndexOf("\"", startPosition);

                if (startPosition >= 0)
                {
                    int endPosition = ret.IndexOf("\"", startPosition + 1);
                    string labelValue = ret.Substring(startPosition + 1, (endPosition - startPosition - 1));

                    if (this.validateLabelValue(labelValue) && !this.IsCommented(sourceCode, startPosition))
                    {
                        var labelId = this.labelManager.createLabel(labelValue);
                        ret = ret.Remove(startPosition + 1, (endPosition - startPosition - 1));
                        ret = ret.Insert(startPosition + 1, labelId);
                        endPosition = startPosition + labelId.Length + 1;
                    }

                    startPosition = endPosition + 1;
                }
            }

            return ret;
        }

        protected bool validateLabelValue(string labelValue)
        {
            bool ret = false;

            if (!labelValue.StartsWith("@") && labelValue != "" && Regex.Replace(labelValue, @"[^a-zA-z]", "") != "")
            {
                ret = true;
            }

            return ret;
        }

        /// <summary>
        /// Checks if the current line of the specified index is a comment
        /// </summary>
        /// <param name="sourceCode">entire source code</param>
        /// <param name="currQuoteIndex">The current index of the Quote</param>
        /// <returns>True if the line is commented</returns>
        protected bool IsCommented(string sourceCode, int currQuoteIndex)
        {
            bool isCommented = false;

            var prevLineBreak = sourceCode.LastIndexOf("\r\n", currQuoteIndex, StringComparison.InvariantCultureIgnoreCase);
            if (prevLineBreak >= 0)
            {
                string textBetween = sourceCode.Substring(prevLineBreak + 2, (currQuoteIndex - prevLineBreak));
                // search for a comment between prevLineBreak & currQuoteIndex
                var commentIndex = textBetween.IndexOf(@"//", 0);
                if (commentIndex > 0)
                {
                    isCommented = true;
                }
            }

            if (!isCommented)
            {
                //check if this is in a code block
                var prevCodeBlockOpen = sourceCode.LastIndexOf(@"/*", currQuoteIndex, StringComparison.InvariantCultureIgnoreCase);

                if (prevCodeBlockOpen >= 0)
                {
                    string textBetween = sourceCode.Substring(prevCodeBlockOpen + 2, (currQuoteIndex - (prevCodeBlockOpen + 2)));
                    var prevCodeBlockClose = textBetween.IndexOf(@"*/", 0);
                    if (prevCodeBlockClose <= 0  // Could not fix the end block
                        || prevCodeBlockClose > currQuoteIndex) // end block is after the current quote index (we shouldnt need this ever)
                    {
                        isCommented = true;
                    }
                }
            }

            return isCommented;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Contructs a classes based on the element type
        /// </summary>
        /// <param name="element">Current element</param>
        /// <returns>The instance of label service</returns>
        static public HMLabelService construct(NamedElement _element, bool _generateForCodeLabel, Boolean _throwError = true)
        {
            HMLabelService labelService = null;
            switch (_element.GetType().Name)
            {
                case "Table":
                    labelService = new HMLabelService_Table(_element as Table);
                    break;

                case "TableExtension":
                    labelService = new HMLabelService_TableExtension(_element as TableExtension);
                    break;

                case "View":
                    labelService = new HMLabelService_View(_element as View);
                    break;

                case "ViewExtension":
                    labelService = new HMLabelService_ViewExtension(_element as ViewExtension);
                    break;

                case "EdtBase":
                case "EdtString":
                case "EdtContainer":
                case "EdtDate":
                case "EdtEnum":
                case "EdtDateTime":
                case "EdtGuid":
                case "EdtReal":
                case "EdtInt":
                case "EdtInt64":
                    labelService = new HMLabelService_Edt(_element as EdtBase);
                    break;

                case "BaseEnum":
                    labelService = new HMLabelService_BaseEnum(_element as BaseEnum);
                    break;

                case "BaseEnumExtension":
                    labelService = new HMLabelService_BaseEnumExtension(_element as BaseEnumExtension);
                    break;

                case "MenuItem":
                case "MenuItemAction":
                case "MenuItemDisplay":
                case "MenuItemOutput":
                    labelService = new HMLabelService_MenuItem(_element as MenuItem);
                    break;

                case "Form":
                    labelService = new HMLabelService_Form(_element as Form);
                    break;

                case "FormExtension":
                    labelService = new HMLabelService_FormExtension(_element as FormExtension);
                    break;

                case "SecurityPrivilege":
                    labelService = new HMLabelService_SecurityPrivilege(_element as SecurityPrivilege);
                    break;

                case "SecurityDuty":
                    labelService = new HMLabelService_SecurityDuty(_element as SecurityDuty);
                    break;
                

                //case "WorkflowHierarchyAssignmentProvider":
                //    return new HMLabelService_WorkflowHierarchyAssignmentProvider(_element as WorkflowHierarchyAssignmentProvider);

                //case "WorkflowApproval":
                //    return new HMLabelService_WorkflowApproval(_element as WorkflowApproval);

                //case "WorkflowCategory":
                //    return new HMLabelService_WorkflowCategory(_element as WorkflowCategory);

                //case "WorkflowTask":
                //    return new HMLabelService_WorkflowTask(_element as WorkflowTask);

                //case "WorkflowTemplate"://WorkflowType Object
                //    return new HMLabelService_WorkflowType(_element as WorkflowTemplate);

                // v1.2
                case "DataEntityView":
                    labelService = new HMLabelService_DataEntityView(_element as DataEntityView);
                    break;

                case "DataEntityViewExtension":
                    labelService = new HMLabelService_DataEntityViewExtension(_element as DataEntityViewExtension);
                    break;

                case "SimpleQuery":
                    labelService = new HMLabelService_QuerySimple(_element as SimpleQuery);
                    break;

                case "SimpleQueryExtension":
                    labelService = new HMLabelService_QuerySimpleExtension(_element as SimpleQueryExtension);
                    break;

                //case "Tile":
                //    return new HMLabelService_Tile(_element as Tile);

                default:
                    if (_throwError)
                    {
                        throw new NotImplementedException($"The type {_element.GetType().Name} is not implemented.");
                    }
                    else
                    {
                        return null;
                    }
            }

            labelService.generateForCode = _generateForCodeLabel;

            return labelService;
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Contructs a classes based on the element type
        /// </summary>
        /// <param name="_element">Current element</param>
        /// <param name="_throwError">Throw error or not</param>
        /// <returns>The instance of label service</returns>
        static public HMLabelService construct(IMetaElement _element, bool _generateForCodeLabel, Boolean _throwError = true)
        {
            HMLabelService labelService = null;
            switch (_element.GetType().Name)
            {
                case "AxMenuExtension":
                    labelService = new HMLabelService_MenuExtension(_element as AxMenuExtension);
                    break;
                case "AxSecurityRole":
                    labelService = new HMLabelService_SecurityRole(_element as AxSecurityRole);
                    break;
                case "AxTable":
                    labelService = new HMLabelService_Table(_element as AxTable);
                    break;

                case "AxTableExtension":
                    labelService = new HMLabelService_TableExtension(_element as AxTableExtension);
                    break;

                case "AxView":
                    labelService = new HMLabelService_View(_element as AxView);
                    break;

                case "AxViewExtension":
                    labelService = new HMLabelService_ViewExtension(_element as AxViewExtension);
                    break;

                case "AxEdtString":
                case "AxEdtContainer":
                case "AxEdtDate":
                case "AxEdtEnum":
                case "AxEdtUtcDateTime":
                case "AxEdtGuid":
                case "AxEdtReal":
                case "AxEdtInt":
                case "AxEdtInt64":
                    labelService = new HMLabelService_Edt(_element as AxEdt);
                    break;


                case "AxEnum":
                    labelService = new HMLabelService_BaseEnum(_element as AxEnum);
                    break;

                case "AxEnumExtension":
                    labelService = new HMLabelService_BaseEnumExtension(_element as AxEnumExtension);
                    break;

                case "typeof(AxMenuItem":
                case "AxMenuItemAction":
                case "AxMenuItemDisplay":
                case "AxMenuItemOutput":
                    labelService = new HMLabelService_MenuItem(_element as AxMenuItem);
                    break;

                case "AxForm":
                    labelService = new HMLabelService_Form(_element as AxForm);
                    break;


                case "AxFormExtension":
                    labelService = new HMLabelService_FormExtension(_element as AxFormExtension);
                    break;

                case "AxSecurityPrivilege":
                    labelService = new HMLabelService_SecurityPrivilege(_element as AxSecurityPrivilege);
                    break;

                case "AxSecurityDuty":
                    labelService = new HMLabelService_SecurityDuty(_element as AxSecurityDuty);
                    break;

                case "AxWorkflowHierarchyAssignmentProvider":
                    labelService = new HMLabelService_WorkflowHierarchyAssignmentProvider(_element as AxWorkflowHierarchyAssignmentProvider);
                    break;


                case "AxWorkflowApproval":
                    labelService = new HMLabelService_WorkflowApproval(_element as AxWorkflowApproval);
                    break;


                case "AxWorkflowCategory":
                    labelService = new HMLabelService_WorkflowCategory(_element as AxWorkflowCategory);
                    break;

                case "AxWorkflowTask":
                    labelService = new HMLabelService_WorkflowTask(_element as AxWorkflowTask);
                    break;

                case "AxWorkflowTemplate":
                    labelService = new HMLabelService_WorkflowType(_element as AxWorkflowTemplate);
                    break;

                // v1.2
                case "AxDataEntityView":
                    labelService = new HMLabelService_DataEntityView(_element as AxDataEntityView);
                    break;

                case "AxDataEntityViewExtension":
                    labelService = new HMLabelService_DataEntityViewExtension(_element as AxDataEntityViewExtension);
                    break;

                case "AxQuerySimple":
                    labelService = new HMLabelService_QuerySimple(_element as AxQuerySimple);
                    break;

                case "AxQuerySimpleExtension":
                    labelService = new HMLabelService_QuerySimpleExtension(_element as AxQuerySimpleExtension);
                    break;

                case "AxTile":
                    labelService = new HMLabelService_Tile(_element as AxTile);
                    break;

                // v1.3
                case "AxClass":
                    labelService = new HMLabelService_Class(_element as AxClass);
                    break;

                default:
                    if (_throwError)
                    {
                        throw new NotImplementedException($"The type {_element.GetType().Name} is not implemented.");
                    }
                    else
                    {
                        return null;
                    }
            }
            labelService.generateForCode = _generateForCodeLabel;

            return labelService;
        }
    }

    /// <summary>
    /// Willie Yao - 08/22/2024
    /// The Label Service for Role
    /// </summary>
    public class HMLabelService_SecurityRole : HMLabelService
    {
        protected SecurityRole securityRole;
        protected AxSecurityRole axSecurityRole;

        public HMLabelService_SecurityRole(SecurityRole securityRole)
        {
            this.securityRole = securityRole;
        }

        public HMLabelService_SecurityRole(AxSecurityRole axSecurityRole)
        {
            this.axSecurityRole = axSecurityRole;
        }

        public override void run()
        {
            
        }

        public override void runAX()
        {
            this.axSecurityRole.Label = this.labelManager.createLabel(this.axSecurityRole.Label);

            this.axSecurityRole.Description = this.labelManager.createLabel(this.axSecurityRole.Description);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetSecurityRoleModelInfo(axSecurityRole.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateSecurityRole(axSecurityRole, saveInfo);
            }
        }
    }

    public class HMLabelService_MenuExtension : HMLabelService
    {
        protected MenuExtension menuExtension;
        protected AxMenuExtension axMenuExtension;

        public HMLabelService_MenuExtension(MenuExtension menuExtension)
        {
            this.menuExtension = menuExtension;
        }

        public HMLabelService_MenuExtension(AxMenuExtension axMenuExtension)
        {
            this.axMenuExtension = axMenuExtension;
        }

        public override void run()
        {
            
        }

        public override void runAX()
        {
            var elements = this.axMenuExtension.Elements;

            foreach (var element in elements)
            {
                AxMenuElementSubMenu axMenuElementSubMenu = (AxMenuElementSubMenu) element.MenuElement;

                axMenuElementSubMenu.Label = this.labelManager.createLabel(axMenuElementSubMenu.Label);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetMenuExtensionModelInfo(axMenuExtension.Name).FirstOrDefault<ModelInfo>());

                metaModelService.MetadataProvider.MenuExtensions.Update(axMenuExtension, saveInfo);
            }
        }
    }

    public class HMLabelService_Report : HMLabelService
    {
        protected Report report;
        protected AxReport axReport;

        public HMLabelService_Report(Report report)
        {
            this.report = report;
        }

        public HMLabelService_Report(AxReport axReport)
        {
            this.axReport = axReport;
        }

        public override void run()
        {
            // this.report.Title = this.labelManager.createLabel(this.report.Title);
        }

        public override void runAX()
        {           
            //this.axReport.Title = this.labelManager.createLabel(this.axReport.Title);

            //if (metaModelService != null)
            //{
            //    ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetReportModelInfo(axReport.Name).FirstOrDefault<ModelInfo>());
            //    metaModelService.UpdateReport(axReport, saveInfo);
            //}
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to table and child elements
    /// </summary>
    /// <remarks>Currently, the following elements are covered: table, fields, field groups</remarks>
    public class HMLabelService_Table : HMLabelService
    {
        protected Table table;
        protected AxTable axTable;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="table">Selected table element</param>
        public HMLabelService_Table(Table table)
        {
            this.table = table;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axTable">Selected table element</param>
        public HMLabelService_Table(AxTable axTable)
        {
            this.axTable = axTable;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.table.Label = this.labelManager.createLabel(this.table.Label);

            // Dev doc
            this.table.DeveloperDocumentation = this.labelManager.createLabel(this.table.DeveloperDocumentation);

            foreach (BaseField field in this.table.BaseFields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Tables.FieldGroup fieldGroup in this.table.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axTable.Label = this.labelManager.createLabel(this.axTable.Label);

            // Dev doc
            this.axTable.DeveloperDocumentation = this.labelManager.createLabel(this.axTable.DeveloperDocumentation);

            foreach (AxTableField field in this.axTable.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (AxTableFieldGroup fieldGroup in this.axTable.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }

            if (generateForCode)
            {
                // 10/23/2020 Add logic to process labels in code
                foreach (AxMethod method in this.axTable.Methods)
                {
                    method.Source = this.processLabelsInCode(method.Source);
                }
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetTableModelInfo(axTable.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateTable(axTable, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to table extension and child elements
    /// </summary>
    /// <remarks>Currently, the following elements are covered: table extension, fields, field groups</remarks>
    public class HMLabelService_TableExtension : HMLabelService
    {
        protected TableExtension table;
        protected AxTableExtension axTable;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="table">Selected table element</param>
        public HMLabelService_TableExtension(TableExtension table)
        {
            this.table = table;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axTable">Selected table element</param>
        public HMLabelService_TableExtension(AxTableExtension axTable)
        {
            this.axTable = axTable;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            foreach (BaseField field in this.table.BaseFields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Tables.FieldGroup fieldGroup in this.table.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            foreach (AxTableField field in this.axTable.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);
                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (AxTableFieldGroup fieldGroup in this.axTable.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetTableExtensionModelInfo(axTable.Name).FirstOrDefault<ModelInfo>());
                metaModelService.MetadataProvider.TableExtensions.Update(axTable, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to EDT
    /// </summary>
    public class HMLabelService_Edt : HMLabelService
    {
        protected EdtBase edt;
        protected AxEdt axEDT;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="edt">Selected EDT element</param>
        public HMLabelService_Edt(EdtBase edt)
        {
            this.edt = edt;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axEDT">Selected EDT element</param>
        public HMLabelService_Edt(AxEdt axEDT)
        {
            this.axEDT = axEDT;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.edt.Label = this.labelManager.createLabel(this.edt.Label);

            // Help text
            this.edt.HelpText = this.labelManager.createLabel(this.edt.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axEDT.Label = this.labelManager.createLabel(this.axEDT.Label);

            // Help text
            this.axEDT.HelpText = this.labelManager.createLabel(this.axEDT.HelpText);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetExtendedDataTypeModelInfo(axEDT.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateExtendedDataType(axEDT, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to BaseEnum and child elements
    /// </summary>
    public class HMLabelService_BaseEnum : HMLabelService
    {
        protected BaseEnum baseEnum;
        protected AxEnum axEnum;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="baseEnum">Selected baseenum element</param>
        public HMLabelService_BaseEnum(BaseEnum baseEnum)
        {
            this.baseEnum = baseEnum;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axEnum">Selected baseenum element</param>
        public HMLabelService_BaseEnum(AxEnum axEnum)
        {
            this.axEnum = axEnum;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.baseEnum.Label = this.labelManager.createLabel(this.baseEnum.Label);
            this.baseEnum.Help = this.labelManager.createLabel(this.baseEnum.Help);

            // Elements labels
            foreach (BaseEnumValue values in this.baseEnum.BaseEnumValues)
            {
                values.Label = this.labelManager.createLabel(values.Label);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axEnum.Label = this.labelManager.createLabel(this.axEnum.Label);
            this.axEnum.Help = this.labelManager.createLabel(this.axEnum.Help);

            // Elements labels
            foreach (AxEnumValue values in this.axEnum.EnumValues)
            {
                values.Label = this.labelManager.createLabel(values.Label);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetEnumModelInfo(axEnum.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateEnum(axEnum, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to BaseEnum extension and child elements
    /// </summary>
    public class HMLabelService_BaseEnumExtension : HMLabelService
    {
        protected BaseEnumExtension baseEnumExtension;
        protected AxEnumExtension axEnumExtension;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="baseEnumExtension">Selected baseenum extension element</param>
        public HMLabelService_BaseEnumExtension(BaseEnumExtension baseEnumExtension)
        {
            this.baseEnumExtension = baseEnumExtension;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axEnumExtension">Selected baseenum extension element</param>
        public HMLabelService_BaseEnumExtension(AxEnumExtension axEnumExtension)
        {
            this.axEnumExtension = axEnumExtension;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Elements labels
            foreach (BaseEnumValue values in this.baseEnumExtension.BaseEnumValues)
            {
                values.Label = this.labelManager.createLabel(values.Label);
            }
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Elements labels
            foreach (AxEnumValue values in this.axEnumExtension.EnumValues)
            {
                values.Label = this.labelManager.createLabel(values.Label);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetEnumExtensionModelInfo(axEnumExtension.Name).FirstOrDefault<ModelInfo>());
                metaModelService.MetadataProvider.EnumExtensions.Update(axEnumExtension, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to MenuItem and child elements
    /// </summary>
    public class HMLabelService_MenuItem : HMLabelService
    {
        protected MenuItem menuItem;
        protected AxMenuItem axMenuItem;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="menuItem">Selected menuitem element</param>
        public HMLabelService_MenuItem(MenuItem menuItem)
        {
            this.menuItem = menuItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axMenuItem">Selected menuitem element</param>
        public HMLabelService_MenuItem(AxMenuItem axMenuItem)
        {
            this.axMenuItem = axMenuItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.menuItem.Label = this.labelManager.createLabel(this.menuItem.Label);

            // Help text
            this.menuItem.HelpText = this.labelManager.createLabel(this.menuItem.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axMenuItem.Label = this.labelManager.createLabel(this.axMenuItem.Label);

            // Help text
            this.axMenuItem.HelpText = this.labelManager.createLabel(this.axMenuItem.HelpText);

            ModelSaveInfo saveInfo;

            if (metaModelService != null)
            {
                if (axMenuItem is AxMenuItemAction)
                {
                    saveInfo = new ModelSaveInfo(metaModelService.GetMenuItemActionModelInfo(axMenuItem.Name).FirstOrDefault<ModelInfo>());
                    metaModelService.UpdateMenuItemAction(axMenuItem as AxMenuItemAction, saveInfo);
                }
                else if (axMenuItem is AxMenuItemDisplay)
                {
                    saveInfo = new ModelSaveInfo(metaModelService.GetMenuItemDisplayModelInfo(axMenuItem.Name).FirstOrDefault<ModelInfo>());
                    metaModelService.UpdateMenuItemDisplay(axMenuItem as AxMenuItemDisplay, saveInfo);
                }
                else
                {
                    saveInfo = new ModelSaveInfo(metaModelService.GetMenuItemOutputModelInfo(axMenuItem.Name).FirstOrDefault<ModelInfo>());
                    metaModelService.UpdateMenuItemOutput(axMenuItem as AxMenuItemOutput, saveInfo);
                }
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to view and child elements
    /// </summary>
    /// <remarks>Currently, the following elements are covered: view, fields, field groups</remarks>
    public class HMLabelService_View : HMLabelService
    {
        protected View view;
        protected AxView axView;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="view">Selected view element</param>
        public HMLabelService_View(View view)
        {
            this.view = view;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axView">Selected view element</param>
        public HMLabelService_View(AxView axView)
        {
            this.axView = axView;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.view.Label = this.labelManager.createLabel(this.view.Label);

            // Dev doc
            this.view.DeveloperDocumentation = this.labelManager.createLabel(this.view.DeveloperDocumentation);

            foreach (ViewBaseField field in this.view.ViewBaseFields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Views.FieldGroup fieldGroup in this.view.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axView.Label = this.labelManager.createLabel(this.axView.Label);

            // Dev doc
            this.axView.DeveloperDocumentation = this.labelManager.createLabel(this.axView.DeveloperDocumentation);

            foreach (AxViewField field in this.axView.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (AxTableFieldGroup fieldGroup in this.axView.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }

            if (generateForCode)
            {
                // 10/23/2020 Add logic to process labels in code
                foreach (AxMethod method in this.axView.Methods)
                {
                    method.Source = this.processLabelsInCode(method.Source);
                }
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetViewModelInfo(axView.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateView(axView, saveInfo);
            }
        }
    }


    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to view extension and child elements
    /// </summary>
    /// <remarks>Currently, the following elements are covered: view extension, fields, field groups</remarks>
    public class HMLabelService_ViewExtension : HMLabelService
    {
        protected ViewExtension view;
        protected AxViewExtension axView;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="view">Selected view element</param>
        public HMLabelService_ViewExtension(ViewExtension view)
        {
            this.view = view;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axView">Selected view element</param>
        public HMLabelService_ViewExtension(AxViewExtension axView)
        {
            this.axView = axView;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            foreach (ViewBaseField field in this.view.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Views.FieldGroup fieldGroup in this.view.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            foreach (AxViewField field in this.axView.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (AxTableFieldGroup fieldGroup in this.axView.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetViewExtensionModelInfo(axView.Name).FirstOrDefault<ModelInfo>());
                metaModelService.MetadataProvider.ViewExtensions.Update(axView, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to form and child elements
    /// </summary>
    /// <remarks>Currently, the following elements are covered: design and controls</remarks>
    public class HMLabelService_Form : HMLabelService
    {
        protected Form form;
        protected AxForm axForm;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="form">Selected form element</param>
        public HMLabelService_Form(Form form)
        {
            this.form = form;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axForm">Selected form element</param>
        public HMLabelService_Form(AxForm axForm)
        {
            this.axForm = axForm;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Generate labels
        /// </summary>
        /// <param name="_controls">The children controls</param>
        public static void updateFormControls(ICollectionElement _controls, LabelManager _labelManager)
        {
            foreach (NamedElement namedcontrol in _controls)
            {
                FormControl control;

                if (namedcontrol is FormExtensionControl)
                {
                    FormExtensionControl extControl = namedcontrol as FormExtensionControl;
                    control = extControl.FormControl as FormControl;
                }
                else
                {
                    control = namedcontrol as FormControl;
                }

                switch (control.Type)
                {
                    case FormControlType.String:
                        FormStringControl stringControl = control as FormStringControl;
                        stringControl.Label = _labelManager.createLabel(stringControl.Label);
                        break;

                    case FormControlType.CheckBox:
                        FormCheckBoxControl checkboxControl = control as FormCheckBoxControl;
                        checkboxControl.Label = _labelManager.createLabel(checkboxControl.Label);
                        break;

                    case FormControlType.Group:
                        FormGroupControl groupControl = control as FormGroupControl;
                        groupControl.Caption = _labelManager.createLabel(groupControl.Caption);

                        updateFormControls(groupControl.FormControls, _labelManager);
                        break;

                    case FormControlType.Button:
                        FormButtonControl buttonControl = control as FormButtonControl;
                        buttonControl.Text = _labelManager.createLabel(buttonControl.Text);
                        break;

                    case FormControlType.Real:
                        FormRealControl realControl = control as FormRealControl;
                        realControl.Label = _labelManager.createLabel(realControl.Label);
                        break;

                    case FormControlType.Integer:
                        FormIntegerControl integerControl = control as FormIntegerControl;
                        integerControl.Label = _labelManager.createLabel(integerControl.Label);
                        break;

                    case FormControlType.ComboBox:
                        FormComboBoxControl comboboxControl = control as FormComboBoxControl;
                        comboboxControl.Label = _labelManager.createLabel(comboboxControl.Label);
                        break;

                    case FormControlType.Image:
                        FormImageControl imageControl = control as FormImageControl;
                        imageControl.Label = _labelManager.createLabel(imageControl.Label);
                        break;

                    case FormControlType.Date:
                        FormDateControl dateControl = control as FormDateControl;
                        dateControl.Label = _labelManager.createLabel(dateControl.Label);
                        break;

                    case FormControlType.RadioButton:
                        FormRadioButtonControl radioControl = control as FormRadioButtonControl;
                        radioControl.Caption = _labelManager.createLabel(radioControl.Caption);
                        break;

                    case FormControlType.ButtonGroup:
                        FormButtonGroupControl buttonGroupCaption = control as FormButtonGroupControl;
                        buttonGroupCaption.Caption = _labelManager.createLabel(buttonGroupCaption.Caption);

                        updateFormControls(buttonGroupCaption.FormControls as ICollectionElement, _labelManager);
                        break;

                    case FormControlType.Tab:
                        FormTabControl tabControl = control as FormTabControl;

                        updateFormControls(tabControl.FormControls as ICollectionElement, _labelManager);
                        break;

                    case FormControlType.TabPage:
                        FormTabPageControl tabpageControl = control as FormTabPageControl;
                        tabpageControl.Caption = _labelManager.createLabel(tabpageControl.Caption);

                        updateFormControls(tabpageControl.FormControls as ICollectionElement, _labelManager);
                        break;

                    case FormControlType.CommandButton:
                        FormCommandButtonControl commandbuttonControl = control as FormCommandButtonControl;
                        commandbuttonControl.Text = _labelManager.createLabel(commandbuttonControl.Text);
                        break;

                    case FormControlType.MenuButton:
                        FormMenuButtonControl menubuttonControl = control as FormMenuButtonControl;
                        menubuttonControl.Text = _labelManager.createLabel(menubuttonControl.Text);

                        updateFormControls(menubuttonControl.FormControls as ICollectionElement, _labelManager);
                        break;

                    case FormControlType.MenuFunctionButton:
                        FormMenuFunctionButtonControl menufunctionControl = control as FormMenuFunctionButtonControl;
                        menufunctionControl.Text = _labelManager.createLabel(menufunctionControl.Text);
                        break;

                    case FormControlType.ListBox:
                        FormListBoxControl listboxControl = control as FormListBoxControl;
                        listboxControl.Label = _labelManager.createLabel(listboxControl.Label);
                        break;

                    case FormControlType.Time:
                        FormTimeControl timeControl = control as FormTimeControl;
                        timeControl.Label = _labelManager.createLabel(timeControl.Label);
                        break;

                    case FormControlType.ButtonSeparator:
                        FormButtonSeparatorControl buttonseparatorControl = control as FormButtonSeparatorControl;
                        buttonseparatorControl.Text = _labelManager.createLabel(buttonseparatorControl.Text);
                        break;

                    case FormControlType.Guid:
                        FormGuidControl guidControl = control as FormGuidControl;
                        guidControl.Label = _labelManager.createLabel(guidControl.Label);
                        break;

                    case FormControlType.Int64:
                        FormInt64Control int64Control = control as FormInt64Control;
                        int64Control.Label = _labelManager.createLabel(int64Control.Label);
                        break;

                    case FormControlType.DateTime:
                        FormDateTimeControl datetimeControl = control as FormDateTimeControl;
                        datetimeControl.Label = _labelManager.createLabel(datetimeControl.Label);
                        break;

                    case FormControlType.ActionPane:
                        FormActionPaneControl actionpaneControl = control as FormActionPaneControl;
                        actionpaneControl.Caption = _labelManager.createLabel(actionpaneControl.Caption);
                        break;

                    case FormControlType.ActionPaneTab:
                        FormActionPaneTabControl actionpanetabControl = control as FormActionPaneTabControl;
                        actionpanetabControl.Caption = _labelManager.createLabel(actionpanetabControl.Caption);
                        break;

                    case FormControlType.SegmentedEntry:
                        FormSegmentedEntryControl segmentedEntryControl = control as FormSegmentedEntryControl;
                        segmentedEntryControl.Label = _labelManager.createLabel(segmentedEntryControl.Label);
                        break;

                    case FormControlType.DropDialogButton:
                        FormDropDialogButtonControl dropDialogButtonControl = control as FormDropDialogButtonControl;
                        dropDialogButtonControl.Text = _labelManager.createLabel(dropDialogButtonControl.Text);
                        break;

                    case FormControlType.ReferenceGroup:
                        FormReferenceGroupControl referenceGroupControl = control as FormReferenceGroupControl;
                        referenceGroupControl.Label = _labelManager.createLabel(referenceGroupControl.Label);

                        updateFormControls(referenceGroupControl.FormControls as ICollectionElement, _labelManager);
                        break;

                    case FormControlType.Grid:
                        FormGridControl gridControl = control as FormGridControl;
                        updateFormControls(gridControl.FormControls as ICollectionElement, _labelManager);
                        break;

                    default:
                        break;
                }

                control.HelpText = _labelManager.createLabel(control.HelpText);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Generate labels
        /// </summary>
        /// <param name="_controls">The children controls</param>
        public void updateFormControlsAX(KeyedObjectCollection<AxFormControl> _controls, LabelManager _labelManager)
        {
            foreach (AxFormControl control in _controls)
            {
                switch (control.Type)
                {
                    case FormControlType.String:
                        AxFormStringControl stringControl = control as AxFormStringControl;
                        stringControl.Label = _labelManager.createLabel(stringControl.Label);
                        break;

                    case FormControlType.CheckBox:
                        AxFormCheckBoxControl checkboxControl = control as AxFormCheckBoxControl;
                        checkboxControl.Label = _labelManager.createLabel(checkboxControl.Label);
                        break;

                    case FormControlType.Group:
                        AxFormGroupControl groupControl = control as AxFormGroupControl;
                        groupControl.Caption = _labelManager.createLabel(groupControl.Caption);

                        updateFormControlsAX(groupControl.Controls, _labelManager);
                        break;

                    case FormControlType.Button:
                        AxFormButtonControl buttonControl = control as AxFormButtonControl;
                        buttonControl.Text = _labelManager.createLabel(buttonControl.Text);
                        break;

                    case FormControlType.Real:
                        AxFormRealControl realControl = control as AxFormRealControl;
                        realControl.Label = _labelManager.createLabel(realControl.Label);
                        break;

                    case FormControlType.Integer:
                        AxFormIntegerControl integerControl = control as AxFormIntegerControl;
                        integerControl.Label = _labelManager.createLabel(integerControl.Label);
                        break;

                    case FormControlType.ComboBox:
                        AxFormComboBoxControl comboboxControl = control as AxFormComboBoxControl;
                        comboboxControl.Label = _labelManager.createLabel(comboboxControl.Label);
                        break;

                    case FormControlType.Image:
                        AxFormImageControl imageControl = control as AxFormImageControl;
                        imageControl.Label = _labelManager.createLabel(imageControl.Label);
                        break;

                    case FormControlType.Date:
                        AxFormDateControl dateControl = control as AxFormDateControl;
                        dateControl.Label = _labelManager.createLabel(dateControl.Label);
                        break;

                    case FormControlType.RadioButton:
                        AxFormRadioButtonControl radioControl = control as AxFormRadioButtonControl;
                        radioControl.Caption = _labelManager.createLabel(radioControl.Caption);
                        break;

                    case FormControlType.ButtonGroup:
                        AxFormButtonGroupControl buttonGroupCaption = control as AxFormButtonGroupControl;
                        buttonGroupCaption.Caption = _labelManager.createLabel(buttonGroupCaption.Caption);

                        updateFormControlsAX(buttonGroupCaption.Controls, _labelManager);
                        break;

                    case FormControlType.Tab:
                        AxFormTabControl tabControl = control as AxFormTabControl;

                        updateFormControlsAX(tabControl.Controls, _labelManager);
                        break;

                    case FormControlType.TabPage:
                        AxFormTabPageControl tabpageControl = control as AxFormTabPageControl;
                        tabpageControl.Caption = _labelManager.createLabel(tabpageControl.Caption);

                        updateFormControlsAX(tabpageControl.Controls, _labelManager);
                        break;

                    case FormControlType.CommandButton:
                        AxFormCommandButtonControl commandbuttonControl = control as AxFormCommandButtonControl;
                        commandbuttonControl.Text = _labelManager.createLabel(commandbuttonControl.Text);
                        break;

                    case FormControlType.MenuButton:
                        AxFormMenuButtonControl menubuttonControl = control as AxFormMenuButtonControl;
                        menubuttonControl.Text = _labelManager.createLabel(menubuttonControl.Text);

                        updateFormControlsAX(menubuttonControl.Controls, _labelManager);
                        break;

                    case FormControlType.MenuFunctionButton:
                        AxFormMenuFunctionButtonControl menufunctionControl = control as AxFormMenuFunctionButtonControl;
                        menufunctionControl.Text = _labelManager.createLabel(menufunctionControl.Text);
                        break;

                    case FormControlType.ListBox:
                        AxFormListBoxControl listboxControl = control as AxFormListBoxControl;
                        listboxControl.Label = _labelManager.createLabel(listboxControl.Label);
                        break;

                    case FormControlType.Time:
                        AxFormTimeControl timeControl = control as AxFormTimeControl;
                        timeControl.Label = _labelManager.createLabel(timeControl.Label);
                        break;

                    case FormControlType.ButtonSeparator:
                        AxFormButtonSeparatorControl buttonseparatorControl = control as AxFormButtonSeparatorControl;
                        buttonseparatorControl.Text = _labelManager.createLabel(buttonseparatorControl.Text);
                        break;

                    case FormControlType.Guid:
                        AxFormGuidControl guidControl = control as AxFormGuidControl;
                        guidControl.Label = _labelManager.createLabel(guidControl.Label);
                        break;

                    case FormControlType.Int64:
                        AxFormInt64Control int64Control = control as AxFormInt64Control;
                        int64Control.Label = _labelManager.createLabel(int64Control.Label);
                        break;

                    case FormControlType.DateTime:
                        AxFormDateTimeControl datetimeControl = control as AxFormDateTimeControl;
                        datetimeControl.Label = _labelManager.createLabel(datetimeControl.Label);
                        break;

                    case FormControlType.ActionPane:
                        AxFormActionPaneControl actionpaneControl = control as AxFormActionPaneControl;
                        actionpaneControl.Caption = _labelManager.createLabel(actionpaneControl.Caption);
                        break;

                    case FormControlType.ActionPaneTab:
                        AxFormActionPaneTabControl actionpanetabControl = control as AxFormActionPaneTabControl;
                        actionpanetabControl.Caption = _labelManager.createLabel(actionpanetabControl.Caption);
                        break;

                    case FormControlType.SegmentedEntry:
                        AxFormSegmentedEntryControl segmentedEntryControl = control as AxFormSegmentedEntryControl;
                        segmentedEntryControl.Label = _labelManager.createLabel(segmentedEntryControl.Label);
                        break;

                    case FormControlType.DropDialogButton:
                        AxFormDropDialogButtonControl dropDialogButtonControl = control as AxFormDropDialogButtonControl;
                        dropDialogButtonControl.Text = _labelManager.createLabel(dropDialogButtonControl.Text);
                        break;

                    case FormControlType.ReferenceGroup:
                        AxFormReferenceGroupControl referenceGroupControl = control as AxFormReferenceGroupControl;
                        referenceGroupControl.Label = _labelManager.createLabel(referenceGroupControl.Label);

                        updateFormControlsAX(referenceGroupControl.Controls, _labelManager);
                        break;

                    case FormControlType.Grid:
                        AxFormGridControl gridControl = control as AxFormGridControl;
                        updateFormControlsAX(gridControl.Controls, _labelManager);
                        break;

                    default:
                        break;
                }

                control.HelpText = _labelManager.createLabel(control.HelpText);

                if (generateForCode)
                {
                    foreach (AxMethod method in control.Methods)
                    {
                        method.Source = this.processLabelsInCode(method.Source);
                    }
                }
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Caption
            this.form.FormDesign.Caption = this.labelManager.createLabel(this.form.FormDesign.Caption);

            // Form controls
            updateFormControls(this.form.FormDesign.FormControls as ICollectionElement, this.labelManager);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Caption
            this.axForm.Design.Caption = this.labelManager.createLabel(this.axForm.Design.Caption);

            // 03/26/2024 Add logic to process labels in code
            if (generateForCode)
            {
                foreach (AxMethod method in this.axForm.Methods)
                {
                    method.Source = this.processLabelsInCode(method.Source);
                }
            }

            foreach (AxFormDataSourceRoot datasource in this.axForm.DataSources)
            {
                if (generateForCode)
                {
                    foreach (AxMethod method in datasource.Methods)
                    {
                        method.Source = this.processLabelsInCode(method.Source);
                    }
                }

                foreach (var axDataField in datasource.Fields)
                {
                    foreach (var method in axDataField.Methods)
                    {
                        method.Source = this.processLabelsInCode(method.Source);
                    }
                }
            }

            // Form controls
            updateFormControlsAX(this.axForm.Design.Controls, this.labelManager);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetFormModelInfo(axForm.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateForm(axForm, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to form extension and child elements
    /// </summary>
    /// <remarks>Currently, the following elements are covered: design and controls</remarks>
    public class HMLabelService_FormExtension : HMLabelService
    {
        protected FormExtension form;
        protected AxFormExtension axForm;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="form">Selected form element</param>
        public HMLabelService_FormExtension(FormExtension form)
        {
            this.form = form;
            this.axForm = DesignMetaModelService.Instance.GetFormExtension(form.Name);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axForm">Selected form element</param>
        public HMLabelService_FormExtension(AxFormExtension axForm)
        {
            this.axForm = axForm;
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Generate labels
        /// </summary>
        /// <param name="_controls">The children controls</param>
        public void updateFormControlsAX(KeyedObjectCollection<AxFormControl> _controls, LabelManager _labelManager)
        {
            foreach (AxFormControl control in _controls)
            {
                switch (control.Type)
                {
                    case FormControlType.String:
                        AxFormStringControl stringControl = control as AxFormStringControl;
                        stringControl.Label = _labelManager.createLabel(stringControl.Label);
                        break;

                    case FormControlType.CheckBox:
                        AxFormCheckBoxControl checkboxControl = control as AxFormCheckBoxControl;
                        checkboxControl.Label = _labelManager.createLabel(checkboxControl.Label);
                        break;

                    case FormControlType.Group:
                        AxFormGroupControl groupControl = control as AxFormGroupControl;
                        groupControl.Caption = _labelManager.createLabel(groupControl.Caption);

                        updateFormControlsAX(groupControl.Controls, _labelManager);
                        break;

                    case FormControlType.Button:
                        AxFormButtonControl buttonControl = control as AxFormButtonControl;
                        buttonControl.Text = _labelManager.createLabel(buttonControl.Text);
                        break;

                    case FormControlType.Real:
                        AxFormRealControl realControl = control as AxFormRealControl;
                        realControl.Label = _labelManager.createLabel(realControl.Label);
                        break;

                    case FormControlType.Integer:
                        AxFormIntegerControl integerControl = control as AxFormIntegerControl;
                        integerControl.Label = _labelManager.createLabel(integerControl.Label);
                        break;

                    case FormControlType.ComboBox:
                        AxFormComboBoxControl comboboxControl = control as AxFormComboBoxControl;
                        comboboxControl.Label = _labelManager.createLabel(comboboxControl.Label);
                        break;

                    case FormControlType.Image:
                        AxFormImageControl imageControl = control as AxFormImageControl;
                        imageControl.Label = _labelManager.createLabel(imageControl.Label);
                        break;

                    case FormControlType.Date:
                        AxFormDateControl dateControl = control as AxFormDateControl;
                        dateControl.Label = _labelManager.createLabel(dateControl.Label);
                        break;

                    case FormControlType.RadioButton:
                        AxFormRadioButtonControl radioControl = control as AxFormRadioButtonControl;
                        radioControl.Caption = _labelManager.createLabel(radioControl.Caption);
                        break;

                    case FormControlType.ButtonGroup:
                        AxFormButtonGroupControl buttonGroupCaption = control as AxFormButtonGroupControl;
                        buttonGroupCaption.Caption = _labelManager.createLabel(buttonGroupCaption.Caption);

                        updateFormControlsAX(buttonGroupCaption.Controls, _labelManager);
                        break;

                    case FormControlType.Tab:
                        AxFormTabControl tabControl = control as AxFormTabControl;

                        updateFormControlsAX(tabControl.Controls, _labelManager);
                        break;

                    case FormControlType.TabPage:
                        AxFormTabPageControl tabpageControl = control as AxFormTabPageControl;
                        tabpageControl.Caption = _labelManager.createLabel(tabpageControl.Caption);

                        updateFormControlsAX(tabpageControl.Controls, _labelManager);
                        break;

                    case FormControlType.CommandButton:
                        AxFormCommandButtonControl commandbuttonControl = control as AxFormCommandButtonControl;
                        commandbuttonControl.Text = _labelManager.createLabel(commandbuttonControl.Text);
                        break;

                    case FormControlType.MenuButton:
                        AxFormMenuButtonControl menubuttonControl = control as AxFormMenuButtonControl;
                        menubuttonControl.Text = _labelManager.createLabel(menubuttonControl.Text);

                        updateFormControlsAX(menubuttonControl.Controls, _labelManager);
                        break;

                    case FormControlType.MenuFunctionButton:
                        AxFormMenuFunctionButtonControl menufunctionControl = control as AxFormMenuFunctionButtonControl;
                        menufunctionControl.Text = _labelManager.createLabel(menufunctionControl.Text);
                        break;

                    case FormControlType.ListBox:
                        AxFormListBoxControl listboxControl = control as AxFormListBoxControl;
                        listboxControl.Label = _labelManager.createLabel(listboxControl.Label);
                        break;

                    case FormControlType.Time:
                        AxFormTimeControl timeControl = control as AxFormTimeControl;
                        timeControl.Label = _labelManager.createLabel(timeControl.Label);
                        break;

                    case FormControlType.ButtonSeparator:
                        AxFormButtonSeparatorControl buttonseparatorControl = control as AxFormButtonSeparatorControl;
                        buttonseparatorControl.Text = _labelManager.createLabel(buttonseparatorControl.Text);
                        break;

                    case FormControlType.Guid:
                        AxFormGuidControl guidControl = control as AxFormGuidControl;
                        guidControl.Label = _labelManager.createLabel(guidControl.Label);
                        break;

                    case FormControlType.Int64:
                        AxFormInt64Control int64Control = control as AxFormInt64Control;
                        int64Control.Label = _labelManager.createLabel(int64Control.Label);
                        break;

                    case FormControlType.DateTime:
                        AxFormDateTimeControl datetimeControl = control as AxFormDateTimeControl;
                        datetimeControl.Label = _labelManager.createLabel(datetimeControl.Label);
                        break;

                    case FormControlType.ActionPane:
                        AxFormActionPaneControl actionpaneControl = control as AxFormActionPaneControl;
                        actionpaneControl.Caption = _labelManager.createLabel(actionpaneControl.Caption);
                        break;

                    case FormControlType.ActionPaneTab:
                        AxFormActionPaneTabControl actionpanetabControl = control as AxFormActionPaneTabControl;
                        actionpanetabControl.Caption = _labelManager.createLabel(actionpanetabControl.Caption);
                        break;

                    case FormControlType.SegmentedEntry:
                        AxFormSegmentedEntryControl segmentedEntryControl = control as AxFormSegmentedEntryControl;
                        segmentedEntryControl.Label = _labelManager.createLabel(segmentedEntryControl.Label);
                        break;

                    case FormControlType.DropDialogButton:
                        AxFormDropDialogButtonControl dropDialogButtonControl = control as AxFormDropDialogButtonControl;
                        dropDialogButtonControl.Text = _labelManager.createLabel(dropDialogButtonControl.Text);
                        break;

                    case FormControlType.ReferenceGroup:
                        AxFormReferenceGroupControl referenceGroupControl = control as AxFormReferenceGroupControl;
                        referenceGroupControl.Label = _labelManager.createLabel(referenceGroupControl.Label);

                        updateFormControlsAX(referenceGroupControl.Controls, _labelManager);
                        break;

                    case FormControlType.Grid:
                        AxFormGridControl gridControl = control as AxFormGridControl;
                        updateFormControlsAX(gridControl.Controls, _labelManager);
                        break;

                    default:
                        break;
                }

                control.HelpText = _labelManager.createLabel(control.HelpText);
            }
        }


        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Form controls
            //HMLabelService_Form.updateFormControls(this.form.Controls as ICollectionElement, this.labelManager);

            // Use the other run method instead.
            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            KeyedObjectCollection<AxFormExtensionControl> extControls = this.axForm.Controls;
            KeyedObjectCollection<AxFormControl> controls = new KeyedObjectCollection<AxFormControl>();

            foreach (AxFormExtensionControl extControl in extControls)
            {
                controls.Add(extControl.FormControl);
            }

            // Form controls
            this.updateFormControlsAX(controls, this.labelManager);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetFormExtensionModelInfo(axForm.Name).FirstOrDefault<ModelInfo>());
                metaModelService.MetadataProvider.FormExtensions.Update(axForm, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to security privileges and child elements
    /// </summary>
    public class HMLabelService_SecurityPrivilege : HMLabelService
    {
        protected SecurityPrivilege securityPrivilege;
        protected AxSecurityPrivilege axSecurityPrivilege;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="securityPrivilege">Selected SecurityPrivilege element</param>
        public HMLabelService_SecurityPrivilege(SecurityPrivilege securityPrivilege)
        {
            this.securityPrivilege = securityPrivilege;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axSecurityPrivilege">Selected SecurityPrivilege element</param>
        public HMLabelService_SecurityPrivilege(AxSecurityPrivilege axSecurityPrivilege)
        {
            this.axSecurityPrivilege = axSecurityPrivilege;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.securityPrivilege.Label = this.labelManager.createLabel(this.securityPrivilege.Label);
            this.securityPrivilege.Description = this.labelManager.createLabel(this.securityPrivilege.Description);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axSecurityPrivilege.Label = this.labelManager.createLabel(this.axSecurityPrivilege.Label);
            this.axSecurityPrivilege.Description = this.labelManager.createLabel(this.axSecurityPrivilege.Description);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetSecurityPrivilegeModelInfo(axSecurityPrivilege.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateSecurityPrivilege(axSecurityPrivilege, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to security duties and child elements
    /// </summary>
    public class HMLabelService_SecurityDuty : HMLabelService
    {
        protected SecurityDuty securityDuty;
        protected AxSecurityDuty axSecurityDuty;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="securityPrivilege">Selected SecurityPrivilege element</param>
        public HMLabelService_SecurityDuty(SecurityDuty securityDuty)
        {
            this.securityDuty = securityDuty;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axSecurityDuty">Selected SecurityPrivilege element</param>
        public HMLabelService_SecurityDuty(AxSecurityDuty axSecurityDuty)
        {
            this.axSecurityDuty = axSecurityDuty;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.securityDuty.Label = this.labelManager.createLabel(this.securityDuty.Label);
            this.securityDuty.Description = this.labelManager.createLabel(this.securityDuty.Description);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axSecurityDuty.Label = this.labelManager.createLabel(this.axSecurityDuty.Label);
            this.axSecurityDuty.Description = this.labelManager.createLabel(this.axSecurityDuty.Description);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetSecurityDutyModelInfo(axSecurityDuty.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateSecurityDuty(axSecurityDuty, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to workflow object
    /// </summary>
    public class HMLabelService_WorkflowHierarchyAssignmentProvider : HMLabelService
    {
        protected WorkflowHierarchyAssignmentProvider provider;
        protected AxWorkflowHierarchyAssignmentProvider axProvider;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="provider">Selected provider element</param>
        public HMLabelService_WorkflowHierarchyAssignmentProvider(WorkflowHierarchyAssignmentProvider provider)
        {
            this.provider = provider;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axProvider">Selected provider element</param>
        public HMLabelService_WorkflowHierarchyAssignmentProvider(AxWorkflowHierarchyAssignmentProvider axProvider)
        {
            this.axProvider = axProvider;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.provider.Label = this.labelManager.createLabel(this.provider.Label);

            // Help text
            this.provider.HelpText = this.labelManager.createLabel(this.provider.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axProvider.Label = this.labelManager.createLabel(this.axProvider.Label);

            // Help text
            this.axProvider.HelpText = this.labelManager.createLabel(this.axProvider.HelpText);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetWorkflowHierarchyAssignmentProviderModelInfo(axProvider.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateWorkflowHierarchyAssignmentProvider(axProvider, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to workflow object
    /// </summary>
    public class HMLabelService_WorkflowApproval : HMLabelService
    {
        protected WorkflowApproval approval;
        protected AxWorkflowApproval axApproval;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="approval">Selected approval element</param>
        public HMLabelService_WorkflowApproval(WorkflowApproval approval)
        {
            this.approval = approval;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axApproval">Selected approval element</param>
        public HMLabelService_WorkflowApproval(AxWorkflowApproval axApproval)
        {
            this.axApproval = axApproval;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.approval.Label = this.labelManager.createLabel(this.approval.Label);

            // Help text
            this.approval.HelpText = this.labelManager.createLabel(this.approval.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axApproval.Label = this.labelManager.createLabel(this.axApproval.Label);

            // Help text
            this.axApproval.HelpText = this.labelManager.createLabel(this.axApproval.HelpText);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetWorkflowApprovalModelInfo(axApproval.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateWorkflowApproval(axApproval, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to workflow object
    /// </summary>
    public class HMLabelService_WorkflowCategory : HMLabelService
    {
        protected WorkflowCategory category;
        protected AxWorkflowCategory axCategory;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="category">Selected category element</param>
        public HMLabelService_WorkflowCategory(WorkflowCategory category)
        {
            this.category = category;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axCategory">Selected category element</param>
        public HMLabelService_WorkflowCategory(AxWorkflowCategory axCategory)
        {
            this.axCategory = axCategory;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.category.Label = this.labelManager.createLabel(this.category.Label);

            // Help text
            this.category.HelpText = this.labelManager.createLabel(this.category.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axCategory.Label = this.labelManager.createLabel(this.axCategory.Label);

            // Help text
            this.axCategory.HelpText = this.labelManager.createLabel(this.axCategory.HelpText);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetWorkflowCategoryModelInfo(axCategory.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateWorkflowCategory(axCategory, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to workflow object
    /// </summary>
    public class HMLabelService_WorkflowTask : HMLabelService
    {
        protected WorkflowTask task;
        protected AxWorkflowTask axTask;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="category">Selected task element</param>
        public HMLabelService_WorkflowTask(WorkflowTask task)
        {
            this.task = task;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axTask">Selected task element</param>
        public HMLabelService_WorkflowTask(AxWorkflowTask axTask)
        {
            this.axTask = axTask;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.task.Label = this.labelManager.createLabel(this.task.Label);

            // Help text
            this.task.HelpText = this.labelManager.createLabel(this.task.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axTask.Label = this.labelManager.createLabel(this.axTask.Label);

            // Help text
            this.axTask.HelpText = this.labelManager.createLabel(this.axTask.HelpText);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetWorkflowTaskModelInfo(axTask.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateWorkflowTask(axTask, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// Creates labels to workflow object
    /// </summary>
    public class HMLabelService_WorkflowType : HMLabelService
    {
        protected WorkflowTemplate type;
        protected AxWorkflowTemplate axType;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="type">Selected task element</param>
        public HMLabelService_WorkflowType(WorkflowTemplate type)
        {
            this.type = type;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initiaze the global variable
        /// </summary>
        /// <param name="type">Selected task element</param>
        public HMLabelService_WorkflowType(AxWorkflowTemplate axType)
        {
            this.axType = axType;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.type.Label = this.labelManager.createLabel(this.type.Label);

            // Help text
            this.type.HelpText = this.labelManager.createLabel(this.type.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axType.Label = this.labelManager.createLabel(this.axType.Label);

            // Help text
            this.axType.HelpText = this.labelManager.createLabel(this.axType.HelpText);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetWorkflowTemplateModelInfo(axType.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateWorkflowTemplate(axType, saveInfo);
            }
        }
    }

    // v1.2

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 1/29/2018
    /// Creates labels to data entity view object
    /// </summary>
    public class HMLabelService_DataEntityView : HMLabelService
    {
        protected DataEntityView item;
        protected AxDataEntityView axItem;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="item">Selected element</param>
        public HMLabelService_DataEntityView(DataEntityView item)
        {
            this.item = item;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axItem">Selected element</param>
        public HMLabelService_DataEntityView(AxDataEntityView axItem)
        {
            this.axItem = axItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.item.Label = this.labelManager.createLabel(this.item.Label);

            // Help text
            this.item.DeveloperDocumentation = this.labelManager.createLabel(this.item.DeveloperDocumentation);

            foreach (DataEntityViewMappedField field in this.item.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.DataEntityViews.FieldGroup fieldGroup in this.item.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axItem.Label = this.labelManager.createLabel(this.axItem.Label);

            // Help text
            this.axItem.DeveloperDocumentation = this.labelManager.createLabel(this.axItem.DeveloperDocumentation);

            foreach (AxDataEntityViewField field in this.axItem.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (AxTableFieldGroup fieldGroup in this.axItem.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }

            if (generateForCode)
            {
                // 10/23/2020 Add logic to process labels in code
                foreach (AxMethod method in this.axItem.Methods)
                {
                    method.Source = this.processLabelsInCode(method.Source);
                }
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetDataEntityViewModelInfo(axItem.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateDataEntityView(axItem, saveInfo);
            }
        }
    }


    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 1/29/2018
    /// Creates labels to data entity view extension object
    /// </summary>
    public class HMLabelService_DataEntityViewExtension : HMLabelService
    {
        protected DataEntityViewExtension item;
        protected AxDataEntityViewExtension axItem;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="item">Selected element</param>
        public HMLabelService_DataEntityViewExtension(DataEntityViewExtension item)
        {
            this.item = item;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axItem">Selected element</param>
        public HMLabelService_DataEntityViewExtension(AxDataEntityViewExtension axItem)
        {
            this.axItem = axItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void run()
        {
            foreach (BaseField field in this.item.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.DataEntityViews.FieldGroup fieldGroup in this.item.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            foreach (AxDataEntityViewField field in this.axItem.Fields)
            {
                // Field label
                field.Label = this.labelManager.createLabel(field.Label);

                // Field help text
                field.HelpText = this.labelManager.createLabel(field.HelpText);
            }

            foreach (AxTableFieldGroup fieldGroup in this.axItem.FieldGroups)
            {
                // Field group label
                fieldGroup.Label = this.labelManager.createLabel(fieldGroup.Label);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.MetadataProvider.DataEntityViewExtensions.GetModelInfo(axItem.Name).FirstOrDefault<ModelInfo>());
                metaModelService.MetadataProvider.DataEntityViewExtensions.Update(axItem, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 1/29/2018
    /// Creates labels to data entity view object
    /// </summary>
    public class HMLabelService_QuerySimple : HMLabelService
    {
        protected SimpleQuery item;
        protected AxQuerySimple axItem;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="item">Selected element</param>
        public HMLabelService_QuerySimple(SimpleQuery item)
        {
            this.item = item;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axItem">Selected element</param>
        public HMLabelService_QuerySimple(AxQuerySimple axItem)
        {
            this.axItem = axItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Generate labels
        /// </summary>
        /// <param name="_ds">The children data sources</param>
        internal static void updateLabel(ICollectionElement _ds, LabelManager _labelManager)
        {
            foreach (QueryDataSource ds in _ds)
            {
                ds.Label = _labelManager.createLabel(ds.Label);

                foreach (QueryDataSourceRange range in ds.QueryDataSourceRanges)
                {
                    range.Label = _labelManager.createLabel(range.Label);
                }

                updateLabel(ds.QueryEmbeddedDataSources, _labelManager);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Generate labels
        /// </summary>
        /// <param name="_ds">The children data sources</param>
        internal static void updateLabelAX(KeyedObjectCollection<AxQuerySimpleEmbeddedDataSource> _ds, LabelManager _labelManager)
        {
            foreach (AxQuerySimpleDataSource ds in _ds)
            {
                ds.Label = _labelManager.createLabel(ds.Label);

                foreach (AxQuerySimpleDataSourceRange range in ds.Ranges)
                {
                    range.Label = _labelManager.createLabel(range.Label);
                }

                updateLabelAX(ds.DataSources, _labelManager);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.item.Title = this.labelManager.createLabel(this.item.Title);

            // Help text
            this.item.Description = this.labelManager.createLabel(this.item.Description);

            foreach (QueryRootDataSource ds in this.item.QueryRootDataSources)
            {
                ds.Label = this.labelManager.createLabel(ds.Label);

                foreach (QueryDataSourceRange range in ds.QueryDataSourceRanges)
                {
                    range.Label = this.labelManager.createLabel(range.Label);
                }

                foreach (QueryHavingPredicate having in ds.QueryHavingPredicates)
                {
                    having.Label = this.labelManager.createLabel(having.Label);
                }

                updateLabel(ds.QueryEmbeddedDataSources, this.labelManager);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axItem.Title = this.labelManager.createLabel(this.axItem.Title);

            // Help text
            this.axItem.Description = this.labelManager.createLabel(this.axItem.Description);

            foreach (AxQuerySimpleRootDataSource ds in this.axItem.DataSources)
            {
                ds.Label = this.labelManager.createLabel(ds.Label);

                foreach (AxQuerySimpleDataSourceRange range in ds.Ranges)
                {
                    range.Label = this.labelManager.createLabel(range.Label);
                }

                foreach (AxQuerySimpleHavingPredicate having in ds.Having)
                {
                    having.Label = this.labelManager.createLabel(having.Label);
                }

                updateLabelAX(ds.DataSources, this.labelManager);
            }
            if (generateForCode)
            {
                // 10/23/2020 Add logic to process labels in code
                foreach (AxMethod method in axItem.Methods)
                {
                    method.Source = this.processLabelsInCode(method.Source);
                }
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetQueryModelInfo(axItem.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateQuery(axItem, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 1/29/2018
    /// Creates labels to data entity view object
    /// </summary>
    public class HMLabelService_QuerySimpleExtension : HMLabelService
    {
        protected SimpleQueryExtension item;
        protected AxQuerySimpleExtension axItem;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="item">Selected element</param>
        public HMLabelService_QuerySimpleExtension(SimpleQueryExtension item)
        {
            this.item = item;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axItem">Selected element</param>
        public HMLabelService_QuerySimpleExtension(AxQuerySimpleExtension axItem)
        {
            this.axItem = axItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Generate labels
        /// </summary>
        /// <param name="_ds">The children data sources</param>
        internal static void updateLabel(ICollectionElement _ds, LabelManager _labelManager)
        {
            foreach (QueryDataSource ds in _ds)
            {
                foreach (AxQuerySimpleDataSourceRange range in ds.QueryDataSourceRanges)
                {
                    range.Label = _labelManager.createLabel(range.Label);
                }

                updateLabel(ds.QueryEmbeddedDataSources, _labelManager);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Generate labels
        /// </summary>
        /// <param name="_ds">The children data sources</param>
        internal static void updateLabelAX(KeyedObjectCollection<AxQueryExtensionEmbeddedDataSource> _ds, LabelManager _labelManager)
        {
            foreach (AxQueryExtensionEmbeddedDataSource ds in _ds)
            {
                foreach (AxQuerySimpleDataSourceRange range in ds.DataSource.Ranges)
                {
                    range.Label = _labelManager.createLabel(range.Label);
                }
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void run()
        {
            foreach (QueryRootDataSource ds in this.item.DataSources)
            {
                foreach (QueryDataSourceRange range in ds.QueryDataSourceRanges)
                {
                    range.Label = this.labelManager.createLabel(range.Label);
                }

                foreach (QueryHavingPredicate having in ds.QueryHavingPredicates)
                {
                    having.Label = this.labelManager.createLabel(having.Label);
                }

                updateLabel(ds.QueryEmbeddedDataSources, this.labelManager);
            }
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            foreach (AxQueryExtensionEmbeddedDataSource ds in this.axItem.DataSources)
            {
                foreach (AxQuerySimpleDataSourceRange range in ds.DataSource.Ranges)
                {
                    range.Label = this.labelManager.createLabel(range.Label);
                }
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetQuerySimpleExtensionModelInfo(axItem.Name).FirstOrDefault<ModelInfo>());
                metaModelService.MetadataProvider.QuerySimpleExtensions.Update(axItem, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 1/29/2018
    /// Creates labels to data entity view object
    /// </summary>
    public class HMLabelService_Tile : HMLabelService
    {
        protected Tile item;
        protected AxTile axItem;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="item">Selected element</param>
        public HMLabelService_Tile(Tile item)
        {
            this.item = item;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axItem">Selected element</param>
        public HMLabelService_Tile(AxTile axItem)
        {
            this.axItem = axItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void run()
        {
            // Label
            this.item.Label = this.labelManager.createLabel(this.item.Label);

            // Help text
            this.item.HelpText = this.labelManager.createLabel(this.item.HelpText);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 1/29/2018
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            // Label
            this.axItem.Label = this.labelManager.createLabel(this.axItem.Label);

            // Help text
            this.axItem.HelpText = this.labelManager.createLabel(this.axItem.HelpText);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetTileModelInfo(axItem.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateTile(axItem, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 10/23/2020
    /// Creates labels to data entity view object
    /// </summary>
    public class HMLabelService_Class : HMLabelService
    {
        protected AxClass axItem;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 10/23/2020
        /// Initiaze the global variable
        /// </summary>
        /// <param name="axItem">Selected element</param>
        public HMLabelService_Class(AxClass axItem)
        {
            this.axItem = axItem;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 10/23/2020
        /// Run the process
        /// </summary>
        public override void run()
        {
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 10/23/2020
        /// Run the process
        /// </summary>
        public override void runAX()
        {
            if (generateForCode)
            {
                // Apply label to class declaration
                axItem.Declaration = this.processLabelsInCode(axItem.Declaration);

                // 10/23/2020 Add logic to process labels in code
                foreach (AxMethod method in axItem.Methods)
                {
                    method.Source = this.processLabelsInCode(method.Source);
                }
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetClassModelInfo(axItem.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateClass(axItem, saveInfo);
            }
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// This class was designed to generate labels.
    /// </summary>
    public class LabelManager
    {
        protected const string PREFIX = "@";
        protected string gLableFileId;
        protected string gDescription;

        private List<AxLabelFile> labelFiles;

        protected EnvDTE.DTE dte;
        protected VSApplicationContext context;

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Initialize the class
        /// </summary>
        /// <param name="_labelFileId">Label file ID</param>
        /// <param name="_labelFile">Label file element</param>
        /// <param name="_description">Description</param>
        public LabelManager(string _labelFileId, AxLabelFile _labelFile, string _description)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            gLableFileId = _labelFileId;
            gDescription = _description;
            this.labelFiles = new List<AxLabelFile>();
            this.dte = CoreUtility.ServiceProvider.GetService(typeof(DTE2)) as DTE2;
            this.context = new VSApplicationContext((IServiceProvider)dte); 
            this.labelFiles.Add(_labelFile);
            
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Checks if the label should be created, based on the prefix @@@
        /// </summary>
        /// <param name="propertyText">Label string</param>
        /// <returns>true/false</returns>
        protected bool isPrefixed(string propertyText)
        {
            return propertyText.StartsWith(PREFIX);
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// If property text is prefixed, then create a new label.
        /// Otherwise, return the very same property text value
        /// </summary>
        /// <param name="propertyText">Label text from element property (label, help text, caption, etc)</param>
        /// <returns>The new label id created</returns>
        public string createLabel(string propertyText)
        {
            string ret = propertyText;

            if (propertyText != string.Empty)
            {
                if (!this.isPrefixed(propertyText))
                {
                    // Model simple name + label id name. It can be avoid that the first character is a number.
                    string labelId = gLableFileId + LabelManager.getLabelId(propertyText); 
                    string label = propertyText;

                    foreach (AxLabelFile labelfile in this.labelFiles)
                    {
                        LabelControllerFactory factory = new LabelControllerFactory();
                        LabelEditorController labelEditorController = factory.GetOrCreateLabelController(labelfile, context);

                        if (!labelEditorController.Exists(labelId))
                        {
                            labelEditorController.Insert(labelId, label, gDescription);
                            labelEditorController.Save();
                        }
                    }

                    ret = $"@{gLableFileId}:{labelId}";
                }
            }

            return ret;
        }

        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Extracts the label id from label string property
        /// </summary>
        /// <param name="propertyText">Label string e.g. @@@MyNewLabelId=My new label id</param>
        /// <returns>The label string e.g. MyNewLabelId</returns>
        public static string getLabelId(string propertyText)
        {
            string ret = string.Empty;

            ret = TextTools.UpperFirst(propertyText);
            ret = Regex.Replace(ret, @"[^a-zA-z0-9]", "");

            return ret;
        }
    }

    /// <summary>
    /// HM_D365_Addin_LabelGenerator
    /// Byron Zhang - 12/26/2017
    /// This class was designed to upper every word in a string
    /// </summary>
    public static class TextTools
    {
        /// <summary>
        /// HM_D365_Addin_LabelGenerator
        /// Byron Zhang - 12/26/2017
        /// Uppercase first letters of all words in the string.
        /// </summary>
        public static string UpperFirst(string s)
        {
            return Regex.Replace(s, @"\b[a-z]\w+", delegate (Match match)
            {
                string v = match.ToString();
                return char.ToUpper(v[0]) + v.Substring(1);
            });
        }
    }
}
