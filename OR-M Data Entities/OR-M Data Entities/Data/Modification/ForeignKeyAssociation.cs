using System;
using System.Reflection;

namespace OR_M_Data_Entities.Data.Modification
{
    public class ForeignKeyAssociation
    {
        public ForeignKeyAssociation(object parent, object value, PropertyInfo property)
        {
            Parent = parent;
            Value = value;
            Property = property;
        }

        public object Parent { get; private set; }

        public PropertyInfo Property { get; private set; }

        public Type ParentType
        {
            get { return Parent == null ? null : Parent.GetTypeListCheck(); }
        }

        public object Value { get; private set; }

        public Type ChildType
        {
            get { return Value == null ? null : Value.GetTypeListCheck(); }
        }
    }
}
