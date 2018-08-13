using FastMember;
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
            var tableTypes = new List<Type> { typeof(T) };

            // create object map all the way to the property, this
            // will be the key so we can build an alias for a table
            // For each type, store list of different maps that exist for that type

            // can we put levels on the types we loop through?
            // level 0 = main object, level 1 = any child objects, 2 = children of children?

            // can we create a list of lists to loop through? 
            // We need to know where in the current object we are,
            // If we do not know we cannot properly create an alias
            var objectReader = new ObjectReader<T>();

            while (objectReader.Read())
            {
                var record = objectReader.GetRecord();

                // skip anything already mapped
                if (currentMap.ContainsKey(record.Type)) { continue; }

                var tableSchema = new TableSchema(record.Type.GetTableName(), currentMap.Count);
                var columns = new List<ColumnSchema>();

                foreach (var member in record.Members)
                {
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
