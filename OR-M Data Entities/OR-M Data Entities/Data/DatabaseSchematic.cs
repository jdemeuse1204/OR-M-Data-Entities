/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Loading;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Scripts.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data
{
    public abstract partial class DatabaseSchematic : Database
    {
        #region Properties
        protected TableFactory DbTableFactory { get; private set; }

        private List<KeyValuePair<Type, Type>> _tableScriptMappings { get; set; }

        protected IEnumerable<KeyValuePair<Type, Type>> TableScriptMappings
        {
            get { return _tableScriptMappings; }
        }

        #endregion

        #region Constructor
        protected DatabaseSchematic(string connectionStringOrName)
            : base(connectionStringOrName)
        {
            DbTableFactory = new TableFactory();
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
            DbTableFactory = null;
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
        protected class TableFactory : ITableFactory
        {
            private readonly IDictionary<Type, ITable> _internal;

            public TableFactory()
            {
                _internal = new Dictionary<Type, ITable>();
            }

            public ITable Find(Type type, IConfigurationOptions configuration)
            {
                ITable table;

                _internal.TryGetValue(type, out table);

                if (table != null) return table;

                table = new Table(type, configuration, this);

                _internal.Add(type, table);

                return table;
            }

            public ITable Find<T>(IConfigurationOptions configuration)
            {
                return Find(typeof(T), configuration);
            }
        }

        private class AutoLoadKeyRelationship : IAutoLoadKeyRelationship
        {
            public AutoLoadKeyRelationship(IColumn parentColumn, IColumn childColumn, IColumn autoLoadPropertyColumn, JoinType joinType, bool isNullableOrListJoin)
            {
                ParentColumn = parentColumn;
                ChildColumn = childColumn;
                AutoLoadPropertyColumn = autoLoadPropertyColumn;
                JoinType = joinType;
                IsNullableOrListJoin = isNullableOrListJoin;
            }

            public IColumn ChildColumn { get; private set; }

            public IColumn ParentColumn { get; private set; }

            public JoinType JoinType { get; private set; }

            public IColumn AutoLoadPropertyColumn { get; private set; }

            public bool IsNullableOrListJoin { get; private set; }
        }

        private class AutoLoadRelationshipList : DelayedEnumerationCachedList<IAutoLoadKeyRelationship>
        {
            private readonly TableFactory _tableCache;

            public AutoLoadRelationshipList(ITable table, IConfigurationOptions configuration, TableFactory cache, int count)
                : base(table, configuration, count)
            {
                _tableCache = cache;
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
                var isNullable = column.IsNullable;

                if (!column.IsList)
                {
                    // need to make sure parent is not a nullable FK or list,
                    // we cannot inner join is this case

                    if (column.IsForeignKey)
                    {
                        var columnName = column.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                        var property = column.Table.GetProperty(columnName);

                        if (property == null) throw new Exception(string.Format("Property not found. Property Name: {0}", columnName));

                        isNullable = property.PropertyType.IsNullable();
                    }

                    if (column.IsPseudoKey)
                    {
                        var columnName = column.GetCustomAttribute<PseudoKeyAttribute>().ParentTableColumnName;
                        var property = column.Table.GetProperty(columnName);

                        if (property == null) throw new Exception(string.Format("Property not found. Property Name: {0}", columnName));

                        isNullable = property.PropertyType.IsNullable();
                    }
                }

                var joinType = column.IsList || isNullable ? JoinType.Left : JoinType.Inner;
                var childColumn = _getChildColumn(column);
                var parentColumn = _getParentColumn(column);

                if (childColumn == null) throw new KeyNotFoundException(string.Format("Cannot find {0}.  Key Name - {1}", column.IsForeignKey ? "Foreign Key" : "Pseudo Key", column.PropertyName));

                return new AutoLoadKeyRelationship(parentColumn, childColumn, column, joinType, column.IsList || isNullable);
            }

            private IColumn _getChildColumn(IColumn column)
            {
                var childTable = _tableCache.Find(column.PropertyType.GetUnderlyingType(), Configuration);

                if (column.IsList)
                {
                    return column.IsForeignKey
                        ? childTable.Columns.FirstOrDefault(
                            w => string.Equals(w.PropertyName,
                                     column.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName,
                                     StringComparison.CurrentCultureIgnoreCase))
                        : childTable.Columns.FirstOrDefault(
                            w => string.Equals(w.PropertyName,
                                    column.GetCustomAttribute<PseudoKeyAttribute>().ChildTableColumnName,
                                    StringComparison.CurrentCultureIgnoreCase));
                }

                return column.IsForeignKey
                    ? childTable.Columns.FirstOrDefault(w => w.IsPrimaryKey)
                    : childTable.Columns.FirstOrDefault(
                        w => string.Equals(w.PropertyName,
                                column.GetCustomAttribute<PseudoKeyAttribute>().ChildTableColumnName,
                                StringComparison.CurrentCultureIgnoreCase));
            }

            private IColumn _getParentColumn(IColumn column)
            {
                if (column.IsList)
                {
                    return column.IsForeignKey
                        ? column.Table.Columns.FirstOrDefault(w => w.IsPrimaryKey)
                        : column.Table.Columns.FirstOrDefault(
                            w =>
                                string.Equals(w.PropertyName,
                                    column.GetCustomAttribute<PseudoKeyAttribute>().ParentTableColumnName,
                                    StringComparison.CurrentCultureIgnoreCase));
                }

                return column.IsForeignKey
                    ? column.Table.Columns.FirstOrDefault(
                        w =>
                            w.PropertyName ==
                            column.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName)
                    : column.Table.Columns.FirstOrDefault(
                        w =>
                            string.Equals(w.PropertyName,
                                column.GetCustomAttribute<PseudoKeyAttribute>().ParentTableColumnName,
                                StringComparison.CurrentCultureIgnoreCase));
            }
        }

        private class ColumnList : DelayedEnumerationCachedList<IColumn>
        {
            #region Constructor
            public ColumnList(ITable table, IConfigurationOptions configuration, int count)
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

            public bool IsAutoLoadKeyNullableOrList()
            {
                // one to many
                if (IsList) return true;

                // one to one
                if (IsForeignKey)
                {
                    var fkAttribute = GetCustomAttribute<ForeignKeyAttribute>();

                    // attribute not found, should not happen
                    if (fkAttribute == null) return false;

                    var autoLoadColumn = Table.GetColumn(fkAttribute.ForeignKeyColumnName);

                    if (autoLoadColumn == null) throw new Exception(string.Format("Cannot find Foreign Key.  Property Name: {0}", fkAttribute.ForeignKeyColumnName));

                    return autoLoadColumn.IsNullable;
                }

                if (IsPseudoKey)
                {
                    var pskAttribute = GetCustomAttribute<PseudoKeyAttribute>();

                    // attribute not found, should not happen
                    if (pskAttribute == null) return false;

                    var autoLoadColumn = Table.GetColumn(pskAttribute.ParentTableColumnName);

                    if (autoLoadColumn == null) throw new Exception(string.Format("Cannot find Pseudo Key.  Property Name: {0}", pskAttribute.ParentTableColumnName));

                    return autoLoadColumn.IsNullable;
                }

                return false;
            }

            #endregion
        }

        protected abstract class ReflectionCacheTable
        {
            #region Constructor
            protected ReflectionCacheTable(Type type, IConfigurationOptions configuration)
            {
                var linkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>();
                var schemaAttribute = type.GetCustomAttribute<SchemaAttribute>();

                if (linkedServerAttribute != null && schemaAttribute != null)
                {
                    throw new InvalidTableException(
                        string.Format(
                            "Class {0} cannot have LinkedServerAttribute and SchemaAttribute, use one or the other",
                            type.Name));
                }

                AllProperties = type.GetProperties().ToList();
                AllColumns = AllProperties.Where(IsColumn).ToList();
                Type = type;

                LinkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>();
                TableAttribute = type.GetCustomAttribute<TableAttribute>();
                SchemaAttribute = type.GetCustomAttribute<SchemaAttribute>();
                ReadOnlyAttribute = type.GetCustomAttribute<ReadOnlyAttribute>();
                LookupTableAttribute = type.GetCustomAttribute<LookupTableAttribute>();

                _configurationDefaultSchema = configuration.DefaultSchema;
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

            private readonly string _configurationDefaultSchema;

            private List<PropertyInfo> _primaryKeys;

            private string _schema;

            #endregion

            #region Primary Keys

            public static List<PropertyInfo> GetPrimaryKeys(Type type)
            {
                return _getPrimaryKeys(type.Name, type.GetProperties().ToList());
            }

            public static List<PropertyInfo> GetPrimaryKeys(object entity)
            {
                var type = entity.GetUnderlyingType();

                return GetPrimaryKeys(type);
            }

            private static List<PropertyInfo> _getPrimaryKeys(string className, List<PropertyInfo> propertyInfos)
            {
                var keyList = propertyInfos.Where(IsColumnPrimaryKey).ToList();

                if (keyList.Count != 0) return keyList;

                throw new Exception(string.Format("Cannot find PrimaryKey(s) for type of {0}", className));
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

            public List<PropertyInfo> GetAllForeignAndPseudoKeys()
            {
                if (_allForeignAndPseudoKeys != null) return _allForeignAndPseudoKeys;

                _allForeignAndPseudoKeys = AllProperties.Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null).ToList();

                return _allForeignAndPseudoKeys;
            }

            private static bool _hasForeignKeys(List<PropertyInfo> properties)
            {
                return properties.Any(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null);
            }

            private static bool _hasPrimaryKeysOnly(IReadOnlyList<PropertyInfo> properties)
            {
                return properties.Count(IsColumn) == properties.Count(IsColumnPrimaryKey);
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

            public static bool IsColumnPrimaryKey(PropertyInfo property)
            {
                return property.Name.ToUpper() == "ID"
                    || property.GetCustomAttribute<KeyAttribute>() != null
                    || GetColumnName(property).ToUpper() == "ID";
            }
            #endregion

            #region Non-Static Methods
            public List<PropertyInfo> GetPrimaryKeys()
            {
                if (_primaryKeys != null) return _primaryKeys;

                _primaryKeys = _getPrimaryKeys(Type.Name, AllProperties);

                return _primaryKeys;
            }

            public static List<string> GetPrimaryKeyNames(Type type)
            {
                var primaryKeys = GetPrimaryKeys(type);

                return primaryKeys.Select(GetColumnName).ToList();
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

            public string Schema()
            {
                if (!string.IsNullOrEmpty(_schema)) return _schema;

                _schema = LinkedServerAttribute == null
                    ? SchemaAttribute == null ? _configurationDefaultSchema : SchemaAttribute.SchemaName
                    : LinkedServerAttribute.SchemaName;

                return _schema;
            }

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
                        LinkedServerAttribute == null ? Schema() : string.Empty,
                        ToString(TableNameFormat.Plain)));
            }

            private void _setTableName()
            {
                _tableNamePlain = Table.GetTableName(Type);
            }
            #endregion
        }

        protected class Table : ReflectionCacheTable, ITable
        {
            #region Constructor
            public Table(object entity, IConfigurationOptions configuration, TableFactory tableCache)
                : this(entity.GetType(), configuration, tableCache)
            {

            }

            public Table(Type type, IConfigurationOptions configuration, TableFactory tableCache)
                : base(type, configuration)
            {
                ClassName = type.Name;

                IsEntityStateTrackingOn = type.IsSubclassOf(typeof(EntityStateTrackable));

                Columns = new ColumnList(this, configuration, AllProperties.Count);

                // count the number of auto load properties
                // needed for the cached list
                var autoLoadColumnCount = GetAllForeignAndPseudoKeys();

                AutoLoadKeyRelationships = new AutoLoadRelationshipList(this, configuration, tableCache, autoLoadColumnCount.Count);
            }
            #endregion

            #region Properties

            public bool IsReadOnly { get { return ReadOnlyAttribute != null; } }

            public string PlainTableName { get { return ToString(TableNameFormat.Plain); } }

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

            public bool IsEntityStateTrackingOn { get; private set; }
            #endregion

            #region Methods

            public IColumn GetColumn(string propertyName)
            {
                return Columns.FirstOrDefault(w => w.PropertyName == propertyName);
            }

            public ReadOnlySaveOption? GetReadOnlySaveOption()
            {
                return ReadOnlyAttribute == null ? null : (ReadOnlySaveOption?)ReadOnlyAttribute.ReadOnlySaveOption;
            }

            public static string GetTableName(Type type)
            {
                var tableAttribute = type.GetCustomAttribute<TableAttribute>();

                return tableAttribute == null ? type.Name : tableAttribute.Name;
            }

            public static string GetTableName(object entity)
            {
                var type = entity.GetUnderlyingType();

                var tableAttribute = type.GetCustomAttribute<TableAttribute>();

                return tableAttribute == null ? type.Name : tableAttribute.Name;
            }

            public override string ToString()
            {
                return ToString(TableNameFormat.SqlWithSchema);
            }

            public static bool IsPrimaryKey(Type type, string columnName)
            {
                var property = type.GetProperty(columnName);

                if (property == null) throw new KeyNotFoundException(string.Format("Cannot find primary key. Table - {0} Key - {1}", type.Name, columnName));

                return IsColumnPrimaryKey(property);
            }

            public bool IsPrimaryKey(string columnName)
            {
                return IsPrimaryKey(Type, columnName);
            }

            public bool IsPrimaryKey(PropertyInfo property)
            {
                return IsColumnPrimaryKey(property);
            }

            public bool IsSelectable(PropertyInfo property)
            {
                return property.GetCustomAttribute<NonSelectableAttribute>() == null;
            }

            public bool IsForeignKey(PropertyInfo property)
            {
                return property.GetCustomAttribute<ForeignKeyAttribute>() != null;
            }

            public static List<PropertyInfo> GetForeignKeys(Type type)
            {
                return type.GetProperties().Where(w =>
                    w.GetCustomAttribute<ForeignKeyAttribute>() != null).ToList();
            }

            public static List<PropertyInfo> GetForeignKeys(object entity)
            {
                return GetForeignKeys(entity.GetUnderlyingType());
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

            public PropertyInfo GetProperty(string databaseColumnName)
            {
                var firstCheck =
                    AllProperties.FirstOrDefault(
                        w =>
                            w.GetCustomAttribute<ColumnAttribute>() != null &&
                            w.GetCustomAttribute<ColumnAttribute>().Name == databaseColumnName);

                return firstCheck ?? AllProperties.FirstOrDefault(w => w.Name == databaseColumnName);
            }

            public static string GetColumnName(List<PropertyInfo> properties, string propertyName)
            {
                var property = properties.FirstOrDefault(w => w.Name == propertyName);

                // property will be in list only if it has a custom attribute
                return property == null ? propertyName : GetColumnName(property);
            }

            public static string GetColumnName(PropertyInfo property)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                return columnAttribute == null ? property.Name : columnAttribute.Name;
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
            public Entity(object entity, IConfigurationOptions configuration, TableFactory tableCache)
                : base(entity, configuration, tableCache)
            {
                if (entity == null) throw new ArgumentNullException("entity");

                Value = entity;
            }
            #endregion

            #region Property Methods
            public List<Attribute> GetAllForeignAndPseudoKeyAttributes()
            {
                var type = Value.GetType();

                return type.GetCustomAttributes(typeof(AutoLoadKeyAttribute)).ToList();
            }

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

            public object TryGetPristineEntityPropertyValue(string propertyName)
            {
                if (!IsEntityStateTrackingOn) return null;

                var pristineEntity = _getPristineEntity();

                if (pristineEntity == null)
                {
                    // we have an insert, return null
                    return null;
                }

                var property = pristineEntity.GetType().GetProperty(propertyName);

                if (property == null) throw new Exception(string.Format("Cannot find property name: {0}.  Search failed in pristine entity", propertyName));

                return property.GetValue(pristineEntity);
            }


            public static void SetPropertyValue(object parent, object child, string propertyNameToSet)
            {
                if (parent == null) return;

                var foreignKeyProperty =
                    GetForeignKeys(parent)
                        .First(
                            w =>
                                (w.PropertyType.IsList()
                                    ? w.PropertyType.GetGenericArguments()[0]
                                    : w.PropertyType) == child.GetType() &&
                                w.Name == propertyNameToSet);

                var foreignKeyAttribute = foreignKeyProperty.GetCustomAttribute<ForeignKeyAttribute>();

                if (foreignKeyProperty.PropertyType.IsList())
                {
                    var parentPrimaryKey = GetPrimaryKeys(parent).First();
                    var value = parent.GetType().GetProperty(parentPrimaryKey.Name).GetValue(parent);

                    ObjectLoader.SetPropertyInfoValue(child, foreignKeyAttribute.ForeignKeyColumnName, value);
                }
                else
                {
                    var childPrimaryKey = GetPrimaryKeys(child).First();
                    var value = child.GetType().GetProperty(childPrimaryKey.Name).GetValue(child);

                    ObjectLoader.SetPropertyInfoValue(parent, foreignKeyAttribute.ForeignKeyColumnName, value);
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

            protected IReadOnlyList<IModificationItem> ModificationItems { get; set; }

            // table can have two foreign keys of the same time, this will tell them apart
            private readonly string _uniqueKey;
            #endregion

            #region Constructor
            public ModificationEntity(object entity, string uniqueKey, IConfigurationOptions configuration, TableFactory tableCache)
                : base(entity, configuration, tableCache)
            {
                _uniqueKey = uniqueKey;
            }
            #endregion

            #region Methods
            public IReadOnlyList<IModificationItem> Changes()
            {
                // this is for updates and we should never update a PK
                return ModificationItems.Where(w => w.IsModified && !w.IsPrimaryKey).ToList();
            }

            public void CalculateChanges(IConfigurationOptions configuration)
            {
                var primaryKeys = GetPrimaryKeys();

                // make sure the table is valid
                _isEntityValid(Value);

                var hasPrimaryKeysOnly = HasPrimaryKeysOnly();
                var areAnyPkGenerationOptionsNone = false;

                UpdateType = UpdateType.Skip;

                // check to see if anything has updated, if not we can skip everything
                _setChanges();

                // if there are no changes then exit
                if (State == EntityState.UnChanged) return;

                // validate all max length attributes
                _checkMaxLengthViolations(Value, Type);

                UpdateType = UpdateType.Insert;

                // need to find the Update Type
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var pkValue = key.GetValue(Value);
                    var generationOption = GetGenerationOption(key);

                    // make sure the primary key is not null
                    _checkPkNotNull(pkValue, key);

                    // check to see if we are updating or not
                    var isUpdating = !IsKeyInInsertArray(configuration, pkValue);

                    if (generationOption == DbGenerationOption.None) areAnyPkGenerationOptionsNone = true;

                    // break because we are already updating, do not want to set to false
                    if (!isUpdating)
                    {
                        if (generationOption != DbGenerationOption.None) continue;

                        // check to see if FK corresponds to PK, if it does and its one-one,
                        // then check to see if the FK property has a value, if it does then
                        // we are ok because it will be saved before its parent
                        var foreignKeyCheck =
                            AllProperties.FirstOrDefault(
                                w =>
                                    w.GetCustomAttribute<ForeignKeyAttribute>() != null &&
                                    w.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName == key.Name);

                        if (foreignKeyCheck != null)
                        {
                            var value = GetPropertyValue(foreignKeyCheck);

                            if (value != null) continue;
                        }

                        var pseudoKeyCheck =
                            AllProperties.FirstOrDefault(
                                w =>
                                    w.GetCustomAttribute<PseudoKeyAttribute>() != null &&
                                    w.GetCustomAttribute<PseudoKeyAttribute>().ParentTableColumnName == key.Name);

                        if (pseudoKeyCheck != null)
                        {
                            var value = GetPropertyValue(pseudoKeyCheck);

                            if (value != null) continue;
                        }

                        // if the db generation option is none and there is no pk value this is an error because the db doesnt generate the pk
                        //throw new SqlSaveException(string.Format("Primary Key must not be an insert value when DbGenerationOption is set to None.  Primary Key Name: {0}, Table: {1}", key.Name, ToString(TableNameFormat.Plain)));
                    }

                    // If we have only primary keys we need to perform a try insert and see if we can try to insert our data.
                    // if we have any Pk's with a DbGenerationOption of None we need to first see if a record exists for the pks, 
                    // if so we need to perform an update, otherwise we perform an insert
                    UpdateType = hasPrimaryKeysOnly ? UpdateType.TryInsert : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;

                    if (UpdateType != UpdateType.Update && UpdateType != UpdateType.TryInsertUpdate) continue;

                    // make sure identity columns were not updated
                    _checkForIdentityColumnUpdates(EntityTrackable, configuration, AllColumns);
                }

                // make sure timestamps are not updated.
                // Process here because we need to know the update type
                _validateTimestamps();
            }

            // get the keys from AllProperties.  If we are deleting,
            // all the modification items will be empty
            public IReadOnlyList<IModificationItem> Keys()
            {
                return AllProperties.Where(IsPrimaryKey).Select(w => new ModificationItem(w, _uniqueKey)).ToList();
            }

            public IReadOnlyList<IModificationItem> All()
            {
                return ModificationItems;
            }

            private void _setChanges()
            {
                if (!IsEntityStateTrackingOn)
                {
                    // mark everything has changed so it will be updated.  Skip time stamps,
                    // they are db generated and should not be inserted or updated
                    ModificationItems = AllColumns.Select(w => new ModificationItem(w, _uniqueKey)).ToList();

                    State = EntityState.Modified;
                    return;
                }

                ModificationItems = _getChanges(EntityTrackable, _uniqueKey, AllColumns);

                State = _getState(ModificationItems);
            }

            private static EntityState _getState(IReadOnlyList<IModificationItem> changes)
            {
                return changes.Any(w => w.IsModified) ? EntityState.Modified : EntityState.UnChanged;
            }

            public static EntityState GetState(EntityStateTrackable entityStateTrackable)
            {
                var changes = _getChanges(entityStateTrackable, string.Empty, GetAllColumns(entityStateTrackable));

                return _getState(changes);
            }

            public static bool IsKeyInInsertArray(IConfigurationOptions configuration, object value)
            {
                try
                {
                    var translation = SqlDbTypeTranslator.Translate(value.GetType());

                    switch (translation.ResolvedType)
                    {
                        case "INT16":
                            return configuration.InsertKeys.Int16.Contains(Convert.ToInt16(value));
                        case "INT32":
                            return configuration.InsertKeys.Int32.Contains(Convert.ToInt32(value));
                        case "INT64":
                            return configuration.InsertKeys.Int64.Contains(Convert.ToInt64(value));
                        case "GUID":
                            return configuration.InsertKeys.Guid.Contains(Guid.Parse(value.ToString()));
                        case "STRING":
                            return configuration.InsertKeys.String.Contains(value.ToString());
                        case "DATETIME":
                            return configuration.InsertKeys.DateTime.Contains(Convert.ToDateTime(value));
                        case "BOOLEAN":
                            return configuration.InsertKeys.Boolean.Contains(Convert.ToBoolean(value));
                        case "DATETIMEOFFSET":
                            return configuration.InsertKeys.DateTimeOffest.Contains(DateTimeOffset.Parse(value.ToString()));
                        case "DECIMAL":
                            return configuration.InsertKeys.Decimal.Contains(Convert.ToDecimal(value));
                        case "DOUBLE":
                            return configuration.InsertKeys.Double.Contains(Convert.ToDouble(value));
                        case "SINGLE":
                            return configuration.InsertKeys.Single.Contains(Convert.ToSingle(value));
                        case "TIMESPAN":
                            return configuration.InsertKeys.TimeSpan.Contains(TimeSpan.Parse(value.ToString()));
                        case "BYTE":
                            return configuration.InsertKeys.Byte.Contains(byte.Parse(value.ToString()));
                        case "BYTE[]":
                            return configuration.InsertKeys.ByteArray.Contains((byte[])value);
                        case "CHAR[]":
                            return configuration.InsertKeys.CharArray.Contains((char[])value);
                        default:
                            throw new Exception(string.Format("Type of {0} not allowed as primary key.", translation.ResolvedType));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error checking for insert value on Primary Key, see inner exception", ex);
                }
            }

            private static IReadOnlyList<IModificationItem> _getChanges(EntityStateTrackable entityStateTrackable, string uniqueKey, List<PropertyInfo> allColumns)
            {
                return (from item in allColumns
                    let current = _getCurrentObject(entityStateTrackable, item.Name)
                    let pristineEntity = _getPristineProperty(entityStateTrackable, item.Name)
                    let hasChanged = ObjectComparison.HasChanged(pristineEntity, current)
                    select new ModificationItem(item, uniqueKey, hasChanged)).ToList();
            }

            private void _checkMaxLengthViolations(object entity, Type type)
            {
                var properties =
                    type.GetProperties()
                        .Where(
                            w => w.GetCustomAttribute<MaxLengthAttribute>() != null && w.PropertyType == typeof (string))
                        .ToList();

                foreach (var property in properties)
                {
                    var value = (string) property.GetValue(entity);
                    var maxLengthAttribute = property.GetCustomAttribute<MaxLengthAttribute>();

                    if (value == null || value.Length <= maxLengthAttribute.Length) continue;

                    if (maxLengthAttribute.ViolationType == MaxLengthViolationType.Truncate)
                    {
                        ObjectLoader.SetPropertyInfoValue(entity, property, value.Substring(0, maxLengthAttribute.Length));
                        continue;
                    }

                    throw new MaxLengthException(string.Format("Max Length violated on column: {0}", property.Name));
                }
            }

            private void _isEntityValid(object entity)
            {
                var primaryKeys = GetPrimaryKeys(entity.GetType());
                var plainTableName = GetTableName(entity);

                if (primaryKeys.Count == 0)
                {
                    throw new InvalidTableException(string.Format("{0} must have at least one Primary Key defined", plainTableName));
                }
            }

            private void _checkForIdentityColumnUpdates(EntityStateTrackable entityStateTrackable, IConfigurationOptions configuration, IReadOnlyList<PropertyInfo> columns)
            {
                // cannot check if entity state trackable is null (entity state tracking is off) or if the pristine entity is null (insert)
                if (entityStateTrackable == null) return;

                List<string> errorColumns;

                var allIdentityColumns = columns.Where(
                    w =>
                        w.GetCustomAttribute<DbGenerationOptionAttribute>() != null &&
                        w.GetCustomAttribute<DbGenerationOptionAttribute>().Option ==
                        DbGenerationOption.IdentitySpecification).ToList();

                if (allIdentityColumns.Count == 0) return;

                // entity state tracking is on, check to see if the identity column has been updated
                if (GetPristineEntity(entityStateTrackable) == null)
                {
                    // any identity columns should be zero/null or whatever the insert value is
                    errorColumns = (from column in allIdentityColumns
                                    let value = column.GetValue(entityStateTrackable)
                                    let hasError = !IsKeyInInsertArray(configuration, value)
                                    where hasError
                                    select column.Name).ToList();
                }
                else
                {
                    // can only check when entity state tracking is on
                    // only can get here when updating, try insert, or try insert update
                    errorColumns =
                        allIdentityColumns.Where(
                            column => HasColumnChanged(entityStateTrackable, column.Name))
                            .Select(w => w.Name)
                            .ToList();
                }

                if (!errorColumns.Any()) return;

                const string error = "Cannot update value of IDENTITY column.  Column: {0}\r\r";
                var message = errorColumns.Aggregate(string.Empty,
                    (current, item) => string.Concat(current, string.Format(error, item)));

                throw new SqlSaveException(message);
            }

            private void _checkPkNotNull(object pkValue, MemberInfo key)
            {
                if (pkValue == null) throw new SqlSaveException(string.Format("Primary Key cannot be null: {0}", GetColumnName(key)));
            }

            private void _validateTimestamps()
            {
                var timeStamps = ModificationItems.Where(w => w.DbTranslationType == SqlDbType.Timestamp).ToList();

                // check to see if timestamp has changed
                foreach (var item in timeStamps)
                {
                    var value = GetProperty(item.DatabaseColumnName).GetValue(Value);

                    // cannot have a value in timestamp column when inserting
                    if (value != null && 
                        (UpdateType == UpdateType.Insert || UpdateType == UpdateType.TryInsert || UpdateType == UpdateType.TryInsertUpdate))
                    {
                        throw new SqlSaveException(string.Format("Cannot insert value into Timestamp column.  Column Name - {0}", item.DatabaseColumnName));
                    }

                    if (IsEntityStateTrackingOn && 
                        (UpdateType == UpdateType.Update || UpdateType == UpdateType.TryInsertUpdate) && 
                        HasColumnChanged(EntityTrackable, item.PropertyName))
                    {
                        throw new SqlSaveException(string.Format("Cannot update value in Timestamp column.  Column Name - {0}", item.DatabaseColumnName));
                    }
                }
            }

            public static bool HasColumnChanged(EntityStateTrackable entity, string propertyName)
            {
                // only check when the pristine entity is not null, try insert update will fail otherwise.
                // if the pristine entity is null then there is nothing to compare to
                var pristineEntity = _getPristineProperty(entity, propertyName);
                var current = _getCurrentObject(entity, propertyName);

                return ObjectComparison.HasChanged(pristineEntity, current);
            }

            private static object _getCurrentObject(EntityStateTrackable entity, string propertyName)
            {
                return entity.GetType().GetProperty(propertyName).GetValue(entity);
            }

            private static object _getPristineProperty(EntityStateTrackable entity, string propertyName)
            {
                var tableOnLoad = GetPristineEntity(entity);

                return tableOnLoad == null ? new object() // need to make sure the current doesnt equal the current if the pristine entity is null
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
                    var items = table.GetType().GetProperties().Where(IsColumn).ToList();

                    for (var i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var value = item.GetValue(table);

                        instance.GetType().GetProperty(item.Name).SetValue(instance, value);
                    }

                    return instance;
                }
            }

            #endregion
        }

        private class SqlDbTypeResult
        {
            public string SqlDbTypeString { get; private set; }

            public string ResolvedType { get; private set; }

            public SqlDbType SqlDbType { get; private set; }

            public SqlDbTypeResult(string sqlDbTypeString, string resolvedType, SqlDbType sqlDbType)
            {
                SqlDbType = sqlDbType;
                ResolvedType = resolvedType;
                SqlDbTypeString = sqlDbTypeString;
            }
        }

        private static class SqlDbTypeTranslator
        {
            /// sqldbtype, string name,
            public static SqlDbTypeResult Translate(Type type, string precision = null)
            {
                var sqlDbType = SqlDbType.VarChar;
                var resolvedType = type.GetUnderlyingType().Name.ToUpper();

                switch (resolvedType)
                {
                    case "INT16":
                        sqlDbType = SqlDbType.SmallInt;
                        break;
                    case "INT32":
                        sqlDbType = SqlDbType.Int;
                        break;
                    case "INT64":
                        sqlDbType = SqlDbType.BigInt;
                        break;
                    case "GUID":
                        sqlDbType = SqlDbType.UniqueIdentifier;
                        break;
                    case "STRING":
                        sqlDbType = SqlDbType.VarChar;
                        break;
                    case "DATETIME":
                        sqlDbType = SqlDbType.DateTime;
                        break;
                    case "BOOLEAN":
                        sqlDbType = SqlDbType.Bit;
                        break;
                    case "DATETIMEOFFSET":
                        sqlDbType = SqlDbType.DateTimeOffset;
                        break;
                    case "DECIMAL":
                        sqlDbType = SqlDbType.Decimal;
                        break;
                    case "DOUBLE":
                        sqlDbType = SqlDbType.Float;
                        break;
                    case "SINGLE":
                        sqlDbType = SqlDbType.Real;
                        break;
                    case "OBJECT":
                        sqlDbType = SqlDbType.Variant;
                        break;
                    case "TIMESPAN":
                        sqlDbType = SqlDbType.Time;
                        break;
                    case "BYTE":
                        sqlDbType = SqlDbType.TinyInt;
                        break;
                    case "XML":
                        sqlDbType = SqlDbType.Xml;
                        break;
                    case "BYTE[]":
                        sqlDbType = SqlDbType.VarBinary;
                        break;
                    case "CHAR[]":
                        sqlDbType = SqlDbType.Text;
                        break;
                }

                return new SqlDbTypeResult(TranslateSqlDbType(sqlDbType, precision), resolvedType, sqlDbType);
            }

            public static string TranslateSqlDbType(SqlDbType sqlDbType, string precision = null)
            {
                switch (sqlDbType)
                {
                    case SqlDbType.Variant:
                        return "sql_variant";

                    case SqlDbType.Decimal:
                        return string.Format("{0}({1})", sqlDbType, precision ?? "18,6");

                    case SqlDbType.Date:
                    case SqlDbType.Xml:
                    case SqlDbType.TinyInt:
                    case SqlDbType.Text:
                    case SqlDbType.Timestamp:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.SmallInt:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.UniqueIdentifier:
                    case SqlDbType.Real:
                    case SqlDbType.NText:
                    case SqlDbType.Image:
                    case SqlDbType.Money:
                    case SqlDbType.Int:
                    case SqlDbType.Float:
                    case SqlDbType.DateTime:
                    case SqlDbType.Bit:
                    case SqlDbType.BigInt:
                        return sqlDbType.ToString();

                    case SqlDbType.DateTimeOffset:
                    case SqlDbType.DateTime2:
                    case SqlDbType.Time:
                        return string.Format("{0}({1})", sqlDbType, precision ?? "7");

                    case SqlDbType.NChar:
                    case SqlDbType.Binary:
                    case SqlDbType.Char:
                        return string.Format("{0}({1})", sqlDbType, precision ?? "8000");

                    case SqlDbType.VarChar:
                    case SqlDbType.NVarChar:
                    case SqlDbType.VarBinary:
                        return string.Format("{0}({1})", sqlDbType, precision ?? "MAX");

                    default:
                        throw new ArgumentOutOfRangeException(sqlDbType.ToString());
                }
            }
        }

        public class ModificationItem : IModificationItem
        {
            #region Properties

            public bool IsModified { get; private set; }

            public string SqlDataTypeString { get; private set; }

            public string PropertyDataType { get; private set; }

            public string PropertyName { get; private set; }

            public string DatabaseColumnName { get; private set; }

            public SqlDbType DbTranslationType { get; private set; }

            public bool IsPrimaryKey { get; private set; }

            public DbGenerationOption Generation { get; private set; }

            public bool TranslateDataType { get; private set; }

            public int? MaxLength { get; private set; }

            public bool NeedsAlias
            {
                get { return DatabaseColumnName != PropertyName; }
            }

            private readonly string _uniqueParentKey;

            private string _uniqueKey;
            #endregion

            #region Constructor

            public ModificationItem(PropertyInfo property, string uniqueParentKey, bool isModified = true)
            {
                // each column needs a unqiue key so we can created a variable if needed for it.
                // when using transactions is it possible for a property to be in the query 
                // twice, we need to avoid this
                _uniqueParentKey = uniqueParentKey;

                PropertyName = property.Name;
                DatabaseColumnName = Table.GetColumnName(property);
                IsPrimaryKey = ReflectionCacheTable.IsColumnPrimaryKey(property);
                PropertyDataType = property.PropertyType.Name.ToUpper();
                Generation = IsPrimaryKey ? Table.GetGenerationOption(property) : property.GetCustomAttribute<DbGenerationOptionAttribute>() != null ? property.GetCustomAttribute<DbGenerationOptionAttribute>().Option : DbGenerationOption.None;

                var maxLengthAttribute = property.GetCustomAttribute<MaxLengthAttribute>();

                MaxLength = maxLengthAttribute != null ? (int?) maxLengthAttribute.Length : null;

                // set in case entity tracking isnt on
                IsModified = isModified;

                // check for sql data translation, used mostly for datetime2 inserts and updates
                var translation = property.GetCustomAttribute<DbTypeAttribute>();

                if (translation != null)
                {
                    DbTranslationType = translation.Type;
                    SqlDataTypeString = SqlDbTypeTranslator.TranslateSqlDbType(translation.Type, MaxLength != null ? MaxLength.Value.ToString() : translation.Precision);
                    TranslateDataType = true;
                    return;
                }

                var translationResult = SqlDbTypeTranslator.Translate(property.PropertyType, MaxLength != null ? MaxLength.Value.ToString() : null);
                DbTranslationType = translationResult.SqlDbType;
                SqlDataTypeString = translationResult.SqlDbTypeString;
            }

            #endregion

            #region Methods
            public string GetUniqueKey()
            {
                if (!string.IsNullOrEmpty(_uniqueKey)) return _uniqueKey;

                _uniqueKey = string.Format("{0}{1}", _uniqueParentKey, PropertyName);

                return _uniqueKey;
            }

            public string AsOutput(string appendToEnd)
            {
                return string.Format("[INSERTED].[{0}]{1}{2}", DatabaseColumnName, NeedsAlias ? string.Format(" as [{0}]", PropertyName) : string.Empty, appendToEnd);
            }

            public string AsField(string appendToEnd)
            {
                return string.Format("[{0}]{1}", DatabaseColumnName, appendToEnd);
            }

            public string AsFieldPropertyName(string appendToEnd)
            {
                return string.Format("[{0}]{1}", PropertyName, appendToEnd);
            }

            #endregion
        }
    }
}

