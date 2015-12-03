﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Data.Execution
{
    public sealed class ForeignKeySaveNode : IEquatable<ForeignKeySaveNode>
    {
        // compare tablename

        public ForeignKeySaveNode(PropertyInfo property, object value, object parent)
        {
            Property = property;
            Parent = parent;
            Entity = new Entity(value);
        }

        public ForeignKeySaveNode(PropertyInfo property, Entity entity, object parent)
        {
            Property = property;
            Parent = parent;
            Entity = entity;
        }

        public PropertyInfo Property { get; set; }

        public object Parent { get; set; }

        public readonly Entity Entity;

        public bool Equals(ForeignKeySaveNode other)
        {
            //Check whether the compared object is null.
            if (ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Entity == other.Entity;
        }

        public override bool Equals(object obj)
        {
            return Entity.Equals(obj);
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            //Calculate the hash code for the product.
            return Entity.Value.GetHashCode();
        }
    }
}
