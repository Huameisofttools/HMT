using EnvDTE;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMT.Kernel
{
    /// <summary>
    /// Willie Yao - 09/03/2024
    /// This class is used to check the visibility of the global function
    /// </summary>
    public class HMTGlobalFunctionVisibleHelper
    {
        /// <summary>
        /// Willie Yao - 09/03/2024
        /// This method is used to check the visibility of the global function
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        public static void EnabledIfD365ProjectExists(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HMTProjectService projectService = new HMTProjectService();
            Array allProject = projectService.CurrentProjects();
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand == null)
            {
                return;
            }

            bool isVisible = false;     
            foreach (OAVSProject project in allProject)
            {
                if (project == null)
                {
                    continue;
                }

                if (project.Project.ProjectType == "FinanceOperations")
                {
                    isVisible = true;
                    break;
                }
            }

            menuCommand.Visible = isVisible;
            menuCommand.Enabled = isVisible;
        }
    }
}
