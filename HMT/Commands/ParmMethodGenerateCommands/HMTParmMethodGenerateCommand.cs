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
using HMT.HMTAXEditorUtils.HMTParmMethodGenerator;
using Microsoft.VisualStudio.Shell.Interop;

namespace HMT.HMTCommands.HMTParmMethodGenerateCommands
{
    internal sealed class HMTParmMethodGenerateCommand
    {
        public const int CommandId = 0x0142;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// <c>HMTParmMethodGenerateCommand</c> constructor
        /// </summary>
        /// <param name="package">Package</param>
        /// <param name="commandService">Menu command service</param>
        /// <exception cref="ArgumentNullException">
        /// ArgumentNullException
        /// </exception>
        private HMTParmMethodGenerateCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));            

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(Execute, menuCommandID);
            // Bug 130 Parm method generator issue modified by Willie Yao on 2024/10/29 Begin
            // "Parm method generator" can only be used when this file is class type and current project is D365 project.
            menuItem.BeforeQueryStatus += projectmenuItem_BeforeQueryStatus;
            menuItem.BeforeQueryStatus += HMTGlobalFunctionVisibleHelper.EnabledIfD365ProjectExists;
            // Bug 130 Parm method generator issue modified by Willie Yao on 2024/10/29 End
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
                bool flag = dte.ActiveDocument != null;
                if (flag)
                {
                    bool flag2 = dte.ActiveDocument.Name.IndexOf("AxClass") >= 0;
                    if (flag2)
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

        public static HMTParmMethodGenerateCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTParmMethodGenerateCommand(package, commandService);
        }

        /// <summary>
        /// HMTParmMethodGenerateCommand
        /// Willie Yao - 2024/04/02
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            EnvDTE80.DTE2 dte = (EnvDTE80.DTE2)LocalUtils.DTE;
            EnvDTE.Document doc = null;
            object obj = null;
            bool flag = LocalUtils.DTE.ActiveDocument != null;
            if (flag)
            {
                doc = LocalUtils.DTE.ActiveDocument;
            }
            bool flag2 = obj == null;
            if (flag2)
            {
                obj = LocalUtils.getAOTObjectByName(doc.Name);

                AxClass axClass = obj as AxClass;

                HMTParmMethodGenerateService service = new HMTParmMethodGenerateService(axClass);
                HMTParmMethodGenerateDialog dialog = new HMTParmMethodGenerateDialog();
                dialog.initParameters(service);
                dialog.ShowDialog();
            }
        }
            
    }
}
