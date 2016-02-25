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
        #region Properties
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

            return new ExpressionQuery<T>(this, schematic, Configuration);
        }

        /// <summary>
        /// checks to see if the entity exists by 
        /// looking at the pks in the table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected bool Exists<T>(T entity)
        {
            var table = Tables.Find<T>(Configuration);

            // get the schematic
            var schematic = _schematicManager.FindAndReset(table, Configuration);

            var expressionQuery = new ExpressionQuery<T>(this, schematic, Configuration);

            // initialize the selected columns, we tell it false always for find so the whole object will be returned
            // Find does not have options to include or exclude tables
            schematic.InitializeSelect(false);

            // resolve the pks for the where statement
            expressionQuery.ResolveExists(entity);

            return ((IExpressionQuery<T>)expressionQuery).Any();
        }

        public T Find<T>(params object[] pks)
        {
            // grab the base table
            var table = Tables.Find<T>(Configuration);

            // get the schematic
            var schematic = _schematicManager.FindAndReset(table, Configuration);

            var expressionQuery = new ExpressionQuery<T>(this, schematic, Configuration);

            // initialize the selected columns, we tell it false always for find so the whole object will be returned
            // Find does not have options to include or exclude tables
            schematic.InitializeSelect(false);

            // resolve the pks for the where statement
            expressionQuery.ResolveFind<T>(pks, Configuration);

            return ((IExpressionQuery<T>)expressionQuery).FirstOrDefault();
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

            /// <summary>
            /// Never used with foreign keys
            /// </summary>
            /// <param name="types"></param>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public IQuerySchematic CreateTemporarySchematic(List<Type> types, IConfigurationOptions configuration, Type selectedType)
            {
                const string aliasString = "AkA{0}";
                var index = 0;
                var mappedTables = new List<IMappedTable>();
                var from = types.First();

                foreach (var currentMappedtable in from type in types
                                                   let nextAlias = string.Format(aliasString, index)
                                                   let tableSchematic = Tables.Find(type, configuration)
                                                   select new MappedTable(tableSchematic, nextAlias, nextAlias, false, true))
                {
                    mappedTables.Add(currentMappedtable);

                    index++;
                }

                var dataLoadSchematic = new DataLoadSchematic(null, selectedType, selectedType, mappedTables.First(), null);

                return new QuerySchematic(from, mappedTables, dataLoadSchematic, configuration);
            }

            // creates the schematic so we know how to load the object
            private IQuerySchematic _createSchematic(ITable from, IConfigurationOptions configuration)
            {
                const string aliasString = "AkA{0}";

                // base table is always included
                var initMappedTable = new MappedTable(from, string.Format(aliasString, 0), from.ToString(TableNameFormat.Plain), false, true);
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

                            currentMappedtable = new MappedTable(tableSchematic, nextAlias,
                                autoLoadRelationship.AutoLoadPropertyColumn.PropertyName,
                                autoLoadRelationship.IsNullableOrListJoin || (parentOfCurrent != null && parentOfCurrent.IsNullableOrListJoin),
                                !configuration.IsLazyLoading);

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
                var relationshipType = RelationshipType.OneToOne;

                // if the parent join is a left join then the rest have to be left
                // joins because if there is no data in the child the whole query will fail
                if (parentTable.IsNullableOrListJoin || column.IsAutoLoadKeyNullableOrList())
                {
                    relationshipType = !column.IsList ? RelationshipType.OneToOneLeftJoin : RelationshipType.OneToMany;

                    // process one to many join
                    sql = _getRelationshipSql(currentMappedTable, parentTable, column, relationshipType);
                    relationship = new TableRelationship(relationshipType, currentMappedTable, sql);
                    parentTable.Relationships.Add(relationship);
                    return;
                }

                // process one to one join
                sql = _getRelationshipSql(currentMappedTable, parentTable, column, relationshipType);
                relationship = new TableRelationship(relationshipType, currentMappedTable, sql);
                parentTable.Relationships.Add(relationship);
            }

            private string _getRelationshipSql(IMappedTable currentMappedTable, IMappedTable parentTable, IColumn column,
                RelationshipType relationshipType)
            {
                string columnName;
                string pskColumnName;
                const string sql = "{0} JOIN {1} ON {2} = {3}";
                const string tableColumn = "[{0}].[{1}]";
                var alias = string.Format("{0} AS [{1}]", currentMappedTable.Table.ToString(TableNameFormat.SqlWithSchema),
                    currentMappedTable.Alias);
                var fkAttribute = column.GetCustomAttribute<ForeignKeyAttribute>();
                var pskAttribute = column.GetCustomAttribute<PseudoKeyAttribute>();

                switch (relationshipType)
                {
                    case RelationshipType.OneToOneLeftJoin:
                    case RelationshipType.OneToOne:

                        var joinType = relationshipType == RelationshipType.OneToOneLeftJoin ? "LEFT" : "INNER";

                        pskColumnName = pskAttribute == null ? string.Empty : pskAttribute.ParentTableColumnName;

                        // column might be renamed, grab the correct name of the column
                        columnName = _getAutoLoadChildDatabaseColumnName(parentTable, fkAttribute, pskColumnName);

                        var oneToOneParent = string.Format(tableColumn, parentTable.Alias, columnName);
                        // pk in parent
                        var oneToOneChild = string.Format(tableColumn, currentMappedTable.Alias,
                            currentMappedTable.Table.GetPrimaryKeyName(0)); // fk attribute in child

                        return string.Format(sql, joinType, alias, oneToOneChild, oneToOneParent);
                    case RelationshipType.OneToMany:

                        pskColumnName = pskAttribute == null ? string.Empty : pskAttribute.ChildTableColumnName;

                        // column might be renamed, grab the correct name of the column
                        columnName = _getAutoLoadChildDatabaseColumnName(currentMappedTable, fkAttribute, pskColumnName);

                        var oneToManyParent = string.Format(tableColumn, parentTable.Alias,
                            parentTable.Table.GetPrimaryKeyName(0)); // pk in parent
                        var oneToManyChild = string.Format(tableColumn, currentMappedTable.Alias, columnName);
                        // fk attribute in parent... in other table

                        return string.Format(sql, "LEFT", alias, oneToManyChild, oneToManyParent);
                    default:
                        throw new ArgumentOutOfRangeException("relationshipType");
                }
            }

            private string _getAutoLoadChildDatabaseColumnName(IMappedTable table, ForeignKeyAttribute fkAttribute, string pskColumnName)
            {
                var searchName = fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskColumnName;

                // grab the column name off of the parent table
                var columnName = table.Table.GetColumnName(searchName);

                if (string.IsNullOrEmpty(columnName))
                {
                    throw new Exception(string.Format("Foreign Key property name must match property name not the column attribute name.  Foreign Key Attribute: {0}", searchName));
                }

                return columnName;
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

                public bool HasTable(Type type)
                {
                    return MappedTables.Any(w => w.Table.Type == type);
                }

                public bool HasTable(string tableKey)
                {
                    return MappedTables.Any(w => w.Key == tableKey);
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

                public IColumn Column { get; private set; }

                public int Ordinal { get; private set; }
            }

            private class MappedTable : IMappedTable
            {
                public string Alias { get; private set; }

                public string Key { get; private set; } // either table name or FK/PSK property name

                public ITable Table { get; private set; }

                public HashSet<ITableRelationship> Relationships { get; private set; }

                public HashSet<ISelectedColumn> SelectedColumns { get; private set; }

                public HashSet<ISelectedColumn> OrderByColumns { get; private set; }

                public bool IsIncluded { get; private set; }

                public bool IsNullableOrListJoin { get; private set; }

                public MappedTable(ITable table, string alias, string key, bool isNullableOrListJoin, bool isIncluded)
                {
                    Key = key;
                    Alias = alias;
                    Table = table;
                    Relationships = new HashSet<ITableRelationship>();
                    SelectedColumns = new HashSet<ISelectedColumn>();
                    OrderByColumns = new HashSet<ISelectedColumn>();
                    IsIncluded = isIncluded;
                    IsNullableOrListJoin = isNullableOrListJoin;
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

        private class ExpressionQuerySqlResolutionContainer
        {
            #region Fields And Parameters
            private string _orderBy { get; set; }

            private string _where { get; set; }

            private string _select { get; set; }

            private string _join { get; set; }

            private string _columns { get; set; }

            private bool _min { get; set; }

            private bool _max { get; set; }

            private bool _count { get; set; }

            private bool _distinct { get; set; }

            private bool _exists { get; set; }

            private int _take { get; set; }

            private IMappedTable _from { get; set; }

            private readonly ParameterCollection _parameters;
            #endregion

            #region Constructor

            public ExpressionQuerySqlResolutionContainer()
            {
                _parameters = new ParameterCollection();
            }

            public ExpressionQuerySqlResolutionContainer(ParameterCollection parameters)
            {
                // combine parameters
                if (_parameters == null) _parameters = new ParameterCollection();

                // combine parameters
                _parameters.AddRange(parameters);
            }
            #endregion

            #region Methods

            public List<SqlDbParameter> Parameters()
            {
                return _parameters.ToList();
            }

            public bool AreColumnsSelected()
            {
                return !string.IsNullOrEmpty(_columns);
            }

            public void AddParameter(object value, out string parameterKey)
            {
                _parameters.Add(value, out parameterKey);
            }

            public void AddOrderBy(string statement)
            {
                _orderBy = string.Concat(_orderBy, string.IsNullOrEmpty(_orderBy) ? "ORDER BY " : string.Empty, statement);
            }

            // if we are adding from another where clause that was already resolved
            // we need to combine the parameters
            public void AddWhere(string statement, ParameterCollection parameters)
            {
                _addWhere(statement, parameters);
            }

            public void AddWhere(string statement)
            {
                _addWhere(statement);
            }

            private void _addWhere(string statement, ParameterCollection parameters = null)
            {
                if (parameters != null) _parameters.AddRange(parameters);

                var finalStatement = statement.StartsWith("(") ? statement : string.Format("({0})", statement);

                _where = string.Concat(_where, string.Format(string.IsNullOrEmpty(_where) ? "WHERE {0}\r" : "\tAND {0}\r", finalStatement));
            }

            public void SetWhere(string statement)
            {
                _where = statement;
            }

            public void SetSelect(string statement)
            {
                _select = statement;
            }

            public void SetColumns(string columns)
            {
                _columns = columns;
            }

            public void From(IMappedTable fromTable)
            {
                _from = fromTable;
            }

            public void AddJoin(string statement)
            {
                _join = string.Concat(_join, statement);
            }

            public void MarkAsMinStatement()
            {
                _min = true;
            }

            public void MarkAsMaxStatement()
            {
                _max = true;
            }

            public void MarkAsCountStatement()
            {
                _count = true;
            }

            public void MarkAsSelectDistinct()
            {
                _distinct = true;
            }

            public void MarkAsExistsStatement()
            {
                _exists = true;
            }

            public void SetTakeAmount(int rows)
            {
                _take = rows;
            }

            public string Sql()
            {
                var fromTable = _from;
                var select = _resolveSelect();
                var columns = _resolveColumns();
                var from = _resolveFrom(fromTable);
                var where = _resolveWhere();
                var orderBy = _resolveOrderBy();
                var join = _resolveJoin();

                return string.Concat(select, columns, from, join, where, orderBy);
            }

            private string _resolveOrderBy()
            {
                return _count ? string.Empty : _orderBy != null ? _orderBy.TrimEnd(',') : string.Empty;
            }

            private string _resolveJoin()
            {
                return _join;
            }

            private string _resolveWhere()
            {
                if (string.IsNullOrEmpty(_where)) return string.Empty;

                // make sure ending is correct
                return _where.EndsWith("\r") ? _where : string.Format("{0}\r", _where);
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
                    ? string.Format("\tMIN({0})\r\n", _columns.Replace("\r", "").Replace("\n", "").Replace("\t", ""))
                    : _max
                        ? string.Format("\tMAX({0})\r\n", _columns.Replace("\r", "").Replace("\n", "").Replace("\t", ""))
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
        }

        private class ExpressionQuery<T> : IExpressionQuery<T>, IExpressionQueryResolvable<T>, IOrderedExpressionQuery<T>
        {
            #region Properties and Fields
            private readonly DatabaseExecution _context;

            private readonly ExpressionQuerySqlResolutionContainer _expressionQuerySql;

            private readonly IQuerySchematic _schematic;

            private readonly IConfigurationOptions _configuration;
            #endregion

            #region Constructor

            public ExpressionQuery(DatabaseExecution context, IQuerySchematic schematic, IConfigurationOptions configuration)
            {
                _expressionQuerySql = new ExpressionQuerySqlResolutionContainer();

                _context = context;
                _schematic = schematic;
                _configuration = configuration;
            }

            #endregion

            #region Methods
            public bool AreForeignKeysSelected()
            {
                return _schematic.AreForeignKeysSelected();
            }

            public bool HasForeignKeys()
            {
                return _schematic.DataLoadSchematic.Children != null && _schematic.DataLoadSchematic.Children.Any();
            }

            public void ResolveWhere(Expression<Func<T, bool>> expression)
            {
                ExpressionQueryWhereResolver.Resolve(expression, _expressionQuerySql, _schematic);
            }

            public IExpressionQuery<TResult> ResolveSelect<TResult>(IExpressionQuery<T> source, Expression<Func<T, TResult>> selector)
            {
                ExpressionQuerySelectResolver.Resolve(selector, _schematic, _expressionQuerySql);

                return ExpressionQuerySelectResolver.ChangeExpressionQueryGenericType<T, TResult>(source);
            }

            public IOrderedExpressionQuery<TResult> ResolveSelect<TResult>(IOrderedExpressionQuery<T> source, Expression<Func<T, TResult>> selector)
            {
                ExpressionQuerySelectResolver.Resolve(selector, _schematic, _expressionQuerySql);

                return (IOrderedExpressionQuery<TResult>)ExpressionQuerySelectResolver.ChangeExpressionQueryGenericType<T, TResult>((IExpressionQuery<T>)source);
            }

            public void ResolveFind<TResult>(object[] pks, IConfigurationOptions configuration)
            {
                var table = _schematic.FindTable(typeof(TResult));

                ExpressionQueryWhereResolver.ResolveFind(table, _expressionQuerySql, pks);
            }

            public void ResolveExists<TResult>(TResult entity)
            {
                // mark as exists so we do a select top 1 1
                _expressionQuerySql.MarkAsExistsStatement();

                var table = _schematic.FindTable(typeof(TResult));

                var keys = table.Table.GetPrimaryKeys();
                var pks = keys.Select(key => key.GetValue(entity)).ToArray();

                ExpressionQueryWhereResolver.ResolveFind(table, _expressionQuerySql, pks);
            }

            public void ResolveOrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            {
                ExpressionQueryOrderByResolver.ResolveOrderBy(_schematic, _expressionQuerySql, keySelector);
            }

            public void ResolveOrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            {
                ExpressionQueryOrderByResolver.ResolveOrderByDescending(_schematic, _expressionQuerySql, keySelector);
            }

            public void ResolveMax()
            {
                _expressionQuerySql.MarkAsMaxStatement();
            }

            public void ResolveAny()
            {
                _expressionQuerySql.MarkAsExistsStatement();
            }

            public void ResolveCount()
            {
                _expressionQuerySql.MarkAsCountStatement();
            }

            public void ResolveMin()
            {
                _expressionQuerySql.MarkAsMinStatement();
            }

            public void OrderByPrimaryKeys()
            {
                ExpressionQueryOrderByResolver.ResolveOrderByPrimaryKeys(_schematic, _expressionQuerySql);
            }

            public void SelectAll()
            {
                ExpressionQuerySelectResolver.ResolveSelectAll(_schematic, _expressionQuerySql);
            }

            public void ResolveForeignKeyJoins()
            {
                ExpressionQueryJoinResolver.ResolveForeignKeyJoins(_schematic, _expressionQuerySql);
            }

            public void MakeDistinct()
            {
                _expressionQuerySql.MarkAsSelectDistinct();
            }

            public void Take(int count)
            {
                _expressionQuerySql.SetTakeAmount(count);
            }

            public string Sql()
            {
                return _expressionQuerySql.Sql();
            }

            public DataReader<TSource> ExecuteReader<TSource>()
            {
                // need to happen before read because of include statements
                // select all if nothing was selected
                if (!_expressionQuerySql.AreColumnsSelected()) SelectAll();

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
                return _context.ExecuteQuery<TSource>(Sql(), _expressionQuerySql.Parameters(), _schematic);
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

            public IExpressionQuery<TResult> ResolveJoin<TOuter, TInner, TKey, TResult>(
                IExpressionQuery<TOuter> outer,
                IExpressionQuery<TInner> inner,
                Expression<Func<TOuter, TKey>> outerKeySelector,
                Expression<Func<TInner, TKey>> innerKeySelector,
                Expression<Func<TOuter, TInner, TResult>> resultSelector,
                JoinType joinType)
            {
                // make sure the current schematic has the inner type, if not make
                // a temporary schematic
                if (!_schematic.HasTable(typeof (TInner)))
                {
                    var types = new List<Type> {typeof (TOuter), typeof (TInner)};

                    // change the selected schematic
                    ChangeSchematic(_schematicManager.CreateTemporarySchematic(types, _configuration, typeof (TOuter)));
                }

                // combine schematics before sending them in
                ExpressionQueryJoinResolver.Resolve(outer,
                    inner,
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector,
                    joinType,
                    _expressionQuerySql,
                    _schematic);

                ExpressionQuerySelectResolver.Resolve(resultSelector, _schematic, _expressionQuerySql);

                return ExpressionQuerySelectResolver.ChangeExpressionQueryGenericType<TOuter, TResult>(outer);
            }

            // change query schematic with reflection.  it should never be changed except for special circumstances
            private void ChangeSchematic(IQuerySchematic schematic)
            {
                var field = GetType().GetField("_schematic", BindingFlags.Instance | BindingFlags.NonPublic);

                if (field == null) throw new Exception("Cannot find query schematic");

                field.SetValue(this, schematic);
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
            public static void Resolve<T>(Expression<Func<T, bool>> expressionQuery,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                _evaluate(expressionQuery.Body as dynamic, expressionQuerySql, schematic);
            }

            public static void ResolveFind(IMappedTable table,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                object[] pks)
            {
                // keys will be returned in order they are in the poco
                var primaryKeys = table.SelectedColumns.Where(w => w.Column.IsPrimaryKey).OrderBy(w => w.Ordinal).ToList();

                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var primaryKey = primaryKeys[i];
                    var pkValue = pks[i];
                    string parameterKey;

                    expressionQuerySql.AddParameter(pkValue, out parameterKey);

                    var sql = string.Format("({0} = {1})\r", primaryKey.Column.ToString(table.Alias), parameterKey);

                    expressionQuerySql.AddWhere(sql);
                }
            }

            private static void _evaluate(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                // get the sql from the expression
                var sql = WhereUtilities.GetSqlFromExpression(expression);

                _processExpression(expression, expressionQuerySql, schematic, expression.ToString(), false, ref sql);

                expressionQuerySql.AddWhere(sql);
            }

            private static void _evaluate(UnaryExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                _evaluate(expression.Operand as dynamic, expressionQuerySql, schematic);
            }

            private static void _evaluate(MemberExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                // get the sql from the expression
                var sql = WhereUtilities.GetSqlFromExpression(expression);

                _processExpression(expression, expressionQuerySql, schematic, expression.ToString(), false, ref sql);

                expressionQuerySql.AddWhere(sql);
            }

            private static void _evaluate(BinaryExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                // get the sql from the expression
                var sql = WhereUtilities.GetSqlFromExpression(expression);

                // decompile the expression and break it into individual expressions
                var expressions = new List<Expression> { expression };

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
                            _processNotExpression(e, expressionQuerySql, schematic, replacementString, ref sql);
                            continue;
                        }

                        // process normal expression
                        _processExpression(e, expressionQuerySql, schematic, replacementString, false, ref sql);
                        continue;
                    }

                    expressions.Add(((BinaryExpression)e).Left);
                    expressions.Add(((BinaryExpression)e).Right);
                }

                expressionQuerySql.AddWhere(sql);
            }

            private static void _processNotExpression(dynamic item,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                string replacementString,
                ref string sql)
            {
                var unaryExpression = item as UnaryExpression;

                if (unaryExpression != null)
                {
                    _processExpression(unaryExpression.Operand, expressionQuerySql, schematic, replacementString, true, ref sql);
                    return;
                }

                var binaryExpression = item as BinaryExpression;

                if (binaryExpression != null)
                {
                    _processExpression(binaryExpression, expressionQuerySql, schematic, replacementString, true, ref sql);
                    return;
                }

                throw new Exception(string.Format("Expression Type not valid.  Type: {0}", item.NodeType));
            }

            private static void _processExpression(dynamic item,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                string replacementString,
                bool isNotExpressionType,
                ref string sql)
            {
                if (item.NodeType == ExpressionType.Call)
                {
                    sql = sql.Replace(replacementString, _getSql(item as MethodCallExpression, expressionQuerySql, schematic, isNotExpressionType));
                    return;
                }

                if (item.NodeType == ExpressionType.Equal)
                {
                    // check to see if the user is using (test == true) instead of (test)
                    bool outLeft;
                    if (WhereUtilities.IsLeftBooleanValue(item, out outLeft))
                    {
                        sql = sql.Replace(replacementString,
                            _getSql(item.Right as MethodCallExpression, expressionQuerySql, schematic, outLeft || isNotExpressionType));
                        return;
                    }

                    // check to see if the user is using (test == true) instead of (test)
                    bool outRight;
                    if (WhereUtilities.IsRightBooleanValue(item, out outRight))
                    {
                        sql = sql.Replace(replacementString,
                            _getSql(item.Left as MethodCallExpression, expressionQuerySql, schematic, outRight || isNotExpressionType));
                        return;
                    }

                    sql = sql.Replace(replacementString,
                        _getSqlEquals(item as BinaryExpression, expressionQuerySql, schematic, isNotExpressionType));
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
                        sql = sql.Replace(replacementString,
                            _getSqlGreaterThanLessThan(item as BinaryExpression, expressionQuerySql, schematic, isNotExpressionType, item.NodeType));
                        return;
                    }

                    // check to see the order the user typed the statement
                    if (WhereUtilities.IsConstant(item.Right))
                    {
                        sql = sql.Replace(replacementString,
                            _getSqlGreaterThanLessThan(item as BinaryExpression, expressionQuerySql, schematic, isNotExpressionType, item.NodeType));
                        return;
                    }

                    throw new Exception("invalid comparison");
                }

                // double negative check will go into recursive check
                if (item.NodeType == ExpressionType.Not)
                {
                    _processNotExpression(item, expressionQuerySql, schematic, replacementString, ref sql);
                    return;
                }

                throw new ArgumentNullException(item.NodeType);
            }

            private static string _getSqlEquals(BinaryExpression expression, ExpressionQuerySqlResolutionContainer expressionQuerySql, IQuerySchematic schematic, bool isNotExpressionType)
            {
                var comparison = isNotExpressionType ? "!=" : "=";
                var left = string.Empty;
                var right = string.Empty;

                // check to see if the left and right are not constants, if so they need to be evaulated
                if (WhereUtilities.IsConstant(expression.Left) || WhereUtilities.IsConstant(expression.Right))
                {
                    var leftTableAndColumnname = _getTableAliasAndColumnName(expression, schematic);

                    left = leftTableAndColumnname.GetTableAndColumnName();

                    expressionQuerySql.AddParameter(GetValue(expression), out right);

                    return string.Format("({0} {1} {2})", left, comparison, right);
                }

                var isLeftLambdaMethod = WhereUtilities.IsLambdaMethod(expression.Left as dynamic);
                var isRightLambdaMethod = WhereUtilities.IsLambdaMethod(expression.Right as dynamic);

                if (isLeftLambdaMethod)
                {
                    // first = Select Top 1 X From Y Where X
                    left = _getSqlFromLambdaMethod(expression.Left as dynamic, expressionQuerySql, schematic);
                    right = LoadColumnAndTableName(expression.Right as dynamic, schematic).GetTableAndColumnName();
                }

                if (isRightLambdaMethod)
                {
                    right = _getSqlFromLambdaMethod(expression.Right as dynamic, expressionQuerySql, schematic);
                    left = LoadColumnAndTableName(expression.Left as dynamic, schematic).GetTableAndColumnName();
                }

                if (!isLeftLambdaMethod && !isRightLambdaMethod)
                {
                    right = LoadColumnAndTableName(expression.Right as dynamic, schematic).GetTableAndColumnName();
                    left = LoadColumnAndTableName(expression.Left as dynamic, schematic).GetTableAndColumnName();
                }

                return string.Format("({0} {1} {2})", left, comparison, right);
            }

            private static string _getSqlFromLambdaMethod(MemberExpression expression, ExpressionQuerySqlResolutionContainer expressionQuerySql, IQuerySchematic schematic)
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

                        var c = Resolve(lambdaExpression.Body, expressionQuerySql, schematic);

                        return string.Format("(SELECT TOP 1 {0} FROM {1} {2}",
                            string.Format("[{0}].[{1}])", tableName, columnName), tableName, c.Sql);
                }

                throw new Exception(string.Format("Lambda Method not recognized.  Method Name: {0}", methodName));
            }

            private static string _getSqlGreaterThanLessThan(BinaryExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType,
                ExpressionType comparisonType)
            {
                var aliasAndColumnName = _getTableAliasAndColumnName(expression, schematic);
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

                expressionQuerySql.AddParameter(GetValue(expression), out parameter);

                var result = string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(),
                    comparison, parameter);

                return isNotExpressionType ? string.Format("(NOT{0})", result) : result;
            }

            private static string _getSqlEquality(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic, schematic);
                var value = GetValue(expression as dynamic);
                var comparison = isNotExpressionType ? "!=" : "=";
                string parameter;

                expressionQuerySql.AddParameter(value, out parameter);

                return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(),
                    comparison, parameter);
            }

            private static string _getSqlStartsEndsWith(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType,
                bool isStartsWith)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic, schematic);
                var value = GetValue(expression as dynamic);
                var comparison = isNotExpressionType ? "NOT LIKE" : "LIKE";
                string parameter;

                expressionQuerySql.AddParameter(string.Format(isStartsWith ? "{0}%" : "%{0}", value), out parameter);

                return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(),
                    comparison, parameter);
            }

            private static string _getSqlContains(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic, schematic);
                var value = GetValue(expression);
                var isEnumerable = IsEnumerable(value.GetType());
                var comparison = isEnumerable
                    ? (isNotExpressionType ? "NOT IN ({0})" : "IN ({0})")
                    : (isNotExpressionType ? "NOT LIKE" : "LIKE");

                if (!isEnumerable)
                {
                    string containsParameter;
                    expressionQuerySql.AddParameter(string.Format("%{0}%", value), out containsParameter);

                    return string.Format("{0} {1} {2}", aliasAndColumnName.GetTableAndColumnName(), comparison, containsParameter);
                }

                var inString = string.Empty;

                foreach (var item in ((ICollection)value))
                {
                    string inParameter;
                    expressionQuerySql.AddParameter(item, out inParameter);

                    inString = string.Concat(inString, string.Format("{0},", inParameter));
                }

                return string.Format("({0} {1})", aliasAndColumnName.GetTableAndColumnName(),
                    string.Format(comparison, inString.TrimEnd(',')));
            }

            private static string _getSql(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType)
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
                        return _getSqlEquality(expression, expressionQuerySql, schematic, isNotExpressionType);
                    case "CONTAINS":
                        return _getSqlContains(expression, expressionQuerySql, schematic, isNotExpressionType);
                    case "STARTSWITH":
                        return _getSqlStartsEndsWith(expression, expressionQuerySql, schematic, isNotExpressionType, true);
                    case "ENDSWITH":
                        return _getSqlStartsEndsWith(expression, expressionQuerySql, schematic, isNotExpressionType, false);
                }

                throw new Exception(string.Format("Method does not translate into Sql.  Method Name: {0}",
                    methodName));
            }

            private static TableColumnContainer _getTableAliasAndColumnName(BinaryExpression expression, IQuerySchematic schematic)
            {
                return WhereUtilities.IsConstant(expression.Right)
                    ? LoadColumnAndTableName(expression.Left as dynamic, schematic)
                    : LoadColumnAndTableName(expression.Right as dynamic, schematic);
            }
        }

        private class ExpressionQueryJoinResolver : ExpressionQueryResolverBase
        {
            // needs to happen before reading
            public static void ResolveForeignKeyJoins(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                var includedTables = schematic.MappedTables.Where(w => w.IsIncluded).ToList();

                var joins = includedTables.Aggregate(string.Empty,
                    (current1, includedTable) =>
                        includedTable.Relationships.Where(w => w.ChildTable.IsIncluded)
                            .Aggregate(current1,
                                (current, relationship) => current + string.Format("{0}\r", relationship.Sql)));

                // we are ok to add all joins here
                expressionQuerySql.AddJoin(joins);
            }

            public static void Resolve<TOuter, TInner, TKey, TResult>(
            IExpressionQuery<TOuter> outer,
            IExpressionQuery<TInner> inner,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector,
            JoinType joinType,
            ExpressionQuerySqlResolutionContainer expressionQuerySql,
            IQuerySchematic schematic)
            {
                const string join = "{0} JOIN {1} AS [{2}] ON {3} = {4}\r";
                var joinString = joinType.ToString().ToUpper();
                var innerKeySelectorString = LoadColumnAndTableName(innerKeySelector.Body as dynamic, schematic);
                var outerKeySelectorString = LoadColumnAndTableName(outerKeySelector.Body as dynamic, schematic);
                var table = schematic.FindTable(typeof(TInner));

                expressionQuerySql.AddJoin(string.Format(join, 
                    joinString,
                    table.Table.ToString(TableNameFormat.SqlWithSchema),
                    table.Alias,
                    innerKeySelectorString.GetTableAndColumnName(),
                    outerKeySelectorString.GetTableAndColumnName()));
            }
        }

        private class ExpressionQueryOrderByResolver : ExpressionQuerySelectResolver
        {
            public static void ResolveOrderByPrimaryKeys(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                var result = schematic.MappedTables.Where(w => w.IsIncluded).Select(w => w.OrderByPrimaryKeysInline())
                    .Aggregate("",
                        (current1, table) =>
                            current1 +
                            table.OrderByColumns.Aggregate("",
                                (current, selectedColumn) =>
                                    current + selectedColumn.Column.ToString(table.Alias, " ASC,"))).TrimEnd(',');

                expressionQuerySql.AddOrderBy(result);
            }

            public static void ResolveOrderBy<T, TKey>(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql, Expression<Func<T, TKey>> keySelector)
            {
                // make sure table being selected is in the query
                TableColumnContainer tableAndColumnName = LoadColumnAndTableName(keySelector.Body as dynamic, schematic);

                _makeSureTableIsInQuery(schematic, tableAndColumnName);

                var sql = string.Format("{0} ASC,", tableAndColumnName.GetTableAndColumnName());

                expressionQuerySql.AddOrderBy(sql);
            }

            public static void ResolveOrderByDescending<T, TKey>(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql, Expression<Func<T, TKey>> keySelector)
            {
                TableColumnContainer tableAndColumnName = LoadColumnAndTableName(keySelector.Body as dynamic, schematic);

                _makeSureTableIsInQuery(schematic, tableAndColumnName);

                var sql = string.Format("{0} DESC,", tableAndColumnName.GetTableAndColumnName());

                expressionQuerySql.AddOrderBy(sql);
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

            public static void Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> expressionQuery, IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                _resolve(schematic, expressionQuerySql, expressionQuery.Body);
            }

            public static void Resolve<TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> expressionQuery, IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                _resolve(schematic, expressionQuerySql, expressionQuery.Body);
            }

            //Expression<Func<TOuter, TInner, TResult>>
            private static void _resolve(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql, Expression expression)
            {
                expressionQuerySql.From(schematic.MappedTables[0]);

                ReturnObject = null;

                var columns = _evaluate(expression as dynamic, schematic);

                // only format the columns if they are not empty
                if (!string.IsNullOrEmpty(columns))
                {
                    columns = string.Format("{0}\r\n", columns.TrimEnd('\n').TrimEnd('\r').TrimEnd(','));
                }

                expressionQuerySql.SetColumns(columns);
            }

            public static IExpressionQuery<TResultType> ChangeExpressionQueryGenericType<TSourceType, TResultType>(IExpressionQuery<TSourceType> source)
            {
                const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                var fields = source.GetType().GetFields(bindingFlags);

                var schematic = fields.First(w => w.Name == "_schematic").GetValue(source);
                var context = fields.First(w => w.Name == "_context").GetValue(source);
                var configurartion = fields.First(w => w.Name == "_configuration").GetValue(source);
                var expressionQuerySql = fields.First(w => w.Name == "_expressionQuerySql").GetValue(source);
                
                var instance = (IExpressionQuery<TResultType>)Activator.CreateInstance(typeof(ExpressionQuery<TResultType>), context, schematic, configurartion);

                // set the expression query sql
                var expressionQuerySqlField = instance.GetType().GetField("_expressionQuerySql", bindingFlags);

                if (expressionQuerySqlField == null)
                {
                    throw new Exception("Critical error occurred cloning Expression Query.  See Inner Exception",
                        new Exception("_expressionQuerySql field was null"));
                }

                expressionQuerySqlField.SetValue(instance, expressionQuerySql);

                return instance;
            }

            private static object _getPropertyValue(object entity, string propertyName)
            {
                var property = entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

                return property.GetValue(entity);
            }

            private static object _getStaticPropertyValue(object entity, Type baseType, string propertyName)
            {
                var property = baseType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic);

                return property.GetValue(entity);
            }

            private static object _getEventValue(object entity, string propertyName)
            {
                var e = entity.GetType().GetEvent(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

                return e.GetRaiseMethod();
            }

            private static object _getPropertyValue(object entity, Type type, string propertyName)
            {
                var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

                return property.GetValue(entity);
            }

            public static void ResolveSelectAll(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                expressionQuerySql.From(schematic.MappedTables[0]);

                var result = schematic.MappedTables.Where(w => w.IsIncluded && w.SelectedColumns.Any())
                    .OrderBy(w => w.SelectedColumns.First().Ordinal)
                    .Aggregate(string.Empty,
                        (current1, mappedTable) =>
                            string.Concat(current1,
                                mappedTable.SelectedColumns.Aggregate("",
                                    (current, selectedColumn) =>
                                        string.Concat(current, "\t",
                                            selectedColumn.Column.ToString(mappedTable.Alias, ",\r")))));

                expressionQuerySql.SetColumns(result.TrimEnd('\r').TrimEnd(','));
            }

            private static string _evaluate(MemberInitExpression expression, IQuerySchematic schematic)
            {
                var sql = _getSql(expression, schematic);

                sql = sql.Replace(expression.ToString(), sql);

                ReturnObject = expression;

                return sql;
            }

            private static string _evaluate(NewExpression expression, IQuerySchematic schematic)
            {
                var sql = string.Empty;

                for (var i = 0; i < expression.Arguments.Count; i++)
                {
                    var argument = expression.Arguments[i];
                    var member = expression.Members[i];

                    var tableAndColumnName = LoadColumnAndTableName((dynamic)argument, schematic);
                    var tableAndColmnNameSql = SelectUtilities.GetTableAndColumnNameWithAlias(tableAndColumnName, member.Name);

                    sql = string.Concat(sql, SelectUtilities.GetSqlSelectColumn(tableAndColmnNameSql));
                }

                sql = sql.Replace(expression.ToString(), sql);

                ReturnObject = expression;

                return sql;
            }

            private static string _evaluate(ParameterExpression expression, IQuerySchematic schematic)
            {
                // comes from select.  We should do a select * from the parameter that is being passed in
                var table = schematic.FindTable(expression.Type);

                // exclude all mapped tables
                foreach (var mappedTable in schematic.MappedTables)
                {
                    mappedTable.Clear();
                    mappedTable.Exclude();
                }

                // include only the selected table
                table.Include();
                table.SelectAll(0);

                // return an empty string so SelectAll() will be called
                return string.Empty;
            }

            private static string _evaluate(MemberExpression expression, IQuerySchematic schematic)
            {
                var tableAndColumnName = LoadColumnAndTableName(expression, schematic);
                var tableAndColumnNameSql = tableAndColumnName.GetTableAndColumnName();

                var sql = expression.ToString();

                sql = sql.Replace(sql, SelectUtilities.GetSqlSelectColumn(tableAndColumnNameSql));

                return sql;
            }

            private static string _getSql(MemberInitExpression expression, IQuerySchematic schematic)
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
                                sql = string.Concat(sql, _getSql(memberInitExpression, schematic));
                                continue;
                            }

                            var tableAndColumnName = LoadColumnAndTableName((dynamic)assignment.Expression, schematic);
                            var tableAndColmnNameSql =
                                SelectUtilities.GetTableAndColumnNameWithAlias(tableAndColumnName, assignment.Member.Name);

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
                return expression.ToString().Replace("OrElse", "\r\n\tOR").Replace("AndAlso", "\r\n\tAND");
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
                if (memberExpression != null) return memberExpression.Member.DeclaringType == memberExpression.Type || memberExpression.NodeType == ExpressionType.MemberAccess;

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
                return string.Format("{0} AS [{1}],", container.GetTableAndColumnName(), alias);
            }
        }

        private abstract class ExpressionQueryResolverBase
        {
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

                        if (argument.NodeType == ExpressionType.Constant) return ((ConstantExpression)argument).Value;
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

            protected static TableColumnContainer LoadColumnAndTableName(MemberExpression expression, IQuerySchematic schematic)
            {
                var columnAttribute = expression.Member.GetCustomAttribute<ColumnAttribute>();
                var columnName = columnAttribute != null ? columnAttribute.Name : expression.Member.Name;
                string alias;
                string tableName;

                if (expression.Expression.NodeType == ExpressionType.Parameter)
                {
                    alias = schematic.FindTable(expression.Expression.Type).Alias;
                    tableName = Table.GetTableName(expression.Expression.Type);
                }
                else
                {
                    alias = schematic.FindTable(((MemberExpression)expression.Expression).Member.Name).Alias;
                    tableName = ((MemberExpression)expression.Expression).Member.Name;
                }

                return new TableColumnContainer(tableName, columnName, alias);
            }

            protected static TableColumnContainer LoadColumnAndTableName(MethodCallExpression expression, IQuerySchematic schematic)
            {
                if (expression.Object == null)
                {
                    return
                        LoadColumnAndTableName(
                            expression.Arguments.First(w => HasParameter(w as dynamic)) as dynamic, schematic);
                }

                if (IsEnumerable(expression.Object.Type))
                {
                    foreach (var memberExpression in expression.Arguments.OfType<MemberExpression>())
                    {
                        return LoadColumnAndTableName(memberExpression, schematic);
                    }
                }

                return LoadColumnAndTableName(expression.Object as MemberExpression, schematic);
            }

            protected static TableColumnContainer LoadColumnAndTableName(UnaryExpression expression, IQuerySchematic schematic)
            {
                return LoadColumnAndTableName(expression.Operand as MemberExpression, schematic);
            }

            protected static bool IsEnumerable(Type type)
            {
                return (type.IsGenericType &&
                        type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) ||
                        type.IsArray);
            }

            #endregion
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

            public string GetTableAndColumnName()
            {
                return string.Format("[{0}].[{1}]", Alias, ColumnName);
            }
        }
        #endregion
        #endregion
    }
}
