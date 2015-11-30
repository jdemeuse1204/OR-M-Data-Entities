using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition.Base
{
    public class EntityInfo : TableInfo
    {
        #region Properties and Fields
        public readonly object Entity;

        public bool IsEntityStateTrackingOn
        {
            get { return Entity != null && EntityTrackable != null; }
        }

        protected EntityStateTrackable EntityTrackable {
            get { return Entity as EntityStateTrackable; }
        }

        private List<PropertyInfo> _properties;

        protected List<PropertyInfo> Properties
        {
            get
            {
                return _properties ??
                       (_properties = (Entity == null ? new List<PropertyInfo>() : EntityType.GetProperties().ToList()));
            }
        }
        #endregion

        #region Constructor
        public EntityInfo(object entity)
            : base(entity)
        {
            if (entity == null) throw new ArgumentNullException("Entity");

            Entity = entity;
        }
        #endregion

        #region Primary Key Methods
        public List<PropertyInfo> GetPrimaryKeys()
        {
            return _getPrimaryKeys(EntityType);
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
                   EntityType.GetProperty(columnName).GetCustomAttribute<KeyAttribute>() != null;
        }

        public bool HasPrimaryKeysOnly()
        {
            return Properties.Count(IsColumn) == Properties.Count(IsPrimaryKey);
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

        public List<PropertyInfo> GetAllColumns()
        {
            return
                Properties.Where(
                    w =>
                        w.GetCustomAttribute<UnmappedAttribute>() == null &&
                        w.GetCustomAttribute<AutoLoadKeyAttribute>() == null).ToList();
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
            return Properties.Any(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null);
        }

        public List<JoinColumnPair> GetAllForeignKeysAndPseudoKeys( Guid expressionQueryId, string viewId)
        {
            var autoLoadProperties = string.IsNullOrWhiteSpace(viewId)
                ? EntityType.GetProperties().Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null)
                : EntityType.GetProperties()
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
                            new PartialColumn(expressionQueryId, EntityType,
                                fkAttribute != null
                                    ? property.PropertyType.IsList()
                                        ? _getPrimaryKeys(EntityType).First().Name
                                        : fkAttribute.ForeignKeyColumnName
                                    : pskAttribute.ParentTableColumnName),
                        JoinType =
                            property.PropertyType.IsList()
                                ? JoinType.Left
                                : EntityType.GetProperty(fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ParentTableColumnName).PropertyType.IsNullable()
                                    ? JoinType.Left
                                    : JoinType.Inner,
                        JoinPropertyName = property.Name,
                        FromType = property.PropertyType
                    }).ToList();
        }
        #endregion

        #region Entity Methods

        #endregion
    }
}
