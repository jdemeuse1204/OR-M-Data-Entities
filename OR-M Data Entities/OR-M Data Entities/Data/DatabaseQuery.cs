/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
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
using OR_M_Data_Entities.Data.Loading;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseQuery : DatabaseExecution
    {
        #region Properties
        protected IQuerySchematicFactory SchematicFactory { get; set; }

        #endregion

        #region Thread Management

        private readonly object _queryLock = new object();
        #endregion

        #region Constructor

        protected DatabaseQuery(string connectionStringOrName)
            : base(connectionStringOrName)
        {
            SchematicFactory = new QuerySchematicFactory();
        }
        #endregion

        #region Expression Query Methods

        public IExpressionQuery<T> From<T>()
        {
            lock (_queryLock)
            {
                // grab the base table and reset it
                var table = DbTableFactory.Find<T>(Configuration);

                // get the schematic
                var schematic = SchematicFactory.FindAndReset(table, Configuration, DbTableFactory);

                // initialize the selected columns
                schematic.InitializeSelect(Configuration.IsLazyLoading);

                return new ExpressionQuery<T>(this, schematic, Configuration, SchematicFactory, DbTableFactory);
            }
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
            lock (_queryLock)
            {
                var table = DbTableFactory.Find<T>(Configuration);

                // get the schematic
                var schematic = SchematicFactory.FindAndReset(table, Configuration, DbTableFactory);

                var expressionQuery = new ExpressionQuery<T>(this, schematic, Configuration, SchematicFactory,
                    DbTableFactory);

                // initialize the selected columns, we tell it false always for find so the whole object will be returned
                // Find does not have options to include or exclude tables
                schematic.InitializeSelect(false);

                // resolve the pks for the where statement
                expressionQuery.ResolveExists(entity);

                return ((IExpressionQuery<T>)expressionQuery).Any();
            }
        }

        public T Find<T>(params object[] pks)
        {
            lock (_queryLock)
            {
                // grab the base table
                var table = DbTableFactory.Find<T>(Configuration);

                // get the schematic
                var schematic = SchematicFactory.FindAndReset(table, Configuration, DbTableFactory);

                var expressionQuery = new ExpressionQuery<T>(this, schematic, Configuration, SchematicFactory,
                    DbTableFactory);

                // initialize the selected columns, we tell it false always for find so the whole object will be returned
                // Find does not have options to include or exclude tables
                schematic.InitializeSelect(false);

                // resolve the pks for the where statement
                expressionQuery.Find<T>(pks, Configuration);

                return ((IExpressionQuery<T>)expressionQuery).FirstOrDefault();
            }
        }

        public override void Dispose()
        {
            SchematicFactory = null;

            base.Dispose();
        }

        #endregion

        #region helpers

        // manages the schematics for all queries
        private class QuerySchematicFactory : IQuerySchematicFactory
        {
            public QuerySchematicFactory()
            {
                _mappings = new Dictionary<Type, IQuerySchematic>();
                _mappedTables = new HashSet<IMappedTable>();
            }

            // cache for all current maps
            private IDictionary<Type, IQuerySchematic> _mappings { get; set; }

            // cache for all mapped tables
            private HashSet<IMappedTable> _mappedTables { get; set; }

            public IQuerySchematic FindAndReset(ITable table, IConfigurationOptions configuration, ITableFactory tableFactory)
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

                result = _createSchematic(table, configuration, tableFactory);

                _mappings.Add(table.Type, result);

                // reset the previous selected items
                result.Reset();

                return result;
            }

            /// <summary>
            /// Never used with foreign keys, only used when joining
            /// </summary>
            /// <param name="types"></param>
            /// <param name="configuration"></param>
            /// <returns></returns>
            public IQuerySchematic CreateTemporarySchematic(List<Type> types, IConfigurationOptions configuration, ITableFactory tableFactory, Type selectedType)
            {
                const string aliasString = "AkA{0}";
                var index = 0;
                var mappedTables = new List<IMappedTable>();
                var from = types.First();

                foreach (var currentMappedtable in from type in types
                                                   let nextAlias = string.Format(aliasString, index)
                                                   let tableSchematic = tableFactory.Find(type, configuration)
                                                   select new MappedTable(tableSchematic, nextAlias, nextAlias, false, true))
                {
                    mappedTables.Add(currentMappedtable);

                    index++;
                }

                var dataLoadSchematic = new DataLoadSchematic(null, selectedType, selectedType, mappedTables.First(), null);

                return new QuerySchematic(from, mappedTables, dataLoadSchematic, configuration);
            }

            // creates the schematic so we know how to load the object
            private IQuerySchematic _createSchematic(ITable from, IConfigurationOptions configuration, ITableFactory tableFactory)
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
                                tableFactory.Find(
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

            private string _getOptimizedJoin(IMappedTable currentMappedTable, IMappedTable parentTable, string parentColumnName, string childColumnName)
            {
                var from = currentMappedTable.Table.ToString(TableNameFormat.SqlWithSchema);

                // we only want to optimize joins across a linked server table and a non linked server table
                if (!currentMappedTable.Table.IsUsingLinkedServer ||
                    (currentMappedTable.Table.IsUsingLinkedServer && parentTable.Table.IsUsingLinkedServer))
                {
                    return from;
                }

                const string body = "({0})";
                const string distinctParentSelect = "(SELECT DISTINCT {0} FROM {1})";
                const string linkedServerSelectConstraint = "SELECT {0} FROM {1} WHERE {2} IN {3}";
                var columns = currentMappedTable.Table.Columns.Aggregate(string.Empty,
                    (current, column) =>
                        string.Concat(current,
                            string.Format("[{0}].[{1}],", column.Table.PlainTableName,
                                column.DatabaseColumnName))).TrimEnd(',');

                // need to trim off the alias so the where clase is valid
                var childSplit = childColumnName.Split('.');
                var childColumn = childSplit.Length == 2 ? childSplit[1] : childColumnName;
                var parentSplit = parentColumnName.Split('.');
                var parentColumn = parentSplit.Length == 2 ? parentSplit[1] : parentColumnName;

                return string.Format(body,
                    string.Format(linkedServerSelectConstraint, columns, from, childColumn,
                        string.Format(distinctParentSelect, parentColumn,
                            parentTable.Table.ToString(TableNameFormat.SqlWithSchema))));
            }

            private string _getRelationshipSql(IMappedTable currentMappedTable, IMappedTable parentTable, IColumn column,
                RelationshipType relationshipType)
            {
                string columnName;
                string pskColumnName;
                const string sql = "{0} JOIN {1} ON {2} = {3}";
                const string tableColumn = "[{0}].[{1}]";
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
                            pskAttribute == null ? currentMappedTable.Table.GetPrimaryKeyName(0) : pskAttribute.ChildTableColumnName); // fk attribute in child

                        // grab the alias body
                        var oneToOneAlias = string.Format("{0} AS [{1}]", _getOptimizedJoin(currentMappedTable, parentTable, oneToOneParent, oneToOneChild), currentMappedTable.Alias);

                        return string.Format(sql, joinType, oneToOneAlias, oneToOneChild, oneToOneParent);
                    case RelationshipType.OneToMany:

                        pskColumnName = pskAttribute == null ? string.Empty : pskAttribute.ChildTableColumnName;

                        // column might be renamed, grab the correct name of the column
                        columnName = _getAutoLoadChildDatabaseColumnName(currentMappedTable, fkAttribute, pskColumnName);

                        var oneToManyParent = string.Format(tableColumn, parentTable.Alias,
                            pskAttribute == null ? parentTable.Table.GetPrimaryKeyName(0) : pskAttribute.ParentTableColumnName); // pk in parent
                        var oneToManyChild = string.Format(tableColumn, currentMappedTable.Alias, columnName);
                        // fk attribute in parent... in other table

                        // grab the alias body
                        var oneToManyAlias = string.Format("{0} AS [{1}]", _getOptimizedJoin(currentMappedTable, parentTable, oneToManyParent, oneToManyChild), currentMappedTable.Alias);

                        return string.Format(sql, "LEFT", oneToManyAlias, oneToManyChild, oneToManyParent);
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
                    ReturnOverride = null;
                }

                public IConfigurationOptions ConfigurationOptions { get; private set; }

                public Type Key { get; private set; }

                // map tells the DbReader how the object should be loaded
                public List<IMappedTable> MappedTables { get; private set; }

                public Expression ReturnOverride { get; private set; }

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

                    // select all if we are not lazy loading
                    SelectAll();
                }

                public int NextOrdinal()
                {
                    return MappedTables.Any(w => w.SelectedColumns.Any())
                        ? MappedTables.Where(w => w.SelectedColumns.Any())
                            .Select(w => w.SelectedColumns.Select(x => x.Ordinal).Max())
                            .Max() + 1
                        : 0;
                }

                public void UnSelectAll()
                {
                    if (MappedTables == null) return;

                    for (var i = 0; i < MappedTables.Count; i++)
                    {
                        // we need to make sure the table still joins to children, only
                        // exclude the selected columns, do not exclude the table
                        // or else the joins will be incorrect
                        var mappedTable = MappedTables[i];
                        mappedTable.Clear();
                    }
                }

                public void ExcludeAll()
                {
                    if (MappedTables == null) return;

                    for (var i = 0; i < MappedTables.Count; i++)
                    {
                        var mappedTable = MappedTables[i];
                        mappedTable.Exclude();
                    }
                }

                public void SelectAll()
                {
                    var startingOrdinal = 0;

                    // select all if we are not lazy loading
                    for (var i = 0; i < MappedTables.Count; i++)
                    {
                        var mappedTable = MappedTables[i];
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
                    UnSelectAll();
                    ExcludeAll();

                    // recursive clear
                    DataLoadSchematic.ClearRowReadCache();

                    ReturnOverride = null;
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
                    LoadedCompositePrimaryKeys = new HashSet<CompositeKey>();
                    Children = new HashSet<IDataLoadSchematic>();
                    MappedTable = mappedTable;
                    Parent = parent;
                }

                public HashSet<IDataLoadSchematic> Children { get; private set; }

                public IDataLoadSchematic Parent { get; private set; }

                public Type ActualType { get; private set; }

                public string[] PrimaryKeyNames { get; private set; }

                public HashSet<CompositeKey> LoadedCompositePrimaryKeys { get; private set; }

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
                    LoadedCompositePrimaryKeys = new HashSet<CompositeKey>();

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
                    LoadedCompositePrimaryKeys = new HashSet<CompositeKey>();
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

                    // column is already selected
                    if (SelectedColumns.Select(w => w.Column.DatabaseColumnName).Contains(column.DatabaseColumnName)) return;

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

                public int MaxOrdinal()
                {
                    return !SelectedColumns.Any() ? 0 : SelectedColumns.Max(w => w.Ordinal);
                }

                public IMappedTable OrderByPrimaryKeysInline()
                {
                    OrderByPrimaryKeys();

                    return this;
                }

                public bool HasColumn(string propertyName)
                {
                    return Table.Columns.Any(w => w.PropertyName == propertyName && w.IsSelectable);
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

        protected class ExpressionQuerySqlResolutionContainer
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

            public bool CanUseOrderBy()
            {
                return !_exists && !_min && !_max && !_count;
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

            public IMappedTable From()
            {
                return _from;
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
                    ? string.Format("\tMIN({0})\r\n", _columns.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty))
                    : _max
                        ? string.Format("\tMAX({0})\r\n", _columns.Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty))
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

            private readonly IQuerySchematicFactory _querySchematicFactory;

            private readonly ITableFactory _dbTableFactory;
            #endregion

            #region Constructor

            public ExpressionQuery(DatabaseExecution context, IQuerySchematic schematic, IConfigurationOptions configuration, IQuerySchematicFactory querySchematicFactory, ITableFactory dbTableFactory)
            {
                _expressionQuerySql = new ExpressionQuerySqlResolutionContainer();

                _context = context;
                _schematic = schematic;
                _configuration = configuration;
                _querySchematicFactory = querySchematicFactory;
                _dbTableFactory = dbTableFactory;
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

            public void Where(Expression<Func<T, bool>> expression)
            {
                ExpressionQueryWhereResolver.Resolve(expression, _expressionQuerySql, _schematic);
            }

            public IExpressionQuery<TResult> Select<TResult>(IExpressionQuery<T> source, Expression<Func<T, TResult>> selector)
            {
                ExpressionQuerySelectResolver.Resolve(selector, _schematic, _expressionQuerySql);

                return ExpressionQuerySelectResolver.ChangeExpressionQueryGenericType<T, TResult>(source);
            }

            public IOrderedExpressionQuery<TResult> Select<TResult>(IOrderedExpressionQuery<T> source, Expression<Func<T, TResult>> selector)
            {
                ExpressionQuerySelectResolver.Resolve(selector, _schematic, _expressionQuerySql);

                return (IOrderedExpressionQuery<TResult>)ExpressionQuerySelectResolver.ChangeExpressionQueryGenericType<T, TResult>((IExpressionQuery<T>)source);
            }

            public void Find<TResult>(object[] pks, IConfigurationOptions configuration)
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

            public void OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            {
                ExpressionQueryOrderByResolver.ResolveOrderBy(_schematic, _expressionQuerySql, keySelector);
            }

            public void OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
            {
                ExpressionQueryOrderByResolver.ResolveOrderByDescending(_schematic, _expressionQuerySql, keySelector);
            }

            public void Max()
            {
                _expressionQuerySql.MarkAsMaxStatement();
            }

            public void Any()
            {
                _expressionQuerySql.MarkAsExistsStatement();
            }

            public void Count()
            {
                _expressionQuerySql.MarkAsCountStatement();
            }

            public void Min()
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

            public void ForeignKeyJoins()
            {
                ExpressionQueryJoinResolver.ResolveForeignKeyJoins(_schematic, _expressionQuerySql);
            }

            public void Distinct()
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

            public IDataTranslator<TSource> ExecuteReader<TSource>()
            {
                // need to happen before read because of include statements
                // select all if nothing was selected
                if (!_expressionQuerySql.AreColumnsSelected()) SelectAll();

                // only do below if object has foreign keys
                if (_schematic.AreForeignKeysSelected())
                {
                    // order by primary keys, so we can select the data
                    // back correctly from the data reader
                    // can only order if we are not doing a 
                    // Min/Max/Count/Exists
                    if (_expressionQuerySql.CanUseOrderBy()) OrderByPrimaryKeys();

                    // resolve the foreign key joins
                    ForeignKeyJoins();
                }

                // generate out sql statement
                return _context.ExecuteQuery<TSource>(Sql(), _expressionQuerySql.Parameters(), _schematic);
            }

            public void IncludeAll()
            {
                var nextOrdinal = _schematic.NextOrdinal();
                var mappedTables = _schematic.MappedTables.Where(w => !w.IsIncluded).ToList();

                // also need to select the columns
                foreach (var mappedTable in mappedTables)
                {
                    nextOrdinal += mappedTable.SelectAll(nextOrdinal);
                    mappedTable.Include();
                }
            }

            public void Include(string pathOrTableName)
            {
                if (string.IsNullOrEmpty(pathOrTableName)) throw new ArgumentNullException("pathOrTableName");

                var tables = pathOrTableName.Split('.');
                var nextOrdinal = _schematic.NextOrdinal();
                var schematicToScan = _schematic.DataLoadSchematic;

                foreach (var table in tables)
                {
                    schematicToScan =
                        schematicToScan.Children.FirstOrDefault(
                            w =>
                                w.MappedTable.Key == table);

                    if (schematicToScan == null) throw new Exception(string.Format("Could not find property name - {0}", table));

                    // table might already be included
                    if (schematicToScan.MappedTable.IsIncluded) continue;

                    schematicToScan.MappedTable.Include();
                    nextOrdinal += schematicToScan.MappedTable.SelectAll(nextOrdinal);
                }
            }

            public void Disconnect()
            {
                _context.Disconnect();
            }

            public IExpressionQuery<TResult> Join<TOuter, TInner, TKey, TResult>(
                IExpressionQuery<TOuter> outer,
                IExpressionQuery<TInner> inner,
                Expression<Func<TOuter, TKey>> outerKeySelector,
                Expression<Func<TInner, TKey>> innerKeySelector,
                Expression<Func<TOuter, TInner, TResult>> resultSelector,
                JoinType joinType)
            {
                // make sure the current schematic has the inner type, if not make
                // a temporary schematic, should almost always happen
                if (!_schematic.HasTable(typeof(TInner)))
                {
                    var types = new List<Type> { typeof(TOuter), typeof(TInner) };

                    // change the selected schematic
                    ChangeSchematic(_querySchematicFactory.CreateTemporarySchematic(types, _configuration, _dbTableFactory, typeof(TOuter)));
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
                foreach (T item in ExecuteReader<T>()) yield return item;

                _context.Dispose();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }

        #region Expression Query Resolution
        protected class ExpressionQueryWhereResolver : ExpressionQueryResolverBase
        {
            public static void Resolve<T>(Expression<Func<T, bool>> expressionQuery,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                var sql = _evaluate(expressionQuery.Body as dynamic, expressionQuerySql, schematic);

                expressionQuerySql.AddWhere(sql);
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

            private static string _evaluate(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                // get the sql from the expression
                var sql = WhereUtilities.GetSqlFromExpression(expression);

                return _processExpression(expression, expressionQuerySql, schematic, expression.ToString(), false, sql);
            }

            private static string _evaluate(UnaryExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                return _evaluate(expression.Operand as dynamic, expressionQuerySql, schematic);
            }

            private static string _evaluate(MemberExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic)
            {
                // get the sql from the expression
                var sql = WhereUtilities.GetSqlFromExpression(expression);

                return _processExpression(expression, expressionQuerySql, schematic, expression.ToString(), false, sql);
            }

            private static string _evaluate(BinaryExpression expression,
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
                        if (e.NodeType == ExpressionType.Not || e.NodeType == ExpressionType.NotEqual)
                        {
                            sql = _processNotExpression(e, expressionQuerySql, schematic, replacementString, sql);
                            continue;
                        }

                        // process normal expression
                        sql = _processExpression(e, expressionQuerySql, schematic, replacementString, false, sql);
                        continue;
                    }

                    expressions.Add(((BinaryExpression)e).Left);
                    expressions.Add(((BinaryExpression)e).Right);
                }

                return sql;
            }

            private static string _processNotExpression(dynamic item,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                string replacementString,
                string expressionString)
            {
                var unaryExpression = item as UnaryExpression;

                if (unaryExpression != null)
                {
                    return _processExpression(unaryExpression.Operand, expressionQuerySql, schematic, replacementString, true, expressionString);
                }

                var binaryExpression = item as BinaryExpression;

                if (binaryExpression != null)
                {
                    return _processExpression(binaryExpression, expressionQuerySql, schematic, replacementString, true, expressionString);
                }

                throw new Exception(string.Format("Expression Type not valid.  Type: {0}", item.NodeType));
            }

            private static string _processExpression(dynamic item,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                string replacementString,
                bool isNotExpressionType,
                string expressionString)
            {
                var memberExpression = item as MemberExpression;

                if (item.NodeType == ExpressionType.Call)
                {
                    // try and evaluate the expression to get the internal value
                    var expressionValue = GetValue(item);

                    return expressionString.Replace(replacementString, _getSql(item as MethodCallExpression, expressionQuerySql, schematic, isNotExpressionType, expressionValue));
                }

                if (item.NodeType == ExpressionType.Equal || item.NodeType == ExpressionType.NotEqual)
                {
                    // check to see if the user is using (test == true) instead of (test)
                    bool outLeftValue;
                    if (WhereUtilities.IsLeftBooleanValue(item, out outLeftValue))
                    {
                        var booleanValue = WhereUtilities.GetBooleanValueFromExpression(item.Left);

                        return expressionString.Replace(replacementString,
                            _getSql(item.Right, expressionQuerySql, schematic, isNotExpressionType, booleanValue));
                    }

                    // check to see if the user is using (test == true) instead of (test)
                    bool outRightValue;
                    if (WhereUtilities.IsRightBooleanValue(item, out outRightValue))
                    {
                        var booleanValue = WhereUtilities.GetBooleanValueFromExpression(item.Right);

                        return expressionString.Replace(replacementString,
                            _getSql(item.Left, expressionQuerySql, schematic, isNotExpressionType, booleanValue));
                    }

                    return expressionString.Replace(replacementString,
                        _getSqlEquals(item as BinaryExpression, expressionQuerySql, schematic, isNotExpressionType));
                }

                if (item.NodeType == ExpressionType.GreaterThan ||
                    item.NodeType == ExpressionType.GreaterThanOrEqual ||
                    item.NodeType == ExpressionType.LessThan ||
                    item.NodeType == ExpressionType.LessThanOrEqual)
                {
                    // check to see the order the user typed the statement
                    if (WhereUtilities.IsConstantOrConvertible(item.Left))
                    {
                        return expressionString.Replace(replacementString,
                            _getSqlGreaterThanLessThan(item as BinaryExpression, expressionQuerySql, schematic, isNotExpressionType, item.NodeType));
                    }

                    // check to see the order the user typed the statement
                    if (WhereUtilities.IsConstantOrConvertible(item.Right))
                    {
                        return expressionString.Replace(replacementString,
                            _getSqlGreaterThanLessThan(item as BinaryExpression, expressionQuerySql, schematic, isNotExpressionType, item.NodeType));
                    }

                    throw new Exception("invalid comparison");
                }

                // double negative check will go into recursive check
                if (item.NodeType == ExpressionType.Not)
                {
                    return _processNotExpression(item, expressionQuerySql, schematic, replacementString, expressionString);
                }

                if (memberExpression != null)
                {
                    return expressionString.Replace(replacementString,
                            _getSql(memberExpression, expressionQuerySql, schematic, isNotExpressionType, true));
                }

                throw new ArgumentNullException(item.NodeType);
            }

            private static string _getSqlEquals(BinaryExpression expression, ExpressionQuerySqlResolutionContainer expressionQuerySql, IQuerySchematic schematic, bool isNotExpressionType)
            {
                var comparison = isNotExpressionType ? "!=" : "=";
                var left = string.Empty;
                var right = string.Empty;

                // check to see if the left and right are not constants, if so they need to be evaulated
                if (WhereUtilities.IsConstantOrConvertible(expression.Left) || WhereUtilities.IsConstantOrConvertible(expression.Right))
                {
                    var leftTableAndColumnname = _getTableAliasAndColumnName(expression, schematic);

                    left = leftTableAndColumnname.GetTableAndColumnName();

                    var value = GetValue(expression);

                    _processExpressionValue(expressionQuerySql, value, isNotExpressionType, ref right, ref comparison);

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
                bool isNotExpressionType,
                object expressionValue)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic, schematic);
                var comparison = isNotExpressionType ? "!=" : "=";
                var parameter = string.Empty;

                _processExpressionValue(expressionQuerySql, expressionValue, isNotExpressionType, ref parameter, ref comparison);

                return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(),
                    comparison, parameter);
            }

            private static bool _processExpressionValue(ExpressionQuerySqlResolutionContainer expressionQuerySql,
                object value,
                bool isNotExpressionType,
                ref string parameter,
                ref string comparison)
            {
                comparison = isNotExpressionType ? "!=" : "=";

                if (value == null)
                {
                    parameter = "NULL";
                    comparison = "IS";

                    if (isNotExpressionType) parameter = "NOT NULL";

                    return true;
                }

                expressionQuerySql.AddParameter(value, out parameter);
                return false;
            }

            private static string _getSqlStartsEndsWith(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType,
                bool isStartsWith,
                object expressionValue)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic, schematic);
                var comparison = isNotExpressionType ? "NOT LIKE" : "LIKE";
                string parameter;

                expressionQuerySql.AddParameter(string.Format(isStartsWith ? "{0}%" : "%{0}", expressionValue), out parameter);

                return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(),
                    comparison, parameter);
            }

            private static string _getSqlContains(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType,
                object expressionValue)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic, schematic);
                var isEnumerable = IsEnumerable(expressionValue.GetType());
                var comparison = isEnumerable
                    ? (isNotExpressionType ? "NOT IN ({0})" : "IN ({0})")
                    : (isNotExpressionType ? "NOT LIKE" : "LIKE");

                if (!isEnumerable)
                {
                    string containsParameter;
                    expressionQuerySql.AddParameter(string.Format("%{0}%", expressionValue), out containsParameter);

                    return string.Format("{0} {1} {2}", aliasAndColumnName.GetTableAndColumnName(), comparison, containsParameter);
                }

                var inString = string.Empty;

                foreach (var item in ((ICollection)expressionValue))
                {
                    string inParameter;
                    expressionQuerySql.AddParameter(item, out inParameter);

                    inString = string.Concat(inString, string.Format("{0},", inParameter));
                }

                return string.Format("({0} {1})", aliasAndColumnName.GetTableAndColumnName(),
                    string.Format(comparison, inString.TrimEnd(',')));
            }

            private static string _getSql(MemberExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType,
                object expressionValue)
            {
                var aliasAndColumnName = LoadColumnAndTableName(expression as dynamic, schematic);
                var comparison = isNotExpressionType ? "!=" : "=";
                string parameter;

                expressionQuerySql.AddParameter(expressionValue, out parameter);

                return string.Format("({0} {1} {2})", aliasAndColumnName.GetTableAndColumnName(),
                    comparison, parameter);
            }

            // in order for this to work we need expressionValue even though its never used 
            private static string _getSql(MethodCallExpression expression,
                ExpressionQuerySqlResolutionContainer expressionQuerySql,
                IQuerySchematic schematic,
                bool isNotExpressionType,
                object expressionValue)
            {
                var methodName = expression.Method.Name;

                switch (methodName.ToUpper())
                {
                    case "EQUALS":
                    case "OP_EQUALITY":
                        return _getSqlEquality(expression, expressionQuerySql, schematic, isNotExpressionType, expressionValue);
                    case "CONTAINS":
                        return _getSqlContains(expression, expressionQuerySql, schematic, isNotExpressionType, expressionValue);
                    case "STARTSWITH":
                        return _getSqlStartsEndsWith(expression, expressionQuerySql, schematic, isNotExpressionType, true, expressionValue);
                    case "ENDSWITH":
                        return _getSqlStartsEndsWith(expression, expressionQuerySql, schematic, isNotExpressionType, false, expressionValue);
                    case "ANY":
                        return _getSqlFromAny(expression, expressionQuerySql, schematic);
                }

                throw new Exception(string.Format("Method does not translate into Sql.  Method Name: {0}",
                    methodName));
            }

            private static string _getSqlFromAny(MethodCallExpression expression, ExpressionQuerySqlResolutionContainer expressionQuerySql, IQuerySchematic schematic)
            {
                // first argument is the table, after that are the arguments to be compiled
                for (var i = 0; i < expression.Arguments.Count; i++)
                {
                    var argument = expression.Arguments[i];

                    // subquery, nothing to do on the first argument
                    if (i == 0) continue;

                    var lambdaExpression = argument as LambdaExpression;

                    if (lambdaExpression != null) return _evaluate(lambdaExpression.Body as dynamic, expressionQuerySql, schematic);

                    throw new Exception("Subquery expression not recognized");
                }

                throw new Exception("Invalid subquery");
            }

            private static TableColumnContainer _getTableAliasAndColumnName(BinaryExpression expression, IQuerySchematic schematic)
            {
                return WhereUtilities.IsConstantOrConvertible(expression.Right)
                    ? LoadColumnAndTableName(expression.Left as dynamic, schematic)
                    : LoadColumnAndTableName(expression.Right as dynamic, schematic);
            }
        }

        protected class ExpressionQueryJoinResolver : ExpressionQueryResolverBase
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

        protected class ExpressionQueryOrderByResolver : ExpressionQuerySelectResolver
        {
            public static void ResolveOrderByPrimaryKeys(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                var result = schematic.MappedTables.Where(w => w.IsIncluded).Select(w => w.OrderByPrimaryKeysInline())
                    .Aggregate(string.Empty,
                        (current1, table) =>
                            current1 +
                            table.OrderByColumns.Aggregate(string.Empty,
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
                    throw new OrderByException(string.Format("Cannot order by {0} from {1}, {1} is not part of the query.", tableAndColumnName.DatabaseColumnName, tableAndColumnName.TableName));
                }
            }
        }

        protected class ExpressionQuerySelectResolver : ExpressionQueryResolverBase
        {
            public static void Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> expressionQuery, IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                _resolve(schematic, expressionQuerySql, expressionQuery.Body);
            }

            public static void Resolve<TOuter, TInner, TResult>(Expression<Func<TOuter, TInner, TResult>> expressionQuery, IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                _resolve(schematic, expressionQuerySql, expressionQuery.Body);
            }

            private static void _resolve(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql, Expression expression)
            {
                // we will be selecting new columns, unselect everything
                schematic.UnSelectAll();

                expressionQuerySql.From(schematic.MappedTables[0]);

                _evaluate(expression as dynamic, schematic);
            }

            public static IExpressionQuery<TResultType> ChangeExpressionQueryGenericType<TSourceType, TResultType>(IExpressionQuery<TSourceType> source)
            {
                const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                var fields = source.GetType().GetFields(bindingFlags);

                var schematic = fields.First(w => w.Name == "_schematic").GetValue(source);
                var context = fields.First(w => w.Name == "_context").GetValue(source);
                var configurartion = fields.First(w => w.Name == "_configuration").GetValue(source);
                var expressionQuerySql = fields.First(w => w.Name == "_expressionQuerySql").GetValue(source);
                var querySchematicFactory = fields.First(w => w.Name == "_querySchematicFactory").GetValue(source);
                var dbTableFactory = fields.First(w => w.Name == "_dbTableFactory").GetValue(source);

                var instance = (IExpressionQuery<TResultType>)Activator.CreateInstance(typeof(ExpressionQuery<TResultType>), context, schematic, configurartion, querySchematicFactory, dbTableFactory);

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

            public static void ResolveSelectAll(IQuerySchematic schematic, ExpressionQuerySqlResolutionContainer expressionQuerySql)
            {
                expressionQuerySql.From(schematic.MappedTables[0]);

                var result = schematic.MappedTables.Where(w => w.IsIncluded && w.SelectedColumns.Any())
                    .OrderBy(w => w.SelectedColumns.First().Ordinal)
                    .Aggregate(string.Empty,
                        (current1, mappedTable) =>
                            string.Concat(current1,
                                mappedTable.SelectedColumns.Aggregate(string.Empty,
                                    (current, selectedColumn) =>
                                        string.Concat(current, "\t",
                                            selectedColumn.Column.ToString(mappedTable.Alias, ",\r")))));

                expressionQuerySql.SetColumns(result.TrimEnd('\r').TrimEnd(','));
            }

            private static void _evaluate(MemberInitExpression expression, IQuerySchematic schematic)
            {
                var sql = _getSql(expression, schematic);

                sql = sql.Replace(expression.ToString(), sql);

            }

            private static void _evaluate(NewExpression expression, IQuerySchematic schematic)
            {
                for (var i = 0; i < expression.Arguments.Count; i++)
                {
                    _evaluate((MemberExpression)expression.Arguments[i], schematic);
                }

                schematic.SetReturnOverride(expression);
            }

            private static void _evaluate(ParameterExpression expression, IQuerySchematic schematic)
            {
                // comes from select.  We should do a select * from the parameter that is being passed in
                var table = schematic.FindTable(expression.Type);

                // include only the selected table
                table.Include();
                table.SelectAll(0);
            }

            private static void _evaluate(MemberExpression expression, IQuerySchematic schematic)
            {
                // set the return override so we can load our object correctly
                schematic.SetReturnOverride(expression);

                var tableAndColumnName = LoadColumnAndTableName((dynamic)expression, schematic);

                if (tableAndColumnName.IsNotMapped)
                {
                    throw new QueryNotValidException(string.Format("Cannot select an Unmapped Column.  Column: {0}, Table: {1}", tableAndColumnName.DatabaseColumnName, tableAndColumnName.TableName));
                }

                // find our table
                var table = schematic.FindTable(expression.Expression.Type);

                // select the column
                var nextOrdinal = schematic.NextOrdinal();

                // if the column is not in the table then its a reference to a autoload property
                if (!table.HasColumn(tableAndColumnName.PropertyName))
                {
                    table = schematic.FindTable(((PropertyInfo)expression.Member).PropertyType.GetUnderlyingType());

                    // include the table in the selection
                    table.Include();

                    // select all from the table
                    table.SelectAll(nextOrdinal);
                    return;
                }

                // include the table
                table.Include();

                table.Select(tableAndColumnName.PropertyName, nextOrdinal);
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
                return string.Empty;
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

            public static bool? GetBooleanValueFromExpression(Expression expression)
            {
                var e = expression as ConstantExpression;

                if (e != null && e.Value is bool)
                {
                    return (bool)e.Value;
                }

                return null;
            }

            public static bool IsConstantOrConvertible(Expression expression)
            {
                var constant = expression as ConstantExpression;

                if (constant != null) return true;

                var memberExpression = expression as MemberExpression;

                // could be something like Guid.Empty, DateTime.Now
                if (memberExpression != null) return memberExpression.Member.DeclaringType == memberExpression.Type || memberExpression.NodeType == ExpressionType.MemberAccess;

                var unaryExpression = expression as UnaryExpression;

                return unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert;
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
                    ExpressionType.LessThanOrEqual, ExpressionType.MemberAccess,ExpressionType.NotEqual
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

        protected abstract class ExpressionQueryResolverBase
        {
            #region Get Value

            protected static object GetValue(ConstantExpression expression)
            {
                return expression.Value;
            }

            protected static object GetValue(MemberExpression expression)
            {
                var objectMember = Expression.Convert(expression, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                var getter = getterLambda.Compile();

                var value = getter();

                return value;
            }

            private static object _getValueFromConstantExpression(ConstantExpression expression, string memberName)
            {
                var value = expression.Value;

                if (value == null) throw new ArgumentNullException("value");

                var valueType = value.GetType();

                if (IsSystemType(value)) return value;

                var field = valueType.GetField(memberName);

                return field.GetValue(value);
            }

            protected static object GetValue(MethodCallExpression expression)
            {
                var methodName = expression.Method.Name.ToUpper();

                switch (methodName)
                {
                    case "CONTAINS":
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

                    case "EQUALS":
                    case "STARTSWITH":
                    case "ENDSWITH":
                        // need to look for the argument that has the constant
                        foreach (var argument in expression.Arguments)
                        {
                            var contstant = argument as ConstantExpression;

                            // no need to use _getValueFromConstantExpression here, value will always be of system type here
                            if (contstant != null) return contstant.Value;

                            var memberExpression = argument as MemberExpression;

                            if (memberExpression == null) continue;

                            contstant = memberExpression.Expression as ConstantExpression;

                            if (contstant != null) return _getValueFromConstantExpression(contstant, memberExpression.Member.Name);
                        }

                        // check for inverted expression
                        var inversion = expression.Object as MemberExpression;

                        if (inversion != null)
                        {
                            var invertedConstant = inversion.Expression as ConstantExpression;

                            if (invertedConstant != null) return _getValueFromConstantExpression(invertedConstant, inversion.Member.Name);
                        }

                        throw new ArgumentException("Comparison value not found");

                    case "ANY":
                        return null; // has no value to evaluate

                    default:
                        return _compile(expression.Object as MethodCallExpression);
                }
            }

            private static object _compile(Expression expression)
            {
                var objectMember = Expression.Convert(expression, typeof(object));

                var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                var getter = getterLambda.Compile();

                var value = getter();

                return value;
            }

            protected static object GetValue(UnaryExpression expression)
            {
                return GetValue(expression.Operand as dynamic);
            }

            protected static object GetValue(BinaryExpression expression)
            {
                return WhereUtilities.IsConstantOrConvertible(expression.Right)
                    ? GetValue(expression.Right as dynamic)
                    : GetValue(expression.Left as dynamic);
            }

            protected static bool IsSystemType(object value)
            {
                if (value == null) throw new ArgumentNullException("value");

                return value.GetType().Namespace.Contains("System");
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
                var unmappedAttribute = expression.Member.GetCustomAttribute<UnmappedAttribute>();
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

                return new TableColumnContainer(tableName, columnName, alias, expression.Member.Name, unmappedAttribute != null);
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
                    foreach (var e in expression.Arguments.OfType<MemberExpression>())
                    {
                        return LoadColumnAndTableName(e, schematic);
                    }
                }

                var memberExpression = expression.Object as MemberExpression;

                if (memberExpression != null && (memberExpression.Expression is MemberExpression || memberExpression.Expression.NodeType == ExpressionType.Parameter))
                {
                    return LoadColumnAndTableName(memberExpression, schematic);
                }

                // if we get here then the linq is writtn backwards, grab the column and table name
                // from the method call arguments
                return LoadColumnAndTableName(expression.Arguments[0] as MemberExpression, schematic);
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

        protected class TableColumnContainer
        {
            public readonly string TableName;

            public readonly string DatabaseColumnName;

            public readonly string Alias;

            public readonly string PropertyName;

            public readonly bool IsNotMapped; 

            public TableColumnContainer(string tableName, string columnName, string alias, string propertyName, bool isNotMapped)
            {
                TableName = tableName;
                DatabaseColumnName = columnName;
                Alias = alias;
                PropertyName = propertyName;
                IsNotMapped = isNotMapped;
            }

            public string GetTableAndColumnName()
            {
                return string.Format("[{0}].[{1}]", Alias, DatabaseColumnName);
            }
        }
        #endregion
        #endregion
    }
}
