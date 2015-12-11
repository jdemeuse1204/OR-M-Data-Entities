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
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses DataFetching functions to Save, Delete
    /// </summary>
    public abstract partial class DatabaseModifiable : DatabaseFetching
    {
        #region Events And Delegates

        public delegate void OnBeforeSaveHandler(object entity, UpdateType updateType);

        public event OnBeforeSaveHandler OnBeforeSave;

        public delegate void OnAfterSaveHandler(object entity, UpdateType actualUpdateType);

        public event OnAfterSaveHandler OnAfterSave;

        public delegate void OnSavingHandler(object entity);

        public event OnSavingHandler OnSaving;

        #endregion

        #region Constructor

        protected DatabaseModifiable(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }

        #endregion

        #region Properties

        private readonly string _transactionSqlBase = @"
DECLARE @1 VARCHAR(50) = CONVERT(varchar,GETDATE(),126);

BEGIN TRANSACTION @1;
	BEGIN TRY
			{0}
	END TRY
	BEGIN CATCH

		IF @@TRANCOUNT > 0
			BEGIN
				ROLLBACK TRANSACTION;
			END
			
			DECLARE @2 as varchar(max) = ERROR_MESSAGE() + '  Rollback performed, no data committed.',
					@3 as int = ERROR_SEVERITY(),
					@4 as int = ERROR_STATE();

			RAISERROR(@2,@3,@4);
			
	END CATCH

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION @1;
";

        #endregion

        #region Methods

        private string _createTransaction(string sql)
        {
            return string.Format(_transactionSqlBase, sql);
        }

        #endregion

        #region Save Methods

        public virtual bool SaveChanges<T>(T entity)
            where T : class
        {
            return Configuration.UseTransactions ? _saveChangesUsingTransactions(entity) : _saveChanges(entity);
        }
        #endregion

        #region Delete Methods

        public virtual bool Delete<T>(T entity) where T : class
        {
            var saves = new List<UpdateType>();
            var parent = new DeleteEntity(entity);

            // get all items to save and get them in order
            var referenceMap = EntityMapper.GetReferenceMap(parent, Configuration);

            // reverse the order to back them out of the database
            referenceMap.Reverse();

            for (var i = 0; i < referenceMap.Count; i++)
            {
                var reference = referenceMap[i];

                if (OnBeforeSave != null) OnBeforeSave(reference.Entity.Value, UpdateType.Delete);

                var builder = new SqlDeleteBuilder(reference.Entity);

                if (OnSaving != null) OnSaving(reference.Entity.Value);

                // execute the sql
                ExecuteReader(builder);

                // we return the deleted id's to check and see if anything was deleted
                var keyContainer = GetOutput();
                var actionTaken = keyContainer.Count > 0 ? UpdateType.Delete : UpdateType.RowNotFound;

                // add the save to the list so we can tell the user what the save action did
                saves.Add(actionTaken);

                // set the pristine state only if entity tracking is on
                if (reference.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(reference.Entity.Value);

                if (OnAfterSave != null) OnAfterSave(reference.Entity.Value, actionTaken);
            }

            return saves.Any(w => w == UpdateType.Delete);
        }

        #endregion

        #region Reference Mapping
        private class ReferenceMap : IEnumerable<Reference>
        {
            public Reference this[int i]
            {
                get { return _internal[i] as Reference; }
            }

            public int Count { get { return _internal.Count; } }

            private readonly List<object> _internal;

            public ReferenceMap()
            {
                _internal = new List<object>();
            }

            public void Add(ModificationEntity entity)
            {
                _internal.Add(new Reference(entity, _nextAlias()));
            }

            public void AddOneToManySaveReference(ForeignKeyAssociation association, object value)
            {
                var parentIndex = _indexOf(association.Parent);

                _insert(parentIndex + 1, value, association);

                // add the references
                var index = _indexOf(value);
                var oneToManychildIndex = _indexOf(association.Parent);
                var oneToManyChild = this[oneToManychildIndex];
                var oneToManyParent = this[index];
                var oneToOneForeignKeyAttribute = association.Property.GetCustomAttribute<ForeignKeyAttribute>();
                var oneToOneParentProperty = association.ChildType.GetProperty(oneToOneForeignKeyAttribute.ForeignKeyColumnName);
                var oneToOneChildProperty = association.ChildType.GetPrimaryKeys()[0];

                oneToManyParent.References.Add(new ReferenceNode(association.Parent, oneToManyChild.Alias, RelationshipType.OneToMany, new Link(oneToOneParentProperty, oneToOneChildProperty)));
            }

            public void AddOneToOneSaveReference(ForeignKeyAssociation association)
            {
                var oneToOneParentIndex = _indexOf(association.Parent);

                _insert(oneToOneParentIndex, association.Value, association);

                var oneToOneIndex = _indexOf(association.Parent);
                var childIndex = _indexOf(association.Value);
                var child = this[childIndex];
                var parent = this[oneToOneIndex];
                var oneToOneForeignKeyAttribute = association.Property.GetCustomAttribute<ForeignKeyAttribute>();
                var oneToOneParentProperty = association.ParentType.GetProperty(oneToOneForeignKeyAttribute.ForeignKeyColumnName);
                var oneToOneChildProperty = association.ChildType.GetPrimaryKeys()[0];

                parent.References.Add(new ReferenceNode(association.Value, child.Alias, RelationshipType.OneToOne, new Link(oneToOneParentProperty, oneToOneChildProperty)));
            }

            private int _indexOf(object entity)
            {
                return _internal.IndexOf(entity);
            }

            private void _insert(int index, object entity, ForeignKeyAssociation association)
            {
                _internal.Insert(index, new Reference(entity, _nextAlias(), association));
            }

            private string _nextAlias()
            {
                return string.Format("Tbl{0}", _internal.Count);
            }

            public void Reverse()
            {
                _internal.Reverse();
            }

            public IEnumerator<Reference> GetEnumerator()
            {
                return (dynamic)_internal.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class Link
        {
            public Link(PropertyInfo parentProperty, PropertyInfo childProperty)
            {
                ParentPropertyName = parentProperty.Name;
                ParentColumnName = parentProperty.GetColumnName();
                ChildPropertyName = childProperty.Name;
                ChildColumnName = childProperty.GetColumnName();
            }

            public readonly string ParentPropertyName;

            public readonly string ParentColumnName;

            public bool IsParentColumnRenamed
            {
                get { return ParentPropertyName != ParentColumnName; }
            }

            public readonly string ChildPropertyName;

            public readonly string ChildColumnName;

            public bool IsChildColumnRenamed
            {
                get { return ChildPropertyName != ChildColumnName; }
            }
        }

        private class ReferenceNode
        {
            public ReferenceNode(object value, string alias, RelationshipType relationshipType, Link link)
            {
                Value = value;
                Alias = alias;
                Relationship = relationshipType;
                Link = link;
            }

            public readonly object Value;

            public readonly string Alias;

            public readonly RelationshipType Relationship;

            public readonly Link Link;
        }

        public enum RelationshipType
        {
            OneToOne,
            OneToMany,
            Parent
        }

        private class Reference : IEquatable<Reference>
        {
            public Reference(ModificationEntity entity, string alias, ForeignKeyAssociation association = null)
            {
                Entity = entity;
                Alias = alias;
                Parent = association == null ? null : association.Parent;
                Property = association == null ? null : association.Property;
                References = new List<ReferenceNode>();
            }

            public Reference(object entity, string alias, ForeignKeyAssociation association = null) :
                this(new ModificationEntity(entity), alias, association)
            {
            }

            /// <summary>
            /// References needed for saving the object.  Refences are needed because they have values that are needed for insertion
            /// </summary>
            public readonly List<ReferenceNode> References;

            public readonly ModificationEntity Entity;

            public readonly string Alias;

            public readonly object Parent;

            public readonly PropertyInfo Property;

            public bool Equals(Reference other)
            {
                //Check whether the compared object is null.
                if (ReferenceEquals(other, null)) return false;

                //Check whether the compared object references the same data.
                return ReferenceEquals(this, other) || Equals(Entity, other.Entity);
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
                return Entity.GetHashCode();
            }
        }

        private static class EntityMapper
        {
            public static ReferenceMap GetReferenceMap(ModificationEntity entity, ConfigurationOptions options)
            {
                var result = new ReferenceMap();
                var useTransactions = options.UseTransactions;
                var entities = _getForeignKeys(entity.Value);

                entities.Insert(0, new ForeignKeyAssociation(null, entity.Value, null));

                for (var i = 0; i < entities.Count; i++)
                {
                    if (i == 0)
                    {
                        // is the base entity, will never have a parent, set it and continue to the next entity
                        result.Add(entity);
                        continue;
                    }

                    var e = entities[i];
                    var foreignKeyIsList = e.Property.IsList();
                    var tableInfo = new Table(e.Property.GetPropertyType());

                    if (e.Value == null && !useTransactions)
                    {
                        throw new SqlSaveException(string.Format("Foreign Key [{0}] cannot be null", tableInfo.TableNameOnly));
                    }

                    // skip lookup tables
                    if (tableInfo.IsLookupTable) continue;

                    // skip children(foreign keys) if option is set
                    if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.Skip)
                        continue;

                    if (e.Value == null)
                    {
                        if (foreignKeyIsList) continue;

                        var columnName = e.ChildType.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                        var isNullable = e.Parent.GetType().GetProperty(columnName).PropertyType.IsNullable();

                        // we can skip the foreign key if its nullable and one to one
                        if (isNullable) continue;

                        if (!useTransactions)
                        {
                            // database will take care of this if MARS is enabled
                            // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                            // maintaining the relationship
                            throw new SqlSaveException(string.Format("Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save", e.GetType().Name));
                        }
                    }

                    // Check for readonly attribute and see if we should throw an error
                    if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.ThrowException)
                    {
                        throw new SqlSaveException(string.Format("Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys", tableInfo.GetTableName()));
                    }

                    // doesnt have dependencies
                    if (foreignKeyIsList)
                    {
                        // e.Value can not be null, above code will catch it
                        foreach (var item in (e.Value as ICollection))
                        {
                            // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                            // the value property
                            result.AddOneToManySaveReference(e, item);

                            // add any dependencies
                            if (item.HasForeignKeys()) entities.AddRange(_getForeignKeys(item));
                        }
                    }
                    else
                    {
                        // must be saved before the parent
                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        result.AddOneToOneSaveReference(e);

                        // add any dependencies
                        if (e.Value.HasForeignKeys()) entities.AddRange(_getForeignKeys(e.Value));
                    }
                }

                return result;
            }
        }
        #endregion

        #region Methods

        private static List<ForeignKeyAssociation> _getForeignKeys(object entity)
        {
            return entity.GetForeignKeys().OrderBy(w => w.PropertyType.IsList()).Select(w => new ForeignKeyAssociation(entity, w.GetValue(entity), w)).ToList();
        }

        private class ForeignKeyAssociation
        {
            public ForeignKeyAssociation(object parent, object value, PropertyInfo property)
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

        #region shared
        private abstract class SqlSecureExecutable
        {
            #region Fields
            private readonly List<SqlSecureQueryParameter> _parameters;
            #endregion

            #region Constructor
            protected SqlSecureExecutable()
            {
                _parameters = new List<SqlSecureQueryParameter>();
            }

            protected SqlSecureExecutable(List<SqlSecureQueryParameter> parameters)
            {
                _parameters = parameters;
            }
            #endregion

            #region Methods
            // key where the data will be insert into the secure command
            private string _getNextKey()
            {
                return string.Format("@DATA{0}", _parameters.Count);
            }

            protected string AddParameter(ModificationItem item, object value)
            {
                return _addParameter(item, value, false);
            }

            protected string AddPristineParameter(ModificationItem item, object value)
            {
                return _addParameter(item, value, true);
            }

            private string _addParameter(ModificationItem item, object value, bool addPristineParameter)
            {
                var parameterKey = _getNextKey();

                _parameters.Add(new SqlSecureQueryParameter
                {
                    Key = parameterKey,
                    DbColumnName = addPristineParameter ? string.Format("Pristine{0}", item.DatabaseColumnName) : item.DatabaseColumnName,
                    TableName = item.GetTableName(),
                    ForeignKeyPropertyName = item.GetTableName(),
                    Value = item.TranslateDataType ? new SqlSecureObject(value, item.DbTranslationType) : new SqlSecureObject(value)
                });

                return parameterKey;
            }

            public void InsertParameters(SqlCommand cmd)
            {
                foreach (var item in _parameters)
                {
                    cmd.Parameters.Add(cmd.CreateParameter()).ParameterName = item.Key;
                    cmd.Parameters[item.Key].Value = item.Value.Value;

                    if (item.Value.TranslateDataType)
                    {
                        cmd.Parameters[item.Key].SqlDbType = item.Value.DbTranslationType;
                    }
                }
            }

            protected string FindParameterKey(string dbColumnName)
            {
                var parameter = _parameters.FirstOrDefault(w => w.DbColumnName == dbColumnName);

                return parameter != null ? parameter.Key : null;
            }
            #endregion
        }
        #endregion
    }
}
