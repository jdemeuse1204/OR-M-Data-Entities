using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json.Linq;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.WebApi
{
    public class DbSqlApiContext : DbSqlContext
    {
        #region Fields
        public readonly HashSet<Type> _registeredTables;
        #endregion

        #region Constructor
        public DbSqlApiContext(string connectionStringOrName) : base(connectionStringOrName)
        {
            _registeredTables = new HashSet<Type>();
            OnContextCreated();
        }

        public DbSqlApiContext(SqlConnectionStringBuilder connection) : base(connection)
        {
            _registeredTables = new HashSet<Type>();
            OnContextCreated();
        }
        #endregion

        #region Methods
        public virtual void OnContextCreated() { }

        public void RegisterTable<T>() where T : class
        {
            if (!_registeredTables.Contains(typeof(T))) _registeredTables.Add(typeof(T));
        }

        public object ApiQuery(string query)
        {
            // make sure lazy loading is turned on
            Configuration.IsLazyLoading = true;

            object result = null;

            if (string.IsNullOrEmpty(query)) return null;

            try
            {
                var dynamicQuery = JObject.Parse(query);
                var nodes = dynamicQuery.Children().ToList();

                if (nodes.Count == 0)
                {
                    throw new QueryParseException("Query not formed correctly, missing Read/Save/Delete");
                }

                if (nodes.Count >= 2)
                {
                    throw new QueryParseException("Query not formed correctly, too many nodes");
                }

                var node = nodes[0] as JProperty;

                if (node == null)
                {
                    throw new QueryParseException("Invalid query cast");
                }

                if (!QueryValidator.IsParentValid(node))
                {
                    throw new QueryParseException("Query not valid.  Parent nodes are: read, save, delete");
                }

                Type fromType;
                var executableQuery = QueryConverter.Convert(_registeredTables, 
                    node, DbTableFactory, 
                    SchematicFactory,
                    Configuration, out fromType);

                ExecuteReader(executableQuery.Sql(), executableQuery.Parameters());

                if (!Reader.HasRows) return null;

                var dataTranslatorParameterized = typeof (DataTranslator<>).MakeGenericType(fromType);
                var constr = dataTranslatorParameterized.GetConstructor(new[] {typeof (IPeekDataReader)});
                var dataTranslator = constr.Invoke(new object[] {Reader});
                var toList = dataTranslator.GetType().GetMethod("FirstOrDefault");

                result = toList.Invoke(dataTranslator, null);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private object _getResult()
        {
            return null;
        }
        #endregion

        #region helpers

        private static class QueryValidator
        {
            public const string READ = "read";

            public const string SAVE = "save";

            public const string DELETE = "delete";

            private static string[] _acceptableParentNames = { READ, SAVE, DELETE };

            public static bool IsParentValid(JProperty property)
            {
                return _acceptableParentNames.Contains(property.Name);
            }
        }

        private static class QueryConverter
        {
            #region ValidNodeTypes
            private const string WHERE = "where";

            private const string SELECT = "select";

            private const string JOIN = "join";

            private static string[] _operators = {" <> ", " != ", " >= ", " <= ", " > ", " = ", " < ", " in ", " like ", " not like ", " not in "};
            #endregion

            public static ExpressionQuerySqlResolutionContainer Convert(HashSet<Type> registeredTables, 
                JProperty parentRecord, 
                ITableFactory tableFactory, 
                IQuerySchematicFactory querySchematicFactory, 
                IConfigurationOptions configurationOptions,
                out Type type)
            {
                if (parentRecord == null) throw new QueryParseException("Query not valid.  Could not parse parent");

                var children = parentRecord.Children().ToList();

                if (children.Count != 1) throw new QueryParseException("Query not valid.  Can only read/save/delete from one table");

                var fromChild = children[0] as JObject;

                if (fromChild == null) throw new QueryParseException("Query not valid.  Could not parse from type");

                var from = fromChild.First as JProperty;

                if (from == null) throw new QueryParseException("Query not valid.  Could not parse from type");

                type =
                    registeredTables.FirstOrDefault(
                        w => string.Equals(Table.GetTableName(w), from.Name, StringComparison.CurrentCulture));

                if (type == null) throw new QueryParseException(string.Format("Table not registered.  Table: {0}", from.Name));

                // grab the base table and reset it
                var table = tableFactory.Find(type, configurationOptions);

                // get the schematic
                var schematic = querySchematicFactory.FindAndReset(table, configurationOptions, tableFactory);

                // query result
                var result = new ExpressionQuerySqlResolutionContainer();

                // get the from mapped table
                var fromMappedTable = schematic.FindTable(type);

                switch (parentRecord.Name)
                {
                    case QueryValidator.READ:

                        // initialize the selected columns
                        schematic.InitializeSelect(configurationOptions.IsLazyLoading);

                        result.From(fromMappedTable);

                        schematic.SelectAll();

                        _fillReadQuery(result, schematic, registeredTables, from.First as dynamic);
                        break;
                    case QueryValidator.SAVE:
                        _fillSaveQuery(result, from.First as dynamic);
                        break;
                    case QueryValidator.DELETE:
                        _fillDeleteQuery(result, from.First as dynamic);
                        break;
                }

                return result;
            }

            private static IMappedTable _findTable(HashSet<Type> tables, IQuerySchematic schematic, string tableName)
            {
                var type =
                    tables.FirstOrDefault(
                        w => string.Equals(Table.GetTableName(w), tableName, StringComparison.CurrentCulture));

                if (type == null) throw new QueryParseException(string.Format("Table not registered.  Table: {0}", tableName));

                return schematic.FindTable(type);
            }

            #region Read
            // to list
            private static void _fillReadQuery(ExpressionQuerySqlResolutionContainer query, IQuerySchematic schematic, HashSet<Type> registeredTables, JArray property)
            {
                var firstNode = property.First;
                var children = firstNode.Children();

                foreach (var child in children)
                {
                    var node = child as JProperty;

                    if (node == null) throw new QueryParseException("Query not valid.  Parse node into JProperty");

                    switch (node.Name)
                    {
                        case SELECT:
                            _parseSelect(query, schematic, registeredTables, node);

                            // resolve selected columns
                            ExpressionQuerySelectResolver.ResolveSelectAll(schematic, query);
                            break;
                        case JOIN:
                            _parseJoin(query, node);
                            break;
                        case WHERE:
                            _parseWhere(query, schematic, registeredTables, node);
                            break;
                        default:
                            throw new QueryParseException(string.Format("Node not valid.  Name: {0}", node.Name));
                    }
                }
            }

            // one record
            private static void _fillReadQuery(ExpressionQuerySqlResolutionContainer query, IQuerySchematic schematic, HashSet<Type> registeredTables, JObject property)
            {
                var children = property.Children();

                foreach (var child in children)
                {
                    var node = child as JProperty;

                    if (node == null) throw new QueryParseException("Query not valid.  Parse node into JProperty");

                    switch (node.Name)
                    {
                        case SELECT:
                            _parseSelect(query, schematic, registeredTables, node);

                            // resolve selected columns
                            ExpressionQuerySelectResolver.ResolveSelectAll(schematic, query);
                            break;
                        case JOIN:
                            _parseJoin(query, node);
                            break;
                        case WHERE:
                            _parseWhere(query, schematic, registeredTables, node);
                            break;
                        default:
                            throw new QueryParseException(string.Format("Node not valid.  Name: {0}", node.Name));
                    }
                }
            }
            #endregion

            private static void _fillSaveQuery(ExpressionQuerySqlResolutionContainer query, JProperty property)
            {

            }

            private static void _fillDeleteQuery(ExpressionQuerySqlResolutionContainer query, JProperty property)
            {

            }

            private static void _parseWhere(ExpressionQuerySqlResolutionContainer query, IQuerySchematic schematic, HashSet<Type> registeredTables, JProperty property)
            {
                var whereArray = property.First as JArray;

                if (whereArray == null) throw new QueryParseException("Where node is not valid, must be an array");

                var whereStatements = whereArray.Children();

                foreach (var value in whereStatements.Select(statement => statement as JValue))
                {
                    if (value == null) throw new QueryParseException("Where statement not valid");

                    var valueStatement = value.Value.ToString();

                    var operators = _operators.Where(w => valueStatement.Contains(w)).ToList();

                    if (operators.Count == 0) throw new QueryParseException(string.Format("Operator not recognized in where statement. Statement: {0}", valueStatement));

                    if (operators.Count == 2) throw new QueryParseException(string.Format("Cannot have two operators in where statement. Statement: {0}", valueStatement));

                    var segments = valueStatement.Split(operators.ToArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (segments.Count() != 2) throw new QueryParseException(string.Format("Could not parse where statement. Statement: {0}", valueStatement));

                    string parameterKey;
                    query.AddParameter(segments[1], out parameterKey);

                    var firstSegment = segments[0];
                    var whereSplit = firstSegment.Split('.');

                    if (whereSplit.Count() == 1)
                    {
                        var fromTable = query.From();
                        var mainTableColumn = whereSplit[0];

                        // make sure table has the column
                        if (!fromTable.HasColumn(mainTableColumn))
                        {
                            throw new QueryParseException(string.Format("Column not found for table.  Table: {0}, Column: {1}", fromTable.Table.ToString(TableNameFormat.Plain), mainTableColumn));
                        }

                        var finalColumnName = fromTable.Table.GetColumnName(mainTableColumn);

                        query.AddWhere(string.Format("{0}{1}{2}", finalColumnName, operators[0], parameterKey));

                        return;
                    }

                    var table = _findTable(registeredTables, schematic, whereSplit[0]);
                    var column = whereSplit[1];

                    // make sure table has the column
                    if (!table.HasColumn(column))
                    {
                        throw new QueryParseException(string.Format("Column not found for table.  Table: {0}, Column: {1}", table.Table.ToString(TableNameFormat.Plain), column));
                    }

                    var tableAndColumn = string.Format("[{0}].[{1}]", table.Table.ToString(TableNameFormat.Plain), column);

                    query.AddWhere(string.Format("{0}{1}{2}", tableAndColumn, operators[0], parameterKey));

                    return;
                }
            }

            private static void _parseJoin(ExpressionQuerySqlResolutionContainer query, JProperty property)
            {

            }

            private static void _parseSelect(ExpressionQuerySqlResolutionContainer query, IQuerySchematic schematic, HashSet<Type> registeredTables, JProperty property)
            {
                var selectArray = property.First as JArray;

                if (selectArray == null) throw new QueryParseException("Select node is not valid, must be an array");

                var selectStatements = selectArray.Children().ToList();

                // if we are here then we are selecting specific columns
                schematic.UnSelectAll();

                foreach (var value in selectStatements.Select(statement => statement as JValue))
                {
                    if (value == null) throw new QueryParseException("Select statement not valid");

                    var valueStatement = value.Value.ToString();
                    var statementSplit = valueStatement.Split('.');

                    if (statementSplit.Count() == 1)
                    {
                        // select from main table
                        var mainTable = query.From();
                        var ordinal = schematic.NextOrdinal();

                        try
                        {
                            mainTable.Select(statementSplit[0], ordinal);
                            return;
                        }
                        catch (Exception)
                        {
                            throw new QueryParseException(string.Format("Column not found for table.  Table: {0}, Column: {1}", mainTable.Table.ToString(TableNameFormat.Plain), statementSplit[0]));
                        }
                    }

                    // select from other table
                    var fromTable = statementSplit[0];
                    var fromColumn = statementSplit[1];
                    var table = _findTable(registeredTables, schematic, fromTable);
                    var nextOrdinal = schematic.NextOrdinal();

                    try
                    {
                        if (string.Equals(fromColumn, "*"))
                        {
                            table.SelectAll(nextOrdinal);
                            return;
                        }

                        table.Select(fromColumn, nextOrdinal);
                    }
                    catch (Exception)
                    {
                        throw new QueryParseException(string.Format("Column not found for table.  Table: {0}, Column: {1}", table.Table.ToString(TableNameFormat.Plain), fromColumn));
                    }
                }
            }
        }

        #endregion
    }
}
