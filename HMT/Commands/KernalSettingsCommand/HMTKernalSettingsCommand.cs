using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using System.Collections.Generic;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using System.Windows.Forms;
using HMT.KernelSettings;
using HMT.Kernel;

namespace HMT.HMTCommands.HMTKernalSettingsCommand
{
    internal sealed class HMTKernalSettingsCommand
    {
        public const int CommandId = 0x1202;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private HMTKernalSettingsCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += HMTGlobalFunctionVisibleHelper.EnabledIfD365ProjectExists;
            commandService.AddCommand(menuItem);
        }

        public static HMTKernalSettingsCommand Instance
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

        public static async Task InitializeAsync(AsyncPackage package)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTKernalSettingsCommand(package, commandService);
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
            HMTKernelSettings kernelSettings = new HMTKernelSettings(package);

            kernelSettings.ShowDialog();
        }
    }
}
