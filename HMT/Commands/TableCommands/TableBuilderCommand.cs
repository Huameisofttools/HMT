using EnvDTE;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using System.Windows.Forms;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using HMT.Views.Items.Commons;
using HMT.Services.Items.Commons;

namespace HMT.HMTCommands.TableCommands
{
    internal sealed class TableBuilderCommand
    {
        public const int CommandId = 0x1210;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private TableBuilderCommand(AsyncPackage package, OleMenuCommandService commandService)
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
                
                // bool flag = dte.ActiveDocument != null;
                if (item != null)
                {
                    if (item.GetType().Name == "AxEdt"
                        || item.GetType().Name == "AxEdtString"
                        || item.GetType().Name == "AxEdtReal"
                        || item.GetType().Name == "AxEdtBase"
                        || item.GetType().Name == "AxEdtContainer"
                        || item.GetType().Name == "AxEdtDate"
                        || item.GetType().Name == "AxEdtEnum"
                        || item.GetType().Name == "AxEdtDateTime"
                        || item.GetType().Name == "AxEdtGuid"
                        || item.GetType().Name == "AxEdtInt"
                        || item.GetType().Name == "AxEdtInt64")
                    {
                        ret = true;
                    }
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        public static TableBuilderCommand Instance
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
            Instance = new TableBuilderCommand(package, commandService);
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
                if (MyDte != null) {
                    ProjectItem projectItem = MyDte.SelectedItems.Item(1).ProjectItem;
                    IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem);
                    AxEdt edtItem = item as AxEdt;

                    if (edtItem != null)
                    {
                        TableBuilderDialog dialog = new TableBuilderDialog();
                        TableBuilderParms parms = new TableBuilderParms();
                        parms.PrimaryKeyEdtName = edtItem.Name;
                        parms.IsExternalEDT = true;

                        dialog.SetParameters(parms);
                        DialogResult formRes = dialog.ShowDialog();
                    }
                }                
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
    }
}
