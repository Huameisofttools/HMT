using EnvDTE;
using HMT.Kernel;
using HMT.Services.Global;
using HMT.Views.Items.Commons;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;

namespace HMT.HMTCommands.HMTHeaderCommentGeneratorCommands
{
    internal sealed class HMTHeaderCommentGenerateForItem
    {
        public const int CommandId = 0x0120;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private HMTHeaderCommentGenerateForItem(AsyncPackage package, OleMenuCommandService commandService)
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

                if (!(item is AxTable
                    || item is AxClass
                    || item is AxForm
                    || item is AxView
                    || item is AxDataEntity
                    || item is AxTable))
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

        public static HMTHeaderCommentGenerateForItem Instance
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
            Instance = new HMTHeaderCommentGenerateForItem(package, commandService);
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
                HMTProjectService projectService = new HMTProjectService();
                OAVSProject projectNode = projectService.currentProject() as OAVSProject;
                VSProjectNode project = projectNode.Project as VSProjectNode;

                if (projectNode == null)
                {
                    throw new Exception("Please open your project.");
                }

                HMTAddCommentDialog dialog = new HMTAddCommentDialog();
                dialog.CommentValue.Text = "";
                dialog.CommentValue.Text += "/// <summary>\n";
                dialog.CommentValue.Text += "/// " + project.Name + "\n";
                dialog.CommentValue.Text += "/// DeveloperName - " + System.DateTime.Today.ToString("MM/dd/yyyy") + "\n";
                dialog.CommentValue.Text += "/// \n";
                dialog.CommentValue.Text += "/// </summary>";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var comment = dialog.CommentValue.Text;

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
                        HMCommentService commentService = HMCommentService.construct(item, comment);

                        commentService.runAX();
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
