/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Containers;
using OR_M_Data_Entities.Expressions.ObjectFunctions;

namespace OR_M_Data_Entities.Expressions.ObjectMapping
{
	public class ObjectColumn : ObjectFunctionTextBuilder
	{
        public int? SequenceNumber { get; set; }

		public string Name { get; set; }

		public string PropertyName { get; set; }

        public ObjectColumnOrderType OrderType { get; set; }

        public string ColumnAlias { get; set; }

		public SqlDbType DataType { get; set; }

		public bool IsSelected { get; set; }

        public bool HasOrderSequence 
        {
            get { return SequenceNumber != null; }
        }

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

        public bool IsOrdered
        {
            get { return OrderBySequence != null; ; }
        }

		public List<KeyValuePair<object, ComparisonType>> CompareValues { get; set; }

		public List<KeyValuePair<ObjectColumn, JoinType>> Joins { get; set; }

		public string TableName { get; set; }

		public string TableAlias { get; set; }

		public bool HasTableAlias { get { return !TableName.Equals(TableAlias); } }

        public bool HasColumnAlias { get { return !Name.Equals(ColumnAlias); } }

		public bool IsKey { get; private set; }

        public int Order { get; private set; }

        public int? OrderBySequence { get; private set; }

		public string GetText()
		{
			return string.Format("[{0}].[{1}]",
				HasTableAlias ? string.IsNullOrWhiteSpace(TableAlias) ? TableName : TableAlias : TableName,
				Name);
		}

		public string GetJoinText()
		{
			return Joins.Aggregate("",
				(current, @join) =>
					current +
					string.Format(
						@join.Value == JoinType.Inner ? "INNER JOIN {0}On {1} = {2} " : "LEFT JOIN {0}On {1} = {2} ",
						string.Format(@join.Key.HasTableAlias ? "[{0}] As [{1}] " : "{0}[{1}] ",
							@join.Key.HasTableAlias ? @join.Key.TableName : "",
							@join.Key.HasTableAlias ? @join.Key.TableAlias : @join.Key.TableName),
						string.Format("[{0}].[{1}]", @join.Key.HasTableAlias ? @join.Key.TableAlias : @join.Key.TableName,
							@join.Key.Name),
						string.Format("[{0}].[{1}]", HasTableAlias ? TableAlias : TableName,
							Name)));
		}

		public string GetReaderLookupText(bool parentTableHasForeignKey)
		{
			return parentTableHasForeignKey ? string.Format("{0}{1}", HasTableAlias ? TableAlias : TableName, Name) : Name;
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

        private string _resolveContainsObject(object compareValue, ComparisonType comparisonType)
        {
            var compareValueAsString = Convert.ToString(compareValue);

            switch (comparisonType)
            {
                case ComparisonType.Contains:
                    return "%" + compareValueAsString + "%";
                case ComparisonType.BeginsWith:
                    return compareValueAsString + "%";
                case ComparisonType.EndsWith:
                    return "%" + compareValueAsString;
                default:
                    return compareValueAsString;
            }
        }

		public string _getComparisonString(ObjectColumn objectColumn, object compareValue, ComparisonType comparisonType, Dictionary<string, object> parameters)
		{
			var isCompareValueList = compareValue.IsList();

			if (comparisonType == ComparisonType.Contains)
			{
			    return string.Format(isCompareValueList ? " {0} IN ({1}) " : " {0} LIKE {1}", objectColumn.GetText(),
			        isCompareValueList
			            ? DatabaseOperations.EnumerateList(compareValue as IEnumerable, parameters)
			            : _addParameter(_resolveContainsObject(compareValue, comparisonType), parameters));
			}

            if (comparisonType == ComparisonType.NotContains)
            {
                return string.Format(isCompareValueList ? " {0} NOT IN ({1}) " : " {0} NOT LIKE {1}", objectColumn.GetText(),
                    isCompareValueList
                        ? DatabaseOperations.EnumerateList(compareValue as IEnumerable, parameters)
                        : _addParameter(_resolveContainsObject(compareValue, comparisonType), parameters));
            }

			switch (comparisonType)
			{
				case ComparisonType.BeginsWith:
				case ComparisonType.EndsWith:
					return string.Format(" {0} LIKE {1}", objectColumn.GetText(),
						_addParameter(compareValue, parameters));
                case ComparisonType.NotBeginsWith:
                case ComparisonType.NotEndsWith:
                    return string.Format(" {0} NOT LIKE {1}", objectColumn.GetText(),
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

		public ObjectColumn(PropertyInfo memberInfo, int order, string tableName = "", string tableAlias = "", bool isSelected = true)
		{
			Name = DatabaseSchemata.GetColumnName(memberInfo);
		    ColumnAlias = Name;
			PropertyName = memberInfo.Name;
			DataType = DatabaseSchemata.GetSqlDbType(memberInfo.PropertyType);
            IsSelected = isSelected;
			CompareValues = new List<KeyValuePair<object, ComparisonType>>();
			Joins = new List<KeyValuePair<ObjectColumn, JoinType>>();
			TableName = tableName;
			TableAlias = tableAlias;
			IsKey = DatabaseSchemata.IsPrimaryKey(memberInfo);
		    Order = order;
		}
	}
}
