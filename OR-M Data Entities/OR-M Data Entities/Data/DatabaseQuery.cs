using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseQuery : DatabaseExecution
    {
        #region Properties and Fields
        private readonly HashSet<QueryMap> Mappings;
        #endregion

        #region Constructor
        protected DatabaseQuery(string connectionStringOrName)
            : base(connectionStringOrName)
        {
            Mappings = new HashSet<QueryMap>();
        }
        #endregion

        #region Expression Query Methods
        public IExpressionQuery<T> From<T>()
        {
            var table = GetTable<T>();
            var map = _getMap(table);

            return new ExpressionQuery<T>(this, map);
        }

        public IExpressionQuery<T> FromView<T>(string viewId)
        {
            if (!typeof(T).IsPartOfView(viewId))
            {
                throw new ViewException(string.Format("Type Of {0} Does not contain attribute for View - {1}",
                    typeof(T).Name, viewId));
            }

            var table = GetTable<T>();
            var map = _getMap(table, viewId);

            return new ExpressionQuery<T>(this, map, viewId);
        }

        public T Find<T>(params object[] pks)
        {
            var query = (ExpressionQuery<T>)From<T>();


            query.ResolveFind();

            //((ExpressionQueryResolvable<T>)query).ResolveFind(pks);

            return query.FirstOrDefault();
        }
        #endregion

        #region Methods
        private QueryMap _getMap(ITable table, string viewId = null)
        {
            var map = Mappings.FirstOrDefault(w => w.Key == table.Type && w.ViewId == viewId);

            // create map if not found
            if (map != null) return map;

            map = _createMap(table, viewId);

            Mappings.Add(map);

            return map;
        }

        private void _processMappedTable(MappedTable currentMappedTable, MappedTable parentTable, PropertyInfo property)
        {
            string sql;
            TableRelationship relationship;

            if (property.PropertyType.IsList() || property.PropertyType.IsNullable())
            {
                sql = _getRelationshipSql(currentMappedTable, parentTable, property, RelationshipType.OneToMany);
                relationship = new TableRelationship(RelationshipType.OneToMany, sql);
                currentMappedTable.Relationships.Add(relationship);
                return;   
            }

            sql = _getRelationshipSql(currentMappedTable, parentTable, property, RelationshipType.OneToOne);
            relationship = new TableRelationship(RelationshipType.OneToOne, sql);
            currentMappedTable.Relationships.Add(relationship);
        }

        private string _getRelationshipSql(MappedTable currentMappedTable, MappedTable parentTable, PropertyInfo property, RelationshipType relationshipType)
        {
            const string sql = "{0} JOIN {1} ON {2} = {3}";
            const string tableColumn = "{0}.[{1}]";
            var alias = string.Format("{0} AS [{1}]", currentMappedTable.Table.ToString(TableNameFormat.SqlWithSchema), currentMappedTable.Alias);
            var fkAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
            var pskAttribute = property.GetCustomAttribute<PseudoKeyAttribute>();

            switch (relationshipType)
            {
                case RelationshipType.OneToOne:
                    var oneToOneParent = string.Format(tableColumn, parentTable.Table.ToString(TableNameFormat.SqlWithSchema),
                        fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ChildTableColumnName); // pk in parent
                    var oneToOneChild = string.Format(tableColumn,
                        currentMappedTable.Table.ToString(TableNameFormat.SqlWithSchema),
                        currentMappedTable.Table.GetPrimaryKeyName(0));// fk attribute in parent... in other table

                    return string.Format(sql, 
                        "INNER",
                        alias,
                        oneToOneChild,
                        oneToOneParent);
                case RelationshipType.OneToMany:
                    var oneToManyParent = string.Format(tableColumn,
                        parentTable.Table.ToString(TableNameFormat.SqlWithSchema),
                        parentTable.Table.GetPrimaryKeyName(0)); // pk in parent
                    var oneToManyChild = string.Format(tableColumn,
                        currentMappedTable.Table.ToString(TableNameFormat.SqlWithSchema),
                        fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ChildTableColumnName);// fk attribute in parent... in other table

                    return string.Format(sql,
                        "LEFT",
                        alias,
                        oneToManyChild,
                        oneToManyParent);
                default:
                    throw new ArgumentOutOfRangeException("relationshipType");
            }
        }

        private QueryMap _createMap(ITable from, string viewId)
        {
            var aliasString = "AkA{0}";
            var initMappedTable = new MappedTable(from, string.Format(aliasString, 0), from.ToString(TableNameFormat.Plain));
            var tables = new List<MappedTable> {initMappedTable};

            for (var i = 0; i < tables.Count; i++)
            {
                var parentMappedTable = tables[i];
                var nextAlias = string.Format("AkA{0}", i);
                var autoLoadProperties = parentMappedTable.Table.GetAllForeignAndPseudoKeys();

                foreach (var property in autoLoadProperties)
                {
                    var currentMappedtable = tables.FirstOrDefault(w => w.Key == property.Name);

                    if (currentMappedtable == null)
                    {
                        currentMappedtable = new MappedTable(GetTable(property.GetPropertyType()), nextAlias, property.Name);
                        tables.Add(currentMappedtable);
                    }

                    _processMappedTable(currentMappedtable, parentMappedTable, property);
                }
            }

            return new QueryMap(from.Type, viewId, tables);


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

            public readonly string Key; // either table name or FK/PSK property name

            public readonly ITable Table;

            public List<TableRelationship> Relationships;

            public MappedTable(ITable table, string alias, string key)
            {
                Key = key;
                Alias = alias;
                Table = table;
            }
        }

        private class QueryMap
        {
            public QueryMap(Type key, string viewid, List<MappedTable> map)
            {
                Key = key;
                ViewId = viewid;
                Map = map;
            }

            public readonly Type Key;

            public readonly string ViewId;

            public readonly List<MappedTable> Map;
        }

        private class TableRelationship
        {
            public TableRelationship(RelationshipType relationshipType, string sql)
            {
                RelationshipType = relationshipType;
                Sql = sql;
            }

            public readonly RelationshipType RelationshipType;

            public readonly string Sql;
        }

        private enum RelationshipType
        {
            OneToOne,
            OneToMany
        }

        private class ExpressionQuery<T> : IExpressionQuery<T>
        {
            #region Properties and Fields

            private DatabaseExecution _context { get; set; }

            private readonly string _viewId;

            private readonly QueryMap _map;

            public string Sql { get; private set; }

            #endregion

            #region Constructor

            public ExpressionQuery(DatabaseExecution context, QueryMap map, string viewId = null)
            {
                _context = context;
                _viewId = viewId;
                _map = map;
            }

            #endregion

            #region Methods

            public void ResolveFind()
            {
            }

            #endregion

            #region Enumeration

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var item in _context.ExecuteQuery(this)) yield return item;

                _context.Dispose();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion
    }
}
