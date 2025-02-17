using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using System.Collections.Generic;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using HMT.Kernel;
using HMT.OptionsPane;
using HMT.Services.Global;

namespace HMT.HMTCommands.HMTLabelGenerateCommands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class HMTLabelGenerateForProject
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0132;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="HMTLabelGenerateForProject"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private HMTLabelGenerateForProject(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static HMTLabelGenerateForProject Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private AsyncPackage ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in HMTLabelGenerateForProject's constructor requires
            // the UI thread.
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTLabelGenerateForProject(package, commandService);
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
                bool                generateForCodeLabel    = false;
                HMTProjectService    projectService          = new HMTProjectService();
                Project             projectNode             = projectService.currentProject();

                if (projectNode == null || projectService.currentLabelNode() == null)
                {
                    throw new Exception("Please put the label file in your project.");
                }

                generateForCodeLabel = HMTOptionsUtils.getIsLabelForSourceCode(package);

                IList<Tuple<string, object>> iMetaElements = projectService.getAllElements();

                foreach (Tuple<string, object> itemTuple in iMetaElements)
                {
                    IMetaElement    item            = itemTuple.Item2 as IMetaElement;
                    HMLabelService  labelService    = HMLabelService.construct(item, generateForCodeLabel, false);

                    if (labelService != null)
                    {
                        labelService.runAX();
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
