using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Expressions.Query
{
    internal static class SqlCreator
    {
        public static string Create<T>(IReadOnlyDictionary<Type, TableSchema> tableSchemas)
        {
            var types = new List<Type> { typeof(T) };
            var columns = new StringBuilder();
            var joins = new StringBuilder();
            var where = new StringBuilder();
            var from = new StringBuilder();

            for (var i = 0; i < types.Count; i++)
            {
                var tableSchema = tableSchemas[types[i]];
                var alias = CreateAlias(tableSchema.Index);

                foreach (var column in tableSchema.Columns)
                {
                    if (column.ForeignKey != null)
                    {
                        if (column.ForeignKey.MustLeftJoin && column.ForeignKey.IsNullableKey == false)
                        {
                            types.Add(column.ForeignKey.ChildType);

                            // Left Join
                            var childTable = tableSchemas[column.ForeignKey.ChildType];
                            var parentColumn = tableSchema.Columns.First(w => w.IsKey);
                            var childColumn = childTable.Columns.First(w => w.PropertyName == column.ForeignKey.Attribute.ForeignKeyColumnName);
                            var childAlias = CreateAlias(childTable.Index);

                            joins.AppendLine($"\tLeft Join [dbo].[{childTable.Name}] {childAlias} on [{childAlias}].[{childColumn.ColumnName}] = [{alias}].[{parentColumn.ColumnName}]");
                        }
                        else
                        {
                            Type tableType;
                            if (column.ForeignKey.IsNullableKey)
                            {
                                tableType = column.ForeignKey.ChildType;
                                types.Add(column.ForeignKey.ChildType);
                            }
                            else
                            {
                                tableType = column.ForeignKey.ParentType;
                                types.Add(column.ForeignKey.ParentType);
                            }

                            // Inner Join
                            var childTable = tableSchemas[tableType];
                            var childColumn = childTable.Columns.First(w => w.IsKey);
                            var parentColumn = tableSchema.Columns.First(w => w.PropertyName == column.ForeignKey.Attribute.ForeignKeyColumnName);
                            var childAlias = CreateAlias(childTable.Index);

                            joins.AppendLine($"\t{(column.ForeignKey.IsNullableKey ? "Left" : "Inner")} Join [dbo].[{childTable.Name}] {childAlias} on [{childAlias}].[{childColumn.ColumnName}] = [{alias}].[{parentColumn.ColumnName}]");
                        }
                    }
                    else
                    {
                        columns.AppendLine($"\t[{alias}].[{column.ColumnName}],");
                    }
                }

                if (i == 0)
                {
                    from.AppendLine($"[dbo].[{tableSchema.Name}] {alias}");
                }
            }

            return $@"
Select
{columns.ToString().TrimEnd('\r', '\n', ',')}
From {from.ToString()}
{joins.ToString()}
Where {1}
";
        }

        private static string CreateAlias(int index)
        {
            return $"AKA{index}";
        }
    }
}
