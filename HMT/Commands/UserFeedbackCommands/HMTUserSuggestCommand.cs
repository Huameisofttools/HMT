using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace HMT.Commands.UserFeedbackCommand
{
    internal sealed class HMTUserSuggestCommand
    {
        public const int CommandId = 0x0133;

        public static readonly Guid CommandSet = new Guid("194ef7a6-070b-47e5-b084-193c13aa350a");

        private readonly AsyncPackage package;

        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        private HMTUserSuggestCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            OleMenuCommand menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static HMTUserSuggestCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new HMTUserSuggestCommand(package, commandService);
        }

        /// <summary>
        /// HMTUserSuggestCommand Execute method
        /// Willie Yao - 2024/10/25
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Execute(object sender, EventArgs e)
        {
            // This is the URL of the suggestions form
            System.Diagnostics.Process.Start("https://forms.office.com/r/tZPwr2MJd4");
        }
    }
}
