﻿/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Reflection;

namespace OR_M_Data_Entities.Data.Execution
{
    public sealed class ForeignKeySaveNode : IEquatable<ForeignKeySaveNode>
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