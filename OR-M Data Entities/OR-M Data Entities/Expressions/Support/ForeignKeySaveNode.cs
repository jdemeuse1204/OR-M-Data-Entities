using System;
using System.Reflection;

namespace OR_M_Data_Entities.Expressions.Support
{
    public class ForeignKeySaveNode : IEquatable<ForeignKeySaveNode>
    {
        public ForeignKeySaveNode(PropertyInfo property, object value, object parent)
        {
            Property = property;
            Value = value;
            Parent = parent;
        }

        public PropertyInfo Property { get; set; }

        public object Value { get; set; }

        public object Parent { get; set; }

        public bool Equals(ForeignKeySaveNode other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Value == other.Value;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            //Calculate the hash code for the product.
            return Value.GetHashCode();
        }
    }
}
