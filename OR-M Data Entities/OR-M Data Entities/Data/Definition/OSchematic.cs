/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Schema;

namespace OR_M_Data_Entities.Data.Definition
{
    public class OSchematic
    {
        public OSchematic(Type type, Type actualType, string propertyName)
        {
            Type = type;
            ActualType = actualType;
            PropertyName = propertyName;
            PrimaryKeyNames = type.GetPrimaryKeyNames();
            LoadedCompositePrimaryKeys = new OSchematicLoadedKeys();
            Children = new List<OSchematic>();
        }

        // this will tell us if its a list or not
        public readonly Type ActualType;

        public readonly List<OSchematic> Children; 

        public readonly string[] PrimaryKeyNames;

        public OSchematicLoadedKeys LoadedCompositePrimaryKeys { get; private set; }

        public object ReferenceToCurrent { get; set; }

        /// <summary>
        /// used to identity Foreign Key because object can have Foreign Key with same type,
        /// but load different data.  IE - User CreatedBy, User EditedBy
        /// </summary>
        public readonly string PropertyName;

        public readonly Type Type;

        public void ClearRowReadCache()
        {
            LoadedCompositePrimaryKeys = new OSchematicLoadedKeys();

            var toClear = new List<OSchematic>();

            // do not keep reference to the original list
            toClear.AddRange(Children);

            for (var i = 0; i < toClear.Count; i++)
            {
                var child = toClear[i];

                if (child.Children.Count > 0) toClear.AddRange(child.Children);

                child.LoadedCompositePrimaryKeys = new OSchematicLoadedKeys();
            }
        }
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
        public OSchematicKey(int compositeKey, int[] ordinals)
        {
            CompositeKey = compositeKey;
            OrdinalCompositeHashCode = ordinals.Aggregate(0, (current, next) => current + next.GetHashCode());
        }

        public int CompositeKey { get; private set; }

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
