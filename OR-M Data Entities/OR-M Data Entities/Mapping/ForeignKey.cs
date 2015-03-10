using System;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ForeignKey : Attribute
    {
        // SearchableKeyType needed for quick lookup in iterator
        public ForeignKey(Type parentTableType, string parentPropertyName)
        {
            ParentTableType = parentTableType;
            ParentPropertyName = parentPropertyName;
        }

        public Type ParentTableType { get; private set; }
        public string ParentPropertyName { get; private set; }
    }
}
