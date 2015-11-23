/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition
{
    public static class DatabaseEntity
    {
        public static UpdateType GetState(object entity)
        {
            return GetState(entity, entity.GetPrimaryKeys());
        }

        public static UpdateType GetState(object entity, List<PropertyInfo> primaryKeys)
        {
            var areAnyPkGenerationOptionsNone = false;

            var columns =
                entity.GetType().GetProperties().Where(w => !w.IsPrimaryKey()).ToList();

            var entityTrackable = entity as EntityStateTrackable;

            // make sure the user is not trying to update an IDENTITY column, these cannot be updated
            foreach (
                var column in
                    columns.Where(
                        w =>
                            w.GetCustomAttribute<DbGenerationOptionAttribute>() != null &&
                            w.GetCustomAttribute<DbGenerationOptionAttribute>().Option ==
                            DbGenerationOption.IdentitySpecification)
                        .Where(
                            column =>
                                entityTrackable != null &&
                                EntityStateAnalyzer.HasColumnChanged(entityTrackable, column.Name)))
            {
                throw new SqlSaveException(string.Format("Cannot update value if IDENTITY column.  Column: {0}",
                    column.Name));
            }

            for (var i = 0; i < primaryKeys.Count; i++)
            {
                var key = primaryKeys[i];
                var pkValue = key.GetValue(entity);
                var generationOption = key.GetGenerationOption();
                var isUpdating = false;
                var pkValueTypeString = "";
                var pkValueType = "";

                if (generationOption == DbGenerationOption.None) areAnyPkGenerationOptionsNone = true;

                if (generationOption == DbGenerationOption.DbDefault)
                {
                    throw new SqlSaveException("Cannot use DbGenerationOption of DbDefault on a primary key");
                }

                switch (pkValue.GetType().Name.ToUpper())
                {
                    case "INT16":
                        isUpdating = Convert.ToInt16(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT16";
                        break;
                    case "INT32":
                        isUpdating = Convert.ToInt32(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT32";
                        break;
                    case "INT64":
                        isUpdating = Convert.ToInt64(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT64";
                        break;
                    case "GUID":
                        isUpdating = (Guid)pkValue != Guid.Empty;
                        pkValueTypeString = "zero";
                        pkValueType = "INT16";
                        break;
                    case "STRING":
                        isUpdating = !string.IsNullOrWhiteSpace(pkValue.ToString());
                        pkValueTypeString = "null/blank";
                        pkValueType = "STRING";
                        break;
                }

                // break because we are already updating, do not want to set to false
                if (!isUpdating)
                {
                    if (generationOption == DbGenerationOption.None)
                    {
                        // if the db generation option is none and there is no pk value this is an error because the db doesnt generate the pk
                        throw new SqlSaveException(string.Format(
                            "Primary Key cannot be {1} for {2} when DbGenerationOption is set to None.  Primary Key Name: {0}", key.Name,
                            pkValueTypeString, pkValueType));
                    }
                    continue;
                }

                // If we have only primary keys we need to perform a try insert and see if we can try to insert our data.
                // if we have any Pk's with a DbGenerationOption of None we need to first see if a record exists for the pks, 
                // if so we need to perform an update, otherwise we perform an insert
                return entity.HasPrimaryKeysOnly()
                    ? UpdateType.TryInsert
                    : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;
            }

            return UpdateType.Insert;
        }

        public static void SetPropertyValue(object entity, string propertyName, object value)
        {
            var found = entity.GetType().GetProperty(propertyName);

            if (found == null)
            {
                return;
            }

            var propertyType = found.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (value == null)
                {
                    found.SetValue(entity, null, null);
                    return;
                }

                //Get the underlying type property instead of the nullable generic
                propertyType = new System.ComponentModel.NullableConverter(found.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            found.SetValue(
                entity,
                propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType),
                null);
        }

        public static void SetPropertyValue(object entity, PropertyInfo property, object value)
        {
            var propertyType = property.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (value == null)
                {
                    property.SetValue(entity, null, null);
                    return;
                }

                //Get the underlying type property instead of the nullable generic
                propertyType = new System.ComponentModel.NullableConverter(property.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            property.SetValue(
                entity,
                propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType),
                null);
        }

        public static void SetPropertyValue(object parent, object child, string propertyNameToSet)
        {
            if (parent == null) return;

            var foreignKeyProperty =
                parent.GetForeignKeys()
                    .First(
                        w =>
                            (w.PropertyType.IsList()
                                ? w.PropertyType.GetGenericArguments()[0]
                                : w.PropertyType) == child.GetType() &&
                            w.Name == propertyNameToSet);

            var foreignKeyAttribute = foreignKeyProperty.GetCustomAttribute<ForeignKeyAttribute>();

            if (foreignKeyProperty.PropertyType.IsList())
            {
                var parentPrimaryKey = parent.GetPrimaryKeys().First();
                var value = parent.GetType().GetProperty(parentPrimaryKey.Name).GetValue(parent);

                SetPropertyValue(child, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
            else
            {
                var childPrimaryKey = child.GetPrimaryKeys().First();
                var value = child.GetType().GetProperty(childPrimaryKey.Name).GetValue(child);

                SetPropertyValue(parent, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
        }

        public static void GetSaveOrder<T>(T entity,
            List<ForeignKeySaveNode> savableObjects, bool isMARSEnabled)
            where T : class
        {
            var entities = _getForeignKeys(entity);

            entities.Insert(0, new ParentChildPair(null, entity, null));

            for (var i = 0; i < entities.Count; i++)
            {
                if (i == 0)
                {
                    // is the base entity, will never have a parent, set it and continue to the next entity
                    savableObjects.Add(new ForeignKeySaveNode(null, entity, null));
                    continue;
                }

                var e = entities[i];
                int index;
                var foreignKeyIsList = e.Property.IsList();
                var tableInfo = new TableInfo(e.Value.GetTypeListCheck());

                // skip lookup tables
                if (tableInfo.IsLookupTable) continue;

                // skip children(foreign keys) if option is set
                if (tableInfo.IsReadOnly && 
                    tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.Skip) continue;

                if (e.Value == null)
                {
                    if (foreignKeyIsList) continue;

                    var columnName = e.ChildType.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                    var isNullable = entity.GetType().GetProperty(columnName).PropertyType.IsNullable();

                    // we can skip the foreign key if its nullable and one to one
                    if (isNullable) continue;

                    if (!isMARSEnabled)
                    {
                        // database will take care of this if MARS is enabled
                        // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                        // maintaining the relationship
                        throw new SqlSaveException(
                            string.Format(
                                "Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save",
                                e.GetType().Name));
                    }
                }

                // Check for readonly attribute and see if we should throw an error
                if (tableInfo.IsReadOnly &&
                    tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.ThrowException)
                {
                    throw new SqlSaveException(string.Format(
                        "Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys",
                        entity.GetTableName()));
                }

                // doesnt have dependencies
                if (foreignKeyIsList)
                {
                    // e.Value can not be null, above code will catch it
                    foreach (var item in (e.Value as ICollection))
                    {
                        // make sure there are no saving issues only if MARS is disabled
                        if (!isMARSEnabled) GetState(item);

                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = savableObjects.IndexOf(new ForeignKeySaveNode(null, e.Parent, null));

                        savableObjects.Insert(index + 1, new ForeignKeySaveNode(e.Property, item, e.Parent));

                        if (item.HasForeignKeys()) entities.AddRange(_getForeignKeys(item));
                    }
                }
                else
                {
                    // make sure there are no saving issues
                    if (!isMARSEnabled) GetState(e.Value);

                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = savableObjects.IndexOf(new ForeignKeySaveNode(null, e.Parent, null));

                    savableObjects.Insert(index, new ForeignKeySaveNode(e.Property, e.Value, e.Parent));

                    // has dependencies
                    if (e.Value.HasForeignKeys()) entities.AddRange(_getForeignKeys(e.Value));
                }
            }
        }

        private static List<ParentChildPair> _getForeignKeys(object entity)
        {
            return entity.GetForeignKeys()
                .OrderBy(w => w.PropertyType.IsList())
                .Select(w => new ParentChildPair(entity, w.GetValue(entity), w))
                .ToList();
        }

        #region helpers
        class ParentChildPair
        {
            public ParentChildPair(object parent, object value, PropertyInfo property)
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
        #endregion
    }
}
