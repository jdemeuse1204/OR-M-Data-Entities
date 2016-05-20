using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Channels;
using Newtonsoft.Json.Linq;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.WebApi
{
    public class DbSqlApiContext : DbSqlContext
    {
        public readonly HashSet<Type> _registeredTables;

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

        public virtual void OnContextCreated() { }

        public void RegisterTable<T>() where T : class
        {
            if (!_registeredTables.Contains(typeof(T))) _registeredTables.Add(typeof(T));
        }

        public object ApiQuery(string query)
        {
            object result = null;

            if (string.IsNullOrEmpty(query)) return null;

            try
            {
                var dynamicQuery = JObject.Parse(query);

                foreach (var property in dynamicQuery.Children().Select(child => child as JProperty))
                {
                    if (!QueryValidator.IsParentValid(property))
                    {
                        throw new QueryParseException("Query not valid.  Parent nodes are: read, save, delete");
                    }

                    var executableQuery = QueryConverter.Convert(_registeredTables, property, DbTableFactory, SchematicFactory, Configuration);

                    ExecuteQuery(executableQuery.Sql(), executableQuery.Parameters());
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

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

            public static ExpressionQuerySqlResolutionContainer Convert(HashSet<Type> tables, JProperty parentRecord, ITableFactory tableFactory, IQuerySchematicFactory querySchematicFactory, IConfigurationOptions configurationOptions)
            {
                if (parentRecord == null) throw new QueryParseException("Query not valid.  Could not parse parent");

                var children = parentRecord.Children().ToList();

                if (children.Count != 1) throw new QueryParseException("Query not valid.  Can only read/save/delete from one table");

                var fromChild = children[0] as JObject;

                if (fromChild == null) throw new QueryParseException("Query not valid.  Could not parse from type");

                var from = fromChild.First as JProperty;

                if (from == null) throw new QueryParseException("Query not valid.  Could not parse from type");

                var type =
                    tables.FirstOrDefault(
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

                        _fillReadQuery(result, from.First as dynamic);
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

            #region Read
            private static void _fillReadQuery(ExpressionQuerySqlResolutionContainer query, JArray property)
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
                            _parseSelect(query, node);
                            break;
                        case JOIN:
                            _parseJoin(query, node);
                            break;
                        case WHERE:
                            _parseWhere(query, node);
                            break;
                        default:
                            throw new QueryParseException(string.Format("Node not valid.  Name: {0}", node.Name));
                    }
                }
            }

            private static void _fillReadQuery(ExpressionQuerySqlResolutionContainer query, JObject property)
            {
                var s = new ExpressionQuerySqlResolutionContainer();
            }
            #endregion

            private static void _fillSaveQuery(ExpressionQuerySqlResolutionContainer query, JProperty property)
            {

            }

            private static void _fillDeleteQuery(ExpressionQuerySqlResolutionContainer query, JProperty property)
            {

            }

            private static void _parseWhere(ExpressionQuerySqlResolutionContainer query, JProperty property)
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

                    // TODO fix me
                    string parameterKey;
                    query.AddParameter(segments[1], out parameterKey);
                    var statement = string.Format("{0}{1}{2}", "", operators[0], parameterKey);
                    query.AddWhere(statement);
                }
            }

            private static void _parseJoin(ExpressionQuerySqlResolutionContainer query, JProperty property)
            {

            }

            private static void _parseSelect(ExpressionQuerySqlResolutionContainer query, JProperty property)
            {

            }
        }

        #endregion
    }
}
