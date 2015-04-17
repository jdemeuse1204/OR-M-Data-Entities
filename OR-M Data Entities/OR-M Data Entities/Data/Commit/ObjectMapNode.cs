/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.Commit
{
    public sealed class ObjectMapNode : IEquatable<ObjectMapNode>
    {
        public ObjectMapNode()
        {
            Children = new List<ObjectMapNode>();
            CurrentKeyHashCodeList = new List<int>();
        }

        public ObjectMapNode(int parentKey) : this()
        {
            ParentKey = parentKey;
        }

        public int ParentKey { get; set; }

        // current hashcodes
        public List<int> CurrentKeyHashCodeList { get; set; }

        public List<ObjectMapNode> Children { get; set; }

        public bool Equals(ObjectMapNode other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return ParentKey == other.ParentKey;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            //Calculate the hash code for the product.
            return ParentKey.GetHashCode();
        }
    }
}
