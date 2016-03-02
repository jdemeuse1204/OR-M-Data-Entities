/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Linq;

namespace OR_M_Data_Entities.Data.Loading
{
    // This class is used to identify the composite key of a record. 
    // Once the composite key is identified, we can then use it when loading our 
    // object from the database to determine if it was loaded already.  Composite key
    // is a string so we can fit very large objects/numbers/etc into string format.  
    // This way we do not have to worry what the keys look like
    public sealed class CompositeKey : IEquatable<CompositeKey>
    {
        public CompositeKey(string compositeKey, int[] ordinals)
        {
            Value = compositeKey;
            OrdinalCompositeHashCode = ordinals.Aggregate(0, (current, next) => current + next.GetHashCode());
        }

        public readonly string Value;

        public readonly int OrdinalCompositeHashCode;

        public bool Equals(CompositeKey other)
        {
            //Check whether the compared object is null.
            if (ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Value == other.Value &&
                   OrdinalCompositeHashCode == other.OrdinalCompositeHashCode;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            var hashKey = Value.GetHashCode();

            var hashOrdinals = OrdinalCompositeHashCode.GetHashCode();

            return hashKey ^ hashOrdinals;
        }
    }
}
