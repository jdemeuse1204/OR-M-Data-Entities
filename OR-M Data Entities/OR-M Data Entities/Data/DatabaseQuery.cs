using System.Collections.Generic;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseQuery : DatabaseSchematic
    {
        protected DatabaseQuery(string connectionStringOrName) 
            : base(connectionStringOrName)
        {
        }

        #region Properties and Fields
        private readonly ITable _from;

        private readonly string _viewId;
        #endregion

        #region Methods

        public object Resolve<T>(ExpressionQuery<T> query)
        {
            return "";
        }

        private void _initialize()
        {
            var aliasString = "AkA{0}";
            var initMappedTable = new MappedTable(_from, string.Format(aliasString, 0), _from.ToString(TableNameFormat.Plain));
            var tables = new List<MappedTable> { initMappedTable };

            for (var i = 0; i < tables.Count; i++)
            {
                var mappedTable = tables[i];
                var nextAlias = string.Format("AkA{0}", i);
                var autoLoadProperties = mappedTable.Table.GetAllForeignAndPseudoKeys();

                foreach (var property in autoLoadProperties)
                {
                    var table = GetTable(property.GetPropertyType());
                }
            }


            //return (from property in autoLoadProperties
            //        let fkAttribute = property.GetCustomAttribute<ForeignKeyAttribute>()
            //        let pskAttribute = property.GetCustomAttribute<PseudoKeyAttribute>()
            //        select new JoinColumnPair
            //        {
            //            ChildColumn =
            //                new PartialColumn(expressionQueryId, property.GetPropertyType(),
            //                    fkAttribute != null
            //                        ? property.PropertyType.IsList()
            //                            ? fkAttribute.ForeignKeyColumnName
            //                            : GetPrimaryKeys(property.PropertyType).First().Name
            //                        : pskAttribute.ChildTableColumnName),
            //            ParentColumn =
            //                new PartialColumn(expressionQueryId, _fromType,
            //                    fkAttribute != null
            //                        ? property.PropertyType.IsList()
            //                            ? GetPrimaryKeys(_fromType).First().Name
            //                            : fkAttribute.ForeignKeyColumnName
            //                        : pskAttribute.ParentTableColumnName),
            //            JoinType =
            //                property.PropertyType.IsList()
            //                    ? JoinType.Left
            //                    : _fromType.GetProperty(fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ParentTableColumnName).PropertyType.IsNullable()
            //                        ? JoinType.Left
            //                        : JoinType.Inner,
            //            JoinPropertyName = property.Name,
            //            FromType = property.PropertyType
            //        }).ToList();
        }
        #endregion

        #region helpers
        private class MappedTable
        {
            public readonly string Alias;

            public readonly string Key;

            public readonly ITable Table;

            public MappedTable(ITable table, string alias, string key)
            {
                Key = key;
                Alias = alias;
                Table = table;
            }
        }

        private class Column : IColumn
        {
            #region Properties and Fields
            public readonly ITable Table;

            public readonly int Ordinal;

            private readonly PropertyInfo _property;

            public string PropertyName
            {
                get { return _property.Name; }
            }

            private string _databaseColumnName;
            public string DatabaseColumnName
            {
                get
                {
                    if (!string.IsNullOrEmpty(_databaseColumnName)) return _databaseColumnName;

                    _databaseColumnName = _getColumnName(_property);

                    return _databaseColumnName;
                }
            }

            public readonly bool IsForeignKey;

            public bool IsList
            {
                get { return _property != null && _property.IsList(); }
            }

            public readonly bool IsPrimaryKey;
            #endregion

            #region Constructor
            public Column(ITable table, PropertyInfo property)
            {
                Table = table;
                IsPrimaryKey = table.IsPrimaryKey(property);
                IsForeignKey = property.GetCustomAttribute<ForeignKeyAttribute>() != null;
            }
            #endregion

            #region Methods
            private string _getColumnName(PropertyInfo property)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                return columnAttribute == null ? property.Name : columnAttribute.Name;
            }
            #endregion
        }
        #endregion
    }
}
