using EnvDTE;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMT.HMTHeaderCommandGenerator;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;

namespace HMT.HMTCommands.HMTHeaderCommentGeneratorCommands
{
    internal sealed class HMTHeaderCommentGenerateForAll
    {
        public const int                CommandId       = 0x0122;
        public static readonly Guid     CommandSet      = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");
        private const string            gSuccessInfo    = "Comments are added.";
        private readonly AsyncPackage   package;

        private HMTHeaderCommentGenerateForAll(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            
            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }
        
        public static HMTHeaderCommentGenerateForAll Instance
        {
            get;
            private set;
        }
        
        private AsyncPackage ServiceProvider
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
            Instance = new HMTHeaderCommentGenerateForAll(package, commandService);
        }
                                                     
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                HMTProjectService   projectService  = new HMTProjectService();
                OAVSProject         projectNode     = projectService.currentProject() as OAVSProject;
                VSProjectNode       project         = projectNode.Project as VSProjectNode;
                int                 projectCnt      = projectService.getAllProject().Length;

                if (projectNode == null)
                {
                    throw new Exception("Please open your project.");
                }

                HMTAddCommentDialog dialog = new HMTAddCommentDialog();
                dialog.CommentValue.Text = "";
                dialog.CommentValue.Text += "/// <summary>\n";
                if (projectCnt > 1)
                {                     
                    dialog.CommentValue.Text += "/// " + "[Please Input Header Name]" + " Projects\n";
                }
                else
                {
                    dialog.CommentValue.Text += "/// " + project.Name + "\n";
                }
                dialog.CommentValue.Text += "/// " + project.Name + "\n";
                dialog.CommentValue.Text += "/// DeveloperName - " + System.DateTime.Today.ToString("MM/dd/yyyy") + "\n";
                dialog.CommentValue.Text += "/// \n";
                dialog.CommentValue.Text += "/// </summary>";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var comment = dialog.CommentValue.Text;
                    IList<Tuple<string, object>> iMetaElements = projectService.getAllElementsFromSolution();

                    foreach (Tuple<string, object> itemTuple in iMetaElements)
                    {
                        IMetaElement        item            = itemTuple.Item2 as IMetaElement;
                        HMCommentService    commentService  = HMCommentService.construct(item, comment, false);

                        if (commentService != null)
                        {
                            commentService.runAX();
                        }                        
                    }

                    CoreUtility.DisplayInfo(gSuccessInfo);
                }
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
    }
}
