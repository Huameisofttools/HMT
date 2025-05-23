﻿using Microsoft.VisualStudio.Shell;
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
using HMT.OptionsPane;
using System.Xml;
using System.IO;
using HMT.Views.Global;


namespace HMT.HMTCommands.HMTExportOptionsCommands
{
    /// <summary>
    /// Ina Wang on 03/05/2025
    /// This class is responsible for handling the export options command
    /// </summary>
    internal sealed class HMTExportOptionsCommands
    {
        public const int CommandId = 0x0144;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private HMTExportOptionsCommands(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static HMTExportOptionsCommands Instance
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
            Instance = new HMTExportOptionsCommands(package, commandService);
        }

        /// <summary>
        /// Ina Wang on 03/05/2025
        /// This method is used to execute the export options command value to xml file
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        private void Execute(object sender, EventArgs e)
        {
            bool generateForCodeLabel   = false;
            bool methodActived          = false;
            string prefix               = "";

            try
            {
                methodActived           = OptionsPane.HMTOptionsUtils.getIsParmMethodActivated(this.package);
                generateForCodeLabel    = OptionsPane.HMTOptionsUtils.getIsLabelForSourceCode(this.package);
                prefix                  = OptionsPane.HMTOptionsUtils.getPrefix(this.package);

                // create a XmlDocument 
                XmlDocument xmlDoc = new XmlDocument();

                // create XML
                XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlDoc.AppendChild(xmlDeclaration);

                // create root element
                XmlElement rootElement = xmlDoc.CreateElement("Configuration");
                xmlDoc.AppendChild(rootElement);

                // create and add generateForCodeLabel element
                XmlElement generateForCodeLabelElement = xmlDoc.CreateElement("GenerateForCodeLabel");
                generateForCodeLabelElement.InnerText = generateForCodeLabel.ToString();
                rootElement.AppendChild(generateForCodeLabelElement);

                // create and methodActived element
                XmlElement methodActivedElement = xmlDoc.CreateElement("MethodActived");
                methodActivedElement.InnerText = methodActived.ToString();
                rootElement.AppendChild(methodActivedElement);

                // create and add prefix element
                XmlElement prefixElement = xmlDoc.CreateElement("Prefix");
                prefixElement.InnerText = prefix;
                rootElement.AppendChild(prefixElement);


                HMTXMLFileDownloadWinForm form = new HMTXMLFileDownloadWinForm(xmlDoc);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }
    }
}
