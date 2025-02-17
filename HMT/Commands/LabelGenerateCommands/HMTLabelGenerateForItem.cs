using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using HMT.OptionsPane;
using System.Text.RegularExpressions;
using HMT.Services.Global;

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
        /// The command Id for x++ text document
        /// </summary>
        public const int DocCommandId = 0x0137;

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
            bool flag2 = commandService != null;
            if (flag2)
            {
                this.package = package ?? throw new ArgumentNullException(nameof(package));
                commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

                var menuCommandID = new CommandID(CommandSet, CommandId);
                OleMenuCommand menuItem = new OleMenuCommand(new EventHandler(this.Execute), menuCommandID);
                // menuItem.BeforeQueryStatus += this.projectmenuItem_BeforeQueryStatus; Disable validation logic temporarily because of multiple selection features​.            
                commandService.AddCommand(menuItem);

                // Register DocCommandId
                var menuDocCommandID = new CommandID(CommandSet, DocCommandId);
                OleMenuCommand docMenuItem = new OleMenuCommand(new EventHandler(this.PasteLabel), menuDocCommandID);
                docMenuItem.BeforeQueryStatus += this.validateIfAxClass;
                commandService.AddCommand(docMenuItem);
            }
        }

        private void validateIfAxClass(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommand docMenuItem = sender as OleMenuCommand;
            bool flag = docMenuItem != null;
            if (flag)
            {
                bool isEnabled = false;
                EnvDTE80.DTE2 dte = this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
                if (dte == null)
                {
                    isEnabled = false;
                }

                bool _flag = dte.ActiveDocument != null;

                if (_flag)
                {
                    bool flag2 = dte.ActiveDocument.Name.IndexOf("AxClass") >= 0;
                    if (flag2)
                    {
                        isEnabled = true;
                    }
                }

                docMenuItem.Enabled = isEnabled;
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
                    SelectedItems selectedItems = MyDte.SelectedItems; // 2.6.7 version should support multiple selected.

                    foreach (SelectedItem selectedItem in selectedItems)
                    {
                        ProjectItem projectItem = selectedItem.ProjectItem;

                        if (projectItem == null) 
                        {
                            continue;
                        }

                        IMetaElement item = LocalUtils.getNamedElementFromProjectItem(projectItem);

                        if (item == null)
                        {
                            continue;
                        }

                        HMLabelService labelService = HMLabelService.construct(item, generateForCodeLabel, false);

                        if (labelService != null)
                        {
                            labelService.runAX();
                        }
                    }
                }

                CoreUtility.DisplayInfo("Success!");
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }

        /// <summary>
        /// Willie Yao - 2025/01/07
        /// This method used to paste label to x++ text document directly.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void PasteLabel(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            HMTProjectService projectService = new HMTProjectService();
            Project projectNode = projectService.currentProject();
            if (projectNode == null || projectService.currentLabelNode() == null)
            {
                throw new Exception("Please put the label file in your project.");
            }

            DTE MyDte = CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;
            if (MyDte != null) {
                Document doc = MyDte.ActiveDocument;
                TextSelection text = (TextSelection)doc.Selection;
                string selectedText = text.Text;
                text.Text = GenerateLabelId(selectedText);
            }
        }

        /// <summary>
        /// Willie Yao - 2025/01/07
        /// Generate label id from input string
        /// </summary>
        /// <param name="labelValue">Label value</param>
        /// <returns></returns>
        private static string GenerateLabelId(string labelValue)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HMTProjectService projectService = new HMTProjectService();
            Project projectNode = projectService.currentProject();
            ProjectItem labelFileNode = projectService.currentLabelNode() as ProjectItem;
            AxLabelFile labelFile = projectService.currentModel().GetLabelFile(labelFileNode.Name);
            LabelManager labelManager = new LabelManager(labelFile.LabelFileId, labelFile, projectNode.Name);
            string labelId = string.Empty;

            if (ValidateLabelValue(labelValue))
            {
                labelId = labelManager.createLabel(labelValue);
            }

            return labelId;
        }

        /// <summary>
        /// Willie Yao - 2025/01/07
        /// Validate this label value is not the label Id.
        /// </summary>
        /// <param name="labelValue">Label value</param>
        /// <returns>
        /// True if label value is not lable id.
        /// </returns>
        public static bool ValidateLabelValue(string labelValue)
        {
            bool ret = false;

            if (!labelValue.StartsWith("@") && labelValue != "" && Regex.Replace(labelValue, @"[^a-zA-z]", "") != "")
            {
                ret = true;
            }

            return ret;
        }        
    }
}
