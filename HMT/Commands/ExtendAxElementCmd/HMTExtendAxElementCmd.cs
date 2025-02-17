using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using HMT.Kernel;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using HMT.Views.Projects;
using HMT.Services.Projects;

namespace HMT.HMTCommands.HMTExtendAxElementCmd
{
    internal sealed class HMTExtendAxElementCmd
    {
        public const int CommandId = 0x1208;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private HMTExtendAxElementCmd(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
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
                return false;
            }
            try
            {
                ProjectItem projectItem = dte.SelectedItems.Item(1).ProjectItem;
                IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem);

                // bool flag = dte.ActiveDocument != null;
                if (item != null)
                {
                     
                    if (item.GetType().Name == "AxTable"
                    || item.GetType().Name == "AxView"
                    || item.GetType().Name == "AxForm"
                    || item.GetType().Name == "AxDataEntityView"
                    || item.GetType().Name == "AxQuerySimple"
                    || item.GetType().Name == "AxClass")
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

        public static HMTExtendAxElementCmd Instance
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

        public static async Task InitializeAsync(AsyncPackage package)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTExtendAxElementCmd(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            try
            {                
                CreateExtensionClassDialog dialog = new CreateExtensionClassDialog(package);
                CreateExtensionClassParms parms = new CreateExtensionClassParms();

                parms.InitFromOption(package);
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
