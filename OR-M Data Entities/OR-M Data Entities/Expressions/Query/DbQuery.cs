using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
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

            if (where == null && select == null && join == null)
            {
                
            }
        }

        private void _createSelectList(Type startType)
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

                foreach (var info in properties)
                {
                    SelectList.Add(info, item.Type, item.TableName);
                }

                types.AddRange(foreignKeyTypes);
            }
        }

        public void CreateSelectList()
        {
            _createSelectList(BaseType);
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
