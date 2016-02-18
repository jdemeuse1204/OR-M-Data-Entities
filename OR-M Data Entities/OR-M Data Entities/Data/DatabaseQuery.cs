/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseQuery : DatabaseExecution
    {
        #region Properties and Fields
        private static QuerySchematicManager _schematicManager { get; set; }

        #endregion

        #region Constructor

        protected DatabaseQuery(string connectionStringOrName)
            : base(connectionStringOrName)
        {
            _schematicManager = new QuerySchematicManager();
        }
        #endregion

        #region Expression Query Methods

        public IExpressionQuery<T> From<T>()
        {
            // grab the base table and reset it
            var table = Tables.Find<T>(Configuration);

            // get the schematic
            var schematic = _schematicManager.FindAndReset(table, Configuration);
            
            // initialize the selected columns
            schematic.InitializeSelect(Configuration.IsLazyLoading);

            return new ExpressionQuery<T>(this, schematic);
        }

        public T Find<T>(params object[] pks)
        {
            // grab the base table
            var table = Tables.Find<T>(Configuration);

            // get the schematic
            var schematic = _schematicManager.FindAndReset(table, Configuration);

            var expressionQuery = new ExpressionQuery<T>(this, schematic);

            // initialize the selected columns, we tell it false always for find so the whole object will be returned
            // Find does not have options to include or exclude tables
            schematic.InitializeSelect(false);

            // resolve the pks for the where statement
            expressionQuery.ResolveFind<T>(pks, Configuration);

            return ((IExpressionQuery<T>) expressionQuery).FirstOrDefault();
        }

        #endregion

        #region Methods

        public override void Dispose()
        {
            _schematicManager = null;
            base.Dispose();
        }

        #endregion

        #region helpers

        // manages the schematics for all queries
        private class QuerySchematicManager
        {
            public QuerySchematicManager()
            {
                _mappings = new Dictionary<Type, IQuerySchematic>();
                _mappedTables = new HashSet<IMappedTable>();
            }

            // cache for all current maps
            private IDictionary<Type, IQuerySchematic> _mappings { get; set; }

            // cache for all mapped tables
            private HashSet<IMappedTable> _mappedTables { get; set; }

            public IQuerySchematic FindAndReset(ITable table, IConfigurationOptions configuration)
            {
                IQuerySchematic result;

                _mappings.TryGetValue(table.Type, out result);

                // create map if not found
                if (result != null)
                {
                    // reset the previous selected items
                    result.Reset();

                    return result;
                }

                result = _createSchematic(table, configuration);

                _mappings.Add(table.Type, result);

                // reset the previous selected items
                result.Reset();

                return result;
            }

            // creates the schematic so we know how to load the object
            private IQuerySchematic _createSchematic(ITable from, IConfigurationOptions configuration)
            {
                const string aliasString = "AkA{0}";

                // base table is always included
                var initMappedTable = new MappedTable(from, string.Format(aliasString, 0), from.ToString(TableNameFormat.Plain), true);
                var tables = new List<IMappedTable> { initMappedTable };

                // create the schematic so the Data loader knows how to populate the object
                // save the base object so we can search for children that were already processed
                IDataLoadSchematic baseDataLoadSchematic = new DataLoadSchematic(null, from.Type, from.Type, initMappedTable, null);

                // set the current schematic
                var currentDataLoadSchematic = baseDataLoadSchematic;

                for (var i = 0; i < tables.Count; i++)
                {
                    var currentMappedTable = tables[i];
                    var autoLoadProperties = currentMappedTable.Table.AutoLoadKeyRelationships;

                    // tables cannot have the same alias, we are ok to match on alias only
                    var parentOfCurrent = i == 0
                        ? null
                        : tables.FirstOrDefault(w => w.Alias == currentMappedTable.Alias);

                    foreach (var autoLoadRelationship in autoLoadProperties)
                    {
                        var nextAlias = string.Format(aliasString, tables.Count);
                        var currentMappedtable = _mappedTables.FirstOrDefault(w => w.Key == autoLoadRelationship.ParentColumn.PropertyName);

                        if (currentMappedtable == null)
                        {
                            var tableSchematic =
                                Tables.Find(
                                    autoLoadRelationship.AutoLoadPropertyColumn.PropertyType.GetUnderlyingType(),
                                    configuration);

                            currentMappedtable = new MappedTable(tableSchematic, nextAlias, autoLoadRelationship.AutoLoadPropertyColumn.PropertyName, !configuration.IsLazyLoading);

                            _mappedTables.Add(currentMappedtable);
                        }

                        if (i > 0 && autoLoadProperties.Any())
                        {
                            if (parentOfCurrent == null)
                            {
                                throw new Exception(string.Format("Failed to find parent table when creating table read schematic.  Table - {0}", currentMappedTable.Table.DatabaseName));
                            }

                            currentDataLoadSchematic = _findDataLoadSchematic(
                                baseDataLoadSchematic,
                                currentDataLoadSchematic,
                                autoLoadRelationship.ChildColumn.Table.Type,
                                parentOfCurrent.Key,
                                parentOfCurrent.Alias);
                        }

                        currentDataLoadSchematic.Children.Add(new DataLoadSchematic(currentDataLoadSchematic,
                            currentMappedtable.Table.Type,
                            autoLoadRelationship.AutoLoadPropertyColumn.PropertyType,
                            currentMappedtable,
                            currentMappedtable.Key));

                        tables.Add(currentMappedtable);

                        _processMappedTable(currentMappedtable, currentMappedTable, autoLoadRelationship.AutoLoadPropertyColumn);
                    }
                }

                return new QuerySchematic(from.Type, tables, baseDataLoadSchematic, configuration);
            }

            private IDataLoadSchematic _findDataLoadSchematic(IDataLoadSchematic beginningSchematic, IDataLoadSchematic currentSchematic,
                Type childTableType, string parentJoinPropertyName, string parentTableAlias)
            {
                var firstSearch =
                       currentSchematic.Children.FirstOrDefault(
                           w =>
                               w.Type == childTableType &&
                               w.PropertyName == parentJoinPropertyName);

                if (firstSearch != null) return firstSearch;

                // do not want a reference to the list and mess up the schematic
                var schematicsToSearch = new List<IDataLoadSchematic>();

                schematicsToSearch.AddRange(beginningSchematic.Children);

                for (var i = 0; i < schematicsToSearch.Count; i++)
                {
                    var schematic = schematicsToSearch[i];

                    // auto load key columns could technically have the same name, match on parent table too
                    if (schematic.PropertyName == parentJoinPropertyName && schematic.MappedTable.Alias == parentTableAlias) return schematic;

                    schematicsToSearch.AddRange(schematic.Children);
                }

                throw new Exception(string.Format("Cannot find foreign key instance of by name.  NAME: {0}, TYPE: {1}", parentJoinPropertyName, childTableType.Name));
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

            private string _getRelationshipSql(IMappedTable currentMappedTable, IMappedTable parentTable, IColumn column,
                RelationshipType relationshipType)
            {
                const string sql = "{0} JOIN {1} ON {2} = {3}";
                const string tableColumn = "[{0}].[{1}]";
                var alias = string.Format("{0} AS [{1}]", currentMappedTable.Table.ToString(TableNameFormat.SqlWithSchema),
                    currentMappedTable.Alias);
                var fkAttribute = column.GetCustomAttribute<ForeignKeyAttribute>();
                var pskAttribute = column.GetCustomAttribute<PseudoKeyAttribute>();

                switch (relationshipType)
                {
                    case RelationshipType.OneToOne:
                        var oneToOneParent = string.Format(tableColumn, parentTable.Alias,
                            fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ChildTableColumnName);
                        // pk in parent
                        var oneToOneChild = string.Format(tableColumn, currentMappedTable.Alias,
                            currentMappedTable.Table.GetPrimaryKeyName(0)); // fk attribute in child

                        return string.Format(sql, "INNER", alias, oneToOneChild, oneToOneParent);
                    case RelationshipType.OneToMany:
                        var oneToManyParent = string.Format(tableColumn, parentTable.Alias,
                            parentTable.Table.GetPrimaryKeyName(0)); // pk in parent
                        var oneToManyChild = string.Format(tableColumn, currentMappedTable.Alias,
                            fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ChildTableColumnName);
                        // fk attribute in parent... in other table

                        return string.Format(sql, "LEFT", alias, oneToManyChild, oneToManyParent);
                    default:
                        throw new ArgumentOutOfRangeException("relationshipType");
                }
            }

            #region helpers
            /// <summary>
            /// Schematic for loaded and reading data from the server
            /// </summary>
            private class QuerySchematic : IQuerySchematic
            {
                public QuerySchematic(Type key, List<IMappedTable> mappedTables, IDataLoadSchematic dataLoadSchematic, IConfigurationOptions configuration)
                {
                    Key = key;
                    MappedTables = mappedTables;
                    DataLoadSchematic = dataLoadSchematic;
                    ConfigurationOptions = configuration;
                }

                public IConfigurationOptions ConfigurationOptions { get; private set; }

                public Type Key { get; private set; }

                public Expression ReturnOverride { get; private set; }

                // map tells the DbReader how the object should be loaded
                public List<IMappedTable> MappedTables { get; private set; }

                // skeleton for the data being returned
                public IDataLoadSchematic DataLoadSchematic { get; private set; }

                public IMappedTable FindTable(Type type)
                {
                    return MappedTables.FirstOrDefault(w => w.Table.Type == type);
                }

                public IMappedTable FindTable(string tableKey)
                {
                    return MappedTables.FirstOrDefault(w => w.Key == tableKey);
                }

                public bool AreForeignKeysSelected()
                {
                    return DataLoadSchematic.Children != null && DataLoadSchematic.Children.Any(w => w.MappedTable.IsIncluded);
                }

                /// <summary>
                /// need to initialize the selected columns based on
                /// if we are using lazy loading or not
                /// </summary>
                /// <param name="isLazyLoading"></param>
                public void InitializeSelect(bool isLazyLoading)
                {
                    // make sure all columns are selected so we can find
                    // the correct columns when resolving the pk column(s)
                    if (isLazyLoading)
                    {
                        var firstMappedTable = MappedTables.FirstOrDefault();

                        if (firstMappedTable == null) throw new Exception("No mapped tables when Initializing Select in Query Schematic");

                        firstMappedTable.Include();
                        firstMappedTable.SelectAll(0);
                        return;
                    }

                    var startingOrdinal = 0;

                    foreach (var mappedTable in MappedTables)
                    {
                        mappedTable.Include();
                        startingOrdinal += mappedTable.SelectAll(startingOrdinal);
                    }
                }

                public void ClearReadCache()
                {
                    // recursive clear
                    DataLoadSchematic.ClearRowReadCache();
                }

                public void Reset()
                {
                    ReturnOverride = null;

                    foreach (var mappedTable in MappedTables)
                    {
                        mappedTable.Exclude();
                        mappedTable.Clear();
                    }

                    // recursive clear
                    DataLoadSchematic.ClearRowReadCache();
                }

                public void SetReturnOverride(Expression expression)
                {
                    ReturnOverride = expression;
                }
            }

            private class DataLoadSchematic : IDataLoadSchematic
            {
                public DataLoadSchematic(IDataLoadSchematic parent, Type type, Type actualType, IMappedTable mappedTable, string propertyName)
                {
                    Type = type;
                    ActualType = actualType;
                    PropertyName = propertyName;
                    PrimaryKeyNames = ReflectionCacheTable.GetPrimaryKeyNames(type).ToArray();
                    LoadedCompositePrimaryKeys = new OSchematicLoadedKeys();
                    Children = new HashSet<IDataLoadSchematic>();
                    MappedTable = mappedTable;
                    Parent = parent;
                }

                public HashSet<IDataLoadSchematic> Children { get; private set; }
                public IDataLoadSchematic Parent { get; private set; }

                public Type ActualType { get; private set; }

                public string[] PrimaryKeyNames { get; private set; }

                public OSchematicLoadedKeys LoadedCompositePrimaryKeys { get; private set; }

                public object ReferenceToCurrent { get; set; }

                public IMappedTable MappedTable { get; private set; }

                /// <summary>
                /// used to identity Foreign Key because object can have Foreign Key with same type,
                /// but load different data.  IE - User CreatedBy, User EditedBy
                /// </summary>
                public string PropertyName { get; private set; }

                public Type Type { get; private set; }

                public void ClearRowReadCache()
                {
                    LoadedCompositePrimaryKeys = new OSchematicLoadedKeys();

                    var toClear = new List<IDataLoadSchematic>();

                    // do not keep reference to the original list
                    toClear.AddRange(Children);

                    for (var i = 0; i < toClear.Count; i++)
                    {
                        var child = toClear[i];

                        if (child.Children.Count > 0) toClear.AddRange(child.Children);

                        child.ClearLoadedCompositePrimaryKeys();
                    }
                }

                public void ClearLoadedCompositePrimaryKeys()
                {
                    LoadedCompositePrimaryKeys = new OSchematicLoadedKeys();
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

            private class SelectedColumn : ISelectedColumn
            {
                public SelectedColumn(IColumn column, int ordinal)
                {
                    Column = column;
                    Ordinal = ordinal;
                }

                public IColumn Column { get; }

                public int Ordinal { get; }
            }

            private class MappedTable : IMappedTable
            {
                public string Alias { get; private set; }

                public string Key { get; private set; } // either table name or FK/PSK property name

                public ITable Table { get; }

                public HashSet<ITableRelationship> Relationships { get; private set; }

                public HashSet<ISelectedColumn> SelectedColumns { get; private set; }

                public HashSet<ISelectedColumn> OrderByColumns { get; private set; }

                public bool IsIncluded { get; private set; }

                public MappedTable(ITable table, string alias, string key, bool isIncluded = true)
                {
                    Key = key;
                    Alias = alias;
                    Table = table;
                    Relationships = new HashSet<ITableRelationship>();
                    SelectedColumns = new HashSet<ISelectedColumn>();
                    OrderByColumns = new HashSet<ISelectedColumn>();
                    IsIncluded = isIncluded;
                }

                public void Include()
                {
                    IsIncluded = true;
                }

                public void Exclude()
                {
                    IsIncluded = false;
                }

                public void Clear()
                {
                    SelectedColumns = new HashSet<ISelectedColumn>();
                    OrderByColumns = new HashSet<ISelectedColumn>();
                }

                private IColumn _find(string propertyName)
                {
                    var column = Table.Columns.FirstOrDefault(w => w.PropertyName == propertyName);

                    if (column == null) throw new Exception(string.Format("Column not found for table {0}.  Column Name: {1}", Table.DatabaseName, propertyName));

                    return column;
                }

                public void Select(string propertyName, int ordinal)
                {
                    var column = _find(propertyName);

                    if (!column.IsSelectable) throw new Exception(string.Format("Invalid operation, column is not a selectable type.  Column - {0}", column.DatabaseColumnName));

                    _addSelectColumn(column, ordinal);
                }

                public int SelectAll(int startingOrdinal)
                {
                    var selectableColumns = Table.Columns.Where(w => w.IsSelectable).ToList();

                    foreach (var column in selectableColumns)
                    {
                        _addSelectColumn(column, startingOrdinal);
                        startingOrdinal++;
                    }

                    return selectableColumns.Count;
                }

                public void OrderByPrimaryKeys()
                {
                    var columns = Table.Columns.Where(w => w.IsPrimaryKey).ToList();

                    foreach (var column in columns) _addOrderByColumn(column, OrderByColumns.Count);
                }

                public IMappedTable OrderByPrimaryKeysInline()
                {
                    OrderByPrimaryKeys();

                    return this;
                }

                public void OrderByColumn(string propertyName)
                {
                    var column = _find(propertyName);

                    _addOrderByColumn(column, OrderByColumns.Count);
                }

                private void _addOrderByColumn(IColumn column, int ordinal)
                {
                    OrderByColumns.Add(new SelectedColumn(column, ordinal));
                }

                private void _addSelectColumn(IColumn column, int ordinal)
                {
                    SelectedColumns.Add(new SelectedColumn(column, ordinal));
                }
            }
            #endregion
        }

        private class ExpressionQuery<T> : IExpressionQuery<T>, IExpressionQueryResolvable<T>, IOrderedExpressionQuery<T>
        {
            #region Properties and Fields
            private readonly DatabaseExecution _context;

            private readonly IQuerySchematic _schematic;

            private string _orderBy { get; set; }

            private string _where { get; set; }

            private string _select { get; set; }

            private string _join { get; set; }

            private string _columns { get; set; }

            private bool _min { get; set; }

            private bool _max { get; set; }

            private bool _count { get; set; }

            private bool _distinct { get; set; }

            private int _take { get; set; }

            private bool _exists { get; set; }

            private ParameterCollection _parameters { get; set; }

            #endregion

            #region Constructor

            public ExpressionQuery(DatabaseExecution context, IQuerySchematic schematic)
            {
                _parameters = new ParameterCollection();
                _context = context;
                _schematic = schematic;
                _where = string.Empty;
                _select = string.Empty;
                _join = string.Empty;
                _columns = string.Empty;
                _distinct = false;
                _count = false;
                _min = false;
                _max = false;
                _take = 0;
                
                _orderBy = string.Empty;
            }

            #endregion

            #region Methods

            public bool AreForeignKeysSelected()
            {
                return _schematic.AreForeignKeysSelected();
            }

            public void ResolveWhere(Expression<Func<T, bool>> expression)
            {
                var resolution = ExpressionQueryWhereResolver.Resolve(expression, _schematic);

                _parameters = resolution.Parameters;
                _where = resolution.Sql;
            }

            public IExpressionQuery<TResult> ResolveSelect<TResult>(IExpressionQuery<T> source, Expression<Func<T, TResult>> selector)
            {
                var resolution = ExpressionQuerySelectResolver.Resolve(selector, _schematic);

                _columns = resolution.Sql;

                // we need to tell our object loader to override the default loading 
                // and load based on the expression given
                if (resolution.ReturnOverride != null) _schematic.SetReturnOverride(resolution.ReturnOverride);

                return ExpressionQuerySelectResolver.ChangeExpressionQueryGenericType<T, TResult>(source);
            }

            public IOrderedExpressionQuery<TResult> ResolveSelect<TResult>(IOrderedExpressionQuery<T> source, Expression<Func<T, TResult>> selector)
            {
                var resolution = ExpressionQuerySelectResolver.Resolve(selector, _schematic);

                _columns = resolution.Sql;

                // we need to tell our object loader to override the default loading 
                // and load based on the expression given
                if (resolution.ReturnOverride != null) _schematic.SetReturnOverride(resolution.ReturnOverride);

                return (IOrderedExpressionQuery<TResult>)ExpressionQuerySelectResolver.ChangeExpressionQueryGenericType<T, TResult>((IExpressionQuery<T>)source);
            }

            public void ResolveFind<TResult>(object[] pks, IConfigurationOptions configuration)
            {
                var table = _schematic.FindTable(typeof(TResult));
                var resolution = ExpressionQueryWhereResolver.ResolveFind(table, pks);

                _parameters = resolution.Parameters;
                _where = resolution.Sql;
            }

            public void ResolveOrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            {
                _orderBy = string.Concat(_orderBy, string.IsNullOrEmpty(_orderBy) ? "ORDER BY " : string.Empty, ExpressionQueryOrderByResolver.ResolveOrderBy(_schematic, keySelector));
            }

            public void ResolveOrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            {
                _orderBy = string.Concat(_orderBy, string.IsNullOrEmpty(_orderBy) ? "ORDER BY " : string.Empty, ExpressionQueryOrderByResolver.ResolveOrderByDescending(_schematic, keySelector));
            }

            public void ResolveMax()
            {
                _max = true;
            }

            public void ResolveAny()
            {
                _exists = true;
            }

            public void ResolveCount()
            {
                _count = true;
            }

            public void ResolveMin()
            {
                _min = true;
            }

            public void OrderByPrimaryKeys()
            {
                _orderBy = ExpressionQueryOrderByResolver.ResolveOrderByPrimaryKeys(_schematic);
            }

            public void SelectAll()
            {
                _columns = ExpressionQuerySelectResolver.ResolveSelectAll(_schematic);
            }

            public void ResolveForeignKeyJoins()
            {
                _join = ExpressionQueryJoinResolver.ResolveForeignKeyJoins(_schematic);
            }

            public void MakeDistinct()
            {
                _distinct = true;
            }

            public void Take(int count)
            {
                _take = count;
            }

            public DataReader<TSource> ExecuteReader<TSource>()
            {
                // need to happen before read because of include statements

                // select all if nothing was selected
                if (string.IsNullOrEmpty(_columns)) SelectAll();

                // only do below if object has foreign keys
                if (_schematic.AreForeignKeysSelected())
                {
                    // order by primary keys, so we can select the data
                    // back correctly from the data reader
                    OrderByPrimaryKeys();

                    // resolve the foreign key joins
                    ResolveForeignKeyJoins();
                }

                // generate out sql statement
                return _context.ExecuteQuery<TSource>(Sql(), _parameters.ToList(), _schematic);
            }

            public void IncludeAll()
            {
                var nextOrdinal = _getNextOrdinal();
                var mappedTables = _schematic.MappedTables.Where(w => !w.IsIncluded).ToList();

                // also need to select the columns
                foreach (var mappedTable in mappedTables)
                {
                    nextOrdinal += mappedTable.SelectAll(nextOrdinal);
                    mappedTable.Include();
                }
            }

            private int _getNextOrdinal()
            {
                return
                    _schematic.MappedTables.Where(w => w.SelectedColumns.Any())
                        .Select(w => w.SelectedColumns.Select(x => x.Ordinal).Max())
                        .Max() + 1;
            }

            public void Include(string tableName)
            {
                IMappedTable mappedTable;
                var actualTableName = _getActualTableName(tableName);

                // if table names are the same search by FK matched column name
                if (tableName.Contains("["))
                {
                    var attribute = _getTableAttributeName(tableName);

                    mappedTable =
                        _schematic.MappedTables.FirstOrDefault(
                            w => w.Table.PlainTableName == actualTableName && w.Key == attribute);
                }
                else
                {
                    mappedTable = _schematic.MappedTables.FirstOrDefault(w => w.Table.PlainTableName == tableName);
                }

                if (mappedTable == null) throw new Exception(string.Format("Cannot find table  table name not found on Include. Table name - {0}", tableName));

                mappedTable.Include();
            }

            public void IncludeTo(string tableName)
            {
                var actualTableName = _getActualTableName(tableName);
                var attribute = _getTableAttributeName(tableName);
                var searchList = new List<IDataLoadSchematic> { _schematic.DataLoadSchematic };
                var nextOrdinal = _getNextOrdinal();

                // find the reference then select backwards
                for (var i = 0; i < searchList.Count; i++)
                {
                    var dataLoadSchematic = searchList[i];
                    var foundMappedTable = _findDataLoadSchematic(dataLoadSchematic, actualTableName, attribute);

                    if (foundMappedTable != null)
                    {
                        // include found item
                        if (!foundMappedTable.MappedTable.IsIncluded) nextOrdinal += foundMappedTable.MappedTable.SelectAll(nextOrdinal);

                        foundMappedTable.MappedTable.Include();

                        var parent = foundMappedTable.Parent;

                        while (parent != null)
                        {
                            if (!parent.MappedTable.IsIncluded) nextOrdinal += parent.MappedTable.SelectAll(nextOrdinal);

                            parent.MappedTable.Include();

                            parent = parent.Parent;
                        }
                        return;
                    }

                    searchList.AddRange(dataLoadSchematic.Children);
                }

                // should never hit this if table found
                throw new Exception(string.Format("Table not found for Include.  Table Name - {0}", tableName));
            }

            public void Disconnect()
            {
                _context.Disconnect();
            }

            public string Sql()
            {
                var fromTable = _schematic.MappedTables[0];
                var select = _resolveSelect();
                var columns = _resolveColumns();
                var from = _resolveFrom(fromTable);
                var where = _resolveWhere();
                var orderBy = _resolveOrderBy();
                var join = _resolveJoin();

                return string.Concat(select, columns, from, join, where, orderBy);
            }
            
            private string _getActualTableName(string tableName)
            {
                if (!tableName.Contains("[")) return tableName;

                var index = tableName.IndexOf("[", StringComparison.Ordinal);
                return tableName.Substring(0, index);
            }

            private string _getTableAttributeName(string tableName)
            {
                if (!tableName.Contains("[")) return string.Empty;

                var index = tableName.IndexOf("[", StringComparison.Ordinal);
                return tableName.Substring(index + 1, tableName.Length - index - 2);
            }

            private IDataLoadSchematic _findDataLoadSchematic(IDataLoadSchematic schematic, string tableName, string attributeName)
            {
                return string.IsNullOrEmpty(attributeName)
                    ? schematic.Children.FirstOrDefault(w => w.MappedTable.Table.PlainTableName == tableName)
                    : schematic.Children.FirstOrDefault(
                        w => w.MappedTable.Table.PlainTableName == tableName && w.MappedTable.Key == attributeName);
            }

            private string _resolveOrderBy()
            {
                return _count ? string.Empty : _orderBy.TrimEnd(',');
            }

            private string _resolveJoin()
            {
                return _join;
            }

            private string _resolveWhere()
            {
                return string.Format("{0}\r", _where);
            }

            private string _resolveFrom(IMappedTable fromTable)
            {
                return string.Format("FROM {0} AS [{1}]\r", fromTable.Table.ToString(TableNameFormat.SqlWithSchema), fromTable.Alias);
            }

            private string _resolveColumns()
            {
                // when we get here only one column will be selected for min or max, the code does not
                // allow for more.  So we can just wrap the column in a min/max function
                return _min
                    ? string.Format("\tMIN({0})", _columns)
                    : _max
                        ? string.Format("\tMAX({0})", _columns)
                        : _exists ? string.Empty : _count ? "\tCOUNT(*)\r" : string.Format("{0}\r", _columns);
            }

            private string _resolveSelect()
            {
                const string select = "SELECT{0}{1}\r";
                var distinct = _distinct ? " DISTINCT " : string.Empty;

                // do not do exists with top
                var take = _exists ? " TOP 1 1 " : _take > 0 ? string.Format(" TOP {0} ", _take) : string.Empty;

                return string.Format(select, distinct, take);
            }
            #endregion

            #region Enumeration

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var item in ExecuteReader<T>()) yield return item;

                _context.Dispose();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #region Expression Query Resolution
        private class ExpressionQueryWhereResolver : ExpressionQueryResolverBase
        {
            public static LambdaToSqlResolution Resolve<T>(Expression<Func<T, bool>> expressionQuery, IQuerySchematic schematic)
            {
                return Resolve(new ParameterCollection(), expressionQuery, schematic);
            }

            public static LambdaToSqlResolution Resolve<T>(ParameterCollection parameters, Expression<Func<T, bool>> expressionQuery, IQuerySchematic schematic)
            {
                return Resolve(schematic, parameters, expressionQuery.Body);
            }

            public static LambdaToSqlResolution Resolve(IQuerySchematic schematic, ParameterCollection parameters,
                Expression expressionQuery, bool isSubQuery = false)
            {
                Parameters = parameters;
                Order = new Queue<KeyValuePair<string, Expression>>();
                Schematic = schematic;
                Sql = string.Empty;
                IsSubQuery = isSubQuery;

                _evaluate(expressionQuery as dynamic);

                return new LambdaToSqlResolution(Sql, Parameters);
            }

            public static LambdaToSqlResolution ResolveFind(IMappedTable table, object[] pks)
            {
                Parameters = new ParameterCollection();
                Order = new Queue<KeyValuePair<string, Expression>>();
                Sql = string.Empty;

                // keys will be returned in order they are in the poco
                var primaryKeys = table.SelectedColumns.Where(w => w.Column.IsPrimaryKey).OrderBy(w => w.Ordinal).ToList();

                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var primaryKey = primaryKeys[i];
                    var pkValue = pks[i];
                    string parameterKey;

                    Parameters.Add(pkValue, out parameterKey);

                    Sql = string.Concat(Sql, string.Format("{0} ({1} = {2})\r", i == 0 ? "WHERE" : "\tAND", primaryKey.Column.ToString(table.Alias), parameterKey));
                }

                return new LambdaToSqlResolution(Sql, Parameters);
            }

            private static void _evaluate(MethodCallExpression expression)
            {
                // get the sql from the expression
                Sql = WhereUtilities.GetSqlFromExpression(expression);

                _processExpression(expression, expression.ToString(), false);
            }

            private static void _evaluate(UnaryExpression expression)
            {
                _evaluate(expression.Operand as dynamic);
            }

            private static void _evaluate(MemberExpression expression)
            {
                // get the sql from the expression
                Sql = WhereUtilities.GetSqlFromExpression(expression);

                _processExpression(expression, expression.ToString(), false);
            }

            private static void _evaluate(BinaryExpression expression)
            {
                // get the sql from the expression
                Sql = WhereUtilities.GetSqlFromExpression(expression);

                // decompile the expression and break it into individual expressions
                var expressions = new List<Expression>
                    {
                        expression
                    };

                for (var i = 0; i < expressions.Count; i++)
                {
                    var e = expressions[i];

                    if (WhereUtilities.IsFinalExpressionNodeType(e.NodeType))
                    {
                        // the key for an expression is its string value.  We can just
                        // do a ToString on the expression to get what we want.  We just 
                        // need to replace that value in the parent to get our sql value
                        var replacementString = WhereUtilities.GetReplaceString(e);

                        // process the not nodes like this because the actual
                        // expression is embedded inside of it
                        if (e.NodeType == ExpressionType.Not)
                        {
                            _processNotExpression(e, replacementString);
                            continue;
                        }

                        // process normal expression
                        _processExpression(e, replacementString, false);
                        continue;
                    }

                    expressions.Add(((BinaryExpression)e).Left);
                    expressions.Add(((BinaryExpression)e).Right);
                }
            }

            private static void _processNotExpression(dynamic item, string replacementString)
            {
                var unaryExpression = item as UnaryExpression;

                if (unaryExpression != null)
                {
                    _processExpression(unaryExpression.Operand, replacementString, true);
                    return;
                }

                var binaryExpression = item as BinaryExpression;

                if (binaryExpression != null)
                {
                    _processExpression(binaryExpression, replacementString, true);
                    return;
                }

                throw new Exception(string.Format("Expression Type not valid.  Type: {0}", item.NodeType));
            }

            private static void _processExpression(dynamic item, string replacementString, bool isNotExpressionType)
            {
                if (item.NodeType == ExpressionType.Call)
                {
                    Sql = Sql.Replace(replacementString, _getSql(item as MethodCallExpression, isNotExpressionType));
                    return;
                }

                if (item.NodeType == ExpressionType.Equal)
                {
                    // check to see if the user is using (test == true) instead of (test)
                    bool outLeft;
                    if (WhereUtilities.IsLeftBooleanValue(item, out outLeft))
                    {
                        Sql = Sql.Replace(replacementString,
                            _getSql(item.Right as MethodCallExpression, outLeft || isNotExpressionType));
                        return;
                    }

                    // check to see if the user is using (test == true) instead of (test)
                    bool outRight;
                    if (WhereUtilities.IsRightBooleanValue(item, out outRight))
                    {
                        Sql = Sql.Replace(replacementString,
                            _getSql(item.Left as MethodCallExpression, outRight || isNotExpressionType));
                        return;
                    }

                    Sql = Sql.Replace(replacementString,
                        _getSqlEquals(item as BinaryExpression, isNotExpressionType));
                    return;
                }

                if (item.NodeType == ExpressionType.GreaterThan ||
                    item.NodeType == ExpressionType.GreaterThanOrEqual ||
                    item.NodeType == ExpressionType.LessThan ||
                    item.NodeType == ExpressionType.LessThanOrEqual)
                {
                    // check to see the order the user typed the statement
                    if (WhereUtilities.IsConstant(item.Left))
                    {
                        Sql = Sql.Replace(replacementString,
                            _getSqlGreaterThanLessThan(item as BinaryExpression, isNotExpressionType, item.NodeType));
                        return;
                    }

                    // check to see the order the user typed the statement
                    if (WhereUtilities.IsConstant(item.Right))
                    {
                        Sql = Sql.Replace(replacementString,
                            _getSqlGreaterThanLessThan(item as BinaryExpression, isNotExpressionType, item.NodeType));
                        return;
                    }

                    throw new Exception("invalid comparison");
                }

                // double negative check will go into recursive check
                if (item.NodeType == ExpressionType.Not)
                {
                    _processNotExpression(item, replacementString);
                    return;
                }

                throw new ArgumentNullException(item.NodeType);
            }

            private static string _getSqlEquals(BinaryExpression expression, bool isNotExpressionType)
            {
                var comparison = isNotExpressionType ? "!=" : "=";
                var left = string.Empty;
                var right = string.Empty;

                // check to see if the left and right are not constants, if so they need to be evaulated
                if (WhereUtilities.IsConstant(expression.Left) || WhereUtilities.IsConstant(expression.Right))
                {
                    left = _getTableAliasAndColumnName(expression).GetTableAndColumnName(IsSubQuery);

                    Parameters.Add(GetValue(expression), out right);

                    return string.Format("({0} {1} {2})", left, comparison, right);
                }

                var isLeftLambdaMethod = WhereUtilities.IsLambdaMethod(expression.Left as dynamic);
                var isRightLambdaMethod = WhereUtilities.IsLambdaMethod(expression.Right as dynamic);

                if (isLeftLambdaMethod)
                {
                    // first = Select Top 1 X From Y Where X
                    left = _getSqlFromLambdaMethod(expression.Left as dynamic);
                    right = LoadColumnAndTableName(expression.Right as dynamic).GetTableAndColumnName(IsSubQuery);
                }

                if (isRightLambdaMethod)
                {
                    right = _getSqlFromLambdaMethod(expression.Right as dynamic);
                    left = LoadColumnAndTableName(expression.Left as dynamic).GetTableAndColumnName(IsSubQuery);
                }

                if (!isLeftLambdaMethod && !isRightLambdaMethod)
                {
                    right = LoadColumnAndTableName(expression.Right as dynamic).GetTableAndColumnName(IsSubQuery);
                    left = LoadColumnAndTableName(expression.Left as dynamic).GetTableAndColumnName(IsSubQuery);
                }

                return string.Format("({0} {1} {2})", left, comparison, right);
            }

            private static string _getSqlFromLambdaMethod(MemberExpression expression)
            {
                var methodCallExpression = expression.Expression as MethodCallExpression;

                if (methodCallExpression == null)
                {
                    throw new InvalidExpressionException("Expected MethodCallExpression");
                }

                var methodName = methodCallExpression.Method.Name;

                switch (methodName.ToUpper())
                {
                    case "FIRST":
                    case "FIRSTORDEFAULT":

                        dynamic lambdaExpression =
                            methodCallExpression.Arguments.FirstOrDefault(w => w.ToString().Contains("=>"));

                        if (lambdaExpression == null)
                        {
                            throw new Exception("Lambda subquery expression not found");
                        }

                        var columnName = ReflectionCacheTable.GetColumnName(expression.Member);
                        var tableName = WhereUtilities.GetTableNameFromLambdaParameter(lambdaExpression.Body);

                        var c = Resolve(Schematic, Parameters, lambdaExpression.Body, true);
                        return string.Format("(SELECT TOP 1 {0} FROM {1} {2}",
                            string.Format("[{0}].[{1}])", tableName, columnName), tableName, c.Sql);
                }

                throw new Exception(string.Format("Lambda Method not recognized.  Method Name: {0}", methodName));
            }

            private static string _getSqlGreaterThanLessThan(BinaryExpression expression, bool isNotExpressionType,
                ExpressionType comparisonType)
            {
                var aliasAndColumnName = _getTableAliasAndColumnName(expression);
                string comparison;

                switch (comparisonType)
                {
                    case ExpressionType.GreaterThan:
                        comparison = IsValueOnRight(expression) ? ">" : "<";
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        comparison = IsValueOnRight(expression) ? ">=" : "<=";
                        break;
                    case ExpressionType.LessThan:
                        comparison = IsValueOnRight(expression) ? "<" : ">";
                        break;
                    case ExpressionType.LessThanOrEqual:
                        comparison = IsValueOnRight(expression) ? "<=" : ">=";
                        break;
                    default:
                        throw new Exception(string.Format("Comparison not valid.  Comparison Type: {0}",
                            comparisonType));
                }

                string parameter;

                Parameters.Add(GetValue(expression), out parameter);

                var result = string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery),
                    comparison, parameter);

                return isNotExpressionType ? string.Format("(NOT{0})", result) : result;
            }

            private static string _getSqlEquality(MethodCallExpression expression, bool isNotExpressionType)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
                var value = GetValue(expression as dynamic);
                var comparison = isNotExpressionType ? "!=" : "=";
                string parameter;

                Parameters.Add(value, out parameter);

                return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery),
                    comparison, parameter);
            }

            private static string _getSqlStartsEndsWith(MethodCallExpression expression, bool isNotExpressionType,
                bool isStartsWith)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
                var value = GetValue(expression as dynamic);
                var comparison = isNotExpressionType ? "NOT LIKE" : "LIKE";
                string parameter;

                Parameters.Add(string.Format(isStartsWith ? "{0}%" : "%{0}", value), out parameter);

                return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery),
                    comparison, parameter);
            }

            private static string _getSqlContains(MethodCallExpression expression, bool isNotExpressionType)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic);
                var value = GetValue(expression);
                var isEnumerable = IsEnumerable(value.GetType());
                var comparison = isEnumerable
                    ? (isNotExpressionType ? "NOT IN ({0})" : "IN ({0})")
                    : (isNotExpressionType ? "LIKE" : "NOT LIKE");

                if (!isEnumerable)
                {
                    string containsParameter;
                    Parameters.Add(string.Format("%{0}%", value), out containsParameter);

                    return string.Format("{0} {1} {2}", aliasAndColumnName.GetTableAndColumnName(IsSubQuery), comparison, containsParameter);
                }

                var inString = string.Empty;

                foreach (var item in ((ICollection)value))
                {
                    string inParameter;
                    Parameters.Add(item, out inParameter);

                    inString = string.Concat(inString, string.Format("{0},", inParameter));
                }

                return string.Format("({0} {1})", aliasAndColumnName.GetTableAndColumnName(IsSubQuery),
                    string.Format(comparison, inString.TrimEnd(',')));
            }

            private static string _getSql(MethodCallExpression expression, bool isNotExpressionType)
            {
                //var isConverting =
                //    expression.Arguments.Any(
                //        w =>
                //            w is MethodCallExpression &&
                //            ((MethodCallExpression)w).Method.DeclaringType == typeof(DbTransform) &&
                //            ((MethodCallExpression)w).Method.Name == "Convert");
                var methodName = expression.Method.Name;

                switch (methodName.ToUpper())
                {
                    case "EQUALS":
                    case "OP_EQUALITY":
                        return _getSqlEquality(expression, isNotExpressionType);
                    case "CONTAINS":
                        return _getSqlContains(expression, isNotExpressionType);
                    case "STARTSWITH":
                        return _getSqlStartsEndsWith(expression, isNotExpressionType, true);
                    case "ENDSWITH":
                        return _getSqlStartsEndsWith(expression, isNotExpressionType, false);
                }

                throw new Exception(string.Format("Method does not translate into Sql.  Method Name: {0}",
                    methodName));
            }

            private static TableColumnContainer _getTableAliasAndColumnName(BinaryExpression expression)
            {
                if (WhereUtilities.IsConstant(expression.Right))
                {
                    return LoadColumnAndTableName(expression.Left as dynamic);
                }

                return LoadColumnAndTableName(expression.Right as dynamic);
            }
        }

        private class ExpressionQueryJoinResolver : ExpressionQueryResolverBase
        {
            // needs to happen before reading
            public static string ResolveForeignKeyJoins(IQuerySchematic schematic)
            {
                var includedTables = schematic.MappedTables.Where(w => w.IsIncluded).ToList();

                return includedTables.Aggregate(string.Empty,
                    (current1, includedTable) =>
                        includedTable.Relationships.Where(w => w.ChildTable.IsIncluded)
                            .Aggregate(current1,
                                (current, relationship) => current + string.Format("{0}\r", relationship.Sql)));
            }
        }

        private class ExpressionQueryOrderByResolver : ExpressionQuerySelectResolver
        {
            public static string ResolveOrderByPrimaryKeys(IQuerySchematic schematic)
            {
                var result = schematic.MappedTables.Where(w => w.IsIncluded).Select(w => w.OrderByPrimaryKeysInline())
                    .Aggregate("",
                        (current1, table) =>
                            current1 +
                            table.OrderByColumns.Aggregate("",
                                (current, selectedColumn) =>
                                    current + selectedColumn.Column.ToString(table.Alias, " ASC,"))).TrimEnd(',');

                return string.Format("ORDER BY {0}", result);
            }

            public static string ResolveOrderBy<T, TKey>(IQuerySchematic schematic, Expression<Func<T, TKey>> keySelector)
            {
                // make sure table being selected is in the query
                TableColumnContainer tableAndColumnName = LoadColumnAndTableName(keySelector.Body as dynamic);

                _makeSureTableIsInQuery(schematic, tableAndColumnName);

                return string.Format("{0} ASC,", tableAndColumnName.GetTableAndColumnName(false));
            }

            public static string ResolveOrderByDescending<T, TKey>(IQuerySchematic schematic, Expression<Func<T, TKey>> keySelector)
            {
                TableColumnContainer tableAndColumnName = LoadColumnAndTableName(keySelector.Body as dynamic);

                _makeSureTableIsInQuery(schematic, tableAndColumnName);

                return string.Format("{0} DESC,", tableAndColumnName.GetTableAndColumnName(false));
            }

            private static void _makeSureTableIsInQuery(IQuerySchematic schematic, TableColumnContainer tableAndColumnName)
            {
                var found = schematic.MappedTables.FirstOrDefault(w => w.Alias == tableAndColumnName.Alias);

                if (found == null || !found.IsIncluded)
                {
                    throw new OrderByException(string.Format("Cannot order by {0} from {1}, {1} is not part of the query.", tableAndColumnName.ColumnName, tableAndColumnName.TableName));
                }
            }
        }

        private class ExpressionQuerySelectResolver : ExpressionQueryResolverBase
        {
            public static Expression ReturnObject { get; private set; }

            public static SelectResolutionContainer Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> expressionQuery, IQuerySchematic schematic)
            {
                return Resolve(new ParameterCollection(), expressionQuery, schematic);
            }

            public static SelectResolutionContainer Resolve<TSource, TResult>(ParameterCollection parameters, Expression<Func<TSource, TResult>> expressionQuery, IQuerySchematic schematic)
            {
                return Resolve(schematic, parameters, expressionQuery.Body);
            }

            public static SelectResolutionContainer Resolve(IQuerySchematic schematic, ParameterCollection parameters,
                Expression expressionQuery, bool isSubQuery = false)
            {
                Parameters = parameters;
                Order = new Queue<KeyValuePair<string, Expression>>();
                Schematic = schematic;
                Sql = expressionQuery.ToString();
                IsSubQuery = isSubQuery;
                ReturnObject = null;

                _evaluate(expressionQuery as dynamic);

                Sql = string.Format("{0}\r\n", Sql.TrimEnd('\n').TrimEnd('\r').TrimEnd(','));

                return new SelectResolutionContainer(Sql, ReturnObject); ;
            }

            public static IExpressionQuery<TResultType> ChangeExpressionQueryGenericType<TSourceType, TResultType>(IExpressionQuery<TSourceType> source)
            {
                var properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
                var fields = source.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

                var schematic = fields.First(w => w.Name == "_schematic").GetValue(source);
                var context = fields.First(w => w.Name == "_context").GetValue(source);

                var instance = (IExpressionQuery<TResultType>)Activator.CreateInstance(typeof(ExpressionQuery<TResultType>), context, schematic);

                // clone propertyies
                foreach (var property in properties)
                {
                    var value = property.GetValue(source);

                    instance.GetType()
                        .GetProperty(property.Name, BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(instance, value);
                }

                return instance;
            } 

            public static string ResolveSelectAll(IQuerySchematic schematic)
            {
                var result = schematic.MappedTables.Where(w => w.IsIncluded && w.SelectedColumns.Any())
                    .OrderBy(w => w.SelectedColumns.First().Ordinal)
                    .Aggregate(string.Empty,
                        (current1, mappedTable) =>
                            string.Concat(current1,
                                mappedTable.SelectedColumns.Aggregate("",
                                    (current, selectedColumn) =>
                                        string.Concat(current, "\t",
                                            selectedColumn.Column.ToString(mappedTable.Alias, ",\r")))));

                return result.TrimEnd('\r').TrimEnd(',');
            }

            private static void _evaluate(MemberInitExpression expression)
            {
                var sql = _getSql(expression);

                Sql = Sql.Replace(expression.ToString(), sql);

                ReturnObject = expression;
            }

            private static void _evaluate(NewExpression expression)
            {
                var sql = string.Empty;

                for (var i = 0; i < expression.Arguments.Count; i++)
                {
                    var argument = expression.Arguments[i];
                    var member = expression.Members[i];

                    var tableAndColumnName = LoadColumnAndTableName((dynamic)argument);
                    var tableAndColmnNameSql = SelectUtilities.GetTableAndColumnNameWithAlias(tableAndColumnName,
                        member.Name);

                    sql = string.Concat(sql, SelectUtilities.GetSqlSelectColumn(tableAndColmnNameSql));
                }

                Sql = Sql.Replace(expression.ToString(), sql);

                ReturnObject = expression;
            }

            private static void _evaluate(MemberExpression expression)
            {
                var tableAndColumnName = LoadColumnAndTableName(expression);
                var tableAndColumnNameSql = tableAndColumnName.GetTableAndColumnName(false);

                Sql = Sql.Replace(expression.ToString(), SelectUtilities.GetSqlSelectColumn(tableAndColumnNameSql));
            }

            private static string _getSql(MemberInitExpression expression)
            {
                var sql = string.Empty;

                for (var i = 0; i < expression.Bindings.Count; i++)
                {
                    var binding = expression.Bindings[i];

                    switch (binding.BindingType)
                    {
                        case MemberBindingType.Assignment:
                            var assignment = (MemberAssignment)binding;
                            var memberInitExpression = assignment.Expression as MemberInitExpression;

                            if (memberInitExpression != null)
                            {
                                sql = string.Concat(sql, _getSql(memberInitExpression));
                                continue;
                            }

                            var tableAndColumnName = LoadColumnAndTableName((dynamic)assignment.Expression);
                            var tableAndColmnNameSql =
                                SelectUtilities.GetTableAndColumnNameWithAlias(tableAndColumnName,
                                    assignment.Member.Name);

                            sql = string.Concat(sql, SelectUtilities.GetSqlSelectColumn(tableAndColmnNameSql));
                            break;
                        case MemberBindingType.ListBinding:
                            break;
                        case MemberBindingType.MemberBinding:
                            break;
                    }
                }

                return sql;
            }
        }

        public static class WhereUtilities
        {
            public static bool IsLambdaMethod(Expression expression)
            {
                var e = expression as MemberExpression;

                if (e == null) return false;

                var methodCallExpression = e.Expression as MethodCallExpression;

                return methodCallExpression != null && methodCallExpression.ToString().Contains("=>");
            }

            public static string GetTableNameFromLambdaParameter(BinaryExpression expression)
            {
                var left = expression.Left as MemberExpression;
                var right = expression.Right as MemberExpression;

                if (left != null && left.Expression != null && left.Expression is ParameterExpression)
                {
                    return Table.GetTableName(left.Expression.Type);
                }

                if (right != null && right.Expression != null && right.Expression is ParameterExpression)
                {
                    return Table.GetTableName(right.Expression.Type);
                }
                return "";
            }

            public static string GetSqlFromExpression(Expression expression)
            {
                return string.Format("WHERE {0}",
                    expression.ToString().Replace("OrElse", "\r\n\tOR").Replace("AndAlso", "\r\n\tAND"));
            }

            // checks to see if the left side of the binary expression is a boolean value
            public static bool IsLeftBooleanValue(BinaryExpression expression, out bool value)
            {
                value = false;
                var left = expression.Left as ConstantExpression;

                if (left != null && left.Value is bool)
                {
                    value = !(bool)left.Value;
                    // invert for method that asks whether its a not expression, true = false

                    return true;
                }

                return false;
            }

            // checks to see if the right side of the binary expression is a boolean value
            public static bool IsRightBooleanValue(BinaryExpression expression, out bool value)
            {
                value = false;
                var right = expression.Right as ConstantExpression;

                if (right != null && right.Value is bool)
                {
                    value = !(bool)right.Value;
                    // invert for method that asks whether its a not expression, false = true

                    return true;
                }

                return false;
            }

            public static bool IsConstant(Expression expression)
            {
                var constant = expression as ConstantExpression;

                if (constant != null) return true;

                var memberExpression = expression as MemberExpression;

                // could be something like Guid.Empty, DateTime.Now
                if (memberExpression != null) return memberExpression.Member.DeclaringType == memberExpression.Type;

                return false;
            }

            public static string GetReplaceString(Expression expression)
            {
                return expression.ToString();
            }

            public static bool IsFinalExpressionNodeType(ExpressionType expressionType)
            {
                var finalExpressionTypes = new[]
                {
                        ExpressionType.Equal, ExpressionType.Call, ExpressionType.Lambda, ExpressionType.Not,
                        ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan,
                        ExpressionType.LessThanOrEqual
                    };

                return finalExpressionTypes.Contains(expressionType);
            }
        }

        private static class SelectUtilities
        {
            public static string GetSqlSelectColumn(string tableAndColumnName)
            {
                return string.Format("{0}{1}{2}", "\t", tableAndColumnName, "\r\n");
            }

            public static string GetTableAndColumnNameWithAlias(TableColumnContainer container, string alias)
            {
                return string.Format("{0} AS [{1}],", container.GetTableAndColumnName(false), alias);
            }
        }

        private abstract class ExpressionQueryResolverBase
        {
            protected static string Sql { get; set; }

            protected static bool IsSubQuery { get; set; }

            protected static ParameterCollection Parameters { get; set; }

            protected static Queue<KeyValuePair<string, Expression>> Order;

            protected static IQuerySchematic Schematic { get; set; }

            #region Get Value

            protected static object GetValue(ConstantExpression expression)
            {
                return expression.Value ?? "IS NULL";
            }

            protected static object GetValue(MemberExpression expression)
            {
                var objectMember = Expression.Convert(expression, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                var getter = getterLambda.Compile();

                var value = getter();

                return value ?? "IS NULL";
            }

            protected static object GetValue(MethodCallExpression expression)
            {
                var methodName = expression.Method.Name.ToUpper();

                if (methodName.Equals("CONTAINS"))
                {
                    if (expression.Object != null)
                    {
                        var type = expression.Object.Type;

                        if (IsEnumerable(type)) return _compile(expression.Object);
                    }

                    // search in arguments for the Ienumerable
                    foreach (var argument in expression.Arguments)
                    {
                        if (IsEnumerable(argument.Type)) return _compile(argument);
                    }

                    throw new ArgumentException("Comparison value not found");
                }

                if (methodName.Equals("EQUALS") || methodName.Equals("STARTSWITH") || methodName.Equals("ENDSWITH"))
                {
                    // need to look for the argument that has the constant
                    foreach (var argument in expression.Arguments)
                    {
                        var contstant = argument as ConstantExpression;

                        if (contstant != null)
                        {
                            return contstant.Value;
                        }

                        var memberExpression = argument as MemberExpression;

                        if (memberExpression == null) continue;

                        contstant = memberExpression.Expression as ConstantExpression;

                        if (contstant != null) return contstant.Value;
                    }

                    throw new ArgumentException("Comparison value not found");
                }

                return _compile(expression.Object as MethodCallExpression);
            }

            private static object _compile(Expression expression)
            {
                var objectMember = Expression.Convert(expression, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                var getter = getterLambda.Compile();

                var value = getter();

                return value ?? "IS NULL";
            }

            protected static object GetValue(UnaryExpression expression)
            {
                return GetValue(expression.Operand as dynamic);
            }

            protected static object GetValue(BinaryExpression expression)
            {
                return WhereUtilities.IsConstant(expression.Right)
                    ? GetValue(expression.Right as dynamic)
                    : GetValue(expression.Left as dynamic);
            }

            protected static bool IsValueOnRight(BinaryExpression expression)
            {
                return expression.Right is ConstantExpression;
            }

            #endregion

            #region Has Parameter

            protected static bool HasParameter(ConstantExpression expression)
            {
                return false;
            }

            protected static bool HasParameter(UnaryExpression expression)
            {
                return expression == null ? false : HasParameter(expression.Operand as dynamic);
            }

            protected static bool HasParameter(ParameterExpression expression)
            {
                return true;
            }

            protected static bool HasParameter(MemberExpression expression)
            {
                return HasParameter(expression.Expression as dynamic);
            }

            protected static bool HasParameter(MethodCallExpression expression)
            {
                var e = expression.Object;

                return e != null
                    ? HasParameter(expression.Object as dynamic)
                    : expression.Arguments.Select(arg => HasParameter(arg as dynamic))
                        .Any(hasParameter => hasParameter);
            }

            #endregion

            #region Load Table And Column Name

            protected static TableColumnContainer LoadColumnAndTableName(MemberExpression expression)
            {
                var columnAttribute = expression.Member.GetCustomAttribute<ColumnAttribute>();
                var columnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
                string alias;
                string tableName;

                if (expression.Expression.NodeType == ExpressionType.Parameter)
                {
                    alias = Schematic.FindTable(expression.Expression.Type).Alias;
                    tableName = Table.GetTableName(expression.Expression.Type);
                }
                else
                {
                    alias = Schematic.FindTable(((MemberExpression)expression.Expression).Member.Name).Alias;
                    tableName = ((MemberExpression)expression.Expression).Member.Name;
                }

                return new TableColumnContainer(tableName, columnName, alias);
            }

            protected static TableColumnContainer LoadColumnAndTableName(MethodCallExpression expression)
            {
                if (expression.Object == null)
                {
                    return
                        LoadColumnAndTableName(
                            expression.Arguments.First(w => HasParameter(w as dynamic)) as dynamic);
                }

                if (IsEnumerable(expression.Object.Type))
                {
                    foreach (var memberExpression in expression.Arguments.OfType<MemberExpression>())
                    {
                        return LoadColumnAndTableName(memberExpression);
                    }
                }

                return LoadColumnAndTableName(expression.Object as MemberExpression);
            }

            protected static TableColumnContainer LoadColumnAndTableName(UnaryExpression expression)
            {
                return LoadColumnAndTableName(expression.Operand as MemberExpression);
            }

            protected static bool IsEnumerable(Type type)
            {
                return (type.IsGenericType &&
                        type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) ||
                        type.IsArray);
            }

            #endregion
        }

        private class LambdaToSqlResolution
        {
            public LambdaToSqlResolution(string sql, ParameterCollection parameters)
            {
                Sql = sql;
                Parameters = parameters;
            }

            public readonly string Sql;

            public readonly ParameterCollection Parameters;
        }

        private class TableColumnContainer
        {
            public readonly string TableName;

            public readonly string ColumnName;

            public readonly string Alias;

            public TableColumnContainer(string tableName, string columnName, string alias)
            {
                TableName = tableName;
                ColumnName = columnName;
                Alias = alias;
            }

            public string GetTableAndColumnName(bool isSubQuery)
            {
                return isSubQuery
                    ? string.Format("[{0}].[{1}]", TableName, ColumnName)
                    : string.Format("[{0}].[{1}]", Alias, ColumnName);
            }
        }

        private class SelectResolutionContainer
        {
            public SelectResolutionContainer(string sql, Expression expressionTransform)
            {
                Sql = sql;
                ReturnOverride = expressionTransform;
            }

            public readonly string Sql;

            public readonly Expression ReturnOverride;
        }
        #endregion
        #endregion
    }
}
