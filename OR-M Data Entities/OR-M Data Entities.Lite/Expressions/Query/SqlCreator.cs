using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Expressions.Resolvers;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OR_M_Data_Entities.Lite.Expressions.Query
{
    internal static class SqlCreator
    {
        public static SqlQuery Create<T>(IReadOnlyDictionary<Type, TableSchema> tableSchemas, Expression<Func<T, bool>> expression = null)
        {
            var select = new StringBuilder();
            var joins = new StringBuilder();
            var where = new StringBuilder();
            var order = new StringBuilder();
            var reader = new ObjectReader(typeof(T));
            var expressionResolver = new WhereExpressionResolver(typeof(T), tableSchemas);
            var from = string.Empty;

            while (reader.Read())
            {
                var record = reader.GetRecord();
                var tableSchema = tableSchemas[record.Type];
                var alias = GetTableAlias(record.LevelId);

                foreach (var column in tableSchema.Columns)
                {
                    select.AppendLine($"\t[{alias}].[{column.ColumnName}],");

                    if (column.IsKey)
                    {
                        order.Append($"[{alias}].[{column.ColumnName}] ASC, ");
                    }
                }

                if (record.LevelId == "0")
                {
                    // from
                    from = $"[dbo].[{tableSchema.Name}] [{alias}]";
                    continue;
                }

                // join
                var parentRecord = reader.Find(record.ParentLevelId);
                var parentAlias = GetTableAlias(record.ParentLevelId);

                switch (record.ForeignKeyType) {
                    case ForeignKeyType.OneToOne:
                        // inner join
                        var oneToOneKey = tableSchema.Columns.First(w => w.IsKey == true);
                        joins.AppendLine($"\tInner Join [dbo].[{tableSchema.Name}] [{alias}] on [{alias}].[{oneToOneKey.ColumnName}] = [{parentAlias}].[{record.ForeignKeyProperty}]");
                        break;
                    case ForeignKeyType.NullableOneToOne:
                    case ForeignKeyType.LeftOneToOne:
                        // left join one to one
                        var nullableOneToOneKey = tableSchema.Columns.First(w => w.IsKey == true);
                        joins.AppendLine($"\tLeft Join [dbo].[{tableSchema.Name}] [{alias}] on [{alias}].[{nullableOneToOneKey.ColumnName}] = [{parentAlias}].[{record.ForeignKeyProperty}]");
                        break;
                    case ForeignKeyType.OneToMany:
                        // left join one to many
                        var parentTableSchema = tableSchemas[record.FromType];
                        var oneToManyKey = parentTableSchema.Columns.First(w => w.IsKey == true);
                        joins.AppendLine($"\tLeft Join [dbo].[{tableSchema.Name}] [{alias}] on [{alias}].[{record.ForeignKeyProperty}] = [{parentAlias}].[{oneToManyKey.ColumnName}]");
                        break;
                }
            }

            var whereClause = expression == null ? new ResolvedWhereExpression("", new List<SqlParameter>()) : expressionResolver.Resolve(expression);
            var query = $@"Select
{select.ToString().TrimEnd('\r', '\n', ',')}
From {from.ToString()}
{joins.ToString()}
{whereClause.Sql}
    Order By {order.ToString().TrimEnd('\r', '\n', ',', '\t', ' ')}
            ";

            return new SqlQuery(query, whereClause.Parameters);
        }

        public static string GetTableAlias(string level)
        {
            return $"AKA{level}";
        }
    }
}
