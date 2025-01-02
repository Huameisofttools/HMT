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
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation;
using Microsoft.VisualStudio.Shell.Interop;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using HMT.HMTLabelGenerator;
using HMT.HMTCommands.HMTPrivilegeAndDutyGeneratorCommands;
using HMT.OptionsPane;

namespace HMT.HMTCommands.HMTLabelGenerateCommands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class HMTLabelGenerateForItem
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0130;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }
        

        /// <summary>
        /// Initializes a new instance of the <see cref="HMTLabelGenerateForItem"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private HMTLabelGenerateForItem(AsyncPackage package, OleMenuCommandService commandService)
        {
            bool flag = package == null;
            if (flag)
            {
                throw new ArgumentNullException("package");
            }
            this.package = package;
            //OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            bool flag2 = commandService != null;
            if (flag2)
            {
                this.package = package ?? throw new ArgumentNullException(nameof(package));
                commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

                var menuCommandID = new CommandID(CommandSet, CommandId);
                OleMenuCommand menuItem = new OleMenuCommand(new EventHandler(this.Execute), menuCommandID);
                
                menuItem.BeforeQueryStatus += this.projectmenuItem_BeforeQueryStatus;                
                commandService.AddCommand(menuItem);
            }
        }

        private void projectmenuItem_BeforeQueryStatus(object sender, EventArgs e)
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
                // bool flag = dte.ActiveDocument != null;
                if (item != null)
                {
                    if (item.GetType().Name != "AxReport")
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

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HMTLabelGenerateForItem Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async System.Threading.Tasks.Task InitializeAsync(AsyncPackage package)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTLabelGenerateForItem(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                bool generateForCodeLabel = false;
                HMTProjectService projectService = new HMTProjectService();
                Project projectNode = projectService.currentProject();

                if (projectNode == null || projectService.currentLabelNode() == null)
                {
                    throw new Exception("Please put the label file in your project.");
                }

                generateForCodeLabel = HMTOptionsUtils.getIsLabelForSourceCode(package);

                DTE MyDte = CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;

                if (MyDte == null)
                {
                    return;
                }

                if (e != null) 
                {                    
                    ProjectItem projectItem = MyDte.SelectedItems.Item(1).ProjectItem;
                    IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem);

                    if (item == null)
                    {
                        throw new Exception("The selected item is not supported.");
                    }

                    HMLabelService labelService = HMLabelService.construct(item, generateForCodeLabel, false);
                    
                    if (labelService != null)
                    {
                        labelService.runAX();
                    }
                }

                CoreUtility.DisplayInfo("Success!");
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
    }
}
