using Microsoft.Dynamics.AX.Metadata.Core.MetaModel;

namespace HMT.Kernel
{
    public class AxTypeHelper
    {
        public static string getTypeStr(CompilerBaseType type, string typeName)
        {
            switch (type)
            {
                case CompilerBaseType.Date:
                    break;
                case CompilerBaseType.DateTime:
                    break;
                case CompilerBaseType.Container:
                    return "container";
                case CompilerBaseType.FormElementType:
                    break;
                case CompilerBaseType.Int32:
                    return "int";
                case CompilerBaseType.Int64:
                    return "int64";
                case CompilerBaseType.Real:
                    return "real";                
                case CompilerBaseType.String:
                    return "str";
                case CompilerBaseType.Time:
                    break;
                case CompilerBaseType.Void:
                    return "void";
                case CompilerBaseType.ExtendedDataType:
                case CompilerBaseType.Enum:
                case CompilerBaseType.Class:
                case CompilerBaseType.Record:
                    return typeName;
            }

            return "";
        }

        public static string getTableFieldTypeStr(string fieldType)
        {
            switch (fieldType)
            {
                case "AxTableFieldInt":
                    return "int";
                case "AxTableFieldReal":
                    return "real";
                case "AxTableFieldDate":
                    return "date";
                case "AxTableFieldTime":
                    return "time";
                //case "AxTableFieldEnum":
                //    return "AxTableFieldGuid";
                case "AxTableFieldContainer":
                case "AxTableFieldGuid":
                case "AxTableFieldInt64":
                    return "int64";
                case "AxTableFieldString":
                    return "str";
            }

            return "";
        }
    }
}
