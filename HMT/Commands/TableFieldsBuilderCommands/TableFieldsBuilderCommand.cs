using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using System.Collections.Generic;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using System.Windows.Forms;
using EnvDTE80;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using HMT.Views.Items.Tables;
using HMT.Services.Items.Tables;

namespace HMT.HMTCommands.TableFieldsBuilderCommands
{
    class TableFieldsBuilderCommand
    {
        public const int CommandId = 0x1209;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private TableFieldsBuilderCommand(AsyncPackage package, OleMenuCommandService commandService)
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
            DTE2 dte = ServiceProvider.GetService(typeof(DTE)) as DTE2;
            if (dte == null) { 
                return ret;
            }
            try
            {
                ProjectItem projectItem = dte.SelectedItems.Item(1).ProjectItem;
                IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem);

                // bool flag = dte.ActiveDocument != null;
                if (item != null)
                {
                    if (item.GetType().Name == "AxTable")
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

        public static TableFieldsBuilderCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new TableFieldsBuilderCommand(package, commandService);
        }

        /// <summary>
        /// HMTParmMethodGenerateCommand
        /// Willie Yao - 2024/04/02
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                DTE MyDte = CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;
                if (MyDte != null)
                {
                    ProjectItem projectItem2 = MyDte.SelectedItems.Item(1).ProjectItem;
                    IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem2);

                    if (item != null)
                    {
                        AxTable axTable = item as AxTable;
                        TableFieldsBuilderDialog dialog = new TableFieldsBuilderDialog();
                        TableFieldsBuilderParms parms = new TableFieldsBuilderParms();
                        parms.TableName = axTable.Name;

                        dialog.SetParameters(parms);
                        dialog.Show();
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
