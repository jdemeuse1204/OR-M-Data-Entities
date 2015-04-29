using System;

namespace OR_M_Data_Entities.Data.Definition
{
    public class SqlColumnSchematic : IEquatable<SqlColumnSchematic>
    {
        public SqlColumnSchematic(string tableName, string columnName, Type type)
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

        public bool Equals(SqlColumnSchematic other)
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
}
