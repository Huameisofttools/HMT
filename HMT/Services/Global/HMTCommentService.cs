using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Forms;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Tables;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Views;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.BaseTypes;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Menus;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Security;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Workflows;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using System.Text.RegularExpressions;
using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Providers;
using Microsoft.Dynamics.AX.Metadata.Storage;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.AX.Metadata.Core.Collections;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.DataEntityViews;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Presentation;
using Microsoft.Dynamics.AX.Metadata.Collections;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Classes;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.Maps;
using HMT.Kernel;
using Microsoft.VisualStudio.Shell;

namespace HMT.Services.Global
{
    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    abstract public class HMCommentService
    {
        public IMetaModelService metaModelService;
        public string commentSummary;
        public string[] commentSummaryLines;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService(string comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            HMTProjectService projectService = new HMTProjectService();
            Project projectNode = projectService.currentProject();

            metaModelService = projectService.currentModel();
            commentSummary = comment;
            commentSummaryLines = comment.Split('\n');
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        abstract public void run();

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        abstract public void runAX();

        public string removeBlankLine(string source)
        {
            string sourceCode = source.TrimStart();

            while (sourceCode.StartsWith("\r") || sourceCode.StartsWith("\n"))
            {
                sourceCode = sourceCode.Substring(1);
                sourceCode = sourceCode.TrimStart();
            }

            if (sourceCode == string.Empty)
            {
                return source;
            }

            return sourceCode;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 10/23/2020
        /// Check and add header comment to class declaration
        /// </summary>
        public string addCommentToCD(string code)
        {
            string sourceCode = code;
            string ret = this.removeBlankLine(sourceCode);
            string newComment = "";
            string preSpaces = "";

            if (ret.StartsWith("///") || ret.StartsWith("//"))
            {
                return sourceCode;
            }

            if (!ret.StartsWith("/// <summary>"))
            {
                foreach (var line in commentSummaryLines)
                {
                    newComment += preSpaces + line + "\r\n";
                }
            }

            newComment += ret;

            if (newComment == string.Empty)
            {                
                return code;
            }

            return newComment;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 10/23/2020
        /// Check and add header comment
        /// </summary>
        public AxMethod addCommentToMethod(AxMethod axMethod, int level)
        {
            string sourceCode = axMethod.Source;
            string ret = this.removeBlankLine(sourceCode);
            string newComment = "";
            string preSpaces = "";
            int i;

            for (i = 0; i < level * 4; i++)
            {
                preSpaces += " ";
            }

            if (ret.StartsWith("///") || ret.StartsWith("//"))
            {
                return axMethod;
            }

            if (!ret.StartsWith("/// <summary>"))
            {
                foreach (var line in commentSummaryLines)
                {
                    newComment += preSpaces + line + "\r\n";
                }

                foreach (var param in axMethod.Parameters)
                {
                    newComment += preSpaces + "/// <param name=" + '"' + param.Name + '"' + ">" + "</param>\r\n";
                }

                if (axMethod.ReturnType.Type != CompilerBaseType.Void)
                {
                    newComment += preSpaces + "/// <returns>" + axMethod.ReturnType.TypeName + "</returns>\r\n";
                }
            }

            newComment += preSpaces + ret;

            axMethod.Source = newComment;

            return axMethod;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Contructs a classes based on the element type
        /// </summary>
        /// <param name="element">Current element</param>
        /// <returns>The instance of label service</returns>
        static public HMCommentService construct(NamedElement _element, string commentSummary, Boolean _throwError = true)
        {
            switch (_element.GetType().Name)
            {
                case "ClassItem":
                    return new HMCommentService_Class(_element as ClassItem, commentSummary);

                case "Table":
                    return new HMCommentService_Table(_element as Table, commentSummary);

                case "View":
                    return new HMCommentService_View(_element as View, commentSummary);

                case "Form":
                    return new HMCommentService_Form(_element as Form, commentSummary);

                case "DataEntityView":
                    return new HMCommentService_DataEntityView(_element as DataEntityView, commentSummary);

                case "SimpleQuery":
                    return new HMCommentService_QuerySimple(_element as SimpleQuery, commentSummary);

                case "Map":
                    return new HMCommentService_Map(_element as Map, commentSummary);

                case "MacroDictionary":
                    return new HMCommentService_MacroDictionary(_element as MacroDictionary, commentSummary);

                default:
                    if (_throwError)
                    {
                        throw new NotImplementedException($"The type {_element.GetType().Name} is not implemented.");
                    }
                    else
                    {
                        return null;
                    }
            }
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Contructs a classes based on the element type
        /// </summary>
        /// <param name="_element">Current element</param>
        /// <param name="_throwError">Throw error or not</param>
        /// <returns>The instance of label service</returns>
        static public HMCommentService construct(IMetaElement _element, string commentSummary, Boolean _throwError = true)
        {
            switch (_element.GetType().Name)
            {
                case "AxClass":
                    return new HMCommentService_Class(_element as AxClass, commentSummary);

                case "AxTable":
                    return new HMCommentService_Table(_element as AxTable, commentSummary);

                case "AxView":
                    return new HMCommentService_View(_element as AxView, commentSummary);

                case "AxForm":
                    return new HMCommentService_Form(_element as AxForm, commentSummary);

                case "AxDataEntityView":
                    return new HMCommentService_DataEntityView(_element as AxDataEntityView, commentSummary);

                case "AxQuerySimple":
                    return new HMCommentService_QuerySimple(_element as AxQuerySimple, commentSummary);

                case "AxMap":
                    return new HMCommentService_Map(_element as AxMap, commentSummary);

                case "AxMacroDictionary":
                    return new HMCommentService_MacroDictionary(_element as AxMacroDictionary, commentSummary);

                default:
                    if (_throwError)
                    {
                        throw new NotImplementedException($"The type {_element.GetType().Name} is not implemented.");
                    }
                    else
                    {
                        return null;
                    }
            }
        }

    }

    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_Class : HMCommentService
    {
        ClassItem classItem;
        AxClass axClass;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Class(AxClass curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axClass = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Class(ClassItem curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            classItem = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axClass = metaModelService.GetClass(classItem.Name);

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            axClass.Declaration = this.addCommentToCD(axClass.Declaration);

            foreach (var axMethod in axClass.Methods)
            {
                this.addCommentToMethod(axMethod, 1);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetClassModelInfo(axClass.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateClass(axClass, saveInfo);
            }
        }

    }


    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_Table : HMCommentService
    {
        Table table;
        AxTable axTable;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Table(AxTable curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axTable = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Table(Table curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            table = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axTable = metaModelService.GetTable(table.Name);

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            string codeIncludedComment = this.addCommentToCD(axTable.Declaration);           

            axTable.Declaration = codeIncludedComment;

            foreach (var axMethod in axTable.Methods)
            {
                this.addCommentToMethod(axMethod, 1);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetTableModelInfo(axTable.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateTable(axTable, saveInfo);
            }
        }

    }

    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_View : HMCommentService
    {
        View view;
        AxView axView;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_View(AxView curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axView = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_View(View curObj, string comment) : base(comment)
        {

            ThreadHelper.ThrowIfNotOnUIThread();
            view = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axView = metaModelService.GetView(view.Name);

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            axView.Declaration = this.addCommentToCD(axView.Declaration);

            foreach (var axMethod in axView.Methods)
            {
                this.addCommentToMethod(axMethod, 1);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetViewModelInfo(axView.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateView(axView, saveInfo);
            }
        }

    }

    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_DataEntityView : HMCommentService
    {
        DataEntity dataEntity;
        AxDataEntityView axDataEntity;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_DataEntityView(AxDataEntityView curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axDataEntity = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_DataEntityView(DataEntity curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            dataEntity = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axDataEntity = metaModelService.GetDataEntityView(dataEntity.Name);

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            axDataEntity.Declaration = this.addCommentToCD(axDataEntity.Declaration);

            foreach (var axMethod in axDataEntity.Methods)
            {
                this.addCommentToMethod(axMethod, 1);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetDataEntityViewModelInfo(axDataEntity.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateDataEntityView(axDataEntity, saveInfo);
            }
        }

    }

    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_QuerySimple : HMCommentService
    {
        SimpleQuery query;
        AxQuerySimple axQuerySimple;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_QuerySimple(AxQuerySimple curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axQuerySimple = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_QuerySimple(SimpleQuery curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            query = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axQuerySimple = metaModelService.GetQuery(query.Name) as AxQuerySimple;

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            foreach (var axMethod in axQuerySimple.Methods)
            {
                this.addCommentToMethod(axMethod, axMethod.Name == "classDeclaration" ? 0 : 1);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetQueryModelInfo(axQuerySimple.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateQuery(axQuerySimple, saveInfo);
            }
        }

    }

    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_Map : HMCommentService
    {
        Map map;
        AxMap axMap;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Map(AxMap curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axMap = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Map(Map curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            map = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axMap = metaModelService.GetMap(map.Name);

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            axMap.Declaration = this.addCommentToCD(axMap.Declaration);

            foreach (var axMethod in axMap.Methods)
            {
                this.addCommentToMethod(axMethod, 1);
            }

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetMapModelInfo(axMap.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateMap(axMap, saveInfo);
            }
        }

    }

    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_MacroDictionary : HMCommentService
    {
        MacroDictionary macro;
        AxMacroDictionary axMacro;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_MacroDictionary(AxMacroDictionary curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axMacro = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_MacroDictionary(MacroDictionary curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            macro = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axMacro = metaModelService.GetMacroDictionary(macro.Name);

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            axMacro.Source = this.addCommentToCD(axMacro.Source);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetMacroDictionaryModelInfo(axMacro.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateMacroDictionary(axMacro, saveInfo);
            }
        }

    }

    /// <summary>
    /// HM_D365_Addin_HeaderCommentGenerator
    /// Byron Zhang - 09/30/2022
    /// Abstract class for comment service
    /// </summary>
    public class HMCommentService_Form : HMCommentService
    {
        Form form;
        AxForm axForm;

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Form(AxForm curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            axForm = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Initialize class
        /// </summary>
        public HMCommentService_Form(Form curObj, string comment) : base(comment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            form = curObj;
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for named element
        /// </summary>
        public override void run()
        {
            axForm = metaModelService.GetForm(form.Name);

            this.runAX();
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Run the core logic for meta element
        /// </summary>
        public override void runAX()
        {
            foreach (var axMethod in axForm.Methods)
            {
                this.addCommentToMethod(axMethod, axMethod.Name == "classDeclaration" ? 0 : 1);
            }

            foreach (AxFormDataSourceRoot datasource in axForm.DataSources)
            {
                foreach (var axMethod in datasource.Methods)
                {
                    this.addCommentToMethod(axMethod, 2);
                }

                foreach (var axDataField in datasource.Fields)
                {
                    foreach (var axMethod in axDataField.Methods)
                    {
                        this.addCommentToMethod(axMethod, 3);
                    }
                }
            }

            updateFormControlsAX(this.axForm.Design.Controls);

            if (metaModelService != null)
            {
                ModelSaveInfo saveInfo = new ModelSaveInfo(metaModelService.GetFormModelInfo(axForm.Name).FirstOrDefault<ModelInfo>());
                metaModelService.UpdateForm(axForm, saveInfo);
            }
        }

        /// <summary>
        /// HM_D365_Addin_HeaderCommentGenerator
        /// Byron Zhang - 09/30/2022
        /// Add comments
        /// </summary>
        /// <param name="_controls">The children controls</param>
        public void updateFormControlsAX(KeyedObjectCollection<AxFormControl> _controls)
        {
            foreach (AxFormControl control in _controls)
            {
                foreach (var axMethod in control.Methods)
                {
                    this.addCommentToMethod(axMethod, 2);
                }

                switch (control.Type)
                {
                    case FormControlType.Group:
                        AxFormGroupControl groupControl = control as AxFormGroupControl;

                        updateFormControlsAX(groupControl.Controls);
                        break;

                    case FormControlType.ButtonGroup:
                        AxFormButtonGroupControl buttonGroupCaption = control as AxFormButtonGroupControl;

                        updateFormControlsAX(buttonGroupCaption.Controls);
                        break;

                    case FormControlType.Tab:
                        AxFormTabControl tabControl = control as AxFormTabControl;

                        updateFormControlsAX(tabControl.Controls);
                        break;

                    case FormControlType.TabPage:
                        AxFormTabPageControl tabpageControl = control as AxFormTabPageControl;

                        updateFormControlsAX(tabpageControl.Controls);
                        break;

                    case FormControlType.MenuButton:
                        AxFormMenuButtonControl menubuttonControl = control as AxFormMenuButtonControl;

                        updateFormControlsAX(menubuttonControl.Controls);
                        break;

                    case FormControlType.ReferenceGroup:
                        AxFormReferenceGroupControl referenceGroupControl = control as AxFormReferenceGroupControl;

                        updateFormControlsAX(referenceGroupControl.Controls);
                        break;

                    case FormControlType.Grid:
                        AxFormGridControl gridControl = control as AxFormGridControl;

                        updateFormControlsAX(gridControl.Controls);
                        break;

                    default:
                        break;
                }
            }
        }

    }
}
