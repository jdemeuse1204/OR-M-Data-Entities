using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Query
{
    public class DbQuery
    {
        public readonly WhereResolutionContainer WhereResolution;
        public readonly JoinResolutionContainer JoinResolution;
        public readonly Type BaseType;
        public readonly SelectInfoResolutionContainer SelectList;

        public object Shape { get; private set; }

        public string Sql { get; private set; }

        public DbQuery(Type baseType = null, WhereResolutionContainer whereResolution = null, SelectInfoResolutionContainer selectInfoCollection = null, JoinResolutionContainer joinResolution = null)
        {
            WhereResolution = whereResolution ?? new WhereResolutionContainer();
            JoinResolution = joinResolution ?? new JoinResolutionContainer();
            SelectList = selectInfoCollection ?? new SelectInfoResolutionContainer();
            BaseType = baseType;
        }

        protected DbQuery(DbQuery query)
        {
            WhereResolution = query.WhereResolution;
            SelectList = query.SelectList;
            JoinResolution = query.JoinResolution;
            BaseType = query.BaseType;
            Shape = query.Shape;
        }

        public void SetShape(object shape)
        {
            Shape = shape;
        }

        public void Resolve()
        {
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

            // add in our parameters
        }

        private void _initialize(Type startType)
        {
            var types = new List<SqlType> { new SqlType(startType) };

            for (var i = 0; i < types.Count; i++)
            {
                var item = types[i];
                var properties = item.Type.GetProperties();
                var foreignKeyTypes = 
                    properties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null)
                        .Select(
                            w =>
                                new SqlType(
                                    w.PropertyType.IsList() ? w.PropertyType.GetGenericArguments()[0] : w.PropertyType,
                                    w.Name));
                var parentPrimaryKeyName = DatabaseSchemata.GetColumnName(DatabaseSchemata.GetPrimaryKeys(item).First());
                var parentTableName = DatabaseSchemata.GetTableName(item);

                foreach (var info in properties)
                {
                    SelectList.Add(info, item.Type, item.TableName);

                    var fkAttribute = info.GetCustomAttribute<ForeignKeyAttribute>();

                    // foreign key joins.  List = left, single = inner
                    if (fkAttribute != null)
                    {
                        var group = new JoinGroup();

                        if (info.PropertyType.IsList())
                        {
                            group.JoinType = JoinType.ForeignKeyLeft;
                            group.Left = new JoinNode
                            {
                                ColumnName = parentPrimaryKeyName,
                                TableName = parentTableName
                            };

                            group.Right = new JoinNode
                            {
                                ColumnName = fkAttribute.ForeignKeyColumnName,
                                TableName = parentTableName
                            };
                        }
                        else
                        {
                            
                        }

                        JoinResolution.AddJoin(group);
                    }
                }

                types.AddRange(foreignKeyTypes);
            }
        }

        public void Initialize()
        {
            _initialize(BaseType);
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
