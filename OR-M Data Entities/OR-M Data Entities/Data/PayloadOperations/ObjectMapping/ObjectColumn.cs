using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectFunctions;

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

        public List<KeyValuePair<object, ComparisonType>> CompareValues { get; set; }

        public List<KeyValuePair<ObjectColumn, JoinType>> Joins { get; set; }

        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public bool HasAlias { get { return !TableName.Equals(TableAlias); } }

        public bool IsKey { get; private set; }

        public string GetJoinText()
        {
            return Joins.Aggregate("",
                (current, @join) =>
                    current +
                    string.Format(
                        @join.Value == JoinType.Inner ? "INNER JOIN {0}On {1} = {2} " : "LEFT JOIN {0}On {1} = {2} ",
                        string.Format(HasAlias ? "[{0}] As [{1}] " : "{0}[{1}] ",
                            HasAlias ? TableName : "",
                            HasAlias ? TableAlias : TableName),
                        string.Format("[{0}].[{1}]", HasAlias ? TableAlias : TableName,
                            @join.Key.Name),
                        string.Format("[{0}].[{1}]", @join.Key.HasAlias ? @join.Key.TableAlias : @join.Key.TableName,
                            @join.Key.Name)));
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
