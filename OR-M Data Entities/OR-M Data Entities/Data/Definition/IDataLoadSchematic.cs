/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IDataLoadSchematic
    {
        HashSet<IDataLoadSchematic> Children { get; }

        IDataLoadSchematic Parent { get; }

        Type ActualType { get; }

        string[] PrimaryKeyNames { get; }

        LoadedCompositeKeys LoadedCompositePrimaryKeys { get; }

        object ReferenceToCurrent { get; set; }

        IMappedTable MappedTable { get; }

        /// <summary>
        /// used to identity Foreign Key because object can have Foreign Key with same type,
        /// but load different data.  IE - User CreatedBy, User EditedBy
        /// </summary>
        string PropertyName { get; }

        Type Type { get; }

        void ClearRowReadCache();

        void ClearLoadedCompositePrimaryKeys();
    }

    public class LoadedCompositeKeys
    {
        private readonly HashSet<CompositeKey> _internal;

        public LoadedCompositeKeys()
        {
            _internal = new HashSet<CompositeKey>();
        }

        public void Add(CompositeKey key)
        {
            _internal.Add(key);
        }

        public bool Contains(CompositeKey key)
        {
            return _internal.Contains(key);
        }
    }

    public class CompositeKey : IEquatable<CompositeKey>
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
