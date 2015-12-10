using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Enumeration;
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
    public class Entity : Table, IEquatable<Entity>
    {
        #region Properties and Fields

        public readonly object Value;

        public bool IsEntityStateTrackingOn
        {
            get { return Value != null && EntityTrackable != null; }
        }

        protected EntityStateTrackable EntityTrackable
        {
            get { return Value as EntityStateTrackable; }
        }

        private List<PropertyInfo> _properties;

        protected List<PropertyInfo> Properties
        {
            get
            {
                return _properties ??
                       (_properties = (Value == null ? new List<PropertyInfo>() : TableType.GetProperties().ToList()));
            }
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

        public static List<ForeignKeyAssociation> GetForeignKeys(object entity)
        {
            return entity.GetForeignKeys().OrderBy(w => w.PropertyType.IsList()).Select(w => new ForeignKeyAssociation(entity, w.GetValue(entity), w)).ToList();
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

        public object GetPristineEntityPropertyValue(string propertyName)
        {
            if (!IsEntityStateTrackingOn) throw new Exception("Entity State Tracking is not on, error in GetPristineEntityPropertyValue");

            var field = GetPristineEntityFieldInfo();

            var pristineEntity = field.GetValue(Value);

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
