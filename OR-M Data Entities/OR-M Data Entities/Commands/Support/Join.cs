using System;
using System.Reflection;

namespace OR_M_Data_Entities.Commands.Support
{
    public class Join
    {
        public Type ParentTableType { get; set; }

        public PropertyInfo ParentTableColumn { get; set; }

        public Type JoinTableType { get; set; }

        public PropertyInfo JoinTableColumn { get; set; }

        public JoinType Type { get; set; }

        public string GetJoinText()
        {
            var joinText = "{0}[{0}].[{1}] = [{2}].[{3}]";

            switch (Type)
            {
                case JoinType.Equi:
                    return "[{0}].[{1}] = [{2}].[{3}]";
                case JoinType.Inner:
                    return "INNER JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}]";
                case JoinType.Left:
                    return "LEFT JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}]";
                default:
                    return "";
            }
        }
    }
}
