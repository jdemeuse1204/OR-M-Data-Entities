using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseQuery : DatabaseExecution
    {
        #region Properties and Fields
        private HashSet<QuerySchematic> Mappings { get; set; }

        private HashSet<MappedTable> MappedTables { get; set; }
        #endregion

        #region Constructor
        protected DatabaseQuery(string connectionStringOrName)
            : base(connectionStringOrName)
        {
            Mappings = new HashSet<QuerySchematic>();
            MappedTables = new HashSet<MappedTable>();
            OnDisposing += _dispose;
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
        private QuerySchematic _getMap(ITable table, string viewId = null)
        {
            var map = Mappings.FirstOrDefault(w => w.Key == table.Type && w.ViewId == viewId);

            // create map if not found
            if (map != null) return map;

            map = _createMap(table, viewId);

            Mappings.Add(map);

            return map;
        }

        private void _processMappedTable(IMappedTable currentMappedTable, IMappedTable parentTable, IColumn column)
        {
            string sql;
            TableRelationship relationship;

            if (column.IsList || column.IsNullable)
            {
                sql = _getRelationshipSql(currentMappedTable, parentTable, column, RelationshipType.OneToMany);
                relationship = new TableRelationship(RelationshipType.OneToMany, currentMappedTable, sql);
                parentTable.Relationships.Add(relationship);
                return;   
            }

            sql = _getRelationshipSql(currentMappedTable, parentTable, column, RelationshipType.OneToOne);
            relationship = new TableRelationship(RelationshipType.OneToOne, currentMappedTable, sql);
            parentTable.Relationships.Add(relationship);
        }

        private string _getRelationshipSql(IMappedTable currentMappedTable, IMappedTable parentTable, IColumn column, RelationshipType relationshipType)
        {
            const string sql = "{0} JOIN {1} ON {2} = {3}";
            const string tableColumn = "[{0}].[{1}]";
            var alias = string.Format("{0} AS [{1}]", currentMappedTable.Table.ToString(TableNameFormat.SqlWithSchema), currentMappedTable.Alias);
            var fkAttribute = column.GetCustomAttribute<ForeignKeyAttribute>();
            var pskAttribute = column.GetCustomAttribute<PseudoKeyAttribute>();

            switch (relationshipType)
            {
                case RelationshipType.OneToOne:
                    var oneToOneParent = string.Format(tableColumn, parentTable.Alias, fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ChildTableColumnName); // pk in parent
                    var oneToOneChild = string.Format(tableColumn, currentMappedTable.Alias, currentMappedTable.Table.GetPrimaryKeyName(0)); // fk attribute in child

                    return string.Format(sql, "INNER", alias, oneToOneChild, oneToOneParent);
                case RelationshipType.OneToMany:
                    var oneToManyParent = string.Format(tableColumn, parentTable.Alias, parentTable.Table.GetPrimaryKeyName(0)); // pk in parent
                    var oneToManyChild = string.Format(tableColumn, currentMappedTable.Alias, fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ChildTableColumnName);// fk attribute in parent... in other table

                    return string.Format(sql, "LEFT", alias, oneToManyChild, oneToManyParent);
                default:
                    throw new ArgumentOutOfRangeException("relationshipType");
            }
        }

        private QuerySchematic _createMap(ITable from, string viewId)
        {
            const string aliasString = "AkA{0}";
            var initMappedTable = new MappedTable(from, string.Format(aliasString, 0), from.ToString(TableNameFormat.Plain));
            var tables = new List<IMappedTable> {initMappedTable};

            for (var i = 0; i < tables.Count; i++)
            {
                var parentMappedTable = tables[i];
                var autoLoadProperties = parentMappedTable.Table.AutoLoadKeyRelationships;

                foreach (var property in autoLoadProperties)
                {
                    var nextAlias = string.Format("AkA{0}", tables.Count);
                    var currentMappedtable =
                        MappedTables.FirstOrDefault(w => w.Key == property.ParentColumn.PropertyName) ??
                        new MappedTable(GetTable(property.AutoLoadPropertyColumn.PropertyType.GetUnderlyingType()),
                            nextAlias, property.AutoLoadPropertyColumn.PropertyName);

                    tables.Add(currentMappedtable);

                    _processMappedTable(currentMappedtable, parentMappedTable, property.AutoLoadPropertyColumn);
                }
            }

            return new QuerySchematic(from.Type, viewId, tables);
        }

        private void _dispose()
        {
            Mappings = null;
            MappedTables = null;
        }
        #endregion

        #region helpers

        private class MappedTable : IMappedTable
        {
            public string Alias { get; private set; }

            public string Key { get; private set; } // either table name or FK/PSK property name

            public ITable Table { get; private set; }

            public List<ITableRelationship> Relationships { get; private set; }

            public MappedTable(ITable table, string alias, string key)
            {
                Key = key;
                Alias = alias;
                Table = table;
                Relationships = new List<ITableRelationship>();
            }
        }

        /// <summary>
        /// Tells us which tables are involved in the query
        /// </summary>
        private class QuerySchematic : IQuerySchematic
        {
            public QuerySchematic(Type key, string viewid, List<IMappedTable> map)
            {
                Key = key;
                ViewId = viewid;
                Map = map;
            }

            public Type Key { get; private set; }

            public string ViewId { get; private set; }

            public List<IMappedTable> Map { get; private set; }

            public IMappedTable FindTable(Type type)
            {
                return Map.FirstOrDefault(w => w.Table.Type == type);
            }

            public IMappedTable FindTable(string tableKey)
            {
                return Map.FirstOrDefault(w => w.Key == tableKey);
            }
        }

        private class TableRelationship : ITableRelationship
        {
            public TableRelationship(RelationshipType relationshipType, IMappedTable childMappedTable, string sql)
            {
                RelationshipType = relationshipType;
                Sql = sql;
                ChildTable = childMappedTable;
            }

            public RelationshipType RelationshipType { get; private set; }

            public string Sql { get; private set; }

            public IMappedTable ChildTable { get; private set; }
        }

        private class ExpressionQuery<T> : IExpressionQuery<T>
        {
            #region Properties and Fields

            private DatabaseExecution _context { get; set; }

            private readonly string _viewId;

            private readonly QuerySchematic _map;

            public string Sql { get; private set; }

            #endregion

            #region Constructor

            public ExpressionQuery(DatabaseExecution context, QuerySchematic map, string viewId = null)
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
                //foreach (var item in _context.ExecuteQuery(this)) yield return item;

                //_context.Dispose();

                return null;
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
