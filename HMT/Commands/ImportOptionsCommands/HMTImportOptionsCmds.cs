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
using System.IO;
using System.Xml;
using HMT.Views.Global;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;


namespace HMT.HMTCommands.HMTImportOptionsCmds
{
    /// <summary>
    /// ina wang on 03/05/2025
    /// This class is responsible for handling the import options command
    /// </summary>
    internal sealed class HMTImportOptionsCmds
    {
        public const int CommandId = 0x0143;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private HMTImportOptionsCmds(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static HMTImportOptionsCmds Instance
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
            Instance = new HMTImportOptionsCmds(package, commandService);
        }

        /// <summary>
        /// ina Wang on 03/05/2025
        /// this method is used to execute the import options command value to xml file
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        private void Execute(object sender, EventArgs e)
        {
            using (HMTXMLFileUploadWinForm dialog = new HMTXMLFileUploadWinForm())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string xmlContent = dialog.UploadedXmlContent;

                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlContent);

                        XmlNode root = xmlDoc.DocumentElement;
                        foreach (XmlNode node in root.ChildNodes)
                        {
                            switch (node.Name)
                            {
                                case "GenerateForCodeLabel":
                                    if (bool.TryParse(node.InnerText, out bool tempGenerateForCodeLabel))
                                    {
                                        OptionsPane.HMTOptionsUtils.setIsLabelForSourceCode(this.package, tempGenerateForCodeLabel);
                                    }
                                    break;
                                case "MethodActived":
                                    if (bool.TryParse(node.InnerText, out bool tempMethodActived))
                                    {
                                        OptionsPane.HMTOptionsUtils.setIsParmMethodActivated(this.package, tempMethodActived);
                                    }
                                    break;
                                case "Prefix":
                                    OptionsPane.HMTOptionsUtils.setPrefix(this.package, node.InnerText);
                                    break;
                            }
                        }
                    }
                    catch (XmlException xmlEx)
                    {
                        CoreUtility.HandleExceptionWithErrorMessage(xmlEx);
                    }
                    catch (Exception ex)
                    {
                        CoreUtility.HandleExceptionWithErrorMessage(ex);
                    }
                }
            }
        }
    }
}
