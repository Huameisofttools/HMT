using EnvDTE;
using HMT.HMTBatchJobTemplateGenerator;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMT.HMTFormGenerator;
using EnvDTE80;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.VisualStudio.Shell.Interop;

namespace HMT.HMTCommands.HMTFormGeneratorCommand
{
    internal sealed class HMTFormGenerateCommand
    {
        public const int CommandId = 0x0118;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private HMTFormGenerateCommand(AsyncPackage package, OleMenuCommandService commandService)
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
            if (dte == null)
            {
                return ret;
            }
            try
            {
                ProjectItem projectItem = dte.SelectedItems.Item(1).ProjectItem;
                IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem);

                if (item == null)
                {
                    return false;
                }

                if (item.GetType().Name != "AxTable")
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

        public static HMTFormGenerateCommand Instance
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
            Instance = new HMTFormGenerateCommand(package, commandService);
        }

        /// <summary>
        /// HMT Kernal Settings Command Execution
        /// Willie Yao - 03/29/2024
        /// Execute the command to open the HMT Kernal Settings form
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                DTE MyDte = CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;

                if (MyDte == null)
                {
                    return;
                }

                ProjectItem     projectItem = MyDte.SelectedItems.Item(1).ProjectItem;
                IMetaElement    item        = LocalUtils.getNamedElementFromProjectItem(projectItem);
                AxTable         axTable     = item as AxTable;

                if (item.GetType().Name != "AxTable")
                {
                    CoreUtility.DisplayInfo("This utils only execute for table element.");
                }

                if (axTable != null)
                {
                    HMTFormBuilderDialog    dialog  = new HMTFormBuilderDialog();
                    HMTFormService          service = new HMTFormService();
                    service.TableName = axTable.Name;
                    service.InitFromTable();

                    dialog.SetParameters(service);
                    dialog.Show();
                }
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
    }
}
