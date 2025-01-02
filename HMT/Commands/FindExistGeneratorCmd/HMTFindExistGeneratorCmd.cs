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
using HMT.HMTTable.HMTFindExistMethodGenerator;
using Microsoft.Dynamics.Framework.Tools.Core.Common;
using Microsoft.VisualStudio.Shell.Interop;

namespace HMT.HMTCommands.HMTFindExistGeneratorCmd
{
    internal sealed class HMTFindExistGeneratorCmd
    {
        public const int CommandId = 0x1207;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private HMTFindExistGeneratorCmd(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += this.projectmenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
        }
        private void projectmenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            bool flag = menuCommand != null;
            if (flag)
            {
                bool isEnabled = this.checkOpened();
                menuCommand.Visible = OptionsPane.HMTOptionsUtils.getIsParmMethodActivated(package);
                menuCommand.Enabled = isEnabled;
            }
        }
        public static HMTFindExistGeneratorCmd Instance
        {
            get;
            private set;
        }

        //private AsyncPackage ServiceProvider
        //{
        //    get
        //    {
        //        return this.package;
        //    }
        //}

        public static async Task InitializeAsync(AsyncPackage package)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTFindExistGeneratorCmd(package, commandService);
        }

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
                ProjectItem projectItem2 = MyDte.SelectedItems.Item(1).ProjectItem;
                IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem2);
                //string axElType = item.GetType().Name;

                //if (axElType != "AxTable")
                //{
                //    MessageBox.Show("Sorry! This function just execute for class element");
                //    return;
                //}

                AxTable axTable = item as AxTable;
                HMTFindExistMethodGenerateService service = new HMTFindExistMethodGenerateService(axTable);
                HMTFindExistMethodGeneratorDialog dialog = new HMTFindExistMethodGeneratorDialog();
                dialog.initParameters(service);
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
        public bool checkOpened()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 dte = this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                return false;
            }
            if (dte.SelectedItems.Count!=1)
            {
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
                return item.GetType().Name.Contains("AxTable");
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
                return false;
            } 
        }
    }
}
