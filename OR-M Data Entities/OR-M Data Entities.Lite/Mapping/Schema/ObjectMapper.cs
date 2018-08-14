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
            var objectReader = new ObjectReader(typeof(T));

            while (objectReader.Read())
            {
                var record = objectReader.GetRecord();

                // skip anything already mapped
                if (currentMap.ContainsKey(record.Type)) { continue; }

                var tableSchema = new TableSchema(record.Type.GetTableName());
                var columns = new List<ColumnSchema>();

                foreach (var member in record.Members)
                {
                    if (member.GetAttribute<ForeignKeyAttribute>() != null)
                    {
                        // skip foreign keys, they are not actual columns from the db
                        continue;
                    }

                    var memberType = member.Type.Resolve();

                    columns.Add(new ColumnSchema
                    {
                        PropertyName = member.Name,
                        ColumnName = member.GetColumnName(),
                        IsKey = member.GetAttribute<KeyAttribute>() != null
                    });
                }
                
                tableSchema.Columns = columns;

                currentMap.Add(record.Type, tableSchema);
            }
        }
    }
}
