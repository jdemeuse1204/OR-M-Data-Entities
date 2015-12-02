using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition
{
    /// <summary>
    /// Class to interact with an entity (table)
    /// </summary>
    public class Entity : Table
    {
        #region Properties and Fields
        private readonly object _entity;

        public bool IsEntityStateTrackingOn
        {
            get { return _entity != null && EntityTrackable != null; }
        }

        protected EntityStateTrackable EntityTrackable
        {
            get { return _entity as EntityStateTrackable; }
        }

        private List<PropertyInfo> _properties;

        protected List<PropertyInfo> Properties
        {
            get
            {
                return _properties ??
                       (_properties = (_entity == null ? new List<PropertyInfo>() : TableType.GetProperties().ToList()));
            }
        }
        #endregion

        #region Constructor
        public Entity(object entity)
            : base(entity)
        {
            if (entity == null) throw new ArgumentNullException("Entity");

            _entity = entity;
        }
        #endregion

        #region Primary Key Methods
        public List<PropertyInfo> GetPrimaryKeys()
        {
            return _getPrimaryKeys(TableType);
        }

        public static List<PropertyInfo> GetPrimaryKeys(object entity)
        {
            return _getPrimaryKeys(entity.GetType());
        }

        private static List<PropertyInfo> _getPrimaryKeys(Type type)
        {
            var keyList = type.GetProperties().Where(w =>
                (w.GetCustomAttributes<SearchablePrimaryKeyAttribute>() != null
                 && w.GetCustomAttributes<SearchablePrimaryKeyAttribute>().Any(x => x.IsPrimaryKey))
                || (w.Name.ToUpper() == "ID")).ToList();

            if (keyList.Count != 0) return keyList;

            throw new Exception(string.Format("Cannot find PrimaryKey(s) for type of {0}", type.Name));
        }

        public static bool IsPrimaryKey(MemberInfo column)
        {
            return column.Name.ToUpper() == "ID"
                || GetColumnName(column).ToUpper() == "ID"
                || column.GetCustomAttribute<KeyAttribute>() != null;
        }

        public bool IsPrimaryKey(string columnName)
        {
            return columnName.ToUpper() == "ID" ||
                   TableType.GetProperty(columnName).GetCustomAttribute<KeyAttribute>() != null;
        }

        public static bool HasPrimaryKeysOnly(object entity)
        {
            return _hasPrimaryKeysOnly(entity.GetType().GetProperties());
        }

        private static bool _hasPrimaryKeysOnly(IReadOnlyList<PropertyInfo> properties)
        {
            return properties.Count(IsColumn) == properties.Count(IsPrimaryKey);
        }

        public bool HasPrimaryKeysOnly()
        {
            return _hasPrimaryKeysOnly(Properties);
        }

        public static DbGenerationOption GetGenerationOption(PropertyInfo column)
        {
            var dbGenerationColumn = column.GetCustomAttribute<DbGenerationOptionAttribute>();
            return dbGenerationColumn == null ? DbGenerationOption.IdentitySpecification : dbGenerationColumn.Option;
        }
        #endregion

        #region Column Methods
        public static string GetColumnName(MemberInfo column)
        {
            var columnAttribute = column.GetCustomAttribute<ColumnAttribute>();

            return columnAttribute == null ? column.Name : columnAttribute.Name;
        }

        public string GetColumnName(string propertyName)
        {
            var property = Properties.FirstOrDefault(w => w.Name == propertyName);

            // property will be in list only if it has a custom attribute
            if (property == null) return propertyName;
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            return columnAttribute == null ? propertyName : columnAttribute.Name;
        }

        public List<PropertyInfo> GetColumns()
        {
            return _getColumns(Properties);
        }

        private static List<PropertyInfo> _getColumns(IEnumerable<PropertyInfo> properties)
        {
            return
                properties.Where(
                    w =>
                        w.GetCustomAttribute<UnmappedAttribute>() == null &&
                        w.GetCustomAttribute<AutoLoadKeyAttribute>() == null).ToList();
        }

        public static List<PropertyInfo> GetColumns(object entity)
        {
            return _getColumns(entity.GetType().GetProperties());
        }

        public static bool IsColumn(PropertyInfo info)
        {
            var attributes = info.GetCustomAttributes();

            var isNonSelectable = attributes.Any(w => w is NonSelectableAttribute);
            var isPrimaryKey = attributes.Any(w => w is SearchablePrimaryKeyAttribute);
            var hasAttributes = attributes != null && attributes.Any();

            return (hasAttributes && (isPrimaryKey || !isNonSelectable)) || !hasAttributes;
        }
        #endregion

        #region Foreign Key Methods
        public bool HasForeignKeys()
        {
            return _hasForeignKeys(Properties);
        }

        private static bool _hasForeignKeys(IEnumerable<PropertyInfo> properties)
        {
            return properties.Any(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null);
        }

        public static bool HasForeignKeys(object entity)
        {
            return _hasForeignKeys(entity.GetType().GetProperties());
        }

        public List<JoinColumnPair> GetAllForeignKeysAndPseudoKeys(Guid expressionQueryId, string viewId)
        {
            var autoLoadProperties = string.IsNullOrWhiteSpace(viewId)
                ? TableType.GetProperties().Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null)
                : TableType.GetProperties()
                    .Where(
                        w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null &&
                            w.GetPropertyType().GetCustomAttribute<ViewAttribute>() != null &&
                             w.GetPropertyType().GetCustomAttribute<ViewAttribute>().ViewIds.Contains(viewId));

            return (from property in autoLoadProperties
                    let fkAttribute = property.GetCustomAttribute<ForeignKeyAttribute>()
                    let pskAttribute = property.GetCustomAttribute<PseudoKeyAttribute>()
                    select new JoinColumnPair
                    {
                        ChildColumn =
                            new PartialColumn(expressionQueryId, property.GetPropertyType(),
                                fkAttribute != null
                                    ? property.PropertyType.IsList()
                                        ? fkAttribute.ForeignKeyColumnName
                                        : _getPrimaryKeys(property.PropertyType).First().Name
                                    : pskAttribute.ChildTableColumnName),
                        ParentColumn =
                            new PartialColumn(expressionQueryId, TableType,
                                fkAttribute != null
                                    ? property.PropertyType.IsList()
                                        ? _getPrimaryKeys(TableType).First().Name
                                        : fkAttribute.ForeignKeyColumnName
                                    : pskAttribute.ParentTableColumnName),
                        JoinType =
                            property.PropertyType.IsList()
                                ? JoinType.Left
                                : TableType.GetProperty(fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ParentTableColumnName).PropertyType.IsNullable()
                                    ? JoinType.Left
                                    : JoinType.Inner,
                        JoinPropertyName = property.Name,
                        FromType = property.PropertyType
                    }).ToList();
        }


        #endregion

        #region Methods
        /// <summary>
        /// make sure the user is not trying to update an IDENTITY column, these cannot be updated
        /// </summary>
        /// <param name="entityStateTrackable"></param>
        /// <param name="columns"></param>
        private static void _checkIdentityUpdates(EntityStateTrackable entityStateTrackable,
            IReadOnlyList<PropertyInfo> columns)
        {
            foreach (
                var column in
                    columns.Where(
                        w =>
                            w.GetCustomAttribute<DbGenerationOptionAttribute>() != null &&
                            w.GetCustomAttribute<DbGenerationOptionAttribute>().Option ==
                            DbGenerationOption.IdentitySpecification)
                        .Where(
                            column =>
                                entityStateTrackable != null &&
                                EntityStateAnalyzer.HasColumnChanged(entityStateTrackable, column.Name)))
            {
                throw new SqlSaveException(string.Format("Cannot update value if IDENTITY column.  Column: {0}",
                    column.Name));
            }
        }

        public UpdateType GetUpdateType()
        {
            var primaryKeys = GetPrimaryKeys();
            var columns = Properties.Where(w => !IsPrimaryKey(w)).ToList();
            var hasPrimaryKeysOnly = HasPrimaryKeysOnly();
            var areAnyPkGenerationOptionsNone = false;

            _checkIdentityUpdates(EntityTrackable, columns);

            for (var i = 0; i < primaryKeys.Count; i++)
            {
                var key = primaryKeys[i];
                var pkValue = key.GetValue(_entity);
                var generationOption = GetGenerationOption(key);
                var isUpdating = false;
                var pkValueTypeString = "";
                var pkValueType = "";

                if (generationOption == DbGenerationOption.None) areAnyPkGenerationOptionsNone = true;

                if (generationOption == DbGenerationOption.DbDefault)
                {
                    throw new SqlSaveException("Cannot use DbGenerationOption of DbDefault on a primary key");
                }

                // SET CONFIGURATION FOR ZERO/NOT UPDATED VALUES
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
                return hasPrimaryKeysOnly
                    ? UpdateType.TryInsert
                    : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;
            }

            return UpdateType.Insert;
        }

        public List<ForeignKeySaveNode> GetSaveOrder(bool useTransactions)
        {
            var result = new List<ForeignKeySaveNode>();

            var entities = _getForeignKeys(_entity);

            entities.Insert(0, new ParentChildPair(null, _entity, null));

            for (var i = 0; i < entities.Count; i++)
            {
                if (i == 0)
                {
                    // is the base entity, will never have a parent, set it and continue to the next entity
                    result.Add(new ForeignKeySaveNode(null, _entity, null));
                    continue;
                }

                var e = entities[i];
                int index;
                var foreignKeyIsList = e.Property.IsList();
                var tableInfo = new Table(e.Property.GetPropertyType());

                if (e.Value == null && !useTransactions)
                {
                    throw new SqlSaveException(string.Format("Foreign Key {0} cannot be null", tableInfo.TableNameOnly));
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
                    var isNullable = _entity.GetType().GetProperty(columnName).PropertyType.IsNullable();

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
                        // make sure there are no saving issues only if MARS is disabled
                        if (!useTransactions)
                        {
                            var entity = new Entity(item);
                            entity.GetUpdateType();// analyzes the entity for saving issues
                        }

                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = result.IndexOf(new ForeignKeySaveNode(null, e.Parent, null));

                        result.Insert(index + 1, new ForeignKeySaveNode(e.Property, item, e.Parent));

                        if (item.HasForeignKeys()) entities.AddRange(_getForeignKeys(item));
                    }
                }
                else
                {
                    // make sure there are no saving issues
                    if (!useTransactions)
                    {
                        var entity = new Entity(e.Value);
                        entity.GetUpdateType();// analyzes the entity for saving issues
                    }

                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = result.IndexOf(new ForeignKeySaveNode(null, e.Parent, null));

                    result.Insert(index, new ForeignKeySaveNode(e.Property, e.Value, e.Parent));

                    // has dependencies
                    if (e.Value.HasForeignKeys()) entities.AddRange(_getForeignKeys(e.Value));
                }
            }

            return result;
        }

        private static List<ParentChildPair> _getForeignKeys(object entity)
        {
            return entity.GetForeignKeys().OrderBy(w => w.PropertyType.IsList()).Select(w => new ParentChildPair(entity, w.GetValue(entity), w)).ToList();
        }

        public EntityStateTrackable GetEntityStateTrackable()
        {
            return EntityTrackable;
        }

        public object GetPropertyValue(PropertyInfo property)
        {
            return property.GetValue(_entity);
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            _setPropertyValue(_entity, propertyName, value);
        }

        private static void _setPropertyValue(object entity, string propertyName, object value)
        {
            var found = entity.GetType().GetProperty(propertyName);

            if (found == null)
            {
                return;
            }

            var propertyType = found.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
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
            found.SetValue(entity, propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType), null);
        }

        public void SetPropertyValue(PropertyInfo property, object value)
        {
            _setPropertyValue(_entity, property, value);
        }

        private static void _setPropertyValue(object entity, PropertyInfo property, object value)
        {
            var propertyType = property.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
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
            property.SetValue(entity, propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType), null);
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

                _setPropertyValue(child, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
            else
            {
                var childPrimaryKey = child.GetPrimaryKeys().First();
                var value = child.GetType().GetProperty(childPrimaryKey.Name).GetValue(child);

                _setPropertyValue(parent, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
        }
        #endregion

        #region helpers
        private class ParentChildPair
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
