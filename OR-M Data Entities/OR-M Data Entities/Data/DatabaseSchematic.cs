using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Definition.Rules;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseSchematic : Database
    {
        #region Fields

        protected static IDictionary<Type, ITable> TableCache { get; private set; }
        #endregion

        protected DatabaseSchematic(string connectionStringOrName) 
            : base(connectionStringOrName)
        {
            TableCache = new Dictionary<Type, ITable>();
        }

        public ITable GetTable(Type type)
        {
            ITable table;
            TableCache.TryGetValue(type, out table);

            if (table != null) return table;

            table = new Table(type, "dbo");

            TableCache.Add(type, table);

            return table;
        }

        public ITable GetTable<T>()
        {
            return GetTable(typeof (T));
        }

        protected class Table : ReflectionCacheTable, ITable
        {
            #region Constructor
            public Table(object entity)
                : this(entity.GetType())
            {

            }

            public Table(Type type, string defaultSchema = "dbo")
                : base(type, defaultSchema)
            {
                ClassName = type.Name;
            }

            #endregion

            #region Properties

            public bool IsReadOnly
            {
                get { return ReadOnlyAttribute != null; }
            }

            public bool IsUsingLinkedServer
            {
                get { return LinkedServerAttribute != null; }
            }

            public bool IsLookupTable
            {
                get { return LookupTableAttribute != null; }
            }

            public string ClassName { get; private set; }

            public string ServerName
            {
                get { return LinkedServerAttribute == null ? string.Empty : LinkedServerAttribute.ServerName; }
            }

            public string DatabaseName
            {
                get { return LinkedServerAttribute == null ? string.Empty : LinkedServerAttribute.DatabaseName; }
            }

            public string SchemaName
            {
                get
                {
                    return LinkedServerAttribute == null
                        ? SchemaAttribute == null ? DefaultSchema : SchemaAttribute.SchemaName
                        : LinkedServerAttribute.SchemaName;
                }
            }
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
            #endregion
        }

        protected abstract class ReflectionCacheTable
        {
            #region Constructor
            protected ReflectionCacheTable(Type type, string defaultSchema)
            {
                // Make sure the table is setup correctly
                RuleProcessor.ProcessRule<IsTableValidRule>(Type);

                AllProperties = type.GetProperties().ToList();
                Type = type;

                LinkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>(); 
                TableAttribute = type.GetCustomAttribute<TableAttribute>();
                SchemaAttribute = type.GetCustomAttribute<SchemaAttribute>();
                ReadOnlyAttribute = type.GetCustomAttribute<ReadOnlyAttribute>();
                LookupTableAttribute = type.GetCustomAttribute<LookupTableAttribute>();

                DefaultSchema = defaultSchema;
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


            protected readonly string DefaultSchema;

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

        protected class Entity : Table, IEquatable<Entity>
        {
            #region Properties and Fields

            public readonly object Value;

            public bool IsEntityStateTrackingOn
            {
                get { return Value != null && EntityTrackable != null; }
            }

            // check inheritance at table level instead
            protected EntityStateTrackable EntityTrackable
            {
                get { return Value as EntityStateTrackable; }
            }

            #endregion

            #region Constructor
            public Entity(object entity)
                : base(entity)
            {
                if (entity == null) throw new ArgumentNullException("Entity");

                Value = entity;
            }
            #endregion

            #region Primary Key Methods
            public static DbGenerationOption GetGenerationOption(PropertyInfo column)
            {
                var dbGenerationColumn = column.GetCustomAttribute<DbGenerationOptionAttribute>();
                return dbGenerationColumn == null ? DbGenerationOption.IdentitySpecification : dbGenerationColumn.Option;
            }
            #endregion

            #region Column Methods

            #endregion

            #region Foreign Key Methods
            public List<JoinColumnPair> GetAllForeignKeysAndPseudoKeys(Guid expressionQueryId, string viewId)
            {
                var autoLoadProperties = string.IsNullOrWhiteSpace(viewId)
                    ? Type.GetProperties().Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null)
                    : Type.GetProperties()
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
                                new PartialColumn(property.GetPropertyType(),
                                    fkAttribute != null
                                        ? property.PropertyType.IsList()
                                            ? fkAttribute.ForeignKeyColumnName
                                            : GetPrimaryKeys(property.PropertyType).First().Name
                                        : pskAttribute.ChildTableColumnName),
                            ParentColumn =
                                new PartialColumn(Type,
                                    fkAttribute != null
                                        ? property.PropertyType.IsList()
                                            ? GetPrimaryKeys(Type).First().Name
                                            : fkAttribute.ForeignKeyColumnName
                                        : pskAttribute.ParentTableColumnName),
                            JoinType =
                                property.PropertyType.IsList()
                                    ? JoinType.Left
                                    : Type.GetProperty(fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ParentTableColumnName).PropertyType.IsNullable()
                                        ? JoinType.Left
                                        : JoinType.Inner,
                            JoinPropertyName = property.Name,
                            FromType = property.PropertyType
                        }).ToList();
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
    }
}
