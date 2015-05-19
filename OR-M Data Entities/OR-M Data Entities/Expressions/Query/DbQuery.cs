using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class DbQuery
    {
        public readonly WhereResolutionContainer WhereResolution;
        public readonly JoinResolutionContainer JoinResolution;
        public readonly Type BaseType;
        public readonly SelectInfoResolutionContainer SelectList;

        private readonly List<Type> _types;
        public IEnumerable<Type> Types {
            get { return _types; }
        }

        public object Shape { get; private set; }

        public string Sql { get; private set; }

        public DbQuery(Type baseType = null, WhereResolutionContainer whereResolution = null, SelectInfoResolutionContainer selectInfoCollection = null, JoinResolutionContainer joinResolution = null, List<Type> types = null)
        {
            WhereResolution = whereResolution ?? new WhereResolutionContainer();
            JoinResolution = joinResolution ?? new JoinResolutionContainer();
            SelectList = selectInfoCollection ?? new SelectInfoResolutionContainer();
            BaseType = baseType;
            _types = types ?? new List<Type>();
            Shape = baseType;
        }

        protected DbQuery(DbQuery query)
        {
            WhereResolution = query.WhereResolution;
            SelectList = query.SelectList;
            JoinResolution = query.JoinResolution;
            BaseType = query.BaseType;
            Shape = query.Shape;
            _types = query._types;
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

        private void _initialize(Type parentType, string parentTableName)
        {
            foreach (
                var property in
                    parentType.GetProperties().Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null))
            {
                var isList = property.PropertyType.IsList();
                var type = isList
                    ? property.PropertyType.GetGenericArguments()[0]
                    : property.PropertyType;
                var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();

                _types.Add(type);

                // add properties to select statement
                _addPropertiesByType(type);

                // add join here
                var group = new JoinGroup();
                string tableName;

                if (isList)
                {
                    group.JoinType = JoinType.ForeignKeyLeft;
                    tableName = DatabaseSchemata.GetTableName(type);

                    group.Left = new LeftJoinNode 
                    {
                        ColumnName = foreignKeyAttribute.ForeignKeyColumnName,
                        TableName = tableName,
                        TableAlias = property.Name,
                        Change = tableName != property.Name ? new ChangeTableContainer(type, property.Name) : null
                    };

                    group.Right = new RightJoinNode
                    {
                        ColumnName = DatabaseSchemata.GetColumnName(DatabaseSchemata.GetPrimaryKeys(type).First()),
                        TableName = parentTableName
                    };
                }
                else
                {
                    group.JoinType = JoinType.ForeignKeyInner;
                    tableName = DatabaseSchemata.GetTableName(type);

                    group.Left = new LeftJoinNode 
                    { 
                        ColumnName = DatabaseSchemata.GetColumnName(DatabaseSchemata.GetPrimaryKeys(type).First()),
                        TableName = DatabaseSchemata.GetTableName(type),
                        TableAlias = property.Name,
                        Change = tableName != property.Name ? new ChangeTableContainer(type, property.Name) : null
                    };

                    group.Right = new RightJoinNode
                    {
                        ColumnName = foreignKeyAttribute.ForeignKeyColumnName,
                        TableName = parentTableName
                    };
                }

                JoinResolution.AddJoin(group);

                if (DatabaseSchemata.HasForeignKeys(type))
                {
                    _initialize(type, property.Name);
                }
            }
        }

        public void Initialize()
        {
            _addPropertiesByType(BaseType);
            _types.Add(BaseType);
            _initialize(BaseType, DatabaseSchemata.GetTableName(BaseType));
        }

        public void InitializeWithoutForeignKeys()
        {
            _addPropertiesByType(BaseType);
            _types.Add(BaseType);
        }

        private void _addPropertiesByType(Type type)
        {
            var tableName = DatabaseSchemata.GetTableName(type);

            foreach (var info in type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                SelectList.Add(info, type, tableName);
            }
        }
    }

    class SqlType
    {
        public SqlType(Type type, string tableName = "")
        {
            Type = type;
            TableName = string.IsNullOrWhiteSpace(tableName) ? DatabaseSchemata.GetTableName(type) : tableName;
        }

        public Type Type { get; set; }

        public string TableName { get; set; }
    }
}
