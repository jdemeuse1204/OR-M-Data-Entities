/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
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

        OSchematicLoadedKeys LoadedCompositePrimaryKeys { get; }

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

    public class OSchematicLoadedKeys
    {
        private readonly List<OSchematicKey> _internal;

        public OSchematicLoadedKeys()
        {
            _internal = new List<OSchematicKey>();
        }

        public void Add(OSchematicKey key)
        {
            _internal.Add(key);
        }

        public bool Contains(OSchematicKey key)
        {
            return _internal.Contains(key);
        }
    }

    public class OSchematicKey : IEquatable<OSchematicKey>
    {
        public OSchematicKey(long compositeKey, int[] ordinals)
        {
            CompositeKey = compositeKey;
            OrdinalCompositeHashCode = ordinals.Aggregate(0, (current, next) => current + next.GetHashCode());
        }

        public long CompositeKey { get; private set; }

        public int OrdinalCompositeHashCode { get; private set; }

        public bool Equals(OSchematicKey other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return CompositeKey == other.CompositeKey &&
                   OrdinalCompositeHashCode == other.OrdinalCompositeHashCode;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            var hashKey = CompositeKey.GetHashCode();

            var hashOrdinals = OrdinalCompositeHashCode.GetHashCode();

            return hashKey ^ hashOrdinals;
        }
    }
}
