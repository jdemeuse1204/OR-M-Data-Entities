using FastMember;
using OR_M_Data_Entities.Lite.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    internal static class ObjectMapper
    {
        public static void Map(Type type, Dictionary<Type, TableSchema> currentMap)
        {
            var tableTypes = new List<Type> { type };

            for (var i = 0; i < tableTypes.Count; i++)
            {
                var tableType = tableTypes[i];

                // skip anything already mapped
                if (currentMap.ContainsKey(tableType)) { continue; }

                var mainTypeAccessor = TypeAccessor.Create(tableType);
                var members = mainTypeAccessor.GetMembers();
                var tableSchema = new TableSchema(tableType.GetTableName(), currentMap.Count);
                var columns = new List<ColumnSchema>();

                tableSchema.TypeAccessor = mainTypeAccessor;

                foreach (var member in members)
                {
                    var foreignKey = GetForeignKeySchema(member, members, tableType);
                    var memberType = member.Type.Resolve();

                    if (foreignKey != null && !tableTypes.Contains(memberType) && !currentMap.ContainsKey(memberType))
                    {
                        tableTypes.Add(memberType);
                    }

                    columns.Add(new ColumnSchema
                    {
                        PropertyName = member.Name,
                        ColumnName = member.GetColumnName(),
                        ForeignKey = foreignKey,
                        IsKey = member.GetAttribute<KeyAttribute>() != null
                    });
                }

                tableSchema.Columns = columns;

                currentMap.Add(tableType, tableSchema);
            }
        }

        private static ForeignKeySchema GetForeignKeySchema(Member member, MemberSet members, Type currentType)
        {
            var foreignKeyAttribute = member.GetAttribute<ForeignKeyAttribute>();
            ForeignKeySchema result = null;

            if (foreignKeyAttribute != null)
            {
                var mustLeftJoin = false;
                var isNullable = false;
                var isList = member.Type.IsList();
                Type parentColumn;
                Type childColumn;

                if (!isList)
                {
                    // Is not a list
                    var foundMember = members.First(w => w.Name == foreignKeyAttribute.ForeignKeyColumnName);

                    if (foundMember.Type.IsNullableType())
                    {
                        mustLeftJoin = true;
                        isNullable = true;

                        // set the join column names
                        childColumn = member.Type.Resolve();
                        parentColumn = currentType;
                    }
                    else
                    {
                        // set the join column names
                        childColumn = currentType;
                        parentColumn = member.Type;
                    }
                }
                else
                {
                    // Is a list
                    mustLeftJoin = true;

                    // set the join column names
                    childColumn = member.Type.Resolve();
                    parentColumn = currentType;
                }


                result = new ForeignKeySchema
                {
                    Attribute = foreignKeyAttribute,
                    ParentType = parentColumn,
                    ChildType = childColumn,
                    MustLeftJoin = mustLeftJoin,
                    IsNullableKey = isNullable
                };
            }

            return result;
        }
    }
}
