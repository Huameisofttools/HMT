using EnvDTE80;
using HMT.Commands.UserFeedbackCommand;
using HMT.Commands.UserFeedbackCommands;
using HMT.Commands.UserGuideCommands;
using HMT.OptionsPane;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using HMT.Models;
using System.Windows.Controls;
using static Microsoft.VisualStudio.Threading.AsyncReaderWriterLock;

namespace HMT
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>   
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.EmptySolution, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid("4ab38674-8342-44af-9ef8-fdaf145c8972")]    
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(HMTOptions), "HMT D365FFO tools", "D365FFO Page", 0, 0, true)]
    [ProvideToolWindow(typeof(HMT.Views.Global.HMTJsonToDataContractWindow))]
    public sealed class HMTPackage : AsyncPackage
    {
        /// <summary>
        /// HMTPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "4ab38674-8342-44af-9ef8-fdaf145c8972";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await base.InitializeAsync(cancellationToken, progress);

            await HMTCommands.HMTLabelGenerateCommands.HMTLabelGenerateForAll.InitializeAsync(this);
            await HMTCommands.HMTLabelGenerateCommands.HMTLabelGenerateForItem.InitializeAsync(this);
            await HMTCommands.HMTLabelGenerateCommands.HMTLabelGenerateForProject.InitializeAsync(this);
            await HMTCommands.HMTKernalSettingsCommand.HMTKernalSettingsCommand.InitializeAsync(this);
            await HMTCommands.HMTBatchJobGenerateCommand.HMTBatchJobGenerateCommand.InitializeAsync(this);
            await HMTCommands.HMTFormGeneratorCommand.HMTFormGenerateCommand.InitializeAsync(this);
            await HMTCommands.HMTHeaderCommentGeneratorCommands.HMTHeaderCommentGenerateForItem.InitializeAsync(this);
            await HMTCommands.HMTHeaderCommentGeneratorCommands.HMTHeaderCommentGenerateForProject.InitializeAsync(this);
            // await HMTCommands.HMTHeaderCommentGeneratorCommands.HMTHeaderCommentGenerateForAll.InitializeAsync(this); This function abandoned
            await HMTCommands.HMTParmMethodGenerateCommands.HMTParmMethodGenerateCommand.InitializeAsync(this);
            await HMTCommands.HMTFindExistGeneratorCmd.HMTFindExistGeneratorCmd.InitializeAsync(this);
            await HMTCommands.HMTExtendAxElementCmd.HMTExtendAxElementCmd.InitializeAsync(this);
            await HMTCommands.TableFieldsBuilderCommands.TableFieldsBuilderCommand.InitializeAsync(this);
            await HMTCommands.TableCommands.TableBuilderCommand.InitializeAsync(this);
            await HMTCommands.HMTPrivilegeAndDutyGeneratorCommands.HMTPrivilegeAndDutyGenerateForItem.InitializeAsync(this);
            await HMTUserIssueFeedbackCommand.InitializeAsync(this);
            await HMTUserSuggestCommand.InitializeAsync(this);
            await HMTUserGuideCommand.InitializeAsync(this);
            await Commands.WindowCommands.HMTJsonToDataContractWindowCommand.InitializeAsync(this);
        }

        /// <summary>
        /// Willie Yao - 01/08/2025
        /// Override GetAsyncToolWindowFactory method
        /// </summary>
        /// <param name="toolWindowType">toolWindowType</param>
        /// <returns>IVsAsyncToolWindowFactory</returns>
        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            return toolWindowType.Equals(Guid.Parse("e5775bcc-3ede-465c-876b-73cd0aba9cfb")) ? this : null;
        }

        /// <summary>
        /// Willie Yao - 01/08/2025
        /// Override InitializeToolWindowAsync method for transit some data
        /// </summary>
        /// <param name="toolWindowType">toolWindowType</param>
        /// <param name="id">id</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns></returns>
        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = await this.GetServiceAsync(typeof(EnvDTE.DTE)) as DTE2;
            return new HMTJsonToDataContractToolWindowData
            {
                DTE = dte,
                Package = this,
                TextBox = new System.Windows.Forms.TextBox() { Name = nameof(TextBox) }
            };
        }
    }    
}
