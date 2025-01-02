using EnvDTE;
using HMT.HMTBatchJobTemplateGenerator;
using HMT.Kernel;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.Framework.Tools.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMT.HMTTable.HMTFindExistMethodGenerator
{
    public class HMTFindExistMethodGenerateService
    {
        public IMetaModelService metaModelService;
        public AxTable axTable;

        public HMTFindExistMethodGenerateService(AxTable _axTable)
        {
            HMTProjectService projectService = new HMTProjectService();
            metaModelService = projectService.currentModel();
            axTable = _axTable;
        }

        public string generateFindMethod(string methodName, string parameters, bool comment = false)
        {
            CodeGenerateHelper generateHelper = new CodeGenerateHelper();
            generateHelper.IndentSetValue(4);
            generateHelper.AppendLine("");
            generateHelper.AppendLine($"public static {axTable.Name} {methodName}({parameters}, boolean _selectForUpdate = false)");
            generateHelper.AppendLine("{");
            generateHelper.IndentIncrease();
            string variableTableName = char.ToLower(axTable.Name[0]) + axTable.Name.Substring(1);
            generateHelper.AppendLine($"{axTable.Name} {variableTableName};");

            var parameterList = new List<string>();
            if (parameters != string.Empty)
            {
                parameterList.AddRange(parameters.Split(',').Select(p => p.Trim()).ToList());
            }

            foreach (var parameterName in parameterList)
            {
                generateHelper.AppendLine($"if (!{parameterName.Trim().Split(' ')[1]})");
                generateHelper.AppendLine("{");
                generateHelper.IndentIncrease();
                generateHelper.AppendLine($"return {variableTableName};");
                generateHelper.IndentDecrease();
                generateHelper.AppendLine("}");
            }

            generateHelper.AppendLine($"{variableTableName}.selectForUpdate(_selectForUpdate);");                       
            generateHelper.AppendLine($"select firstonly {variableTableName}");
            generateHelper.IndentIncrease();
            if (parameterList.Any())
            {
                generateHelper.AppendLine($"where {string.Join(" && ", parameterList.Select(p => $"{variableTableName}.{p.Trim().Split(' ')[1].Substring(1)} == {p.Trim().Split(' ')[1]}"))};");
            }
            generateHelper.IndentDecrease();
            generateHelper.AppendLine($"return {variableTableName};");
            generateHelper.IndentDecrease();
            generateHelper.AppendLine("}");

            return generateHelper.ResultString.ToString();
        }

        public string generateExistMethod(string methodName, string parameters, bool comment = false)
        {
            CodeGenerateHelper generateHelper = new CodeGenerateHelper();
            generateHelper.IndentSetValue(4);
            generateHelper.AppendLine("");
            generateHelper.AppendLine($"public static boolean {methodName}({parameters})");
            generateHelper.AppendLine("{");
            generateHelper.IndentIncrease();
            var parameterList = new List<string>();
            if (parameters != string.Empty)
            {
                parameterList.AddRange(parameters.Split(',').Select(p => p.Trim()).ToList());
            }

            foreach (var parameterName in parameterList)
            {
                generateHelper.AppendLine($"if (!{parameterName.Trim().Split(' ')[1]})");
                generateHelper.AppendLine("{");
                generateHelper.IndentIncrease();
                generateHelper.AppendLine($"return false;");
                generateHelper.IndentDecrease();
                generateHelper.AppendLine("}");
            }

            if (parameterList.Any())
            {
                generateHelper.AppendLine($"return (select firstonly {axTable.Name}");
                generateHelper.IndentIncrease();
                generateHelper.AppendLine($"where {string.Join(" && ", parameterList.Select(p => $"{axTable.Name}.{p.Trim().Split(' ')[1].Substring(1)} == {p.Trim().Split(' ')[1]}"))}).RecId != 0;");
            }
            else
            {
                generateHelper.AppendLine($"return (select firstonly {axTable.Name}).RecId != 0;");
            }
            generateHelper.IndentDecrease();
            generateHelper.IndentDecrease();
            generateHelper.AppendLine("}");

            return generateHelper.ResultString.ToString();
        }

        public void updateAxTable()
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
                metaModelService.UpdateTable(axTable, saveInfo);
            }
        }

        public static AxMethod addMethod(string methodName, string code)
        {
            AxMethod metaMethod = new AxMethod();
            metaMethod.Name = methodName;
            metaMethod.Source = code;
            metaMethod.IsExtendedMethod = false;
            metaMethod.IsStatic = true;
            return metaMethod;
        }
    }

    
}
