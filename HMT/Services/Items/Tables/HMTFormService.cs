using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel.Extensions;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMT.HMTFormGenerator
{
    public enum FormTemplateType
    {
        SimpleList,
        SimpleListDetails
    }
    public class HMTFormService
    {
        public string TableName { get; set; } = "";
        public FormTemplateType TemplateType { get; set; }
        public Boolean IsCreateMenuItem { get; set; } = true;
        public string FormName { get; set; } = "";
        public string FormLabel { get; set; } = "";
        public string FormHelp { get; set; } = "";
        public string TabLabels { get; set; } = "Details";
        public string GroupNameGrid { get; set; } = "Overview";
        public string GroupNameHeader { get; set; } = "DetailsHeaderGroup";
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
            if (string.IsNullOrEmpty(TableName) || string.IsNullOrEmpty(FormName))
            {
                throw new Exception($"Table name and Form name should be specified");
            }

        }

        public void InitFromTable()
        {
            _axHelper = new AxHelper();
            AxTable newTable = _axHelper.MetadataProvider.Tables.Read(TableName);
            if (newTable != null)
            {
                FormName = TableName;
                FormLabel = newTable.Label;
            }
        }
        public void Run()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _logString = "";
            ValidateData();
            if (_axHelper == null)
            {
                _axHelper = new AxHelper();
            }
            DoTableUpdate();

            DoFormCreate();

            if (IsCreateMenuItem)
            {
                DoMenuItemCreate();
            }
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

        /// <summary>
        /// Add form data source
        /// </summary>
        /// <returns>AxFormDataSourceRoot</returns>
        protected AxFormDataSourceRoot DoAddFormDatsSource()
        {
            AxFormDataSourceRoot axFormDataSource = new AxFormDataSourceRoot();
            axFormDataSource.Name = TableName;
            axFormDataSource.Table = TableName;
            axFormDataSource = AddSystemStandardFields(axFormDataSource);
            AxTable axTable = _axHelper.MetadataProvider.Tables.Read(TableName);
            if (axTable != null)
            {
                axFormDataSource = AddCustomizedFields(axFormDataSource, axTable);
            }            
            return axFormDataSource;
        }

        private AxFormDataSourceRoot AddSystemStandardFields(AxFormDataSourceRoot axFormDataSourceRoot)
        {
            AxFormDataSourceRoot formDataSourceRoot = null;
            if (axFormDataSourceRoot == null)
            {
                formDataSourceRoot = new AxFormDataSourceRoot();
                formDataSourceRoot.Name = TableName;
                formDataSourceRoot.Table = TableName;
                formDataSourceRoot.InsertIfEmpty = NoYes.No;
            }
            formDataSourceRoot = axFormDataSourceRoot;
            var axFormDataSourceFieldCreatedBy = new AxFormDataSourceField();
            var axFormDataSourceFieldCreateDateTime = new AxFormDataSourceField();
            var axFormDataSourceFieldModifiedBy = new AxFormDataSourceField();
            var axFormDataSourceFieldModifiedDateTime = new AxFormDataSourceField();
            var axFormDataSourceFieldTableId = new AxFormDataSourceField();
            var axFormDataSourceFieldDateAreaId = new AxFormDataSourceField();
            var axFormDataSourceFieldPartition = new AxFormDataSourceField();
            var axFormDataSourceFieldRecId = new AxFormDataSourceField();
            var axFormDataSourceFieldDescription = new AxFormDataSourceField();
            axFormDataSourceFieldCreatedBy.Name = "CreatedBy";
            axFormDataSourceFieldCreateDateTime.Name = "CreatedDateTime";
            axFormDataSourceFieldModifiedBy.Name = "ModifiedBy";
            axFormDataSourceFieldModifiedDateTime.Name = "ModifiedDateTime";
            axFormDataSourceFieldTableId.Name = "TableId";
            axFormDataSourceFieldDateAreaId.Name = "DataAreaId";
            axFormDataSourceFieldPartition.Name = "Partition";
            axFormDataSourceFieldRecId.Name = "RecId";
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldCreatedBy);
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldCreateDateTime);
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldModifiedBy);
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldModifiedDateTime);
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldTableId);
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldDateAreaId);
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldPartition);
            formDataSourceRoot.Fields.Add(axFormDataSourceFieldRecId);

            return formDataSourceRoot;
        }

        /// <summary>
        /// Add all fields from the table to the form data source
        /// </summary>
        /// <param name="axFormDataSourceRoot">AxFormDataSourceRoot</param>
        /// <param name="axTable">AxTable</param>
        /// <returns>
        /// AxFormDataSourceRoot
        /// </returns>
        private AxFormDataSourceRoot AddCustomizedFields(AxFormDataSourceRoot axFormDataSourceRoot, AxTable axTable)
        {
            if (axTable == null)
            {
                return axFormDataSourceRoot;
            }

            foreach (AxTableField axTableField in axTable.Fields)
            {
                AxFormDataSourceField axFormDataSourceField = new AxFormDataSourceField();
                axFormDataSourceField.Name = axTableField.Name;
                axFormDataSourceField.AllowEdit = axTableField.AllowEdit;
                axFormDataSourceField.Visible = NoYes.Yes;
                axFormDataSourceField.Mandatory = axTableField.Mandatory;
                axFormDataSourceRoot.Fields.Add(axFormDataSourceField);
            }

            return axFormDataSourceRoot;
        }

        /// <summary>
        /// Add controls to the grid
        /// </summary>
        /// <param name="gridControl">gridControl</param>
        /// <param name="axForm">axForm</param>
        /// <param name="dataSourceName">dataSourceName</param>
        /// <param name="axTableFieldGroup">axTableFieldGroup</param>
        /// <returns>AxFormGridControl</returns>
        private AxFormGridControl AddGridControl(
            AxFormGridControl gridControl, 
            AxForm axForm, 
            string dataSourceName, 
            AxTableFieldGroup axTableFieldGroup)
        {
            AxFormGridControl axFormGridControl = null;
            if (gridControl == null)
            {
                axFormGridControl = new AxFormGridControl() {
                    DataSource = dataSourceName,
                    Name = "MainGrid"
                };
            }
            else
            {
                axFormGridControl = gridControl;
            }
            var table = _axHelper.MetadataProvider.Tables.Read(TableName);
            foreach (var axTableField in axTableFieldGroup.Fields)
            {
                var type =table.GetFieldType(axTableField.Name);
                switch (type.Name)
                {
                    case "AxTableFieldString":
                        AxFormStringControl axFormStringControl = new AxFormStringControl();
                        axFormStringControl.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormStringControl.Visible = NoYes.Yes;
                        axFormStringControl.DataSource = dataSourceName;
                        axFormStringControl.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormStringControl);
                        break;
                    case "AxTableFieldInt":
                        AxFormIntegerControl axFormIntegerControl = new AxFormIntegerControl();
                        axFormIntegerControl.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormIntegerControl.Visible = NoYes.Yes;
                        axFormIntegerControl.DataSource = dataSourceName;
                        axFormIntegerControl.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormIntegerControl);
                        break;
                    case "AxTableFieldReal":
                        AxFormRealControl axFormRealControl = new AxFormRealControl();
                        axFormRealControl.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormRealControl.Visible = NoYes.Yes;
                        axFormRealControl.DataSource = dataSourceName;
                        axFormRealControl.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormRealControl);
                        break;
                    case "AxTableFieldDate":
                        AxFormDateControl axFormDateControl = new AxFormDateControl();
                        axFormDateControl.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormDateControl.Visible = NoYes.Yes;
                        axFormDateControl.DataSource = dataSourceName;
                        axFormDateControl.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormDateControl);
                        break;
                    case "AxTableFieldEnum":                        
                        AxFormCheckBoxControl axFormCheckBoxControl = new AxFormCheckBoxControl();
                        AxFormComboBoxControl axFormEnumControl = new AxFormComboBoxControl();
                        axFormEnumControl.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormEnumControl.Visible = NoYes.Yes;
                        axFormEnumControl.DataSource = dataSourceName;
                        axFormEnumControl.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormEnumControl);

                        break;
                    case "AxTableFieldTime":
                        AxFormTimeControl axFormTimeControl = new AxFormTimeControl();
                        axFormTimeControl.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormTimeControl.Visible = NoYes.Yes;
                        axFormTimeControl.DataSource = dataSourceName;
                        axFormTimeControl.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormTimeControl);
                        break;
                    case "AxTableFieldGuid":
                        AxFormGuidControl axFormGuidControl = new AxFormGuidControl();
                        axFormGuidControl.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormGuidControl.Visible = NoYes.Yes;
                        axFormGuidControl.DataSource = dataSourceName;
                        axFormGuidControl.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormGuidControl);
                        break;
                    case "AxTableFieldInt64":
                        AxFormInt64Control axFormInt64Control = new AxFormInt64Control();
                        axFormInt64Control.Name = axFormGridControl.Name + '_' + axTableField.Name;
                        axFormInt64Control.Visible = NoYes.Yes;
                        axFormInt64Control.DataSource = dataSourceName;
                        axFormInt64Control.DataField = axTableField.Name;
                        axFormGridControl.AddControl(axFormInt64Control);
                        break;                        
                }
            }

            return axFormGridControl;
        }

        private AxFormControlExtension SetupQuickFilterControlExtensionProperty(AxFormControlExtension axFormControlExtension)
        {
            AxFormControlExtension formControlExtension = null;
            if (axFormControlExtension == null)
            {
                axFormControlExtension = new AxFormControlExtension();
            }
            else
            {
                formControlExtension = axFormControlExtension;
            }

            AxFormControlExtensionProperty formControlExtensionProperty = new AxFormControlExtensionProperty();
            formControlExtensionProperty.Name = "targetControlName";
            formControlExtensionProperty.Type = CompilerBaseType.String;
            formControlExtensionProperty.Value = "MainGrid";
            axFormControlExtension.ExtensionProperties.Add(formControlExtensionProperty);

            AxFormControlExtensionProperty placeholderText = new AxFormControlExtensionProperty();
            placeholderText.Name = "placeholderText";
            placeholderText.Type = CompilerBaseType.String;
            axFormControlExtension.ExtensionProperties.Add(placeholderText);

            AxFormControlExtensionProperty defaultColumnName = new AxFormControlExtensionProperty();
            defaultColumnName.Name = "defaultColumnName";
            defaultColumnName.Type = CompilerBaseType.String;
            axFormControlExtension.ExtensionProperties.Add(defaultColumnName);

            return formControlExtension;
        }

        void DoFormCreate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // 1. Create form
            AxForm newForm = _axHelper.MetadataProvider.Forms.Read(FormName);
            if (newForm != null)
            {
                throw new Exception($"Form {FormName} already exists");
            }

            newForm = new AxForm { Name = FormName };

            AxMethod axMethod = new AxMethod();
            axMethod.Name = "classDeclaration";
            axMethod.Source = $"[Form]{Environment.NewLine}public class {newForm.Name} extends FormRun " +
                              Environment.NewLine + "{" + Environment.NewLine + "}";
            newForm.AddMethod(axMethod);
            // 1.1 Apply this creation action
            _axHelper.MetaModelService.CreateForm(newForm, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(newForm);

            // 2. Add form data source
            string dsName = TableName;

            // Initialize form data source
            newForm = _axHelper.MetadataProvider.Forms.Read(FormName);
            AxFormDataSourceRoot axFormDataSource = DoAddFormDatsSource();
            newForm.AddDataSource(axFormDataSource);
            
            newForm.Design.Caption = FormLabel;
            newForm.Design.TitleDataSource = dsName;
            newForm.Design.DataSource = dsName;
            newForm.Design.Pattern = "SimpleList";
            newForm.Design.PatternVersion = "1.1";
            newForm.Design.AddControl(new AxFormActionPaneControl { Name = "MainActionPane" });

            AxFormGroupControl filterGrp,detailsHeaderGroup;
            AxFormGridControl axFormGridControl;
            AxFormControlExtension quickFilterControl;
            AxFormTabControl formTabControl;

            switch (TemplateType)
            {
                case FormTemplateType.SimpleList:
                    newForm.Design.Style = FormStyle.SimpleList;
                    filterGrp = new AxFormGroupControl { Name = "FilterGroup", Pattern = "CustomAndQuickFilters", PatternVersion = "1.1" };
                    filterGrp.WidthMode = FormWidthHeightMode.SizeToAvailable;
                    quickFilterControl = new AxFormControlExtension { Name = "QuickFilterControl" };                    
                    quickFilterControl = SetupQuickFilterControlExtensionProperty(quickFilterControl);
                    filterGrp.ArrangeMethod = ArrangeMethod_ITxt.HorizontalLeft;
                    filterGrp.FrameType = FrameType_ITxt.None;
                    filterGrp.Style = GroupStyle.CustomFilter;
                    filterGrp.ViewEditMode = ViewEditMode.Edit;
                    filterGrp.AddControl(new AxFormControl { Name = "QuickFilter", FormControlExtension = quickFilterControl });
                    newForm.Design.AddControl(filterGrp);
                    axFormGridControl = new AxFormGridControl { Name = "MainGrid", DataSource = dsName };
                    // axFormGridControl.DataGroup = GroupNameGrid;
                    axFormGridControl.Style = GridStyle.Tabular;
                    axFormGridControl = AddGridControl(axFormGridControl, newForm, dsName, _axHelper.MetadataProvider.Tables.Read(TableName).FieldGroups[GroupNameGrid]);
                    newForm.Design.AddControl(axFormGridControl);
                    break;
                case FormTemplateType.SimpleListDetails:
                    newForm.Design.Style = FormStyle.SimpleListDetails;
                    newForm.Design.Pattern = "SimpleListDetails";
                    filterGrp = new AxFormGroupControl { Name = "NavigationListGroup" };
                    filterGrp.WidthMode = FormWidthHeightMode.SizeToAvailable;
                    quickFilterControl = new AxFormControlExtension { Name = "QuickFilterControl" };
                    quickFilterControl = SetupQuickFilterControlExtensionProperty(quickFilterControl);
                    filterGrp.ArrangeMethod = ArrangeMethod_ITxt.HorizontalLeft;
                    filterGrp.FrameType = FrameType_ITxt.None;
                    filterGrp.Style = GroupStyle.CustomFilter;
                    filterGrp.ViewEditMode = ViewEditMode.Edit;
                    filterGrp.AddControl(new AxFormControl { Name = "NavListQuickFilter", FormControlExtension = quickFilterControl });
                    axFormGridControl = new AxFormGridControl { Name = "MainGrid", DataSource = dsName };
                    // axFormGridControl.DataGroup = GroupNameGrid;
                    axFormGridControl.Style = GridStyle.Tabular;
                    axFormGridControl = AddGridControl(axFormGridControl, newForm, dsName, _axHelper.MetadataProvider.Tables.Read(TableName).FieldGroups[GroupNameGrid]);
                    filterGrp.AddControl(axFormGridControl);
                    newForm.Design.AddControl(filterGrp);

                    detailsHeaderGroup = new AxFormGroupControl { Name = "DetailsHeaderGroup" };
                    detailsHeaderGroup.DataSource = dsName;
                    detailsHeaderGroup.DataGroup = GroupNameHeader;

                    newForm.Design.AddControl(detailsHeaderGroup);

                    formTabControl = new AxFormTabControl { Name = "DetailsTab" };

                    List<string> listImp = new List<string>(
                        TabLabels.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                    foreach (string lineImp in listImp)
                    {
                        string tabName = AxHelper.GetTypeNameFromLabel(lineImp) + "TabPage";
                        formTabControl.AddControl(new AxFormTabPageControl { Name = tabName, Caption = lineImp, DataSource = dsName });
                    }

                    newForm.Design.AddControl(formTabControl);
                    break;

            }

            _axHelper.MetaModelService.UpdateForm(newForm, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(newForm);
            
            AxForm formSaved = _axHelper.MetadataProvider.Forms.Read(FormName);

            //Get the grid control
            AxFormGridControl gridControl = formSaved.Design.Controls.OfType<AxFormGridControl>().FirstOrDefault();
            gridControl.DataGroup = GroupNameGrid;
            gridControl.AutoDataGroup = NoYes.Yes;
            _axHelper.MetaModelService.UpdateForm(formSaved, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(formSaved);

            AddLog($"Form: {gridControl.Name} - Restore it before use;");
        }


        void DoTableUpdate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            AxTable newTable = _axHelper.MetadataProvider.Tables.Read(TableName);
            if (newTable != null)
            {
                AxTableFieldGroup axTableFieldGroup;
                bool isTableModified = false;
                switch (TemplateType)
                {
                    case FormTemplateType.SimpleList:
                        if (!string.IsNullOrWhiteSpace(GroupNameGrid) && !newTable.FieldGroups.Contains(GroupNameGrid))
                        {
                            axTableFieldGroup = new AxTableFieldGroup { Name = GroupNameGrid, Label = "Overview" };
                            newTable.AddFieldGroup(axTableFieldGroup);
                            isTableModified = true;
                            AddLog($"Group added: {GroupNameGrid}; ");
                        }
                        break;
                    case FormTemplateType.SimpleListDetails:
                        if (!string.IsNullOrWhiteSpace(GroupNameGrid) && !newTable.FieldGroups.Contains(GroupNameGrid))
                        {
                            axTableFieldGroup = new AxTableFieldGroup { Name = GroupNameGrid, Label = "Overview" };
                            newTable.AddFieldGroup(axTableFieldGroup);
                            isTableModified = true;
                            AddLog($"Group added: {GroupNameGrid}; ");
                        }
                        if (!string.IsNullOrWhiteSpace(GroupNameHeader) && !newTable.FieldGroups.Contains(GroupNameHeader))
                        {
                            axTableFieldGroup = new AxTableFieldGroup { Name = GroupNameHeader, Label = "Details header" };
                            newTable.AddFieldGroup(axTableFieldGroup);
                            isTableModified = true;
                            AddLog($"Group added: {GroupNameHeader}; ");
                        }
                        break;

                }

                if (isTableModified)
                {
                    _axHelper.MetaModelService.UpdateTable(newTable, _axHelper.ModelSaveInfo);
                    _axHelper.AppendToActiveProject(newTable);

                    //AddLog($"Table modified: {newTable.Name}; ");
                }
            }
        }

    }
}
