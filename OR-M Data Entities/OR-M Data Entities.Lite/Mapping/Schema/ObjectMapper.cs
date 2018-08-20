using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    internal static class ObjectMapper
    {
        public static void Map<T>(Dictionary<Type, TableSchema> currentMap) where T : class
        {
            var type = typeof(T);
            lock (type)
            {
                var objectReader = new ObjectReader(type);

                while (objectReader.Read())
                {
                    var record = objectReader.GetRecord();

                    // skip anything already mapped
                    if (currentMap.ContainsKey(record.Type)) { continue; }

                    var tableSchema = new TableSchema(record.Type.GetTableName());
                    var columns = new List<ColumnSchema>();
                    var hasKeysOnly = true;
                    var keyCount = 0;

                    foreach (var member in record.Members)
                    {
                        if (member.GetAttribute<ForeignKeyAttribute>() != null)
                        {
                            // skip foreign keys, they are not actual columns from the db
                            continue;
                        }

                        var memberType = member.Type.Resolve();
                        var isKey = member.GetAttribute<KeyAttribute>() != null;

                        if (isKey)
                        {
                            keyCount++;
                        }
                        else
                        {
                            hasKeysOnly = false;
                        }

                        columns.Add(new ColumnSchema
                        {
                            PropertyName = member.Name,
                            ColumnName = member.GetColumnName(),
                            IsKey = member.GetAttribute<KeyAttribute>() != null
                        });
                    }

                    tableSchema.Columns = columns.OrderByDescending(w => w.IsKey).ToList();
                    tableSchema.HasKeysOnly = hasKeysOnly;
                    tableSchema.KeyCount = keyCount;

                    foreach (var column in columns)
                    {
                        if (column.IsKey == false)
                        {
                            column.IsFirstNonKey = true;
                            break;
                        }
                    }

                    currentMap.Add(record.Type, tableSchema);
                }
            }
        }
    }
}
