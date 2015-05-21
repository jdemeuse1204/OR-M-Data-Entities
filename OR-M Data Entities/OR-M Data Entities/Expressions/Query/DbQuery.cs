using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class DbQuery : DbQueryJoinLoading
    {
        public DbQuery(Type baseType = null,
            WhereResolutionContainer whereResolution = null,
            SelectInfoResolutionContainer selectInfoCollection = null,
            JoinResolutionContainer joinResolution = null,
            List<Type> types = null)
            : base(baseType, whereResolution, selectInfoCollection, joinResolution, types)
        {
        }

        protected DbQuery(DbQueryBase query)
            : base(query)
        {
        }

        public void SetShape(object shape)
        {
            Shape = shape;
        }

        public void Resolve()
        {
            // joins can change table names because of Foreign Keys, get name changes
            var changeTables = JoinResolution.GetChangeTableContainers();

            foreach (var changeTable in changeTables)
            {
                SelectList.ChangeTable(changeTable);
            }

            var where = WhereResolution.HasItems ? WhereResolution.Resolve() : string.Empty;

            var select = SelectList.HasItems ? SelectList.Resolve() : string.Empty;

            var join = JoinResolution.HasItems ? JoinResolution.Resolve() : string.Empty;

            var from = DatabaseSchemata.GetTableName(BaseType);

            Sql = string.Format("SELECT {0}{1} {2} FROM {3} {4} {5} {6}",
                SelectList.IsSelectDistinct ? " DISTINCT" : string.Empty,
                SelectList.TakeRows > 0 ? string.Format("TOP {0}", SelectList.TakeRows) : string.Empty, select,
                from.Contains("[") ? from : string.Format("[{0}]", from),
                join,
                string.Format("WHERE {0}", where),
                "");
        }

        private void _initialize(Type parentType, string parentTableName, bool isParentSearch = true)
        {
            foreach (
                var property in
                    parentType.GetProperties().Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null))
            {
                var isList = property.PropertyType.IsList();
                var type = isList
                    ? property.PropertyType.GetGenericArguments()[0]
                    : property.PropertyType;
                var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
                var pseudoKeyAttribute = property.GetCustomAttribute<PseudoKeyAttribute>();

                _types.Add(type);

                // add properties to select statement
                _addPropertiesByType(type, property.Name, SelectList.GetNextTableReadName());

                // add join here
                if (foreignKeyAttribute != null)
                {
                    JoinResolution.AddJoin(GetJoinGroup(foreignKeyAttribute, property, parentTableName, isParentSearch));
                }
                else if (pseudoKeyAttribute != null)
                {
                    JoinResolution.AddJoin(GetJoinGroup(pseudoKeyAttribute, property, parentTableName, isParentSearch));
                }

                if (DatabaseSchemata.HasForeignKeys(type))
                {
                    _initialize(type, property.Name, false);
                }
            }
        }

        public void Initialize()
        {
            InitializeWithoutForeignKeys();
            _initialize(BaseType, DatabaseSchemata.GetTableName(BaseType));
        }

        public void InitializeWithoutForeignKeys()
        {
            _addPropertiesByType(BaseType, string.Empty, SelectList.GetNextTableReadName());
            _types.Add(BaseType);
        }

        private void _addPropertiesByType(Type type, string foreignKeyTableName, string queryTableName)
        {
            var tableName = DatabaseSchemata.GetTableName(type);

            foreach (var info in type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                SelectList.Add(info, type, tableName, queryTableName, foreignKeyTableName);
            }
        }
    }
}
