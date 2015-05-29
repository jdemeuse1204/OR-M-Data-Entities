using System;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class JoinPair
    {
        public bool HeirarchyContainsList { get; private set; }

        public Type ParentType { get; private set; }

        public Type ChildType { get; private set; }

        public string ComputedParentAlias { get; private set; }

        public string ComputedChildAlias { get; private set; }

        public string ChildPropertyName { get; private set; }

        public string ParentTableName { get; private set; }

        public string ChildTableName { get; private set; }

        public JoinPair(Type parentType, Type childType, bool heirarchyContainsList, string computedParentAlias,
            string computedChildAlias, string childPropertyName, string parentTableName, string childTableName)
        {
            ParentType = parentType;

            ChildType = childType;
            HeirarchyContainsList = heirarchyContainsList;
            ComputedParentAlias = computedParentAlias;
            ComputedChildAlias = computedChildAlias;
            ChildPropertyName = childPropertyName;
            ParentTableName = parentTableName;
            ChildTableName = childTableName;
        }
    }
}
