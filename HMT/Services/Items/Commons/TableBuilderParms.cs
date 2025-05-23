﻿using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel.Extensions;
using Microsoft.Dynamics.AX.Metadata.Modeling.Extensions;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HMT.Services.Items.Commons
{
    public class TableBuilderParms
    {
        public Boolean IsCreateTable { get; set; } = true;
        public Boolean IsExternalEDT { get; set; } = false;
        public string TableName { get; set; } = "";
        public string TableLabel { get; set; } = "";
        public string TableVarName { get; set; } = "";

        public Boolean IsCreateForm { get; set; } = true;
        public string FormName { get; set; } = "";
        public string FormLabel { get; set; } = "";
        public string FormHelp { get; set; } = "";

        public string PrimaryKeyEdtName { get; set; } = "";
        public string EdtExtends { get; set; } = "SysGroup";
        public int EdtStrLen { get; set; } = 10;
        public string EdtLabel { get; set; } = "";
        public string EdtHelpText { get; set; } = "";

        public Boolean IsCreatePrivilege { get; set; } = true;
        public string PrivilegeLabelView { get; set; } = "";
        public string PrivilegeLabelMaintain { get; set; } = "";



        public string KeyFieldName { get; set; } = "Id";

        private AxHelper _axHelper;

        private string _logString;

        void AddLog(string logLocal)
        {
            _logString += logLocal;
        }

        public void DisplayLog()
        {
            CoreUtility.DisplayInfo($"The following elements({_logString}) were created and added to the project");
        }

        public void ValidateData()
        {
            if(_axHelper.MetadataProvider.Tables.Read(TableName) != null)
            {
                throw new Exception($"Table ({TableName}) already exists");
            }

            if (IsCreateForm)
            {
                if (string.IsNullOrEmpty(TableName) || string.IsNullOrEmpty(FormName))
                {
                    throw new Exception($"Table name and Form name should be specified");
                }
            }
        }

        public void CreateTable()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _logString = "";
            
            if (_axHelper == null)
            {
                _axHelper = new AxHelper();
            }
            ValidateData();

            if (IsCreateTable)
            {
                DoEdtCreate();
                DoTableCreate();
            }

            if (IsCreateForm)
            {
                DoFormCreate();
                DoMenuItemCreate();

                if (IsCreatePrivilege)
                {
                    DoPrivilegeCreate();
                }
            }
        }

        void DoEdtCreate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_axHelper.MetadataProvider.Edts.Read(PrimaryKeyEdtName) != null)
            {
                AxEdt axEdtSelected = _axHelper.MetaModelService.GetExtendedDataType(PrimaryKeyEdtName);
                AxEdtString axEdtSelectedStr = (AxEdtString)axEdtSelected;
                axEdtSelectedStr.Label = EdtLabel;
                axEdtSelectedStr.HelpText = EdtHelpText;
                axEdtSelectedStr.StringSize = EdtStrLen;
                _axHelper.MetaModelService.UpdateExtendedDataType(axEdtSelected, _axHelper.ModelSaveInfo);
                return;
            }
            //need to create a EDT
            AxEdtString newEdt = new AxEdtString();
            newEdt.Name = PrimaryKeyEdtName;
            newEdt.Extends = EdtExtends;
            if (EdtStrLen > 0 && newEdt.Extends == String.Empty)
            {
                newEdt.StringSize = EdtStrLen;
            }

            newEdt.Label = EdtLabel;
            newEdt.HelpText = EdtHelpText;

            _axHelper.MetaModelService.CreateExtendedDataType(newEdt, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(newEdt);

            AddLog($"EDT: {newEdt.Name}; ");

        }

        void DoMenuItemCreate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            AxMenuItemDisplay axMenuItemDisplay = _axHelper.MetadataProvider.MenuItemDisplays.Read(FormName);
            if (axMenuItemDisplay != null)
            {
                return;
            }

            axMenuItemDisplay = new AxMenuItemDisplay { Name = FormName, Object = FormName, Label = FormLabel, HelpText = FormHelp };
            _axHelper.MetaModelService.CreateMenuItemDisplay(axMenuItemDisplay, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(axMenuItemDisplay);

            AddLog($"MenuItem: {axMenuItemDisplay.Name}; ");
        }

        void DoFormCreate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            AxForm newForm = _axHelper.MetadataProvider.Forms.Read(FormName);
            if (newForm != null)
            {
                return;
            }

            newForm = new AxForm { Name = FormName };

            AxMethod axMethod = new AxMethod();
            axMethod.Name = "classDeclaration";
            axMethod.Source = $"[Form]{Environment.NewLine}public class {newForm.Name} extends FormRun " +
                              Environment.NewLine + "{" + Environment.NewLine + "}";
            newForm.AddMethod(axMethod);

            _axHelper.MetaModelService.CreateForm(newForm, _axHelper.ModelSaveInfo); 
            _axHelper.AppendToActiveProject(newForm);

            string dsName = TableName;

            AxFormDataSourceRoot axFormDataSource = this.DoAddFormDatsSource();
            newForm.AddDataSource(axFormDataSource);

            _axHelper.MetaModelService.UpdateForm(newForm, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(newForm);

            newForm.Design.Caption = FormLabel;
            newForm.Design.TitleDataSource = dsName;
            newForm.Design.DataSource = dsName;
            newForm.Design.Pattern = "SimpleList";
            newForm.Design.PatternVersion = "1.1";

            newForm.Design.AddControl(new AxFormActionPaneControl { Name = "MainActionPane" });

            var filterGrp = new AxFormGroupControl { Name = "FilterGroup", Pattern = "CustomAndQuickFilters", PatternVersion = "1.1" };

            AxFormControlExtension quickFilterControl = new AxFormControlExtension { Name = "QuickFilterControl" };
            AxFormControlExtensionProperty formControlExtensionProperty = new AxFormControlExtensionProperty();
            formControlExtensionProperty.Name = "targetControlName";
            formControlExtensionProperty.Type = CompilerBaseType.String;
            formControlExtensionProperty.Value = "MainGrid";
            quickFilterControl.ExtensionProperties.Add(formControlExtensionProperty);

            filterGrp.AddControl(new AxFormControl { Name = "QuickFilter", FormControlExtension = quickFilterControl });
            newForm.Design.AddControl(filterGrp);
            AxFormGridControl axFormGridControl = new AxFormGridControl { Name = "MainGrid", DataSource = dsName };

            axFormGridControl.AddControl(new AxFormStringControl { Name = KeyFieldName, DataSource = dsName, DataField = KeyFieldName });
            axFormGridControl.AddControl(new AxFormStringControl { Name = "Description", DataSource = dsName, DataField = "Description" });
            newForm.Design.AddControl(axFormGridControl);
            _axHelper.MetaModelService.UpdateForm(newForm, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(newForm);

            AddLog($"Form: {newForm.Name}; ");
        }

        AxFormDataSourceRoot DoAddFormDatsSource()
        {
            AxFormDataSourceRoot axFormDataSource = new AxFormDataSourceRoot();
            axFormDataSource.Name = TableName;
            axFormDataSource.Table = TableName;
            AxFormDataSourceField axFormDataSourceFieldCreatedBy = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldCreateDateTime = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldModifiedBy = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldModifiedDateTime = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldTableId = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldDateAreaId = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldPartition = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldRecId = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldKeyField = new AxFormDataSourceField();
            AxFormDataSourceField axFormDataSourceFieldDescription = new AxFormDataSourceField();
            axFormDataSourceFieldCreatedBy.Name = "CreatedBy";
            axFormDataSourceFieldCreateDateTime.Name = "CreatedDateTime";
            axFormDataSourceFieldModifiedBy.Name = "ModifiedBy";
            axFormDataSourceFieldModifiedDateTime.Name = "ModifiedDateTime";
            axFormDataSourceFieldTableId.Name = "TableId";
            axFormDataSourceFieldDateAreaId.Name = "DataAreaId";
            axFormDataSourceFieldPartition.Name = "Partition";
            axFormDataSourceFieldRecId.Name = "RecId";
            axFormDataSourceFieldKeyField.Name = KeyFieldName;
            axFormDataSourceFieldDescription.Name = "Description";
            axFormDataSource.Fields.Add(axFormDataSourceFieldCreatedBy);
            axFormDataSource.Fields.Add(axFormDataSourceFieldCreateDateTime);
            axFormDataSource.Fields.Add(axFormDataSourceFieldModifiedBy);
            axFormDataSource.Fields.Add(axFormDataSourceFieldModifiedDateTime);
            axFormDataSource.Fields.Add(axFormDataSourceFieldTableId);
            axFormDataSource.Fields.Add(axFormDataSourceFieldDateAreaId);
            axFormDataSource.Fields.Add(axFormDataSourceFieldPartition);
            axFormDataSource.Fields.Add(axFormDataSourceFieldRecId);
            axFormDataSource.Fields.Add(axFormDataSourceFieldKeyField);
            axFormDataSource.Fields.Add(axFormDataSourceFieldDescription);
            axFormDataSource.InsertIfEmpty = NoYes.No;

            return axFormDataSource;
        }

        void DoPrivilegeCreateSingle(string privilegeName, string privilegeLabel, AccessGrant accessGrant)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (String.IsNullOrWhiteSpace(privilegeLabel))
            {
                return;
            }

            AxSecurityPrivilege privilege;

            privilege = _axHelper.MetadataProvider.SecurityPrivileges.Read(privilegeName);
            if (privilege != null)
            {
                return;
            }
            privilege = new AxSecurityPrivilege();
            AxSecurityEntryPointReference entryPoint = new AxSecurityEntryPointReference();

            entryPoint.Name = FormName;
            entryPoint.Grant = accessGrant;
            entryPoint.ObjectName = FormName;
            entryPoint.ObjectType = EntryPointType.MenuItemDisplay;

            AxSecurityEntryPointReferenceForm formRef = new AxSecurityEntryPointReferenceForm();
            formRef.Name = FormName;
            entryPoint.Forms.Add(formRef);

            privilege.Name = privilegeName;
            privilege.EntryPoints.Add(entryPoint);
            privilege.Label = privilegeLabel;

            _axHelper.MetaModelService.CreateSecurityPrivilege(privilege, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(privilege);

            AddLog($"Privilege: {privilege.Name}; ");
        }

        void DoPrivilegeCreate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string privilegeName;
            AccessGrant accessGrant;

            if (!string.IsNullOrWhiteSpace(PrivilegeLabelView))
            {
                privilegeName = $"{FormName}View";
                accessGrant = AccessGrant.ConstructGrantRead();
                DoPrivilegeCreateSingle(privilegeName, PrivilegeLabelView, accessGrant);
            }

            if (!string.IsNullOrWhiteSpace(PrivilegeLabelMaintain))
            {
                privilegeName = $"{FormName}Maintain";
                accessGrant = AccessGrant.ConstructGrantDelete();
                DoPrivilegeCreateSingle(privilegeName, PrivilegeLabelMaintain, accessGrant);
            }

        }

        void DoTableCreate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            AxTable newTable = _axHelper.MetadataProvider.Tables.Read(TableName);
            if (newTable == null)
            {
                newTable = new AxTable();
                newTable.Name = TableName;
                newTable.Label = TableLabel;
                newTable.TitleField1 = KeyFieldName;
                newTable.CacheLookup = RecordCacheLevel.Found;
                newTable.ClusteredIndex = $"{KeyFieldName}Idx";
                newTable.PrimaryIndex = newTable.ClusteredIndex;
                newTable.ReplacementKey = newTable.ClusteredIndex;
                newTable.TableGroup = TableGroup.Group;
                newTable.CreatedBy = NoYes.Yes;
                newTable.CreatedDateTime = NoYes.Yes;
                newTable.ModifiedBy = NoYes.Yes;
                newTable.ModifiedDateTime = NoYes.Yes;

                AxTableField primaryField = new AxTableFieldString();
                primaryField.Name = KeyFieldName;
                primaryField.ExtendedDataType = PrimaryKeyEdtName;
                primaryField.IgnoreEDTRelation = NoYes.Yes;
                primaryField.AllowEdit = NoYes.No;
                primaryField.Mandatory = NoYes.Yes;
                newTable.AddField(primaryField);

                AxTableField descriptionField = new AxTableFieldString();
                descriptionField.Name = "Description";
                descriptionField.ExtendedDataType = "Description";
                newTable.AddField(descriptionField);

                AxTableIndexField axTableIndexField = new AxTableIndexField();
                axTableIndexField.DataField = KeyFieldName;
                axTableIndexField.Name = KeyFieldName;
                AxTableIndex axTableIndex = new AxTableIndex();
                axTableIndex.Name = newTable.ClusteredIndex;
                axTableIndex.AlternateKey = NoYes.Yes;
                axTableIndex.AddField(axTableIndexField);
                newTable.AddIndex(axTableIndex);

                AxTableFieldGroup axTableFieldGroup;
                AxTableFieldGroupField axTableFieldGroupField;

                axTableFieldGroup = new AxTableFieldGroup { Name = "AutoReport", IsSystemGenerated = NoYes.Yes };
                axTableFieldGroupField = new AxTableFieldGroupField
                {
                    Name = KeyFieldName,
                    DataField = KeyFieldName
                };
                axTableFieldGroup.AddField(axTableFieldGroupField);
                newTable.AddFieldGroup(axTableFieldGroup);

                axTableFieldGroup = new AxTableFieldGroup { Name = "AutoLookup", IsSystemGenerated = NoYes.Yes };
                newTable.AddFieldGroup(axTableFieldGroup);

                axTableFieldGroup = new AxTableFieldGroup { Name = "AutoIdentification", IsSystemGenerated = NoYes.Yes, AutoPopulate = NoYes.Yes };
                newTable.AddFieldGroup(axTableFieldGroup);

                axTableFieldGroup = new AxTableFieldGroup { Name = "AutoSummary", IsSystemGenerated = NoYes.Yes };
                newTable.AddFieldGroup(axTableFieldGroup);

                axTableFieldGroup = new AxTableFieldGroup { Name = "AutoBrowse", IsSystemGenerated = NoYes.Yes };
                newTable.AddFieldGroup(axTableFieldGroup);

                axTableFieldGroup = new AxTableFieldGroup { Name = "Overview", Label = "Overview" };
                axTableFieldGroupField = new AxTableFieldGroupField
                {
                    Name = KeyFieldName,
                    DataField = KeyFieldName
                };
                axTableFieldGroup.AddField(axTableFieldGroupField);
                if (descriptionField != null)
                {
                    axTableFieldGroupField = new AxTableFieldGroupField
                    {
                        Name = descriptionField.Name,
                        DataField = descriptionField.Name
                    };
                    axTableFieldGroup.AddField(axTableFieldGroupField);
                }
                newTable.AddFieldGroup(axTableFieldGroup);

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"public class {newTable.Name} extends common");
                stringBuilder.AppendLine("{");
                stringBuilder.AppendLine("}");
                newTable.SourceCode.Declaration = stringBuilder.ToString();

                //AddTableFindMethodParms findMethodParms = new AddTableFindMethodParms();
                //findMethodParms.IsCreateFind = true;
                //findMethodParms.IsTestMode = true;
                //findMethodParms.TableName = TableName;
                //findMethodParms.VarName = TableVarName;
                //findMethodParms.Fields = new List<AxTableFieldParm>
                //{
                //    new AxTableFieldParm
                //    {
                //        FieldName = KeyFieldName, FieldType = PrimaryKeyEdtName, IsMandatory = true
                //    }
                //};

                //AxMethod axMethod = new AxMethod();
                //axMethod.Name = "find";
                //axMethod.IsStatic = true;
                //axMethod.Source = findMethodParms.GenerateResult();
                //newTable.AddMethod(axMethod);

                _axHelper.MetaModelService.CreateTable(newTable, _axHelper.ModelSaveInfo);
                _axHelper.AppendToActiveProject(newTable);

                AddLog($"Table: {newTable.Name}; ");

                AxEdt edtLocal = _axHelper.MetadataProvider.Edts.Read(PrimaryKeyEdtName);
                if (edtLocal != null)
                {
                    if (String.IsNullOrEmpty(edtLocal.ReferenceTable))//check for the existing EDT. Do not modify it
                    {
                        if (edtLocal.Relations == null || edtLocal.Relations.Count == 0) //no old style relations
                        {
                            edtLocal.ReferenceTable = TableName;
                            edtLocal.AddTableReference(TableName, KeyFieldName);

                            _axHelper.MetaModelService.UpdateExtendedDataType(edtLocal, _axHelper.ModelSaveInfo);
                            _axHelper.AppendToActiveProject(edtLocal);
                        }
                    }

                    //AddLog($"EDT: {edtLocal.Name}; ");
                }
            }
        }
    }
}
