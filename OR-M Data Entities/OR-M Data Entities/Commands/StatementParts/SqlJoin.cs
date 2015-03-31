/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public sealed class SqlJoin : IEquatable<SqlJoin>
    {
        public SqlTableColumnPair ParentEntity { get; set; }

        public SqlTableColumnPair JoinEntity { get; set; }

        public JoinType Type { get; set; }

        public string GetJoinText()
        {
            var parentTableName = DatabaseSchemata.GetTableName(ParentEntity.Table);
            var parentColumnName = DatabaseSchemata.GetColumnName(ParentEntity.Column);
            var joinTableName = DatabaseSchemata.GetTableName(JoinEntity.Table);
            var joinColumnName = DatabaseSchemata.GetColumnName(JoinEntity.Column);

            switch (Type)
            {
                case JoinType.Equi:
                    return string.Format(" [{0}].[{1}] = [{2}].[{3}]", 
                        parentTableName,
                        parentColumnName,
                        joinTableName,
                        joinColumnName);
                case JoinType.Inner:
                    return string.Format(" INNER JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}]",
                        joinTableName,
                        joinColumnName,
                        parentTableName,
                        parentColumnName);
                case JoinType.Left:
                    return string.Format(" LEFT JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}]",
                        joinTableName,
                        joinColumnName,
                        parentTableName,
                        parentColumnName);
                default:
                    return "";
            }
        }


        public bool Equals(SqlJoin other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return ParentEntity.Table == other.ParentEntity.Table && JoinEntity.Table == other.JoinEntity.Table;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null.
            var hashProductName = ParentEntity.Table.GetHashCode();

            //Get hash code for the Code field.
            var hashProductCode = JoinEntity.Table.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductName ^ hashProductCode;
        }
    }
}
