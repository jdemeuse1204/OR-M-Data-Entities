using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectFunctions;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping
{
    public class ObjectColumn : ObjectFunctionTextBuilder
    {
        public string Name { get; set; }

        public SqlDbType DataType { get; set; }

        public bool IsSelected { get; private set; }

        public bool IsPartOfValidation
        {
            get { return CompareValues.Count != 0; }
        }

        public bool HasJoins
        {
            get { return Joins.Count != 0; }
        }

        public bool HasWheres
        {
            get { return CompareValues.Count != 0; ; }
        }

        public List<KeyValuePair<object, ComparisonType>> CompareValues { get; set; }

        public List<KeyValuePair<ObjectColumn, JoinType>> Joins { get; set; }

        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public bool HasAlias { get { return !TableName.Equals(TableAlias); } }

        public bool IsKey { get; private set; }

        public string GetText()
        {
            return string.Format("[{0}].[{1}]",
                HasAlias ? TableAlias : TableName,
                Name);
        }

        public string GetJoinText()
        {
            return Joins.Aggregate("",
                (current, @join) =>
                    current +
                    string.Format(
                        @join.Value == JoinType.Inner ? "INNER JOIN {0}On {1} = {2} " : "LEFT JOIN {0}On {1} = {2} ",
                        string.Format(@join.Key.HasAlias ? "[{0}] As [{1}] " : "{0}[{1}] ",
                            @join.Key.HasAlias ? @join.Key.TableName : "",
                            @join.Key.HasAlias ? @join.Key.TableAlias : @join.Key.TableName),
                        string.Format("[{0}].[{1}]", @join.Key.HasAlias ? @join.Key.TableAlias : @join.Key.TableName,
                            @join.Key.Name),
                        string.Format("[{0}].[{1}]", HasAlias ? TableAlias : TableName,
                            Name)));
        }

        public void GetWhereContainer(WhereContainer whereContainer)
        {
            foreach (var compareValue in CompareValues)
            {
                whereContainer.ValidationStatements.Add(_getComparisonString(this, compareValue.Key, compareValue.Value,
                    whereContainer.Parameters));
            }
        }

        private string _addParameter(object value, Dictionary<string, object> parameters)
        {
            var parameter = parameters.GetNextParameter();
            parameters.Add(parameter, value);

            return parameter;
        }

        public string _getComparisonString(ObjectColumn objectColumn, object compareValue, ComparisonType comparisonType, Dictionary<string, object> parameters)
        {
            var isCompareValueList = compareValue.IsList();

            if (comparisonType == ComparisonType.Contains)
            {
                return string.Format(isCompareValueList ? " {0} IN ({1}) " : " {0} LIKE {1}", objectColumn.GetText(),
                    isCompareValueList
                        ? DatabaseOperations.EnumerateList(compareValue as IEnumerable, parameters)
                        : _addParameter(compareValue, parameters));
            }

            switch (comparisonType)
            {
                case ComparisonType.BeginsWith:
                case ComparisonType.EndsWith:
                    return string.Format(" {0} LIKE {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.Equals:
                    return string.Format(" {0} = {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.EqualsIgnoreCase:
                    return "";
                case ComparisonType.EqualsTruncateTime:
                    return "";
                case ComparisonType.GreaterThan:
                    return string.Format(" {0} > {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.GreaterThanEquals:
                    return string.Format(" {0} >= {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.LessThan:
                    return string.Format(" {0} < {1}", objectColumn.GetText(),
                         _addParameter(compareValue, parameters));
                case ComparisonType.LessThanEquals:
                    return string.Format(" {0} <= {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                case ComparisonType.NotEqual:
                    return string.Format(" {0} != {1}", objectColumn.GetText(),
                        _addParameter(compareValue, parameters));
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Cannot resolve comparison type {0}",
                        comparisonType));
            }
        }

        public ObjectColumn(PropertyInfo memberInfo, string tableName = "", string alias = "")
        {
            Name = DatabaseSchemata.GetColumnName(memberInfo);
            DataType = DatabaseSchemata.GetSqlDbType(memberInfo.PropertyType);
            IsSelected = true;
            CompareValues = new List<KeyValuePair<object, ComparisonType>>();
            Joins = new List<KeyValuePair<ObjectColumn, JoinType>>();
            TableName = tableName;
            TableAlias = alias;
            IsKey = DatabaseSchemata.IsPrimaryKey(memberInfo);
        }
    }
}
