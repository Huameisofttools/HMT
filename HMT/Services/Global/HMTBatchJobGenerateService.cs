using EnvDTE;
using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.AX.Metadata.Service;
using Microsoft.Dynamics.Framework.Tools.Core;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using Microsoft.Dynamics.Framework.Tools.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace HMT.Services.Global
{
    public class HMTBatchJobGenerateService
    {
        public static DTE gDTE
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return CoreUtility.ServiceProvider.GetService(typeof(DTE)) as DTE;
            }
        }

        public static Project gProject;

        public IMetaModelService gMetaModelService;
        public ModelInfo gModel;
        public IDynamicsProjectService gService;


        /// <summary>
        /// HM_D365_Addin_FormGenerator
        /// Byron Zhang - 09/27/2022
        /// Initialize class
        /// </summary>
        public HMTBatchJobGenerateService()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE service = AxServiceProvider.GetService<DTE>();
            if (service == null)
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "No service for DTE found. The DTE must be registered as a service for using this API.", new object[0]));
            }

            VSProjectNode activeProjectNode = HMTBatchJobGenerateService.currentVSProject(service);

            IMetaModelProviders metaModelProviders = AxServiceProvider.GetService(typeof(IMetaModelProviders)) as IMetaModelProviders;
            gMetaModelService = metaModelProviders.CurrentMetaModelService;
            gModel = activeProjectNode.GetProjectsModelInfo();
            gService = AxServiceProvider.GetService<IDynamicsProjectService>();
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Get current project in Visual Studio.
        /// </summary>
        /// <returns>Current project project or null.</returns>
        public Project currentProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array projects = gDTE.ActiveSolutionProjects as Array;

            if (projects.Length > 0)
            {
                gProject = projects.GetValue(0) as Project;
                return gProject;
            }
            return null;
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Get current VS project
        /// </summary>
        public static VSProjectNode currentVSProject(DTE dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array array = dte.ActiveSolutionProjects as Array;
            if (array != null && array.Length > 0)
            {
                Project project = array.GetValue(0) as Project;
                if (project != null)
                {
                    return project.Object as VSProjectNode;
                }
            }
            return null;
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Update comment
        /// </summary>
        public string updateComment(string comment)
        {
            string[] lines = comment.Split(new Char[] { '\n' });

            string ret = "";
            int i = 0;

            foreach (string line in lines)
            {
                if (i > 0)
                {
                    ret += "\n";
                    ret += "    ";
                }

                ret += line;
                i++;
            }

            return ret;
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Add method to class
        /// </summary>
        public void addMethod(AxClass metaClass, string methodName, string code)
        {
            AxMethod metaMethod = new AxMethod();
            metaMethod.Name = methodName;
            metaMethod.Source = code;
            metaClass.Methods.Add(metaMethod);
        }

        #region RunBaseBatch
        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create class for RunBaseBatch
        /// </summary>
        public void run_RunBaseBatch(string classPrefix, string comment)
        {
            string className = classPrefix + "Batch";

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// This is a framework class.
/// </summary>
public class " + className + @" extends RunBaseBatch implements BatchRetryable
{
    // Packed variables
    TransDate       gTransDate;
    CustAccount     gCustAccount;

    // Dialog fields
    DialogField     gDlgTransDate;
    DialogField     gDlgCustAccount;

    #define.CurrentVersion(1)
    #localmacro.CurrentList
        gTransDate,
        gCustAccount
    #endmacro
}";
            comment = this.updateComment(comment);

            // canGoBatch
            this.addMethod(metaClass, "canGoBatch", @"
    /// <summary>
    " + comment + @"
    /// Indicates whether the class can run in batch
    /// </summary>
    /// <returns>true if the class can run in batch; otherwise, false.</returns>
    public boolean canGoBatch()
    {
        return true;
    }");

            // canGoBatchJournal
            this.addMethod(metaClass, "canGoBatchJournal", @"
    /// <summary>
    " + comment + @"
    /// Indicates whether the class is shown in the list of <c>Journal</c> types.
    /// </summary>
    /// <returns>true if the class is shown in the list of <c>Journal</c> types; otherwise, false.</returns>
    public boolean canGoBatchJournal()
    {
        return true;
    }");

            // dialog
            this.addMethod(metaClass, "dialog", @"
    /// <summary>
    " + comment + @"
    /// Returns a class that contains the methods that are described by the <c>RunBaseDialogable</c> interface.
    /// </summary>
    /// <returns>A class that contains the methods that are described by the <c>RunBaseDialogable</c> interface.</returns>
    public Object dialog()
    {
        DialogRunbase       dialog = super();
    
        gDlgTransDate = dialog.addFieldValue(extendedTypeStr(TransDate), gTransDate);
    
        dialog.addTabPage(" + "\"@SYS76580\"" + @");
        gDlgCustAccount = dialog.addFieldValue(extendedTypeStr(CustAccount), gCustAccount);
    
        return dialog;
    }");

            // getFromDialog
            this.addMethod(metaClass, "getFromDialog", @"
    /// <summary>
    " + comment + @"
    /// Get values from dialog
    /// </summary>
    /// <returns>true if valid</returns>
    public boolean getFromDialog()
    {
        gTransDate      = gDlgTransDate.value();
        gCustAccount    = gDlgCustAccount.value();
    
        return super();
    }");

            // pack
            this.addMethod(metaClass, "pack", @"
    /// <summary>
    " + comment + @"
    /// Pack variables 
    /// </summary>
    /// <returns>Package of variables</returns>
    public container pack()
    {
        return [#CurrentVersion, #CurrentList];
    }");

            // run
            this.addMethod(metaClass, "run", @"
    /// <summary>
    " + comment + @"
    /// Contains the code that does the actual job of the class.
    /// </summary>
    public void run()
    {
        #OCCRetryCount
        if (!this.validate())
        {
            throw error("""");
        }
    
        try
        {
            ttsbegin;
    
            // TODO: Put your logic here
    
            ttscommit;
        }
        catch (Exception::Deadlock)
        {
            retry;
        }
        catch (Exception::UpdateConflict)
        {
            if (appl.ttsLevel() == 0)
            {
                if (xSession::currentRetryCount() >= #RetryNum)
                {
                    throw Exception::UpdateConflictNotRecovered;
                }
                else
                {
                    retry;
                }
            }
            else
            {
                throw Exception::UpdateConflict;
            }
        }
    }");

            // runsImpersonated
            this.addMethod(metaClass, "runsImpersonated", @"
    /// <summary>
    " + comment + @"
    /// Runs impersonated.
    /// </summary>
    /// <returns>true if run impersonated</returns>
    public boolean runsImpersonated()
    {
        return true;
    }");

            // unpack
            this.addMethod(metaClass, "unpack", @"
    /// <summary>
    " + comment + @"
    /// Get variables from package
    /// </summary>
    /// <param name=" + "\"_packedClass\"" + @">Class package</param>
    /// <returns>true if valid</returns>
    public boolean unpack(container _packedClass)
    {
        Version version = RunBase::getVersion(_packedClass);
    
        switch (version)
        {
            case #CurrentVersion:
                [version, #CurrentList] = _packedClass;
                break;
    
            default:
                return false;
        }
    
        return true;
    }");

            // validate
            this.addMethod(metaClass, "validate", @"
    /// <summary>
    " + comment + @"
    /// Validate variables
    /// </summary>
    /// <param name=""_calledFrom"">Called from object</param>
    /// <returns>true if valid</returns>
    public boolean validate(Object _calledFrom = null)
    {
        // TODO: Add your validation logic here
        return true;
    }");

            // construct
            this.addMethod(metaClass, "construct", @"
    /// <summary>
    " + comment + @"
    /// Construct the class
    /// </summary>
    /// <returns>This class</returns>
    static " + className + @" construct()
    {
        return new " + className + @"();
    }");

            // description
            this.addMethod(metaClass, "description", @"
    /// <summary>
    " + comment + @"
    /// Here goes a description of the class
    /// </summary>
    /// <returns>Description of the class</returns>
    static ClassDescription description()
    {
        // TODO: Change to your description
        return " + "\"Your Description \"" + @";
    }");

            // main
            this.addMethod(metaClass, "main", @"
    /// <summary>
    " + comment + @"
    /// Entry point
    /// </summary>
    static void main(Args _args)
    {
        " + className + @"      batch;
    ;
        batch = " + className + @"::construct();
    
        if (batch.prompt())
        {
            batch.runOperation();
        }
    }");

            // canRunInNewSession
            this.addMethod(metaClass, "canRunInNewSession", @"
    /// <summary>
    " + comment + @"
    /// Can run in new session
    /// </summary>
    /// <returns>true if this can run in new session</returns>
    protected boolean canRunInNewSession()
    {
        return true;
    }");

            // isRetryable
            this.addMethod(metaClass, "isRetryable", @"
    /// <summary>
    " + comment + @"
    /// Specifies if the batch task is retryable for transient exceptions or not.
    /// </summary>
    /// <returns>If true is returned, the batch task is retryable, otherwise it is not.</returns>
    [Hookable(false)]
    public final boolean isRetryable() 
    {
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);

                
            }
        }
        #endregion

        #region RunBaseBatchWithQuery
        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create class for RunBaseBatch with Query
        /// </summary>
        public void run_RunBaseBatchWithQuery(string classPrefix, string comment)
        {
            string className = classPrefix + "Batch";

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// This is a framework class.
/// </summary>
public class " + className + @" extends RunBaseBatch implements BatchRetryable
{
    // Packed variables
    TransDate       gTransDate;
    CustAccount     gCustAccount;

    SysQueryRun     gQueryRun;

    // Dialog fields
    DialogField     gDlgTransDate;
    DialogField     gDlgCustAccount;

    #define.CurrentVersion(1)
    #localmacro.CurrentList
        gTransDate,
        gCustAccount
    #endmacro
}";
            comment = this.updateComment(comment);

            // canGoBatch
            this.addMethod(metaClass, "canGoBatch", @"
    /// <summary>
    " + comment + @"
    /// Indicates whether the class can run in batch
    /// </summary>
    /// <returns>true if the class can run in batch; otherwise, false.</returns>
    public boolean canGoBatch()
    {
        return true;
    }");

            // canGoBatchJournal
            this.addMethod(metaClass, "canGoBatchJournal", @"
    /// <summary>
    " + comment + @"
    /// Indicates whether the class is shown in the list of <c>Journal</c> types.
    /// </summary>
    /// <returns>true if the class is shown in the list of <c>Journal</c> types; otherwise, false.</returns>
    public boolean canGoBatchJournal()
    {
        return true;
    }");

            // dialog
            this.addMethod(metaClass, "dialog", @"
    /// <summary>
    " + comment + @"
    /// Returns a class that contains the methods that are described by the <c>RunBaseDialogable</c> interface.
    /// </summary>
    /// <returns>A class that contains the methods that are described by the <c>RunBaseDialogable</c> interface.</returns>
    public Object dialog()
    {
        DialogRunbase       dialog = super();
    
        gDlgTransDate = dialog.addFieldValue(extendedTypeStr(TransDate), gTransDate);
    
        dialog.addTabPage(" + "\"@SYS76580\"" + @");
        gDlgCustAccount = dialog.addFieldValue(extendedTypeStr(CustAccount), gCustAccount);
    
        return dialog;
    }");

            // getFromDialog
            this.addMethod(metaClass, "getFromDialog", @"
    /// <summary>
    " + comment + @"
    /// Get values from dialog
    /// </summary>
    /// <returns>true if valid</returns>
    public boolean getFromDialog()
    {
        gTransDate      = gDlgTransDate.value();
        gCustAccount    = gDlgCustAccount.value();
    
        return super();
    }");

            // initParmDefault
            this.addMethod(metaClass, "initParmDefault", @"
    /// <summary>
    " + comment + @"
    /// Initializes the internal variables
    /// </summary>
    public void initParmDefault()
    {
        this.initQuery();
    
        super();
    }");

            // initQuery
            this.addMethod(metaClass, "initQuery", @"
    /// <summary>
    " + comment + @"
    /// Initializes the query
    /// </summary>
    public void initQuery()
    {
        Query query = new Query();

        query.addDataSource(tableNum(InventTable));
        gQueryRun = new SysQueryRun(query);
    }");

            // pack
            this.addMethod(metaClass, "pack", @"
    /// <summary>
    " + comment + @"
    /// Pack variables 
    /// </summary>
    /// <returns>Package of variables</returns>
    public container pack()
    {
        return [#CurrentVersion, #CurrentList, gQueryRun.pack()];
    }");

            // queryRun
            this.addMethod(metaClass, "queryRun", @"
    /// <summary>
    " + comment + @"
    /// Get queryRun
    /// </summary>
    /// <returns>QueryRun</returns>
    public QueryRun queryRun()
    {
        return gQueryRun;
    }");

            // run
            this.addMethod(metaClass, "run", @"
    /// <summary>
    " + comment + @"
    /// Contains the code that does the actual job of the class.
    /// </summary>
    public void run()
    {
        #OCCRetryCount
        if (!this.validate())
        {
            throw error("""");
        }
    
        try
        {
            ttsbegin;
    
            // TODO: Put your logic here
    
            ttscommit;
        }
        catch (Exception::Deadlock)
        {
            retry;
        }
        catch (Exception::UpdateConflict)
        {
            if (appl.ttsLevel() == 0)
            {
                if (xSession::currentRetryCount() >= #RetryNum)
                {
                    throw Exception::UpdateConflictNotRecovered;
                }
                else
                {
                    retry;
                }
            }
            else
            {
                throw Exception::UpdateConflict;
            }
        }
    }");

            // runsImpersonated
            this.addMethod(metaClass, "runsImpersonated", @"
    /// <summary>
    " + comment + @"
    /// Runs impersonated.
    /// </summary>
    /// <returns>true if run impersonated</returns>
    public boolean runsImpersonated()
    {
        return true;
    }");

            // showQueryValues
            this.addMethod(metaClass, "showQueryValues", @"
    /// <summary>
    " + comment + @"
    /// Indicates whether to add a select button to the dialog box
    /// </summary>
    /// <returns>Always true</returns>
    public boolean showQueryValues()
    {
        return true;
    }");

            // unpack
            this.addMethod(metaClass, "unpack", @"
    /// <summary>
    " + comment + @"
    /// Get variables from package
    /// </summary>
    /// <param name=" + "\"_packedClass\"" + @">Class package</param>
    /// <returns>true if valid</returns>
    public boolean unpack(container _packedClass)
    {
        Version version = RunBase::getVersion(_packedClass);
        container queryCon;
    
        switch (version)
        {
            case #CurrentVersion:
                [version, #CurrentList, queryCon] = _packedClass;

                if (SysQuery::isPackedOk(queryCon))
                {
                    gQueryRun = new SysQueryRun(queryCon);
                }
                else
                {
                    this.initQuery();
                }
                break;
    
            default:
                return false;
        }
    
        return true;
    }");

            // validate
            this.addMethod(metaClass, "validate", @"
    /// <summary>
    " + comment + @"
    /// Validate variables
    /// </summary>
    /// <param name=""_calledFrom"">Called from object</param>
    /// <returns>true if valid</returns>
    public boolean validate(Object _calledFrom = null)
    {
        // TODO: Add your validation logic here
        return true;
    }");

            // construct
            this.addMethod(metaClass, "construct", @"
    /// <summary>
    " + comment + @"
    /// Construct the class
    /// </summary>
    /// <returns>This class</returns>
    static " + className + @" construct()
    {
        return new " + className + @"();
    }");

            // description
            this.addMethod(metaClass, "description", @"
    /// <summary>
    " + comment + @"
    /// Here goes a description of the class
    /// </summary>
    /// <returns>Description of the class</returns>
    static ClassDescription description()
    {
        // TODO: Change to your description
        return " + "\"Your Description \"" + @";
    }");

            // main
            this.addMethod(metaClass, "main", @"
    /// <summary>
    " + comment + @"
    /// Entry point
    /// </summary>
    static void main(Args _args)
    {
        " + className + @"      batch;
    ;
        batch = " + className + @"::construct();
    
        if (batch.prompt())
        {
            batch.runOperation();
        }
    }");

            // canRunInNewSession
            this.addMethod(metaClass, "canRunInNewSession", @"
    /// <summary>
    " + comment + @"
    /// Can run in new session
    /// </summary>
    /// <returns>true if this can run in new session</returns>
    protected boolean canRunInNewSession()
    {
        return true;
    }");

            // isRetryable
            this.addMethod(metaClass, "isRetryable", @"
    /// <summary>
    " + comment + @"
    /// Specifies if the batch task is retryable for transient exceptions or not.
    /// </summary>
    /// <returns>If true is returned, the batch task is retryable, otherwise it is not.</returns>
    [Hookable(false)]
    public final boolean isRetryable() 
    {
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }
        #endregion

        #region SysOperation
        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Contract class for SysOperation
        /// </summary>
        public void run_SysOperation_Contract(string classPrefix, string gComment)
        {
            string className = classPrefix + "Contract";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// This class used to prepare parameter for service.
/// </summary>
[
    DataContractAttribute,
    //Register a UIBuilder, it is optional, if you have some custom function for the dialog fields you could register a UIBuilder
    SysOperationContractProcessingAttribute(classStr(" + classPrefix + @"UIBuilder))
]
class " + className + @" implements SysOperationValidatable
{
    InventLocationId    gInventLocationId;
    InventSiteId        gInventSiteId;
}";
            comment = this.updateComment(comment);

            // parmInventLocationId
            this.addMethod(metaClass, "parmInventLocationId", @"
    /// <summary>
    " + comment + @"
    /// Set and parm field
    /// </summary>
    [
        DataMemberAttribute('InventLocationId'),
        SysOperationDisplayOrderAttribute('2')
    ]
    public InventLocationId parmInventLocationId(InventLocationId _InventLocationId = gInventLocationId)
    {
        gInventLocationId = _InventLocationId;
    
        return gInventLocationId;
    }");

            // parmInventSiteId
            this.addMethod(metaClass, "parmInventSiteId", @"
    /// <summary>
    " + comment + @"
    /// Set and parm field
    /// </summary>
    [
        DataMemberAttribute('InventSiteId'),
        SysOperationDisplayOrderAttribute('1')
    ]
    public inventSiteId parmInventSiteId(InventSiteId _InventSiteId = gInventSiteId)
    {
        gInventSiteId = _InventSiteId;

        return gInventSiteId;
    }");

            // validate
            this.addMethod(metaClass, "validate", @"
    /// <summary>
    " + comment + @"
    /// Validate dialog field when dialog closed.
    /// </summary>
    public boolean validate()
    {
        if (!gInventLocationId)
        {
            error('Warehouse must be fill in.');
            return false;
        }
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Controller class for SysOperation
        /// </summary>
        public void run_SysOperation_Controller(string classPrefix, string gComment)
        {
            string className = classPrefix + "Controller";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Enter point for menuitem or other callers.
/// </summary>
[SysOperationJournaledParametersAttribute(true)]
class " + className + @" extends SysOperationServiceController implements BatchRetryable
{
}";
            comment = this.updateComment(comment);

            // new
            this.addMethod(metaClass, "new", @"
    /// <summary>
    " + comment + @"
    /// New a class object
    /// </summary>
    /// <param name=""_className"">Service class name</param>
    /// <param name=""_methodName"">Service execution method name</param>
    /// <param name=""_executionMode"">Execution mode</param>
    public void new(
        IdentifierName              _className      = classStr(" + classPrefix + @"Service),
        IdentifierName              _methodName     = methodStr(" + classPrefix + @"Service, execute),
        SysOperationExecutionMode   _executionMode  = SysOperationExecutionMode::Synchronous)
    {
        super(_className, _methodName, _executionMode);
    }");

            // main
            this.addMethod(metaClass, "main", @"
    /// <summary>
    " + comment + @"
    /// Provide an enter point for calss " + className + @"
    /// </summary>
    public static void main(Args _args)
    {
        " + className + @"    controller;
        IdentifierName                  className, methodName;
        SysOperationExecutionMode       mode;
        Common                          record;
        SalesTable                      salesTable;
        " + classPrefix + @"Contract      contract;

        //If don't have caller records remove this codes
        if (_args.record())
        {
            record = _args.record();

            switch (record.TableId)
            {
                case tableNum(SalesTable):
                    salesTable = record;
                    //TODO: Add process logic
                    break;
                default:
                    //TODO: Add process logic
                    break;
            }
        }

        [className, methodName, mode]   = SysOperationServiceController::parseServiceInfo(_args);
        controller                      = new " + className + @"(className, methodName, mode);
        contract                        = controller.getDataContractObject();

        if (salesTable)
        {
            contract.parmInventSiteId(salesTable.InventSiteId);
            contract.parmInventLocationId(salesTable.InventLocationId);
        }
        //Whether do we need to show dialog
        controller.parmShowDialog(true);
        controller.parmArgs(_args);
        controller.startOperation();
    }");

            // isRetryable
            this.addMethod(metaClass, "isRetryable", @"
    /// <summary>
    " + comment + @"
    /// Specifies if the batch task is retryable for transient exceptions or not.
    /// </summary>
    /// <returns>If true is returned, the batch task is retryable, otherwise it is not.</returns>
    [Hookable(false)]
    public final boolean isRetryable() 
    {
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Service class for SysOperation
        /// </summary>
        public void run_SysOperation_Service(string classPrefix, string gComment)
        {
            string className = classPrefix + "Service";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Process business logic
/// </summary>
class " + className + @"
{
}";
            comment = this.updateComment(comment);

            // new
            this.addMethod(metaClass, "new", @"
    /// <summary>
    " + comment + @"
    /// Business process logic
    /// </summary>
    /// <param name=""_contract"">Contract class used to parm dialog parameter.</param>
    [SysEntryPointAttribute]
    public void execute(" + classPrefix + @"Contract _contract)
    {
        // Add your logic here
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create UIBuilder class for SysOperation
        /// </summary>
        public void run_SysOperation_UIBuilder(string classPrefix, string gComment)
        {
            string className = classPrefix + "UIBuilder";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Used to add custom dialog.
/// </summary>
class " + className + @" extends SysOperationAutomaticUIBuilder
{
    " + classPrefix + @"Contract  gContract;
    DialogField                 gInventLocationId;
    DialogField                 gInventSiteId;
}";
            comment = this.updateComment(comment);

            // build
            this.addMethod(metaClass, "build", @"
    /// <summary>
    " + comment + @"
    /// Initialize dialog and register form control.
    /// </summary>
    public void build()
    {
        super();

        gContract           = this.dataContractObject() as " + classPrefix + @"Contract;
        //Get fields
        gInventLocationId   = this.bindInfo().getDialogField(gContract, methodStr(" + classPrefix + @"Contract, parmInventLocationId));
        gInventSiteId       = this.bindInfo().getDialogField(gContract, methodStr(" + classPrefix + @"Contract, parmInventSiteId));

        // Register and overwrite the lookup method
        gInventLocationId.registerOverrideMethod(methodStr(FormStringControl, lookup), methodStr(" + className + @", warehouseLookup), this);
        gInventLocationId.registerOverrideMethod(methodStr(FormStringControl, modified), methodStr(" + className + @", warehouseModified), this);
    }");

            // warehouseLookup
            this.addMethod(metaClass, "warehouseLookup", @"
    /// <summary>
    " + comment + @"
    /// Use this method instead of form field control lookup.
    /// </summary>
    public void warehouseLookup(FormStringControl _control)
    {
        SysTableLookup          sysTableLookup      = SysTableLookup::newParameters(tableNum(InventLocation), _control);
        Query                   query               = new Query();
        QueryBuildDataSource    locationDS          = query.addDataSource(tableNum(InventLocation));

        locationDS.addRange(fieldNum(InventLocation, InventSiteId)).value(gInventSiteId.value());

        sysTableLookup.addSelectionField(fieldNum(InventLocation, InventLocationId));
        sysTableLookup.addLookupfield(fieldNum(InventLocation, InventLocationId));
        sysTableLookup.addLookupfield(fieldNum(InventLocation, InventSiteId));
        sysTableLookup.addLookupfield(fieldNum(InventLocation, name));
        sysTableLookup.parmQuery(query);
        sysTableLookup.performFormLookup();
    }");

            // warehouseModified
            this.addMethod(metaClass, "warehouseModified", @"
    /// <summary>
    " + comment + @"
    /// Use this method instead of form field control modified.
    /// </summary>
    public boolean warehouseModified(FormStringControl _control)
    {
        InventLocation  inventLocation;

        if (gInventLocationId.value())
        {
            inventLocation = InventLocation::find(gInventLocationId.value());

            if (!gInventSiteId.value())
            {
                gInventSiteId.value(inventLocation.InventSiteId);
            }
        }
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        #endregion

        #region SysOperationWithQuery
        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Contract class for SysOperation with Query
        /// </summary>
        public void run_SysOperationWithQuery_Contract(string classPrefix, string gComment)
        {
            string className = classPrefix + "Contract";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// This class used to prepare parameter for service.
/// </summary>
[
    DataContractAttribute,
    //Register a UIBuilder, it is optional, if you have some custom function for the dialog fields you could register a UIBuilder
    SysOperationContractProcessingAttribute(classStr(" + classPrefix + @"UIBuilder))
]
class " + className + @" implements SysOperationValidatable
{
    InventLocationId    gInventLocationId;
    InventSiteId        gInventSiteId;
    str                 gEncodedQuery;
}";
            comment = this.updateComment(comment);

            // parmInventLocationId
            this.addMethod(metaClass, "parmInventLocationId", @"
    /// <summary>
    " + comment + @"
    /// Set and parm field
    /// </summary>
    [
        DataMemberAttribute('InventLocationId'),
        SysOperationDisplayOrderAttribute('2')
    ]
    public InventLocationId parmInventLocationId(InventLocationId _InventLocationId = gInventLocationId)
    {
        gInventLocationId = _InventLocationId;
    
        return gInventLocationId;
    }");

            // parmInventSiteId
            this.addMethod(metaClass, "parmInventSiteId", @"
    /// <summary>
    " + comment + @"
    /// Set and parm field
    /// </summary>
    [
        DataMemberAttribute('InventSiteId'),
        SysOperationDisplayOrderAttribute('1')
    ]
    public inventSiteId parmInventSiteId(InventSiteId _InventSiteId = gInventSiteId)
    {
        gInventSiteId = _InventSiteId;

        return gInventSiteId;
    }");

            // parmQuery
            this.addMethod(metaClass, "parmQuery", @"
    /// <summary>
    " + comment + @"
    /// Set and parm query
    /// </summary>
    /// <param name=""encodedQuery"">
    /// Set encodedQuery from dialog
    /// </param>
    /// <returns>
    /// Get encodedQuery
    /// </returns>
    [
        DataMemberAttribute,
        AifQueryTypeAttribute('_encodedQuery', queryStr(" + classPrefix + @"Query)),
        SysOperationLabelAttribute('Sales line')
    ]
    public str parmQuery(str _encodedQuery = gEncodedQuery)
    {
        gEncodedQuery = _encodedQuery;

        return gEncodedQuery;
    }");

            // validate
            this.addMethod(metaClass, "validate", @"
    /// <summary>
    " + comment + @"
    /// Validate dialog field when dialog closed.
    /// </summary>
    public boolean validate()
    {
        if (!gInventLocationId)
        {
            error('Warehouse must be fill in.');
            return false;
        }
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Controller class for SysOperation with Query
        /// </summary>
        public void run_SysOperationWithQuery_Controller(string classPrefix, string gComment)
        {
            string className = classPrefix + "Controller";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Enter point for menuitem or other callers.
/// </summary>
[SysOperationJournaledParametersAttribute(true)]
class " + className + @" extends SysOperationServiceController implements BatchRetryable
{
}";
            comment = this.updateComment(comment);

            // new
            this.addMethod(metaClass, "new", @"
    /// <summary>
    " + comment + @"
    /// New a class object
    /// </summary>
    /// <param name=""_className"">Service class name</param>
    /// <param name=""_methodName"">Service execution method name</param>
    /// <param name=""_executionMode"">Execution mode</param>
    public void new(
        IdentifierName              _className      = classStr(" + classPrefix + @"Service),
        IdentifierName              _methodName     = methodStr(" + classPrefix + @"Service, execute),
        SysOperationExecutionMode   _executionMode  = SysOperationExecutionMode::Synchronous)
    {
        super(_className, _methodName, _executionMode);
    }");

            // main
            this.addMethod(metaClass, "main", @"
    /// <summary>
    " + comment + @"
    /// Provide an enter point for calss " + className + @"
    /// </summary>
    public static void main(Args _args)
    {
        " + className + @"    controller;
        IdentifierName                  className, methodName;
        SysOperationExecutionMode       mode;
        Common                          record;
        SalesTable                      salesTable;
        " + classPrefix + @"Contract      contract;

        //If don't have caller records remove this codes
        if (_args.record())
        {
            record = _args.record();

            switch (record.TableId)
            {
                case tableNum(SalesTable):
                    salesTable = record;
                    //TODO: Add process logic
                    break;
                default:
                    //TODO: Add process logic
                    break;
            }
        }

        [className, methodName, mode]   = SysOperationServiceController::parseServiceInfo(_args);
        controller                      = new " + className + @"(className, methodName, mode);
        contract                        = controller.getDataContractObject();

        if (salesTable)
        {
            contract.parmInventSiteId(salesTable.InventSiteId);
            contract.parmInventLocationId(salesTable.InventLocationId);
        }
        //Whether do we need to show dialog
        controller.parmShowDialog(true);
        controller.parmArgs(_args);
        controller.startOperation();
    }");

            // isRetryable
            this.addMethod(metaClass, "isRetryable", @"
    /// <summary>
    " + comment + @"
    /// Specifies if the batch task is retryable for transient exceptions or not.
    /// </summary>
    /// <returns>If true is returned, the batch task is retryable, otherwise it is not.</returns>
    [Hookable(false)]
    public final boolean isRetryable() 
    {
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Service class for SysOperation with Query
        /// </summary>
        public void run_SysOperationWithQuery_Service(string classPrefix, string gComment)
        {
            string className = classPrefix + "Service";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Process business logic
/// </summary>
class " + className + @"
{
}";
            comment = this.updateComment(comment);

            // new
            this.addMethod(metaClass, "new", @"
    /// <summary>
    " + comment + @"
    /// Business process logic
    /// </summary>
    /// <param name=""_contract"">Contract class used to parm dialog parameter.</param>
    [SysEntryPointAttribute]
    public void execute(" + classPrefix + @"Contract _contract)
    {
        QueryRun queryRun;
        Query query;
        SalesLine salesLine;

        query = new query(SysOperationHelper::base64Decode(_contract.parmQuery()));
        queryRun = new queryRun(query);

        while (queryRun.next())
        {
            try
            {
                salesLine = queryRun.get(tableNum(SalesLine));
                if (salesLine.inventDim().InventSiteId == _contract.parmInventSiteId())
                {
                    //info(salesLine.SalesId);
                }
            }
            catch
            {
                //if an error was thrown, error status should be set to true
                throw error("");
            }
        }

    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create UIBuilder class for SysOperation with Query
        /// </summary>
        public void run_SysOperationWithQuery_UIBuilder(string classPrefix, string gComment)
        {
            string className = classPrefix + "UIBuilder";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Used to add custom dialog.
/// </summary>
class " + className + @" extends SysOperationAutomaticUIBuilder
{
    " + classPrefix + @"Contract  gContract;
    DialogField                 gInventLocationId;
    DialogField                 gInventSiteId;
}";
            comment = this.updateComment(comment);

            // build
            this.addMethod(metaClass, "build", @"
    /// <summary>
    " + comment + @"
    /// Initialize dialog and register form control.
    /// </summary>
    public void build()
    {
        super();

        gContract           = this.dataContractObject() as " + classPrefix + @"Contract;
        //Get fields
        gInventLocationId   = this.bindInfo().getDialogField(gContract, methodStr(" + classPrefix + @"Contract, parmInventLocationId));
        gInventSiteId       = this.bindInfo().getDialogField(gContract, methodStr(" + classPrefix + @"Contract, parmInventSiteId));

        // Register and overwrite the lookup method
        gInventLocationId.registerOverrideMethod(methodStr(FormStringControl, lookup), methodStr(" + className + @", warehouseLookup), this);
        gInventLocationId.registerOverrideMethod(methodStr(FormStringControl, modified), methodStr(" + className + @", warehouseModified), this);
    }");

            // warehouseLookup
            this.addMethod(metaClass, "warehouseLookup", @"
    /// <summary>
    " + comment + @"
    /// Use this method instead of form field control lookup.
    /// </summary>
    public void warehouseLookup(FormStringControl _control)
    {
        SysTableLookup          sysTableLookup      = SysTableLookup::newParameters(tableNum(InventLocation), _control);
        Query                   query               = new Query();
        QueryBuildDataSource    locationDS          = query.addDataSource(tableNum(InventLocation));

        locationDS.addRange(fieldNum(InventLocation, InventSiteId)).value(gInventSiteId.value());

        sysTableLookup.addSelectionField(fieldNum(InventLocation, InventLocationId));
        sysTableLookup.addLookupfield(fieldNum(InventLocation, InventLocationId));
        sysTableLookup.addLookupfield(fieldNum(InventLocation, InventSiteId));
        sysTableLookup.addLookupfield(fieldNum(InventLocation, name));
        sysTableLookup.parmQuery(query);
        sysTableLookup.performFormLookup();
    }");

            // warehouseModified
            this.addMethod(metaClass, "warehouseModified", @"
    /// <summary>
    " + comment + @"
    /// Use this method instead of form field control modified.
    /// </summary>
    public boolean warehouseModified(FormStringControl _control)
    {
        InventLocation  inventLocation;

        if (gInventLocationId.value())
        {
            inventLocation = InventLocation::find(gInventLocationId.value());

            if (!gInventSiteId.value())
            {
                gInventSiteId.value(inventLocation.InventSiteId);
            }
        }
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        #endregion

        #region Report
        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Contract class for Report
        /// </summary>
        public void run_Report_Contract(string classPrefix, string gComment)
        {
            string className = classPrefix + "Contract";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Contract class of the report
/// </summary>
[
    DataContractAttribute,
    SysOperationContractProcessingAttribute(classStr(" + classPrefix + @"UIBuilder)),
    SysOperationGroupAttribute('Order', ""@SYS106702"", '1')
]
public class " + className + @" implements SysOperationValidatable, SysOperationInitializable
{
    NoYes gChoose;
    TransDate gFromDate;
    TransDate gToDate;
    PurchId gPurchId;
    List gIdentifierType;
}";
            comment = this.updateComment(comment);

            // getRangeValueFromList
            this.addMethod(metaClass, "getRangeValueFromList", @"
    /// <summary>
    " + comment + @"
    /// List elements to str value
    /// </returns>
    public Range getRangeValueFromList(List _multSelectList)
    {
        Range           rangeValue;
        ListEnumerator  listEnumerator;

        if (_multSelectList && !_multSelectList.empty())
        {
            listEnumerator = _multSelectList.getEnumerator();

            while (listEnumerator.moveNext())
            {
                if (rangeValue)
                {
                    rangeValue += "", "" + listEnumerator.current();
                }
                else
                {
                    rangeValue += listEnumerator.current();
                }
            }
        }

        return rangeValue;
    }");

            // initialize
            this.addMethod(metaClass, "initialize", @"
    /// <summary>
    " + comment + @"
    /// Initializes data contract values.
    /// </summary>
    public void initialize()
    {
        //TODO: Initialize method
    }");

            // parmChoose
            this.addMethod(metaClass, "parmChoose", @"
    /// <summary>
    " + comment + @"
    /// Get or set the choose
    /// </summary>
    /// <param name=""_choose"">Set the paramether choose</param>
    /// <returns>Return the choose</returns>
    [
        DataMemberAttribute('Choose'),
        SysOperationLabelAttribute(literalstr('Choose')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('3')
    ]
    public NoYes parmChoose(NoYes _choose = gChoose)
    {
        gChoose = _choose;

        return gChoose;
    }");

            // parmFromDate
            this.addMethod(metaClass, "parmFromDate", @"
    /// <summary>
    " + comment + @"
    /// Get or set the _fromDate
    /// </summary>
    /// <param name=""_fromDate"">Set the paramether _fromDate</param>
    /// <returns>Return the _fromDate</returns>
    [
        DataMemberAttribute('FromDate'),
        SysOperationLabelAttribute(literalstr('From date')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('1')
    ]
    public TransDate parmFromDate(TransDate _fromDate = gFromDate)
    {
        gFromDate = _fromDate;

        return gFromDate;
    }");

            // parmIdentifierType
            this.addMethod(metaClass, "parmIdentifierType", @"
    /// <summary>
    " + comment + @"
    /// Gets or sets the gIdentifierType parameter.
    /// </summary>
    /// <param name=""_identifierType"">The value to set.</param>
    /// <returns>The value of the gIdentifierType parameter.</returns>
    [
        DataMemberAttribute('IdentifierType'),//TODO:Change the name, labels
        SysOperationLabelAttribute(literalStr(""@SYS107395"")),//TODO:
        SysOperationHelpTextAttribute(literalStr(""@SYS107403"")),//TODO:
        AifCollectionTypeAttribute('return', Types::String),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('5')
    ]
    public List parmIdentifierType(List _identifierType = gIdentifierType)
    {
        gIdentifierType = _identifierType;
        return gIdentifierType;
    }");

            // parmPurchId
            this.addMethod(metaClass, "parmPurchId", @"
    /// <summary>
    " + comment + @"
    /// Get or set the _purchId
    /// </summary>
    /// <param name=""_purchId"">Set the paramether _purchId</param>
    /// <returns>Return the _purchId</returns>
    [
        DataMemberAttribute('PurchId'),
        SysOperationLabelAttribute(literalstr('PurchId')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('4')
    ]
    public PurchId parmPurchId(PurchId _purchId = gPurchId)
    {
        gPurchId = _purchId;

        return gPurchId;
    }");

            // parmToDate
            this.addMethod(metaClass, "parmToDate", @"
    /// <summary>
    " + comment + @"
    /// Get or set the _toDate
    /// </summary>
    /// <param name=""_toDate"">Set the paramether _toDate</param>
    /// <returns>Return the _toDate</returns>
    [
        DataMemberAttribute('ToDate'),
        SysOperationLabelAttribute(literalstr('To date')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('2')
    ]
    public TransDate parmToDate(TransDate _toDate = gToDate)
    {
        gToDate = _toDate;

        return gToDate;
    }");

            // validate
            this.addMethod(metaClass, "validate", @"
    /// <summary>
    " + comment + @"
    /// Validate parameters
    /// </summary>
    /// <returns>
    /// Default return true
    /// </returns>
    /// <remarks>
    /// SSRS report demo Added by Parker Peng 1/20/2015
    /// </remarks>
    public boolean validate()
    {
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Controller class for Report
        /// </summary>
        public void run_Report_Controller(string classPrefix, string gComment)
        {
            string className = classPrefix + "Controller";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// The controller class of report
/// </summary>
public class " + className + @" extends SrsReportRunController
{
}";
            comment = this.updateComment(comment);

            // prePromptModifyContract
            this.addMethod(metaClass, "prePromptModifyContract", @"
    /// <summary>
    " + comment + @"
    ///  Over ride the prePromptModifyContract
    /// </summary>
    protected void prePromptModifyContract()
    {
        " + classPrefix + @"Contract    contract;

        contract    = this.parmReportContract().parmRdpContract() as " + classPrefix + @"Contract;
        // Add logic to set data before dialog shown
    }");

            // templateForm
            this.addMethod(metaClass, "templateForm", @"
    /// <summary>
    " + comment + @"
    /// Modify the template form
    /// </summary>
    /// <returns>Return the template form</returns>
    protected FormName templateForm()
    {
        FormName ret;
        
        if (useReportViewerForm)
        {
            return formStr(" + classPrefix + @"TemplateForm);
        }
        else
        {
            return formStr(" + classPrefix + @"TemplateForm);
        }

        ret = super();

        return ret;
    }");

            // main
            this.addMethod(metaClass, "main", @"
    /// <summary>
    " + comment + @"
    /// Entry point
    /// </summary>
    /// <param name=""_args"">The caller args</param>
    public static void main(Args _args)
    {
        " + className + @" controller = new " + className + @"();

        controller.parmReportName(ssrsReportStr(" + classPrefix + @", Report));
        controller.parmArgs(_args);
        controller.startOperation();
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Service class for Report
        /// </summary>
        public void run_Report_DP(string classPrefix, string gComment)
        {
            string className = classPrefix + "DP";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// The DP class of the report
/// </summary>
[
    SRSReportParameterAttribute(classStr(" + classPrefix + @"Contract))
]
public class " + className + @" extends SrsReportDataProviderPreProcessTempDB
{
    " + classPrefix + @"Tmp gReportTmp;
}";
            comment = this.updateComment(comment);

            // get temporary table
            this.addMethod(metaClass, "get" + classPrefix + "Tmp", @"
    /// <summary>
    " + comment + @"
    /// Get the temporary table
    /// </summary>
    /// <returns>Return the temporary table</returns>
    [
        SRSReportDataSetAttribute('" + classPrefix + @"Tmp')
    ]
    public " + classPrefix + @"Tmp get" + classPrefix + @"Tmp()
    {
        select gReportTmp;
        return gReportTmp;
    }");

            // processReport
            this.addMethod(metaClass, "processReport", @"
    /// <summary>
    " + comment + @"
    /// Process the report
    /// </summary>
    [SysEntryPointAttribute]
    public void processReport()
    {
        CompanyInfo     companyInfo = CompanyInfo::find();
        Query           query       = new Query(); // TODO: add your own query or sql statement
        QueryRun        queryRun    = new QueryRun(query);
        SalesTable      salesTable;
        SalesLine       salesLine;
        InventDim       inventDim;
        BarcodeCode39   barcodeCode = BarcodeCode39::construct();

        while (queryRun.next())
        {
            salesTable  = queryRun.get(tableNum(SalesTable));
            salesLine   = queryRun.get(tableNum(SalesLine));
            inventDim   = queryRun.get(tableNum(InventDim));

            if (queryRun.changed(tableNum(SalesTable)))
            {
                gReportTmp.clear();

                if (!gReportTmp.CompanyLogo)
                {
                    gReportTmp.CompanyLogo          = CompanyImage::findByRecord(companyInfo).Image;
                }
                gReportTmp.SalesId              = salesTable.SalesId;
                gReportTmp.SalesType            = salesTable.SalesType;
                gReportTmp.Address              = salesTable.deliveryAddress().Address;
                gReportTmp.ReceiptDateConfirmed = salesTable.ReceiptDateConfirmed;
                gReportTmp.SalesGroup           = salesTable.SalesGroup;
            }

            gReportTmp.ItemId       = salesLine.ItemId;

            barcodeCode.string(true, gReportTmp.ItemId);
            gReportTmp.ItemBarcode  = barcodeCode.barcodeStr();

            gReportTmp.SalesQty     = salesLine.SalesQty;
            gReportTmp.SalesPrice   = salesLine.SalesPrice;
            gReportTmp.insert();
        }
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create UIBuilder class for Report
        /// </summary>
        public void run_Report_UIBuilder(string classPrefix, string gComment)
        {
            string className = classPrefix + "UIBuilder";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Builds the UI for report
/// </summary>
public class " + className + @" extends SrsReportDataContractUIBuilder
{
    DialogField gPurchIdField;
    DialogField gIdentifierTypeField;
}";
            comment = this.updateComment(comment);

            // build
            this.addMethod(metaClass, "build", @"
    /// <summary>
    " + comment + @"
    /// Builds the <c>" + classPrefix + @"Contract</c> report parameters.
    /// </summary>
    public void build()
    {
        " + classPrefix + @"Contract    contract;
        DialogField             chooseField;

        super();

        contract                = this.dataContractObject() as " + classPrefix + @"Contract;
        gPurchIdField           = this.bindInfo().getDialogField(contract, methodStr(" + classPrefix + @"Contract, parmPurchId));
        chooseField             = this.bindInfo().getDialogField(contract, methodStr(" + classPrefix + @"Contract, parmChoose));
        gIdentifierTypeField    = this.bindInfo().getDialogField(contract, methodStr(" + classPrefix + @"Contract, parmIdentifierType));

        // controlling visibility
        if (gPurchIdField)
        {
            gPurchIdField.registerOverrideMethod(methodStr(FormStringControl, lookup),
                              methodStr(" + classPrefix + @"UIBuilder, purchIdLookup),
                              this);
        }

        if (chooseField)
        {
            chooseField.registerOverrideMethod(methodStr(FormCheckBoxControl, Modified),
                            methodStr(" + classPrefix + @"UIBuilder, chooseModified),
                            this);
        }
    }");

            // chooseModified
            this.addMethod(metaClass, "chooseModified", @"
    /// <summary>
    " + comment + @"
    /// Override the choose modified method
    /// </summary>
    /// <param name=""_control"">The choose checkbox control</param>
    /// <returns>Return the modify result</returns>
    public boolean chooseModified(FormCheckBoxControl _control)
    {
        if (_control.value() == NoYes::Yes)
        {
            gPurchIdField.visible(true);
            this.controller().parmDialogCaption(""Test001"");
        }
        else
        {
            gPurchIdField.visible(false);
            this.controller().parmDialogCaption(""Test002"");
        }

        return true;
    }");

            // identifierTypeLookup
            this.addMethod(metaClass, "identifierTypeLookup", @"
    /// <summary>
    " + comment + @"
    /// Identifier types lookup method
    /// </summary>
    public void identifierTypeLookup()
    {
        Query       query               = new Query(queryStr(MultiLookupQuery));
        QueryRun    queryRun            = new QueryRun(query);
        RefTableId  multiSelectTableNum = tableNum(MultiLookupTable);
        container   selectedFields      = [MultiLookupTable, fieldNum(MultiLookupTable, Name)];

        SysLookupMultiSelectCtrl::constructWithQueryRun(this.dialog().dialogForm().formRun(), gIdentifierTypeField.control(), queryRun, false, selectedFields);
    }");

            // identifierTypeModified
            this.addMethod(metaClass, "identifierTypeModified", @"
    /// <summary>
    " + comment + @"
    /// Override the choose modified method
    /// </summary>
    /// <param name=""_control"">The choose checkbox control</param>
    /// <returns>Return the modify result</returns>
    public boolean identifierTypeModified(FormStringControl _control)
    {
        if (_control.valueStr())
        {
            gPurchIdField.visible(true);
            gPurchIdField.allowEdit(true);
        }

        if (_control.modified())
        {
            info(""True"");
        }
        return true;
    }");

            // postRun
            this.addMethod(metaClass, "postRun", @"
    /// <summary>
    " + comment + @"
    /// Post run action
    /// </summary>
    public void postRun()
    {
        if (gIdentifierTypeField)
        {
            this.identifierTypeLookup();
            gIdentifierTypeField.registerOverrideMethod(methodStr(FormStringControl, Modified),
                        methodStr(" + classPrefix + @"UIBuilder, identifierTypeModified),
                        this);
        }
    }");

            // purchIdLookup
            this.addMethod(metaClass, "purchIdLookup", @"
    /// <summary>
    " + comment + @"
    /// Override the purchId lookup method
    /// </summary>
    /// <param name=""_control"">The purchId control</param>
    private void purchIdLookup(FormStringControl _control)
    {
        SysTableLookup sysTableLookup = SysTableLookup::newParameters(tableNum(PurchTable), _control);
        Query query = new Query();
        QueryBuildDataSource purchDS = query.addDataSource(tableNum(PurchTable));

        purchDS.addRange(fieldNum(PurchTable, PurchaseType)).value(queryValue(PurchaseType::Purch));

        sysTableLookup.addLookupfield(fieldNum(PurchTable, PurchId), true);
        sysTableLookup.addLookupfield(fieldNum(PurchTable, PurchaseType));
        sysTableLookup.parmQuery(query);
        sysTableLookup.performFormLookup();
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        #endregion

        #region ReportWithQuery
        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Contract class for Report with Query
        /// </summary>
        public void run_ReportWithQuery_Contract(string classPrefix, string gComment)
        {
            string className = classPrefix + "Contract";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Contract class of the report
/// </summary>
[
    DataContractAttribute,
    SysOperationContractProcessingAttribute(classStr(" + classPrefix + @"UIBuilder)),
    SysOperationGroupAttribute('Order', ""@SYS106702"", '1')
]
public class " + className + @" implements SysOperationValidatable, SysOperationInitializable
{
    NoYes gChoose;
    TransDate gFromDate;
    TransDate gToDate;
    PurchId gPurchId;
    List gIdentifierType;
}";
            comment = this.updateComment(comment);

            // getRangeValueFromList
            this.addMethod(metaClass, "getRangeValueFromList", @"
    /// <summary>
    " + comment + @"
    /// List elements to str value
    /// </returns>
    public Range getRangeValueFromList(List _multSelectList)
    {
        Range           rangeValue;
        ListEnumerator  listEnumerator;

        if (_multSelectList && !_multSelectList.empty())
        {
            listEnumerator = _multSelectList.getEnumerator();

            while (listEnumerator.moveNext())
            {
                if (rangeValue)
                {
                    rangeValue += "", "" + listEnumerator.current();
                }
                else
                {
                    rangeValue += listEnumerator.current();
                }
            }
        }

        return rangeValue;
    }");

            // initialize
            this.addMethod(metaClass, "initialize", @"
    /// <summary>
    " + comment + @"
    /// Initializes data contract values.
    /// </summary>
    public void initialize()
    {
        //TODO: Initialize method
    }");

            // parmChoose
            this.addMethod(metaClass, "parmChoose", @"
    /// <summary>
    " + comment + @"
    /// Get or set the choose
    /// </summary>
    /// <param name=""_choose"">Set the paramether choose</param>
    /// <returns>Return the choose</returns>
    [
        DataMemberAttribute('Choose'),
        SysOperationLabelAttribute(literalstr('Choose')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('3')
    ]
    public NoYes parmChoose(NoYes _choose = gChoose)
    {
        gChoose = _choose;

        return gChoose;
    }");

            // parmFromDate
            this.addMethod(metaClass, "parmFromDate", @"
    /// <summary>
    " + comment + @"
    /// Get or set the _fromDate
    /// </summary>
    /// <param name=""_fromDate"">Set the paramether _fromDate</param>
    /// <returns>Return the _fromDate</returns>
    [
        DataMemberAttribute('FromDate'),
        SysOperationLabelAttribute(literalstr('From date')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('1')
    ]
    public TransDate parmFromDate(TransDate _fromDate = gFromDate)
    {
        gFromDate = _fromDate;

        return gFromDate;
    }");

            // parmIdentifierType
            this.addMethod(metaClass, "parmIdentifierType", @"
    /// <summary>
    " + comment + @"
    /// Gets or sets the gIdentifierType parameter.
    /// </summary>
    /// <param name=""_identifierType"">The value to set.</param>
    /// <returns>The value of the gIdentifierType parameter.</returns>
    [
        DataMemberAttribute('IdentifierType'),//TODO:Change the name, labels
        SysOperationLabelAttribute(literalStr(""@SYS107395"")),//TODO:
        SysOperationHelpTextAttribute(literalStr(""@SYS107403"")),//TODO:
        AifCollectionTypeAttribute('return', Types::String),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('5')
    ]
    public List parmIdentifierType(List _identifierType = gIdentifierType)
    {
        gIdentifierType = _identifierType;
        return gIdentifierType;
    }");

            // parmPurchId
            this.addMethod(metaClass, "parmPurchId", @"
    /// <summary>
    " + comment + @"
    /// Get or set the _purchId
    /// </summary>
    /// <param name=""_purchId"">Set the paramether _purchId</param>
    /// <returns>Return the _purchId</returns>
    [
        DataMemberAttribute('PurchId'),
        SysOperationLabelAttribute(literalstr('PurchId')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('4')
    ]
    public PurchId parmPurchId(PurchId _purchId = gPurchId)
    {
        gPurchId = _purchId;

        return gPurchId;
    }");

            // parmToDate
            this.addMethod(metaClass, "parmToDate", @"
    /// <summary>
    " + comment + @"
    /// Get or set the _toDate
    /// </summary>
    /// <param name=""_toDate"">Set the paramether _toDate</param>
    /// <returns>Return the _toDate</returns>
    [
        DataMemberAttribute('ToDate'),
        SysOperationLabelAttribute(literalstr('To date')),
        SysOperationGroupMemberAttribute('Order'),
        SysOperationDisplayOrderAttribute('2')
    ]
    public TransDate parmToDate(TransDate _toDate = gToDate)
    {
        gToDate = _toDate;

        return gToDate;
    }");

            // validate
            this.addMethod(metaClass, "validate", @"
    /// <summary>
    " + comment + @"
    /// Validate parameters
    /// </summary>
    /// <returns>
    /// Default return true
    /// </returns>
    /// <remarks>
    /// SSRS report demo Added by Parker Peng 1/20/2015
    /// </remarks>
    public boolean validate()
    {
        return true;
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Controller class for Report with Query
        /// </summary>
        public void run_ReportWithQuery_Controller(string classPrefix, string gComment)
        {
            string className = classPrefix + "Controller";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// The controller class of report
/// </summary>
public class " + className + @" extends SrsReportRunController
{
}";
            comment = this.updateComment(comment);

            // prePromptModifyContract
            this.addMethod(metaClass, "prePromptModifyContract", @"
    /// <summary>
    " + comment + @"
    ///  Over ride the prePromptModifyContract
    /// </summary>
    protected void prePromptModifyContract()
    {
        " + classPrefix + @"Contract    contract;
        Query                   query;

        contract    = this.parmReportContract().parmRdpContract() as " + classPrefix + @"Contract;
        query       = this.parmReportContract().parmQueryContracts().lookup(this.getFirstQueryContractKey());
        if (query.dataSourceTable(tableNum(SalesTable)).findRange(fieldNum(SalesTable, SalesId)))
        {
            query.dataSourceTable(tableNum(SalesTable)).findRange(fieldNum(SalesTable, SalesId)).value(""Test001"");
        }
        else
        {
            query.dataSourceTable(tableNum(SalesTable)).addRange(fieldNum(SalesTable, SalesId)).value(""Test001"");
        }
    }");

            // templateForm
            this.addMethod(metaClass, "templateForm", @"
    /// <summary>
    " + comment + @"
    /// Modify the template form
    /// </summary>
    /// <returns>Return the template form</returns>
    protected FormName templateForm()
    {
        FormName ret;
        
        if (useReportViewerForm)
        {
            return formStr(" + classPrefix + @"TemplateForm);
        }
        else
        {
            return formStr(" + classPrefix + @"TemplateForm);
        }

        ret = super();

        return ret;
    }");

            // main
            this.addMethod(metaClass, "main", @"
    /// <summary>
    " + comment + @"
    /// Entry point
    /// </summary>
    /// <param name=""_args"">The caller args</param>
    public static void main(Args _args)
    {
        " + className + @" controller = new " + className + @"();

        controller.parmReportName(ssrsReportStr(" + classPrefix + @", Report));
        controller.parmArgs(_args);
        controller.startOperation();
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create Service class for Report with Query
        /// </summary>
        public void run_ReportWithQuery_DP(string classPrefix, string gComment)
        {
            string className = classPrefix + "DP";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// The DP class of the report
/// </summary>
[
    SRSReportQueryAttribute(queryStr(" + classPrefix + @"Query)),
    SRSReportParameterAttribute(classStr(" + classPrefix + @"Contract))
]
public class " + className + @" extends SrsReportDataProviderPreProcessTempDB
{
    " + classPrefix + @"Tmp gReportTmp;
}";
            comment = this.updateComment(comment);

            // get temporary table
            this.addMethod(metaClass, "get" + classPrefix + "Tmp", @"
    /// <summary>
    " + comment + @"
    /// Get the temporary table
    /// </summary>
    /// <returns>Return the temporary table</returns>
    [
        SRSReportDataSetAttribute('" + classPrefix + @"Tmp')
    ]
    public " + classPrefix + @"Tmp get" + classPrefix + @"Tmp()
    {
        select gReportTmp;
        return gReportTmp;
    }");

            // processReport
            this.addMethod(metaClass, "processReport", @"
    /// <summary>
    " + comment + @"
    /// Process the report
    /// </summary>
    [SysEntryPointAttribute]
    public void processReport()
    {
        CompanyInfo     companyInfo = CompanyInfo::find();
        Query           query       = this.parmQuery();
        QueryRun        queryRun    = new QueryRun(query);
        SalesTable      salesTable;
        SalesLine       salesLine;
        InventDim       inventDim;
        BarcodeCode39   barcodeCode = BarcodeCode39::construct();

        while (queryRun.next())
        {
            salesTable  = queryRun.get(tableNum(SalesTable));
            salesLine   = queryRun.get(tableNum(SalesLine));
            inventDim   = queryRun.get(tableNum(InventDim));

            if (queryRun.changed(tableNum(SalesTable)))
            {
                gReportTmp.clear();

                if (!gReportTmp.CompanyLogo)
                {
                    gReportTmp.CompanyLogo          = CompanyImage::findByRecord(companyInfo).Image;
                }
                gReportTmp.SalesId              = salesTable.SalesId;
                gReportTmp.SalesType            = salesTable.SalesType;
                gReportTmp.Address              = salesTable.deliveryAddress().Address;
                gReportTmp.ReceiptDateConfirmed = salesTable.ReceiptDateConfirmed;
                gReportTmp.SalesGroup           = salesTable.SalesGroup;
            }

            gReportTmp.ItemId       = salesLine.ItemId;

            barcodeCode.string(true, gReportTmp.ItemId);
            gReportTmp.ItemBarcode  = barcodeCode.barcodeStr();

            gReportTmp.SalesQty     = salesLine.SalesQty;
            gReportTmp.SalesPrice   = salesLine.SalesPrice;
            gReportTmp.insert();
        }
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        /// <summary>
        /// HM_D365_Addin_ClassDemoGenerator
        /// Byron Zhang - 09/27/2022
        /// Run the process to create UIBuilder class for Report with Query
        /// </summary>
        public void run_ReportWithQuery_UIBuilder(string classPrefix, string gComment)
        {
            string className = classPrefix + "UIBuilder";
            string comment = gComment;

            ModelSaveInfo saveInfo = new ModelSaveInfo(gModel);
            AxClass metaClass = new AxClass();

            metaClass.Name = className;
            metaClass.Declaration = @"/// <summary>
" + comment + @"
/// Builds the UI for report
/// </summary>
public class " + className + @" extends SrsReportDataContractUIBuilder
{
    DialogField gPurchIdField;
    DialogField gIdentifierTypeField;
}";
            comment = this.updateComment(comment);

            // build
            this.addMethod(metaClass, "build", @"
    /// <summary>
    " + comment + @"
    /// Builds the <c>" + classPrefix + @"Contract</c> report parameters.
    /// </summary>
    public void build()
    {
        " + classPrefix + @"Contract    contract;
        DialogField             chooseField;

        super();

        contract                = this.dataContractObject() as " + classPrefix + @"Contract;
        gPurchIdField           = this.bindInfo().getDialogField(contract, methodStr(" + classPrefix + @"Contract, parmPurchId));
        chooseField             = this.bindInfo().getDialogField(contract, methodStr(" + classPrefix + @"Contract, parmChoose));
        gIdentifierTypeField    = this.bindInfo().getDialogField(contract, methodStr(" + classPrefix + @"Contract, parmIdentifierType));

        // controlling visibility
        if (gPurchIdField)
        {
            gPurchIdField.registerOverrideMethod(methodStr(FormStringControl, lookup),
                              methodStr(" + classPrefix + @"UIBuilder, purchIdLookup),
                              this);
        }

        if (chooseField)
        {
            chooseField.registerOverrideMethod(methodStr(FormCheckBoxControl, Modified),
                            methodStr(" + classPrefix + @"UIBuilder, chooseModified),
                            this);
        }
    }");

            // chooseModified
            this.addMethod(metaClass, "chooseModified", @"
    /// <summary>
    " + comment + @"
    /// Override the choose modified method
    /// </summary>
    /// <param name=""_control"">The choose checkbox control</param>
    /// <returns>Return the modify result</returns>
    public boolean chooseModified(FormCheckBoxControl _control)
    {
        if (_control.value() == NoYes::Yes)
        {
            gPurchIdField.visible(true);
            this.controller().parmDialogCaption(""Test001"");
        }
        else
        {
            gPurchIdField.visible(false);
            this.controller().parmDialogCaption(""Test002"");
        }

        return true;
    }");

            // identifierTypeLookup
            this.addMethod(metaClass, "identifierTypeLookup", @"
    /// <summary>
    " + comment + @"
    /// Identifier types lookup method
    /// </summary>
    public void identifierTypeLookup()
    {
        Query       query               = new Query(queryStr(MultiLookupQuery));
        QueryRun    queryRun            = new QueryRun(query);
        RefTableId  multiSelectTableNum = tableNum(MultiLookupTable);
        container   selectedFields      = [MultiLookupTable, fieldNum(MultiLookupTable, Name)];

        SysLookupMultiSelectCtrl::constructWithQueryRun(this.dialog().dialogForm().formRun(), gIdentifierTypeField.control(), queryRun, false, selectedFields);
    }");

            // identifierTypeModified
            this.addMethod(metaClass, "identifierTypeModified", @"
    /// <summary>
    " + comment + @"
    /// Override the choose modified method
    /// </summary>
    /// <param name=""_control"">The choose checkbox control</param>
    /// <returns>Return the modify result</returns>
    public boolean identifierTypeModified(FormStringControl _control)
    {
        if (_control.valueStr())
        {
            gPurchIdField.visible(true);
            gPurchIdField.allowEdit(true);
        }

        if (_control.modified())
        {
            info(""True"");
        }
        return true;
    }");

            // postRun
            this.addMethod(metaClass, "postRun", @"
    /// <summary>
    " + comment + @"
    /// Post run action
    /// </summary>
    public void postRun()
    {
        if (gIdentifierTypeField)
        {
            this.identifierTypeLookup();
            gIdentifierTypeField.registerOverrideMethod(methodStr(FormStringControl, Modified),
                        methodStr(" + classPrefix + @"UIBuilder, identifierTypeModified),
                        this);
        }
    }");

            // purchIdLookup
            this.addMethod(metaClass, "purchIdLookup", @"
    /// <summary>
    " + comment + @"
    /// Override the purchId lookup method
    /// </summary>
    /// <param name=""_control"">The purchId control</param>
    private void purchIdLookup(FormStringControl _control)
    {
        SysTableLookup sysTableLookup = SysTableLookup::newParameters(tableNum(PurchTable), _control);
        Query query = new Query();
        QueryBuildDataSource purchDS = query.addDataSource(tableNum(PurchTable));

        purchDS.addRange(fieldNum(PurchTable, PurchaseType)).value(queryValue(PurchaseType::Purch));

        sysTableLookup.addLookupfield(fieldNum(PurchTable, PurchId), true);
        sysTableLookup.addLookupfield(fieldNum(PurchTable, PurchaseType));
        sysTableLookup.parmQuery(query);
        sysTableLookup.performFormLookup();
    }");

            if (gMetaModelService != null)
            {
                gMetaModelService.CreateClass(metaClass, saveInfo);
                gService.AddElementToActiveProject(metaClass);
            }
        }

        #endregion
    }
}
