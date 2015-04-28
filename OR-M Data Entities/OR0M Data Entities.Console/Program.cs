using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.ObjectMapping;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main2(string[] args)
        {
            var context = new DbSqlContext("sqlExpress");
            var ids = new List<int> {1, 2, 3};
            var lst = new SqlSet<Contact>(context);
            var result = lst.FirstOrDefault(w => ids.Contains(w.ID) && w.Number.Phone == "" && w.FirstName.Contains("James"));

            var list = new List<Contact>();
            var item = lst.FirstOrDefault(w => w.ID == 1);
        }

        static void Main(string[] args)
        {
           Main2(args);return;

            var context = new DbSqlContext("sqlExpress");

            var lst = new List<string> { "James", "Megan" };

            var orderedQuery =
                context.SelectAll<Contact>()
                    .Where<Contact>(
                        w =>
                            DbFunctions.Convert(SqlDbType.VarChar, w.ID, null) ==
                            DbFunctions.Convert(SqlDbType.VarChar, "", null))
                    .ToList<Contact>();

            if (orderedQuery != null)
            {
                
            }

            var areader =
                context.Select<Person>(w => new
                {
                    Name = w.FirstName,
                    Trim = w.LastName
                })
                .Include<Car>(w => new
                {
                    Test = w.ID
                })
                    .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
                    .Where<Person>(w => w.FirstName.Equals("James"))
                    .ToList<dynamic>();

            var reader =
                context.SelectAll<Person>().Include<Car>()
                    .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
                    .Where<Person>(w => w.FirstName.Equals("James"))
                    .ToList<dynamic>();

            var sreader =
                context.Select<Person>(w => w.FirstName)
                    .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
                    .Where<Person>(w => w.FirstName.Equals("James"))
                    .ToList<string>();

            var treader =
                context.Select<Person>(w => new Car
                {
                    Name = w.FirstName,
                    Trim = w.LastName
                })
                    .InnerJoin<Person, Car>((person, car) => person.CarID == car.ID)
                    .Where<Person>(w => w.FirstName.Equals("James"))
                    .ToList<Car>();

            if (reader != null && treader != null && sreader != null && areader != null)
            {

            }

            var val = context.ExecuteQuery<int>("Select 1 From Car").First();

            if (val == 1)
            {

            }

            // var parent = context.Find<Parent>(1);
            var c = context.Find<Contact>(1);

            if (c != null)
            {

            }

            var name = context.Find<Name>(2);

            var names = context.SelectAll<Name>().Where<Name>(w => w.ID == 7).ToList<Name>().Select(w => new Car { Name = w.Value });

            if (name != null)
            {

            }

            //if (parent != null)
            //{

            //}

            var s = DateTime.Now;
            var testItem = new Contact();
            //context.From<Contact>()
            //    .Select<Contact>()
            //    .Where<Contact>(w => w.ID == 16)
            //    .First<Contact>();

            //context.Find<Contact>(16); 

            var e = DateTime.Now;

            var testSave = new Contact
            {
                FirstName = "James",
                LastName = "Demeuse Just Added",
            };

            var testAppointment = new Appointment
            {
                Description = "JUST ADDED APT!"
            };

            var testAddress = new Address
            {
                Addy = "JUST ADDED!",
                State = new StateCode
                {
                    Value = "MI"
                }
            };

            var testZip = new Zip
            {
                Zip5 = "55416",
                Zip4 = "WIN!"
            };

            testAddress.ZipCode = new List<Zip>();
            testAddress.ZipCode.Add(testZip);
            testAppointment.Address = new List<Address> { testAddress };
            testSave.Appointments = new List<Appointment>();
            testSave.Name = null;
            //    new List<Name>
            //{ 
            //    new Name
            //    {
            //        Value = "sldfljklsdf"
            //    }
            //};
            testSave.Appointments.Add(testAppointment);

            testSave.Number = new PhoneNumber
            {
                Phone = "(414) 530-3086"
            };

            context.SaveChanges(testSave);

            testItem =
                context.SelectAll<Contact>()
                    .Where<Contact>(w => w.ID == testSave.ID)
                    .First<Contact>();

            context.Delete(testSave);

            testItem =
               context.SelectAll<Contact>()
                   .Where<Contact>(w => w.ID == testSave.ID)
                   .First<Contact>();

            var tt = e - s;

            if (tt.Minutes != 0)
            {

            }

            if (testItem != null)
            {

            }

            var currentDateTime = DateTime.Now;

            var totalMilliseconds = 0d;
            var max = 1000;
            var ct = 0;

            for (var i = 0; i < max; i++)
            {
                var start = DateTime.Now;
                var item = context.SelectAll<Contact>()
                    .Where<Contact>(w => w.ID == 1)
                    .First<Contact>();
                //    context.From<Policy>()
                //.Where<Policy>(w => DbFunctions.Cast(w.CreatedDate, SqlDbType.Date) == DbFunctions.Cast(currentDateTime, SqlDbType.Date))
                //.Select<Policy>()
                //.First<Policy>();

                if (item != null)
                {

                }

                var end = DateTime.Now;

                totalMilliseconds += (end - start).TotalMilliseconds;
                ct++;
            }

            var final = totalMilliseconds / ct;

            if (final != 0)
            {

            }
        }
    }

    public static class Extension
    {
        public static SqlSet<TSource> Where<TSource>(this SqlSet<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            SqlExpressionWhereResolver.Resolve(expression, source.Query);

            return source;
        }

        public static TSource First<TSource>(this SqlSet<TSource> source)
        {
            source.Query.Take = 1;

            return default(TSource);
        }

        public static TSource First<TSource>(this SqlSet<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Query.Take = 1;

            source.Where(expression);

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this SqlSet<TSource> source)
        {
            source.Query.Take = 1;
            source.Query.IsFirstOrDefault = true;

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this SqlSet<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Query.Take = 1;
            source.Query.IsFirstOrDefault = true;
            source.Where(expression);

            var sql = source.Query.ToSql();

            if (sql != "")
            {
                
            }

            return default(TSource);
        }

        public static SqlSet<TResult> Select<TSource, TResult>(this SqlSet<TSource> source,
            Expression<Func<TSource, int, TResult>> selector)
        {
            var result = new SqlSet<TResult>(source.Query, source.Context);

            //SqlExpressionSelectResolver.Resolve(selector, result.Query);

            return result;
        }

        public static SqlSet<TResult> Select<TSource, TResult>(this SqlSet<TSource> source,
            Expression<Func<TSource, TResult>> selector)
        {
            var result = new SqlSet<TResult>(source.Query, source.Context);

            SqlExpressionSelectResolver.Resolve(selector, result.Query);

            return result;
        }
    }

    public class SqlSet<T> : IEnumerable
    {
        public readonly SqlSetQuery Query;
        public readonly DataFetching Context;

        public SqlSet(DataFetching context)
        {
            var tableName = DatabaseSchemata.GetTableName<T>();

            Query = new SqlSetQuery
            {
                ReturnType = typeof (T), 
                IsLazyLoading = context.IsLazyLoadEnabled,
                From = string.Format("[{0}]", tableName)
            };

            Context = context;

            _initializeSelect(typeof(T), tableName);
        }

        public SqlSet(SqlSetQuery query, DataFetching context)
        {
            Query = query;
            Context = context;
        }

        private void _initializeSelect(Type type, string tableName)
        {
            if (Query.IsLazyLoading)
            {
                foreach (
                    var item in
                        type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
                {
                    Query.Select.Add(new SqlSelectColumn(tableName, DatabaseSchemata.GetColumnName(item), type));
                }
            }
            else
            {
                foreach (
                    var item in
                        type.GetProperties())
                {
                    var foreignKeyAttrubute = item.GetCustomAttribute<ForeignKeyAttribute>();

                    if (foreignKeyAttrubute != null)
                    {
                        var isList = item.PropertyType.IsList();
                        var fkPropertyType = isList
                            ? item.PropertyType.GetGenericArguments()[0]
                            : item.PropertyType;

                        _initializeSelect(fkPropertyType, item.Name);
                    }
                    else
                    {
                        Query.Select.Add(new SqlSelectColumn(tableName, DatabaseSchemata.GetColumnName(item), type));
                    }
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }

    public class SqlSetQuery
    {
        public SqlSetQuery()
        {
            Select = new List<SqlSelectColumn>();
            From = String.Empty;
            Where = new List<string>();
            Join = new List<string>();
            OrderBy = new List<string>();
            _parameters = new List<SqlParameter>();
        }

        public bool IsLazyLoading { get; set; }

        public Type ReturnType { get; set; }

        public List<SqlSelectColumn> Select { get; set; }

        public int Take { get; set; }

        public bool Distinct { get; set; }

        public string From { get; set; }

        public List<string> Where { get; set; }

        public List<string> Join { get; set; }

        public List<string> OrderBy { get; set; }

        public bool IsFirstOrDefault { get; set; }

        private readonly List<SqlParameter> _parameters;

        public IEnumerable<SqlParameter> Parameters
        {
            get { return _parameters; }
        } 

        public string GetNextParameter()
        {
            return String.Format("@Param{0}", _parameters.Count);
        }

        public void AddParameter(string parameter, object value)
        {
            _parameters.Add(new SqlParameter(parameter, value));
        }

        public string ToSql()
        {
            var select = string.Format("SELECT {0}{1}", Take > 0 ? string.Format("TOP {0} ", Take) : "",
                Distinct ? "DISTINCT " : "");

            var columns = Select.Aggregate(string.Empty,
                (current, column) => current + string.Format("{0},", column.GetSqlText()));

            var where = Where.Aggregate(string.Empty,
                (current, column) =>
                    string.IsNullOrWhiteSpace(current)
                        ? string.Format("WHERE {0} ", column)
                        : current + string.Format("AND {0} ", column));

            var sql = string.Format("{0}{1} FROM {2}", select, columns, where);
            return sql;
        }
    }

    public class SqlSelectColumn : IEquatable<SqlSelectColumn>
    {
        public SqlSelectColumn(string tableName, string columnName, Type type)
        {
            TableName = tableName;
            ColumnName = columnName;
            Type = type;
            TableAndColumnName = string.Format("[{0}].[{1}]", TableName, ColumnName);
        }

        public Type Type { get; set; }

        public string TableAndColumnName { get; private set; }

        public string TableName { get; private set; }

        public string ColumnName { get; private set; }

        public string Alias { get; set; }

        public string GetSqlText()
        {
            return Alias == ColumnName
                ? TableAndColumnName
                : string.IsNullOrWhiteSpace(Alias)
                    ? TableAndColumnName
                    : string.Format("{0} As [{1}]", TableAndColumnName, Alias);
        }

        public bool Equals(SqlSelectColumn other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return TableAndColumnName == other.TableAndColumnName && Type == other.Type;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null.
            var hashProductName = Type.GetHashCode();

            //Get hash code for the Code field.
            var hashProductCode = TableAndColumnName.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName ^ hashProductCode;
        }
    }

    public abstract class SqlExpressionResolver
    {
        protected static object GetCompareValue(BinaryExpression expression, SqlDbType transformType)
        {
            var leftSideHasParameter = HasParameter(expression.Left);

            return GetValue(leftSideHasParameter ? expression.Right as dynamic : expression.Left as dynamic);
        }

        protected static string[] GetTableAndColumnName(Expression expression, string columnName = "")
        {
            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression != null)
            {
                return GetTableAndColumnName(binaryExpression.Left);
            }

            var memberExpression = expression as MemberExpression;

            if (memberExpression != null && memberExpression.Expression.NodeType != ExpressionType.Parameter)
            {
                return GetTableAndColumnName(memberExpression.Expression, memberExpression.Member.Name);
            }

            return string.IsNullOrWhiteSpace(columnName)
                ? new[]
                {
                    DatabaseSchemata.GetTableName(memberExpression.Expression.Type),
                    DatabaseSchemata.GetColumnName(((MemberExpression) expression).Member)
                }
                : new[]
                {
                    ((MemberExpression) expression).Member.Name, 
                    columnName
                };

        }

        protected static object GetValue(ConstantExpression expression)
        {
            return expression.Value;
        }

        protected static object GetValue(MemberExpression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }

        protected static object GetValue(MethodCallExpression expression)
        {
            var isCasting = DatabaseOperations.IsCasting(expression);
            var isConverting = DatabaseOperations.IsConverting(expression);
            var objectMember = Expression.Convert(expression, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            var result = getter();

            if (!isCasting && !isConverting) return result;

            //var transform = GetTransformType(expression);
            //var transformResult = new SqlValue(result, transform);

            //if (isConverting)
            //{
            //    // only from where statement
            //    transformResult.AddFunction(DbFunctions.Convert, transform, 1);
            //}

            //if (isCasting)
            //{
            //    transformResult.AddFunction(DbFunctions.Cast, transform);
            //}

            return null;
        }

        protected static object GetValue(UnaryExpression expression)
        {
            return GetValue(expression.Operand as dynamic);
        }

        protected static string EnumerateList(IEnumerable list, SqlSetQuery query)
        {
            var result = "";

            foreach (var item in list)
            {
                var parameter = query.GetNextParameter();
                query.AddParameter(parameter, item);

                result += parameter + ",";
            }

            return result.TrimEnd(',');
        }

        protected static bool HasParameter(object expression)
        {
            return HasParameter(expression as dynamic);
        }

        protected static bool HasParameter(MethodCallExpression expression)
        {
            var e = expression.Object;

            return e != null ? HasParameter(expression.Object as dynamic) : expression.Arguments.Select(arg => HasParameter(arg as dynamic)).Any(hasParameter => hasParameter);
        }

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

        protected static bool HasLeft(Expression expression)
        {
            return expression.NodeType == ExpressionType.And
                || expression.NodeType == ExpressionType.AndAlso
                || expression.NodeType == ExpressionType.Or
                || expression.NodeType == ExpressionType.OrElse;
        }
    }

    public class SqlExpressionSelectResolver : SqlExpressionResolver
    {
        public static void Resolve<TSource, TResult>(Expression<Func<TSource, TResult>> selector, SqlSetQuery query)
        {
            _evaltateExpressionTree(selector.Body, query);
        }

        private static void _evaluateMemberInit(Expression expression, SqlSetQuery query, List<SqlSelectColumn> columns)
        {
            var memberInitExpression = expression as MemberInitExpression;

            foreach (MemberAssignment binding in memberInitExpression.Bindings)
            {
                if (binding.Expression.NodeType == ExpressionType.New) continue;

                var bindingExpression = binding.Expression as MemberInitExpression;

                if (bindingExpression != null)
                {
                    _evaluateMemberInit(bindingExpression, query, columns);
                    continue;
                }

                var tableAndColumnName = GetTableAndColumnName(binding.Expression);
                var alias = binding.Member.Name;
                var index = query.Select.IndexOf(new SqlSelectColumn(tableAndColumnName[0], tableAndColumnName[1], ((MemberExpression)binding.Expression).Expression.Type));
                var sqlColumnSelect = query.Select[index];

                sqlColumnSelect.Alias = alias;

                columns.Add(sqlColumnSelect);
            }
        }

        private static void _evaltateExpressionTree(Expression expression, SqlSetQuery query)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.New:
                    var newExpression = expression as NewExpression;

                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        var arg = newExpression.Arguments[i] as MemberExpression;

                    }
                    break;
                case ExpressionType.Convert:
                    _evaltateExpressionTree(((UnaryExpression)expression).Operand, query);
                    break;
                case ExpressionType.MemberInit:
                    var memberInitExpression = expression as MemberInitExpression;

                    var sqlColumns = new List<SqlSelectColumn>();

                    _evaluateMemberInit(memberInitExpression, query, sqlColumns);

                    query.Select = sqlColumns;

                    break;
                case ExpressionType.MemberAccess:
                    var memberAccessExpression = ((MemberExpression) expression);
                    var memberAccessTableName = DatabaseSchemata.GetTableName(memberAccessExpression.Expression.Type);
                    var memberAccessColumnName = DatabaseSchemata.GetColumnName(memberAccessExpression.Member);

                    var idx = query.Select.IndexOf(new SqlSelectColumn(memberAccessTableName, memberAccessColumnName, memberAccessExpression.Expression.Type));

                    var item = query.Select[idx];

                    query.Select = new List<SqlSelectColumn> {item};

                    break;
            }
        }
    }

    public class SqlExpressionWhereResolver : SqlExpressionResolver
    {
        public static void Resolve<T>(Expression<Func<T, bool>> expression, SqlSetQuery query)
        {
            _evaluateExpressionTree(expression.Body, query);
        }

        private static void _evaluateExpressionTree(Expression expression, SqlSetQuery query)
        {
            if (HasLeft(expression))
            {
                _evaluateWhere(((dynamic)expression).Right, null, query);

                _evaluateExpressionTree(((BinaryExpression)expression).Left, query);
            }
            else
            {
                _evaluateWhere((dynamic)expression, null, query);
            }
        }

        private static void _evaluateWhere(object expression, ExpressionType? nodeType, SqlSetQuery query)
        {
            _evaluateWhere(expression as dynamic, nodeType, query);
        }

        private static void _evaluateWhere(MethodCallExpression expression, ExpressionType? nodeType, SqlSetQuery query)
        {
            var argsHaveParameter = false;
            var compareName = string.Format("{0}{1}",
                nodeType != null && nodeType.Value == ExpressionType.Not ? "NOT" : string.Empty,
                expression.Method.Name);

            foreach (var arg in expression.Arguments.Where(HasParameter))
            {
                argsHaveParameter = true;
                var compareValue = GetValue(expression.Object as dynamic);
                var tableAndColumnName = GetTableAndColumnName(arg);
                var compareString = _getComparisonString(compareName,
                    string.Format("[{0}].[{1}]", tableAndColumnName[0], tableAndColumnName[1]), compareValue, query);

                query.Where.Add(compareString);
                break;
            }

            if (!argsHaveParameter)
            {
                var compareValue = GetValue(expression.Arguments[0] as dynamic);
                var tableAndColumnName = GetTableAndColumnName(expression.Object);
                var compareString = _getComparisonString(compareName,
                    string.Format("[{0}].[{1}]", tableAndColumnName[0], tableAndColumnName[1]), compareValue, query);

                query.Where.Add(compareString);
            }
        }



        private static void _evaluateWhere(BinaryExpression expression, ExpressionType? nodeType, SqlSetQuery query)
        {
            var compareName = string.Format("{0}{1}",
                nodeType != null && nodeType.Value == ExpressionType.Not ? "NOT" : string.Empty,
                expression.Type == typeof(bool) ? "EQUALS" : expression.Method.Name);
            var compareValue = GetCompareValue(expression, SqlDbType.VarChar);
            var tableAndColumnName = GetTableAndColumnName(expression);
            var compareString = _getComparisonString(compareName,
                string.Format("[{0}].[{1}]", tableAndColumnName[0], tableAndColumnName[1]), compareValue, query);

            query.Where.Add(compareString);
        }

        private static void _evaluateWhere(UnaryExpression expression, ExpressionType? nodeType, SqlSetQuery query)
        {
            _evaluateWhere(expression.Operand as dynamic, expression.NodeType, query);

        }

        protected static string _addParameter(object value, SqlSetQuery query, string comparisonName)
        {
            var compareValue = _resolveContainsObject(value, comparisonName);

            var parameter = query.GetNextParameter();
            query.AddParameter(parameter, compareValue);

            return parameter;
        }

        private static string _resolveContainsObject(object compareValue, string comparisonName)
        {
            var compareValueAsString = Convert.ToString(compareValue);

            switch (comparisonName)
            {
                case "CONTAINS":
                    return "%" + compareValueAsString + "%";
                case "STARTSWITH":
                case "NOTSTARTSWITH":
                    return compareValueAsString + "%";
                case "ENDSWITH":
                case "NOTENDSWITH":
                    return "%" + compareValueAsString;
                default:
                    return compareValueAsString;
            }
        }

        private static string _getComparisonString(string methodName, string tableColumnName, object compareValue, SqlSetQuery query)
        {
            string comparisonString;
            var isCompareValueList = compareValue.IsList();
            var comparisonName = methodName.ToUpper().Contains("EQUALITY") ? "EQUALS" : methodName.ToUpper().Replace(" ", "");

            if (methodName.ToUpper() == "CONTAINS")
            {
                return string.Format(isCompareValueList ? " {0} IN ({1}) " : " {0} LIKE {1}", tableColumnName,
                    isCompareValueList
                        ? EnumerateList(compareValue as IEnumerable, query)
                        : _addParameter(compareValue, query, comparisonName));
            }

            if (methodName.ToUpper() == "NOTCONTAINS")
            {
                return string.Format(isCompareValueList ? " {0} NOT IN ({1}) " : " {0} NOT LIKE {1}", tableColumnName,
                    isCompareValueList
                        ? EnumerateList(compareValue as IEnumerable, query)
                        : _addParameter(compareValue, query, comparisonName));
            }

            switch (comparisonName)
            {
                case "STARTSWITH":
                case "ENDSWITH":
                    comparisonString = " {0} LIKE {1}";
                    break;
                case "NOTSTARTSWITH":
                case "NOTENDSWITH":
                    comparisonString = " {0} NOT LIKE {1}";
                    break;
                case "EQUALS":
                    comparisonString = " {0} = {1}";
                    break;
                case "GREATERTHAN":
                    comparisonString = " {0} > {1}";
                    break;
                case "GREATERTHANEQUALS":
                    comparisonString = " {0} >= {1}";
                    break;
                case "LESSTHAN":
                    comparisonString = " {0} < {1}";
                    break;
                case "LESSTHANEQUALS":
                    comparisonString = " {0} <= {1}";
                    break;
                case "NOTEQUALS":
                    comparisonString = " {0} != {1}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Cannot resolve comparison type {0}",
                        comparisonName));
            }

            return string.Format(comparisonString, tableColumnName, _addParameter(compareValue, query, comparisonName));
        }
    }
}
