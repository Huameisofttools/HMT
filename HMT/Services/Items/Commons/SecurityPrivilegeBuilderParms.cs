using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Menus;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.DataEntityViews;
using HMT.Kernel;
using Microsoft.VisualStudio.Shell;

namespace HMT.Services.Items.Commons
{
    public enum PrivilegeAccessLevel
    {
        Read,
        Update,
        Create,
        Correct,
        Delete
    }

    public class SecurityPrivilegeBuilderParms
    {
        public string MenuItemName { get; set; } = "";
        public EntryPointType MenuItemType { get; set; }

        public string ObjectName { get; set; } = "";
        public PrivilegeAccessLevel AccessLevel { get; set; } = PrivilegeAccessLevel.Delete;

        public string FormLabel { get; set; } = "";
        private string FormLabelOrig { get; set; } = "";

        private string FormName { get; set; } = "";

        public bool IsDataEntity { get; set; } = false;

        public bool IsDisplay { get; set; } = false;

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
            if (string.IsNullOrWhiteSpace(ObjectName) || string.IsNullOrWhiteSpace(MenuItemName))
            {
                throw new Exception($"Object name should be specified");
            }

        }

        public void InitFromSelectedElement(IMetaElement selectedElement)
        {
            string menuItemName = "";
            string formLableStr = "";
            string formNameStr = "";
            
            if (selectedElement is AxMenuItemAction)
            {
                var formEle = selectedElement as AxMenuItemAction;
                MenuItemType = EntryPointType.MenuItemAction;
                menuItemName = formEle.Name;
                formLableStr = formEle.Label;
                formNameStr = formEle.Object;
                
            }

            if (selectedElement is AxMenuItemOutput)
            {
                var formEle = selectedElement as AxMenuItemOutput;
                MenuItemType = EntryPointType.MenuItemOutput;
                menuItemName = formEle.Name;
                formLableStr = formEle.Label;
                formNameStr = formEle.Object;
            }

            if (selectedElement is AxMenuItemDisplay)
            {
                var formEle = selectedElement as AxMenuItemDisplay;
                MenuItemType = EntryPointType.MenuItemDisplay;
                menuItemName = formEle.Name;
                formLableStr = formEle.Label;
                formNameStr = formEle.Object;
                IsDisplay = true;
            }

            if (menuItemName != "" && formLableStr != "" && formNameStr != "")
            {
                MenuItemName = menuItemName;
                FormLabelOrig = formLableStr;
                FormName = formNameStr;
            }

            GenerateNames();
        }

        public void InitFromDataEntity(IMetaElement selectedElement)
        {
            var formEle = selectedElement as AxDataEntityView;

            MenuItemName = formEle.Name;
            FormLabelOrig = formEle.Label;
            IsDataEntity = true;
            MenuItemType = EntryPointType.None;

            GenerateNames();
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

            DoPrivilegeCreate();

        }

        public void GenerateNames()
        {
            string suffix = string.Empty;

            switch (AccessLevel)
            {
                case PrivilegeAccessLevel.Read:
                    suffix = "View";
                    break;
                case PrivilegeAccessLevel.Update:
                    suffix = "Update";
                    break;
                case PrivilegeAccessLevel.Create:
                    suffix = "Create";
                    break;
                case PrivilegeAccessLevel.Correct:
                    suffix = "Correct";
                    break;
                case PrivilegeAccessLevel.Delete:
                    suffix = "Maintain";
                    break;
                default:
                    throw new NotImplementedException(
                        $"Value {AccessLevel} is not implemented.");
            }

            ObjectName = $"{MenuItemName}{suffix}";
            FormLabel = $"{FormLabelOrig} {suffix.ToLower()}";

        }

        AccessGrant GetGrant()
        {
            AccessGrant grant;
            switch (AccessLevel)
            {
                case PrivilegeAccessLevel.Read:
                    grant = AccessGrant.ConstructGrantRead();
                    break;
                case PrivilegeAccessLevel.Update:
                    grant = AccessGrant.ConstructGrantUpdate();
                    break;
                case PrivilegeAccessLevel.Create:
                    grant = AccessGrant.ConstructGrantCreate();
                    break;
                case PrivilegeAccessLevel.Correct:
                    grant = AccessGrant.ConstructGrantCorrect();
                    break;
                case PrivilegeAccessLevel.Delete:
                    grant = AccessGrant.ConstructGrantDelete();
                    break;
                default:
                    throw new NotImplementedException(
                        $"Value {AccessLevel} is not implemented.");
            }

            return grant;
        }

        void DoPrivilegeCreate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            AxSecurityPrivilege privilege;

            privilege = _axHelper.MetadataProvider.SecurityPrivileges.Read(ObjectName);
            if (privilege != null)
            {
                throw new Exception($"Privilege {ObjectName} already exists");
            }
            privilege = new AxSecurityPrivilege();
            privilege.Name = ObjectName;
            if (IsDataEntity)
            {
                AxSecurityDataEntityPermission dataEntityPermission = new AxSecurityDataEntityPermission();

                dataEntityPermission.Grant = GetGrant();
                dataEntityPermission.IntegrationMode = IntegrationMode.All;
                dataEntityPermission.Name = MenuItemName;

                privilege.DataEntityPermissions.Add(dataEntityPermission);
            }
            else
            {
                AxSecurityEntryPointReference entryPoint = new AxSecurityEntryPointReference();

                entryPoint.Name = MenuItemName;
                entryPoint.Grant = GetGrant();
                entryPoint.ObjectName = MenuItemName;
                entryPoint.ObjectType = MenuItemType;

                if (!string.IsNullOrEmpty(FormName) && IsDisplay)
                {
                    AxSecurityEntryPointReferenceForm formRef = new AxSecurityEntryPointReferenceForm();
                    formRef.Name = FormName;
                    entryPoint.Forms.Add(formRef);
                }

                privilege.EntryPoints.Add(entryPoint);
            }
            privilege.Label = FormLabel;

            _axHelper.MetaModelService.CreateSecurityPrivilege(privilege, _axHelper.ModelSaveInfo);
            _axHelper.AppendToActiveProject(privilege);

            AddLog($"Privilege: {privilege.Name}; ");
        }
    }
}
