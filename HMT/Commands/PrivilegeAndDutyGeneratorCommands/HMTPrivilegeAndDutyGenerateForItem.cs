using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using HMT.Views.Items.Commons;
using HMT.Services.Items.Commons;

namespace HMT.HMTCommands.HMTPrivilegeAndDutyGeneratorCommands
{
    internal sealed class HMTPrivilegeAndDutyGenerateForItem
    {
        public const int CommandId = 0x0134;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private HMTPrivilegeAndDutyGenerateForItem(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += this.EnableIfSelectedElementIsEdt;
            commandService.AddCommand(menuItem);
        }

        private void EnableIfSelectedElementIsEdt(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            bool flag = menuCommand != null;
            if (flag)
            {
                bool isEnabled = this.checkOpened();
                //menuCommand.Visible = CiellosTools.D365.CiellosUtils.getIsParmMethodActivated(this.package);
                menuCommand.Enabled = isEnabled;
            }
        }

        public bool checkOpened()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            bool ret = false;
            EnvDTE80.DTE2 dte = this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

            if (dte == null) { 
                return false;
            }

            try
            {
                ProjectItem projectItem = dte.SelectedItems.Item(1).ProjectItem;
                IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem);

                if (item == null)
                {
                    return false;
                }

                if (!(item is AxMenuItemAction 
                    || item is AxMenuItemOutput
                    || item is AxMenuItemDisplay
                    || item is AxDataEntityView))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }

            return ret;
        }

        public static HMTPrivilegeAndDutyGenerateForItem Instance
        {
            get;
            private set;
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTPrivilegeAndDutyGenerateForItem(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                DTE MyDte = CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;
                ProjectItem projectItem2 = MyDte.SelectedItems.Item(1).ProjectItem;
                IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem2);

                if (MyDte == null)
                {
                    return;
                }

                if (item == null)
                {
                    return;
                }

                if (!(item is AxMenuItemAction 
                    || item is AxMenuItemOutput
                    || item is AxMenuItemDisplay
                    || item is AxDataEntityView))
                {
                    return;
                }

                SecurityPrivilegeBuilderDialog dialog = new SecurityPrivilegeBuilderDialog();
                SecurityPrivilegeBuilderParms parms = new SecurityPrivilegeBuilderParms();

                if (item is AxMenuItem)
                {
                    parms.InitFromSelectedElement(item);
                }
                else if (item is AxDataEntityView)
                {
                    parms.InitFromDataEntity(item);
                }

                dialog.SetParameters(parms);
                dialog.ShowDialog();
                
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
        
    }
}
