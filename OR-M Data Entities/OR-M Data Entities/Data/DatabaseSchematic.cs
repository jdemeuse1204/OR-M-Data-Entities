/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Definition.Rules;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Scripts.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data
{
    public abstract partial class DatabaseSchematic : DatabaseConnection
    {
        #region Properties
        protected static TableCache Tables { get; private set; }

        private static List<KeyValuePair<Type, Type>> _tableScriptMappings { get; set; }

        protected IEnumerable<KeyValuePair<Type, Type>> TableScriptMappings
        {
            get { return _tableScriptMappings; }
        }

        #endregion

        #region Constructor
        protected DatabaseSchematic(string connectionStringOrName)
            : base(connectionStringOrName)
        {
            Tables = new TableCache();
        }

        #endregion

        #region Rules
        public sealed class TimeStampColumnUpdateRule : IRule
        {
            private readonly EntityStateTrackable _entityStateTrackable;

            private readonly IReadOnlyList<PropertyInfo> _columns;

            public TimeStampColumnUpdateRule(EntityStateTrackable entityStateTrackable, IReadOnlyList<PropertyInfo> columns)
            {
                _entityStateTrackable = entityStateTrackable;
                _columns = columns;
            }

            public void Process()
            {
                // cannot check if entity state trackable is null (entity state tracking is off) or if the pristine entity is null (insert)
                if (_entityStateTrackable == null) return;

                List<string> errorColumns;

                var allTimestampColumns = _columns.Where(
                    w =>
                        w.GetCustomAttribute<DbTypeAttribute>() != null &&
                        w.GetCustomAttribute<DbTypeAttribute>().Type ==
                        SqlDbType.Timestamp).ToList();

                if (allTimestampColumns.Count == 0) return;

                // entity state tracking is on, check to see if the identity column has been updated
                if (ModificationEntity.GetPristineEntity(_entityStateTrackable) == null)
                {
                    // any identity columns should be zero/null or whatever the insert value is
                    errorColumns = (from column in allTimestampColumns
                                    let value = column.GetValue(_entityStateTrackable)
                                    let hasError = value != null
                                    where hasError
                                    select column.Name).ToList();
                }
                else
                {
                    // can only check when entity state tracking is on
                    // only can get here when updating, try insert, or try insert update
                    errorColumns =
                        allTimestampColumns.Where(column => ModificationEntity.HasColumnChanged(_entityStateTrackable, column.Name))
                            .Select(w => w.Name)
                            .ToList();
                }

                if (!errorColumns.Any()) return;

                const string error = "Cannot update value of TIMESTAMP column.  Column: {0}\r\r";
                var message = errorColumns.Aggregate(string.Empty, (current, item) => string.Concat(current, string.Format(error, item)));

                throw new SqlSaveException(message);
            }
        }

        private class IsEntityValidRule : IRule
        {
            private readonly object _entity;

            public IsEntityValidRule(object entity)
            {
                _entity = entity;
            }

            public void Process()
            {
                var primaryKeys = ReflectionCacheTable.GetPrimaryKeys(_entity.GetType());
                var plainTableName = _entity.GetTableName();
                var entityType = _entity.GetType();
                var linkedServerAttribute = entityType.GetCustomAttribute<LinkedServerAttribute>();
                var schemaAttribute = entityType.GetCustomAttribute<SchemaAttribute>();

                if (primaryKeys.Count == 0)
                {
                    throw new InvalidTableException(string.Format("{0} must have at least one Primary Key defined", plainTableName));
                }

                if (linkedServerAttribute != null && schemaAttribute != null)
                {
                    throw new Exception(
                        string.Format(
                            "Class {0} cannot have LinkedServerAttribute and SchemaAttribute, use one or the other",
                            entityType.Name));
                }
            }
        }

        // make sure the user is not trying to update an IDENTITY column, these cannot be updated
        private sealed class IdentityColumnUpdateRule : IRule
        {
            private readonly EntityStateTrackable _entityStateTrackable;

            private readonly ConfigurationOptions _configuration;

            private readonly IReadOnlyList<PropertyInfo> _columns;

            public IdentityColumnUpdateRule(EntityStateTrackable entityStateTrackable, ConfigurationOptions configuration, IReadOnlyList<PropertyInfo> columns)
            {
                _entityStateTrackable = entityStateTrackable;
                _configuration = configuration;
                _columns = columns;
            }

            public void Process()
            {
                // cannot check if entity state trackable is null (entity state tracking is off) or if the pristine entity is null (insert)
                if (_entityStateTrackable == null) return;

                List<string> errorColumns;

                var allIdentityColumns = _columns.Where(
                    w =>
                        w.GetCustomAttribute<DbGenerationOptionAttribute>() != null &&
                        w.GetCustomAttribute<DbGenerationOptionAttribute>().Option ==
                        DbGenerationOption.IdentitySpecification).ToList();

                if (allIdentityColumns.Count == 0) return;

                // entity state tracking is on, check to see if the identity column has been updated
                if (ModificationEntity.GetPristineEntity(_entityStateTrackable) == null)
                {
                    // any identity columns should be zero/null or whatever the insert value is
                    errorColumns = (from column in allIdentityColumns
                                    let value = column.GetValue(_entityStateTrackable)
                                    let hasError = !ModificationEntity.IsValueInInsertArray(_configuration, value)
                                    where hasError
                                    select column.Name).ToList();
                }
                else
                {
                    // can only check when entity state tracking is on
                    // only can get here when updating, try insert, or try insert update
                    errorColumns =
                        allIdentityColumns.Where(
                            column => ModificationEntity.HasColumnChanged(_entityStateTrackable, column.Name))
                            .Select(w => w.Name)
                            .ToList();
                }

                if (!errorColumns.Any()) return;

                const string error = "Cannot update value of IDENTITY column.  Column: {0}\r\r";
                var message = errorColumns.Aggregate(string.Empty,
                    (current, item) => string.Concat(current, string.Format(error, item)));

                throw new SqlSaveException(message);
            }
        }
        #endregion

        #region Methods
        public static EntityState GetEntityState(EntityStateTrackable entity)
        {
            return ModificationEntity.GetState(entity);
        }

        public static void TrySetPristineEntity(object entity)
        {
            ModificationEntity.TrySetPristineEntity(entity);
        }

        public void MapTableToScript<T, TK>()
            where T : class
            where TK : IReadScript<T>
        {
            _tableScriptMappings.Add(new KeyValuePair<Type, Type>(typeof(T), typeof(TK)));
        }

        public override void Dispose()
        {
            Tables = null;
            _tableScriptMappings = null;
            base.Dispose();
        }
        #endregion
    }

    /// <summary>
    /// Partial to hide actual implementation from user.  User does not need any of these
    /// classes, they are only used in this class
    /// </summary>
    public abstract partial class DatabaseSchematic
    {
        protected class TableCache
        {
            private readonly IDictionary<Type, ITable> _internal;

            public TableCache()
            {
                _internal = new Dictionary<Type, ITable>();
            }

            public ITable Find(Type type, ConfigurationOptions configuration)
            {
                ITable table;

                _internal.TryGetValue(type, out table);

                if (table != null) return table;

                table = new Table(type, configuration);

                _internal.Add(type, table);

                return table;
            }

            public ITable Find<T>(ConfigurationOptions configuration)
            {
                return Find(typeof(T), configuration);
            }
        }

        private class AutoLoadKeyRelationship : IAutoLoadKeyRelationship
        {
            public AutoLoadKeyRelationship(IColumn parentColumn, IColumn childColumn, IColumn autoLoadPropertyColumn, JoinType joinType)
            {
                ParentColumn = parentColumn;
                ChildColumn = childColumn;
                AutoLoadPropertyColumn = autoLoadPropertyColumn;
                JoinType = joinType;
            }

            public IColumn ChildColumn { get; private set; }

            public IColumn ParentColumn { get; private set; }

            public JoinType JoinType { get; private set; }

            public IColumn AutoLoadPropertyColumn { get; private set; }
        }

        private class AutoLoadRelationshipList : DelayedEnumerationCachedList<IAutoLoadKeyRelationship>
        {
            public AutoLoadRelationshipList(ITable table, int count, ConfigurationOptions configuration)
                : base(table, configuration, count)
            {

            }

            public override IEnumerator<IAutoLoadKeyRelationship> GetEnumerator()
            {
                if (Internal.Count == Count)
                {
                    var enumerator = Internal.GetEnumerator();

                    while (enumerator.MoveNext()) yield return enumerator.Current;
                }
                else
                {
                    // enumerate the columns first
                    var autoLoadRelationshipProperties = ParentTable.Columns.Where(w => w.IsForeignKey || w.IsPseudoKey).ToList();
                    
                    // only perform the above operation once.  Populate list for enumeration later
                    foreach (var relationship in autoLoadRelationshipProperties.Select(_getRelationship)) Internal.Add(relationship);

                    // return the results
                    foreach (var autoLoadKeyRelationship in Internal) yield return autoLoadKeyRelationship;
                }
            }

            private AutoLoadKeyRelationship _getRelationship(IColumn column)
            {
                var joinType = column.IsList || column.IsNullable ? JoinType.Left : JoinType.Inner;
                var childColumn = _getChildColumn(column);
                var parentColumn = _getParentColumn(column);

                if (childColumn == null) throw new KeyNotFoundException(string.Format("Cannot find {0}.  Key Name - {1}", column.IsForeignKey ? "Foreign Key" : "Pseudo Key", column.PropertyName));

                return new AutoLoadKeyRelationship(parentColumn, childColumn, column, joinType);
            }

            private IColumn _getChildColumn(IColumn column)
            {
                var childTable = Tables.Find(column.PropertyType.GetUnderlyingType(), Configuration);

                if (column.IsList)
                {
                    return column.IsForeignKey
                        ? childTable.Columns.FirstOrDefault(
                            w =>
                                w.PropertyName ==
                                column.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName)
                        : childTable.Columns.FirstOrDefault(
                            w => w.PropertyName == w.GetCustomAttribute<PseudoKeyAttribute>().ChildTableColumnName);
                }

                return column.IsForeignKey
                    ? childTable.Columns.FirstOrDefault(w => w.IsPrimaryKey)
                    : childTable.Columns.FirstOrDefault(
                        w => w.PropertyName == w.GetCustomAttribute<PseudoKeyAttribute>().ChildTableColumnName);
            }

            private IColumn _getParentColumn(IColumn column)
            {
                if (column.IsList)
                {
                    return column.IsForeignKey
                        ? column.Table.Columns.FirstOrDefault(w => w.IsPrimaryKey)
                        : column.Table.Columns.FirstOrDefault(
                            w => w.PropertyName == w.GetCustomAttribute<PseudoKeyAttribute>().ChildTableColumnName);
                }

                return column.IsForeignKey
                    ? column.Table.Columns.FirstOrDefault(
                        w =>
                            w.PropertyName ==
                            column.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName)
                    : column.Table.Columns.FirstOrDefault(
                        w => w.PropertyName == w.GetCustomAttribute<PseudoKeyAttribute>().ChildTableColumnName);
            }
        }

        private class ColumnList : DelayedEnumerationCachedList<IColumn>
        {
            #region Constructor
            public ColumnList(ITable table, ConfigurationOptions configuration, int count)
                : base(table, configuration, count)
            {
            }
            #endregion

            public override IEnumerator<IColumn> GetEnumerator()
            {
                if (Internal.Count == Count)
                {
                    var enumerator = Internal.GetEnumerator();

                    while (enumerator.MoveNext()) yield return enumerator.Current;
                }
                else
                {
                    var columns = ParentTable.GetAllProperties().Select(w => new Column(ParentTable, w)).ToList();

                    // load the list right away.  If we do a first or default and we dont 
                    // enumerate through the whole list then we need to perform the above operation again,
                    // we should perform it once
                    foreach (var column in columns) Internal.Add(column);

                    // iterate through the list and yield our return
                    foreach (var column in Internal) yield return column;
                }
            }
        }

        private class Column : IColumn
        {
            public ITable Table { get; private set; }

            public PropertyInfo Property { get; private set; }

            public bool IsPrimaryKey { get; private set; }

            public bool IsForeignKey { get; private set; }

            public bool IsPseudoKey { get; private set; }

            public bool IsList { get; private set; }

            public bool IsNullable { get; private set; }

            public bool IsSelectable { get; private set; }

            public Type PropertyType
            {
                get { return Property.PropertyType; }
            }

            public string PropertyName
            {
                get { return Property.Name; }
            }

            private string _databaseColumnName;
            public string DatabaseColumnName
            {
                get
                {
                    if (!string.IsNullOrEmpty(_databaseColumnName)) return _databaseColumnName;

                    _databaseColumnName = _getColumnName(Property);

                    return _databaseColumnName;
                }
            }

            public Column(ITable table, PropertyInfo property)
            {
                Property = property;
                IsPrimaryKey = table.IsPrimaryKey(property);
                IsForeignKey = table.IsForeignKey(property);
                IsPseudoKey = table.IsPseudoKey(property);
                IsList = property.PropertyType.IsList();
                IsNullable = property.PropertyType.IsNullable();
                Table = table;
                IsSelectable = table.IsSelectable(property);
            }

            #region Methods
            private string _getColumnName(PropertyInfo property)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                return columnAttribute == null ? property.Name : columnAttribute.Name;
            }

            public T GetCustomAttribute<T>() where T : Attribute
            {
                return Property.GetCustomAttribute<T>();
            }

            public string ToString(string tableAlias, string postAppendString = "")
            {
                return string.Format("[{0}].[{1}]{2}", tableAlias, DatabaseColumnName, string.IsNullOrEmpty(postAppendString) ? string.Empty : postAppendString);
            }
            #endregion
        }

        protected abstract class ReflectionCacheTable
        {
            #region Constructor
            protected ReflectionCacheTable(Type type, ConfigurationOptions configuration)
            {
                // Make sure the table is setup correctly
                RuleProcessor.ProcessRule<IsTableValidRule>(type);

                AllProperties = type.GetProperties().ToList();
                AllColumns = AllProperties.Where(IsColumn).ToList();
                Type = type;

                LinkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>();
                TableAttribute = type.GetCustomAttribute<TableAttribute>();
                SchemaAttribute = type.GetCustomAttribute<SchemaAttribute>();
                ReadOnlyAttribute = type.GetCustomAttribute<ReadOnlyAttribute>();
                LookupTableAttribute = type.GetCustomAttribute<LookupTableAttribute>();

                DefaultSchema = configuration.DefaultSchema;
            }
            #endregion

            #region Properties
            public Type Type { get; private set; }

            protected List<PropertyInfo> AllProperties { get; private set; }

            protected List<PropertyInfo> AllColumns { get; private set; }
            #endregion

            #region Fields

            protected readonly LinkedServerAttribute LinkedServerAttribute;

            protected readonly TableAttribute TableAttribute;

            protected readonly SchemaAttribute SchemaAttribute;

            protected readonly ReadOnlyAttribute ReadOnlyAttribute;

            protected readonly LookupTableAttribute LookupTableAttribute;


            private string _tableNamePlain;

            private string _tableNameSql;

            private string _tableNameSqlWithSchema;

            private string _tableNameSqlWithSchemaTrimStartAndEnd;


            private List<PropertyInfo> _primaryKeys;


            public readonly string DefaultSchema;

            #endregion

            #region Primary Keys

            public static List<PropertyInfo> GetPrimaryKeys(Type type)
            {
                return _getPrimaryKeys(type.Name, type.GetProperties().ToList());
            }

            private static List<PropertyInfo> _getPrimaryKeys(string className, List<PropertyInfo> propertyInfos)
            {
                var keyList = propertyInfos.Where(IsPrimaryKey).ToList();

                if (keyList.Count != 0) return keyList;

                throw new Exception(string.Format("Cannot find PrimaryKey(s) for type of {0}", className));
            }

            public static bool IsPrimaryKey(MemberInfo column)
            {
                return column.Name.ToUpper() == "ID"
                    || column.GetCustomAttribute<KeyAttribute>() != null
                    || GetColumnName(column).ToUpper() == "ID"; // remove this?
            }
            #endregion

            #region Foreign Key Methods
            public bool HasForeignKeys()
            {
                return _hasForeignKeys(AllProperties);
            }

            public bool HasPrimaryKeysOnly()
            {
                return _hasPrimaryKeysOnly(AllProperties);
            }

            public static bool HasForeignKeys(object entity)
            {
                return _hasForeignKeys(entity.GetType().GetProperties().ToList());
            }

            public static bool HasPrimaryKeysOnly(object entity)
            {
                return _hasPrimaryKeysOnly(entity.GetType().GetProperties());
            }

            private List<PropertyInfo> _allForeignAndPseudoKeys;

            public List<PropertyInfo> GetAllForeignAndPseudoKeys(string viewId = null)
            {
                if (_allForeignAndPseudoKeys != null) return _allForeignAndPseudoKeys;

                _allForeignAndPseudoKeys = string.IsNullOrWhiteSpace(viewId)
                    ? AllProperties.Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null)
                        .ToList()
                    : AllProperties
                        .Where(
                            w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null &&
                                 w.GetPropertyType().GetCustomAttribute<ViewAttribute>() != null &&
                                 w.GetPropertyType().GetCustomAttribute<ViewAttribute>().ViewIds.Contains(viewId))
                        .ToList();

                return _allForeignAndPseudoKeys;
            }

            private static bool _hasForeignKeys(List<PropertyInfo> properties)
            {
                return properties.Any(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null);
            }

            private static bool _hasPrimaryKeysOnly(IReadOnlyList<PropertyInfo> properties)
            {
                return properties.Count(IsColumn) == properties.Count(IsPrimaryKey);
            }
            #endregion

            #region Columns
            public static string GetColumnName(MemberInfo column)
            {
                var columnAttribute = column.GetCustomAttribute<ColumnAttribute>();

                return columnAttribute == null ? column.Name : columnAttribute.Name;
            }

            public static bool IsColumn(PropertyInfo info)
            {
                var attributes = info.GetCustomAttributes().ToList();
                var hasAttributes = attributes.Any();

                if (!hasAttributes) return true;

                return (attributes.Any(w => w is SearchablePrimaryKeyAttribute) || !attributes.Any(w => w is NonSelectableAttribute));
            }
            #endregion

            #region Non-Static Methods
            public List<PropertyInfo> GetPrimaryKeys()
            {
                if (_primaryKeys != null) return _primaryKeys;

                _primaryKeys = _getPrimaryKeys(Type.Name, AllProperties);

                return _primaryKeys;
            }

            public List<PropertyInfo> GetAllColumns()
            {
                return AllColumns;
            }

            public List<PropertyInfo> GetAllProperties()
            {
                return AllProperties;
            }
            #endregion

            #region Other Methods
            public string ToString(TableNameFormat format)
            {
                switch (format)
                {
                    case TableNameFormat.Plain:
                        if (!string.IsNullOrEmpty(_tableNamePlain)) return _tableNamePlain;

                        _setTableName();

                        return _tableNamePlain;
                    case TableNameFormat.Sql:
                        if (!string.IsNullOrEmpty(_tableNameSql)) return _tableNameSql;

                        _setTableName();
                        _tableNameSql = string.Format("[{0}]", _tableNamePlain);

                        return _tableNameSql;
                    case TableNameFormat.SqlWithSchema:
                        if (!string.IsNullOrEmpty(_tableNameSqlWithSchema)) return _tableNameSqlWithSchema;

                        _setTableNameWithSchema();

                        return _tableNameSqlWithSchema;
                    case TableNameFormat.SqlWithSchemaTrimStartAndEnd:
                        if (!string.IsNullOrEmpty(_tableNameSqlWithSchemaTrimStartAndEnd)) return _tableNameSqlWithSchemaTrimStartAndEnd;

                        _setTableNameWithSchema();
                        _tableNameSqlWithSchemaTrimStartAndEnd = _tableNameSqlWithSchema.TrimStart('[').TrimEnd(']');

                        return _tableNameSqlWithSchemaTrimStartAndEnd;
                    case TableNameFormat.AttributeName:
                        return TableAttribute != null ? TableAttribute.Name : null;
                    default:
                        throw new ArgumentOutOfRangeException(format.ToString());
                }
            }

            private void _setTableNameWithSchema()
            {
                var linkedServerText = string.Format("{0}", LinkedServerAttribute != null ? LinkedServerAttribute.FormattedLinkedServerText : string.Empty);

                _tableNameSqlWithSchema = string.Concat(linkedServerText,
                    string.Format(LinkedServerAttribute == null ? "[{0}].[{1}]" : "{0}.[{1}]",
                        LinkedServerAttribute == null ? DefaultSchema : string.Empty,
                        ToString(TableNameFormat.Plain)));
            }

            private void _setTableName()
            {
                _tableNamePlain = Type.GetTableName();
            }
            #endregion
        }

        protected class Table : ReflectionCacheTable, ITable
        {
            #region Constructor
            public Table(object entity, ConfigurationOptions configuration)
                : this(entity.GetType(), configuration)
            {

            }

            public Table(Type type, ConfigurationOptions configuration)
                : base(type, configuration)
            {
                ClassName = type.Name;

                IsEntityStateTrackingOn = type.IsSubclassOf(typeof(EntityStateTrackable));

                Columns = new ColumnList(this, configuration, AllProperties.Count);

                // count the number of auto load properties
                // needed for the cached list
                var autoLoadColumnCount = GetAllForeignAndPseudoKeys();

                AutoLoadKeyRelationships = new AutoLoadRelationshipList(this, autoLoadColumnCount.Count, configuration);
            }
            #endregion

            #region Properties

            public bool IsReadOnly { get { return ReadOnlyAttribute != null; } }

            public string Schema { get { return DefaultSchema; } }

            public bool IsUsingLinkedServer { get { return LinkedServerAttribute != null; } }

            public bool IsLookupTable { get { return LookupTableAttribute != null; } }

            public string ClassName { get; private set; }

            public string ServerName
            {
                get { return LinkedServerAttribute == null ? string.Empty : LinkedServerAttribute.ServerName; }
            }

            public string DatabaseName
            {
                get { return LinkedServerAttribute == null ? string.Empty : LinkedServerAttribute.DatabaseName; }
            }

            public DelayedEnumerationCachedList<IColumn> Columns { get; private set; }

            public DelayedEnumerationCachedList<IAutoLoadKeyRelationship> AutoLoadKeyRelationships { get; private set; }

            public string SchemaName
            {
                get
                {
                    return LinkedServerAttribute == null
                        ? SchemaAttribute == null ? DefaultSchema : SchemaAttribute.SchemaName
                        : LinkedServerAttribute.SchemaName;
                }
            }

            public bool IsEntityStateTrackingOn { get; private set; }
            #endregion

            #region Methods
            public ReadOnlySaveOption? GetReadOnlySaveOption()
            {
                return ReadOnlyAttribute == null ? null : (ReadOnlySaveOption?)ReadOnlyAttribute.ReadOnlySaveOption;
            }

            public override string ToString()
            {
                return ToString(TableNameFormat.SqlWithSchema);
            }

            public static bool IsPrimaryKey(Type type, string columnName)
            {
                return columnName.ToUpper() == "ID" ||
                       type.GetProperty(columnName).GetCustomAttribute<KeyAttribute>() != null;
            }

            public bool IsPrimaryKey(string columnName)
            {
                return IsPrimaryKey(Type, columnName);
            }

            public bool IsSelectable(PropertyInfo property)
            {
                return property.GetCustomAttribute<NonSelectableAttribute>() == null;
            }

            public bool IsPrimaryKey(PropertyInfo property)
            {
                var columnName = property.GetColumnName().ToUpper();

                return columnName == "ID" || property.GetCustomAttribute<KeyAttribute>() != null;
            }

            public bool IsForeignKey(PropertyInfo property)
            {
                return property.GetCustomAttribute<ForeignKeyAttribute>() != null;
            }

            public bool IsPseudoKey(PropertyInfo property)
            {
                return property.GetCustomAttribute<PseudoKeyAttribute>() != null;
            }

            public string GetPrimaryKeyName(int index)
            {
                var keys = GetPrimaryKeys();

                var key = keys[index];

                return GetColumnName(key);
            }

            public static List<PropertyInfo> GetAllColumns(object entity)
            {
                return GetAllColumns(entity.GetType());
            }

            public static List<PropertyInfo> GetAllColumns(Type type)
            {
                if (type == null) throw new ArgumentNullException("type");

                return type.GetProperties().Where(IsColumn).ToList();
            }

            public string GetColumnName(string propertyName)
            {
                return GetColumnName(AllProperties, propertyName);
            }

            public static string GetColumnName(List<PropertyInfo> properties, string propertyName)
            {
                var property = properties.FirstOrDefault(w => w.Name == propertyName);

                // property will be in list only if it has a custom attribute
                if (property == null) return propertyName;
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                return columnAttribute == null ? propertyName : columnAttribute.Name;
            }

            public static DbGenerationOption GetGenerationOption(PropertyInfo column)
            {
                var dbGenerationColumn = column.GetCustomAttribute<DbGenerationOptionAttribute>();
                return dbGenerationColumn == null ? DbGenerationOption.IdentitySpecification : dbGenerationColumn.Option;
            }
            #endregion
        }

        protected class Entity : Table, IEquatable<Entity>, IEntity
        {
            #region Properties and Fields

            public readonly object Value;

            // check inheritance at table level instead
            protected EntityStateTrackable EntityTrackable
            {
                get { return IsEntityStateTrackingOn ? (EntityStateTrackable)Value : null; }
            }

            #endregion

            #region Constructor
            public Entity(object entity, ConfigurationOptions configuration)
                : base(entity, configuration)
            {
                if (entity == null) throw new ArgumentNullException("entity");

                Value = entity;
            }
            #endregion

            #region Property Methods
            protected static FieldInfo GetPristineEntityFieldInfo()
            {
                return typeof(EntityStateTrackable).GetField("_pristineEntity",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            public object GetPropertyValue(PropertyInfo property)
            {
                return property.GetValue(Value);
            }

            public object GetPropertyValue(string propertyName)
            {
                var property = Value.GetType().GetProperty(propertyName);

                return GetPropertyValue(property);
            }

            public bool IsPristineEntityNull()
            {
                return !IsEntityStateTrackingOn || _getPristineEntity() == null;
            }

            private object _getPristineEntity()
            {
                var field = GetPristineEntityFieldInfo();

                return field.GetValue(Value);
            }

            public object GetPristineEntityPropertyValue(string propertyName)
            {
                if (!IsEntityStateTrackingOn) throw new Exception("Entity State Tracking is not on, error in GetPristineEntityPropertyValue");

                var pristineEntity = _getPristineEntity();

                var property = pristineEntity.GetType().GetProperty(propertyName);

                if (property == null) throw new Exception(string.Format("Cannot find property name: {0}.  Search failed in pristine entity", propertyName));

                return property.GetValue(pristineEntity);
            }

            public void SetPropertyValue(string propertyName, object value)
            {
                _setPropertyValue(Value, propertyName, value);
            }

            private static void _setPropertyValue(object entity, string propertyName, object value)
            {
                var found = entity.GetType().GetProperty(propertyName);

                if (found == null) return;

                _setPropertyValue(entity, found, value);
            }

            public void SetPropertyValue(PropertyInfo property, object value)
            {
                _setPropertyValue(Value, property, value);
            }

            private static void _setPropertyValue(object entity, PropertyInfo property, object value)
            {
                var propertyType = property.PropertyType;

                if (value is DBNull) value = null;

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

            #region IEquatable
            public bool Equals(Entity other)
            {
                //Check whether the compared object is null.
                if (ReferenceEquals(other, null)) return false;

                //Check whether the compared object references the same data.
                if (ReferenceEquals(this, other)) return true;

                //Check whether the products' properties are equal.
                return other.Value == Value;
            }

            public override bool Equals(object obj)
            {
                return Value == obj;
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.
            public override int GetHashCode()
            {
                //Calculate the hash code for the product.
                return Value.GetHashCode();
            }
            #endregion
        }

        protected class ModificationEntity : Entity, IModificationEntity
        {
            #region Properties
            public UpdateType UpdateType { get; private set; }

            public EntityState State { get; private set; }

            protected IReadOnlyList<ModificationItem> ModificationItems { get; set; }

            // table can have two foreign keys of the same time, this will tell them apart
            #endregion

            #region Constructor
            public ModificationEntity(object entity, ConfigurationOptions configuration)
                : this(entity, configuration, false)
            {
            }

            protected ModificationEntity(object entity, ConfigurationOptions configuration, bool isDeleting)
                : base(entity, configuration)
            {
                // do not initialize when deleting because we will get unnecessary errors
                if (isDeleting) return;

                _initialize(configuration);
            }
            #endregion

            #region Methods
            public IReadOnlyList<ModificationItem> Changes()
            {
                // this is for updates and we should never update a PK
                return ModificationItems.Where(w => w.IsModified && !w.IsPrimaryKey).ToList();
            }

            public IReadOnlyList<ModificationItem> Keys()
            {
                return ModificationItems.Where(w => w.IsPrimaryKey).ToList();
            }

            public IReadOnlyList<ModificationItem> All()
            {
                return ModificationItems;
            }

            private void _setChanges()
            {
                if (!IsEntityStateTrackingOn)
                {
                    // mark everything has changed so it will be updated
                    ModificationItems = AllColumns.Select(w => new ModificationItem(w)).ToList();
                    State = EntityState.Modified;
                    return;
                }
                // if _pristineEntity == null then that means a new instance was created and it is an insert

                ModificationItems = _getChanges(EntityTrackable, AllColumns);

                State = _getState(ModificationItems);
            }

            private static EntityState _getState(IReadOnlyList<ModificationItem> changes)
            {
                return changes.Any(w => w.IsModified) ? EntityState.Modified : EntityState.UnChanged;
            }

            public static EntityState GetState(EntityStateTrackable entityStateTrackable)
            {
                var changes = _getChanges(entityStateTrackable, GetAllColumns(entityStateTrackable));

                return _getState(changes);
            }

            public static bool IsValueInInsertArray(ConfigurationOptions configuration, object value)
            {
                // SET CONFIGURATION FOR ZERO/NOT UPDATED VALUES
                switch (value.GetType().Name.ToUpper())
                {
                    case "INT16":
                        return configuration.InsertKeys.SmallInt.Contains(Convert.ToInt16(value));
                    case "INT32":
                        return configuration.InsertKeys.Int.Contains(Convert.ToInt32(value));
                    case "INT64":
                        return configuration.InsertKeys.BigInt.Contains(Convert.ToInt64(value));
                    case "GUID":
                        return configuration.InsertKeys.UniqueIdentifier.Contains((Guid)value);
                    case "STRING":
                        return configuration.InsertKeys.String.Contains(value.ToString());
                }

                return false;
            }

            private static IReadOnlyList<ModificationItem> _getChanges(EntityStateTrackable entityStateTrackable, List<PropertyInfo> allColumns)
            {
                return (from item in allColumns
                        let current = _getCurrentObject(entityStateTrackable, item.Name)
                        let pristineEntity = _getPristineProperty(entityStateTrackable, item.Name)
                        let hasChanged = _hasChanged(pristineEntity, current)
                        select new ModificationItem(item, hasChanged)).ToList();
            }

            private static bool _hasChanged(object pristineEntity, object entity)
            {
                return ((entity == null && pristineEntity != null) || (entity != null && pristineEntity == null))
                    ? entity != pristineEntity
                    : entity == null && pristineEntity == null ? false : !entity.Equals(pristineEntity);
            }

            private void _initialize(ConfigurationOptions configuration)
            {
                var primaryKeys = GetPrimaryKeys();

                // make sure the table is valid
                RuleProcessor.ProcessRule<IsEntityValidRule>(Value);

                var hasPrimaryKeysOnly = HasPrimaryKeysOnly();
                var areAnyPkGenerationOptionsNone = false;

                UpdateType = UpdateType.Skip;

                // check to see if anything has updated, if not we can skip everything
                _setChanges();

                // if there are no changes then exit
                if (State == EntityState.UnChanged) return;

                // validate all max length attributes
                RuleProcessor.ProcessRule<MaxLengthViolationRule>(Value, Type);

                UpdateType = UpdateType.Insert;

                // need to find the Update Type
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var pkValue = key.GetValue(Value);
                    var generationOption = GetGenerationOption(key);

                    RuleProcessor.ProcessRule<PkValueNotNullRule>(pkValue, key);

                    // check to see if we are updating or not
                    var isUpdating = !_isValueInInsertArray(configuration, pkValue);

                    if (generationOption == DbGenerationOption.None) areAnyPkGenerationOptionsNone = true;

                    // break because we are already updating, do not want to set to false
                    if (!isUpdating)
                    {
                        if (generationOption != DbGenerationOption.None)
                        {
                            RuleProcessor.ProcessRule<TimeStampColumnInsertRule>(Value, AllColumns);
                            continue;
                        }

                        // if the db generation option is none and there is no pk value this is an error because the db doesnt generate the pk
                        throw new SqlSaveException(string.Format(
                            "Primary Key must not be an insert value when DbGenerationOption is set to None.  Primary Key Name: {0}, Table: {1}",
                            key.Name,
                            ToString(TableNameFormat.Plain)));
                    }

                    // If we have only primary keys we need to perform a try insert and see if we can try to insert our data.
                    // if we have any Pk's with a DbGenerationOption of None we need to first see if a record exists for the pks, 
                    // if so we need to perform an update, otherwise we perform an insert
                    UpdateType = hasPrimaryKeysOnly
                        ? UpdateType.TryInsert
                        : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;

                    if (UpdateType != UpdateType.Update && UpdateType != UpdateType.TryInsertUpdate) continue;

                    // make sure identity columns were not updated
                    RuleProcessor.ProcessRule<IdentityColumnUpdateRule>(EntityTrackable, configuration, AllColumns);

                    // make sure time stamps were not updated if any
                    RuleProcessor.ProcessRule<TimeStampColumnUpdateRule>(EntityTrackable, AllColumns);
                }
            }

            private static bool _isValueInInsertArray(ConfigurationOptions configuration, object value)
            {
                // SET CONFIGURATION FOR ZERO/NOT UPDATED VALUES
                switch (value.GetType().Name.ToUpper())
                {
                    case "INT16":
                        return configuration.InsertKeys.SmallInt.Contains(Convert.ToInt16(value));
                    case "INT32":
                        return configuration.InsertKeys.Int.Contains(Convert.ToInt32(value));
                    case "INT64":
                        return configuration.InsertKeys.BigInt.Contains(Convert.ToInt64(value));
                    case "GUID":
                        return configuration.InsertKeys.UniqueIdentifier.Contains((Guid)value);
                    case "STRING":
                        return configuration.InsertKeys.String.Contains(value.ToString());
                }

                return false;
            }

            public static bool HasColumnChanged(EntityStateTrackable entity, string propertyName)
            {
                // only check when the pristine entity is not null, try insert update will fail otherwise.
                // if the pristine entity is null then there is nothing to compare to
                var pristineEntity = _getPristineProperty(entity, propertyName);
                var current = _getCurrentObject(entity, propertyName);

                return _hasChanged(pristineEntity, current);
            }

            private static object _getCurrentObject(EntityStateTrackable entity, string propertyName)
            {
                return entity.GetType().GetProperty(propertyName).GetValue(entity);
            }

            private static object _getPristineProperty(EntityStateTrackable entity, string propertyName)
            {
                var tableOnLoad = GetPristineEntity(entity);

                return tableOnLoad == null
                    ? new object() // need to make sure the current doesnt equal the current if the pristine entity is null
                    : tableOnLoad.GetType().GetProperty(propertyName).GetValue(tableOnLoad);
            }

            public static object GetPristineEntity(EntityStateTrackable entity)
            {
                var field = GetPristineEntityFieldInfo();

                // cannot be null here, should never happen
                if (field == null) throw new ArgumentNullException("_pristineEntity");

                return field.GetValue(entity);
            }

            public static void TrySetPristineEntity(object instance)
            {
                var entityTrackable = instance as EntityStateTrackable;

                if (entityTrackable == null) return;

                var field = GetPristineEntityFieldInfo();

                if (field == null) throw new SqlSaveException("Cannot find Pristine Entity");

                field.SetValue(instance, EntityCloner.Clone(instance));
            }
            #endregion

            #region helpers
            private static class EntityCloner
            {
                public static object Clone(object table)
                {
                    var instance = Activator.CreateInstance(table.GetType());

                    foreach (var item in table.GetType().GetProperties().Where(IsColumn).ToList())
                    {
                        var value = item.GetValue(table);

                        instance.GetType().GetProperty(item.Name).SetValue(instance, value);
                    }

                    return instance;
                }
            }
            #endregion
        }

        protected class DeleteEntity : ModificationEntity
        {
            public DeleteEntity(object entity, ConfigurationOptions configuration)
                : base(entity, configuration)
            {
                ModificationItems = AllColumns.Select(w => new ModificationItem(w)).ToList();
            }
        }
    }
}
