using System;
using System.Reflection;

namespace OR_M_Data_Entities.Data.Definition
{
    public class JoinColumnPair
    {
        public JoinType JoinType { get; set; }

        public PropertyInfo ChildColumn { get; set; }

        public PropertyInfo ParentColumn { get; set; }

        public string JoinPropertyName { get; set; }

        public Type FromType { get; set; }
    }
}
