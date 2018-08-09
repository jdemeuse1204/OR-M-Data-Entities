using FastMember;
using OR_M_Data_Entities.Lite.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Extensions
{
    public static class MappingAttributeExtensions
    {
        public static string GetTableName(this Type type)
        {
            var tableAttribute = type.GetAttribute<TableAttribute>();

            return tableAttribute != null ? tableAttribute.Name : type.Name;
        }

        public static string GetColumnName(this Member member)
        {
            var columnAttribute = member.GetAttribute<ColumnAttribute>();

            return columnAttribute != null ? columnAttribute.Name : member.Name;
        }
    }
}
