/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Reflection;

namespace OR_M_Data_Entities.Data.Commit
{
    public sealed class PulledForeignKeyDetail : IEquatable<PulledForeignKeyDetail>
    {
        public PulledForeignKeyDetail(PropertyInfo property)
        {
            Type = property.PropertyType;
            ParentType = property.DeclaringType;
            PropertyName = property.Name;
        }

        public Type Type { get; set; }

        public Type ParentType { get; set; }

        public string PropertyName { get; set; }

        public bool Equals(PulledForeignKeyDetail other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Type == other.Type && 
                ParentType == other.ParentType && 
                PropertyName == other.PropertyName;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Type.GetHashCode();
            hash = (hash * 7) + ParentType.GetHashCode();
            hash = (hash * 7) + PropertyName.GetHashCode();

            return hash;
        }
    }
}
