using System.Linq;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Service;
using HMT.Kernel;
using HMT.HMTBatchJobTemplateGenerator;
using Microsoft.Dynamics.Framework.Tools.Core;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using System.Globalization;
using System;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.VisualStudio.Shell;

namespace HMT.HMTAXEditorUtils.HMTParmMethodGenerator
{
    public class HMTParmMethodGenerateService
    {
        public IMetaModelService metaModelService;
        public AxClass axClass;

        /// <summary>
        /// Willie Yao - 2024/04/02
        /// Generate the parm method metadata string
        /// </summary>
        /// <returns>Parm method metadata string</returns>
        public string generateParmMethod(CompilerBaseType type, string typeName, string memberName, string methodName, string variableName, bool comment = false)
        {
            CodeGenerateHelper generateHelper = new CodeGenerateHelper();
            generateHelper.IndentSetValue(4);
            string typeNameStr = AxTypeHelper.getTypeStr(type, typeName);
            if (comment)
            {
                //generateHelper.AppendLine($"/// <summary>");
                //generateHelper.AppendLine($"/// ");
                //generateHelper.AppendLine($"/// </summary>");
                //generateHelper.AppendLine($"/// <param name = \"_recId\">the record id</param>");
                //generateHelper.AppendLine($"/// <param name = \"_forUpdate\">if its updatable</param>");
                //generateHelper.AppendLine($"/// <returns>a table </returns>");
            }
            generateHelper.AppendLine($"public {typeNameStr} {methodName}({typeNameStr} _{variableName} = {memberName})");
            generateHelper.AppendLine("{");
            generateHelper.IndentIncrease();
            generateHelper.AppendLine($"{memberName} = _{variableName};");
            generateHelper.AppendLine("return " + memberName + ";");
            generateHelper.IndentDecrease();
            generateHelper.AppendLine("}");

            return generateHelper.ResultString.ToString();
        }

        public HMTParmMethodGenerateService(AxClass _axClass)
        {
            HMTProjectService projectService = new HMTProjectService();
            metaModelService = projectService.currentModel();
            axClass = _axClass;
        }

        public void updateAxClass()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (metaModelService != null)
            {                
                DTE service = AxServiceProvider.GetService<DTE>();
                if (service == null)
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "No service for DTE found. The DTE must be registered as a service for using this API.", new object[0]));
                }

                // Get current element's model information.
                VSProjectNode activeProjectNode = HMTBatchJobGenerateService.currentVSProject(service); 
                ModelInfo gModel = activeProjectNode.GetProjectsModelInfo();

                ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);                
                metaModelService.UpdateClass(axClass, saveInfo);              
            }
        }

        public static AxMethod addMethod(string methodName, string code)
        {
            AxMethod metaMethod = new AxMethod();
            metaMethod.Name = methodName;
            metaMethod.Source = code;
            metaMethod.IsExtendedMethod = false;
            return metaMethod;
        }
    }

}
