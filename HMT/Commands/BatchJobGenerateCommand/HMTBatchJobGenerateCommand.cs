using EnvDTE;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using HMT.Kernel;
using HMT.Views.Global;
using HMT.Services.Global;

namespace HMT.HMTCommands.HMTBatchJobGenerateCommand
{
    internal sealed class HMTBatchJobGenerateCommand
    {
        public const int CommandId = 0x1206;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");        

        private readonly AsyncPackage package;

        private HMTBatchJobGenerateCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += HMTGlobalFunctionVisibleHelper.EnabledIfD365ProjectExists;
            commandService.AddCommand(menuItem);
        }

        public static HMTBatchJobGenerateCommand Instance
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
            Instance = new HMTBatchJobGenerateCommand(package, commandService);
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
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                HMTFunctionDemoGeneratorDialog dialog = new HMTFunctionDemoGeneratorDialog(package);

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var selectedValue = dialog.Function.SelectedItem.ToString();
                    var customerPrefix = dialog.CustomerPrefix.Text;
                    var ObjectNamePrefix = dialog.ObjectNamePrefix.Text;
                    var ObjectLabel = dialog.ObjectLabel.Text;
                    var commentText = dialog.Comment.Text;

                    HMTFunctionDemoGeneratorService classService = new HMTFunctionDemoGeneratorService();
                    Project projectNode = classService.currentProject();

                    if (projectNode == null)
                    {
                        throw new Exception("Please open a project.");
                    }

                    classService.generateObjectsFromTemplate(selectedValue, customerPrefix, ObjectNamePrefix, ObjectLabel, commentText);
                    
                    CoreUtility.DisplayInfo("Objects are created.");
                }
                else
                {
                    Console.WriteLine("Cancel");
                }
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }

    }
}
