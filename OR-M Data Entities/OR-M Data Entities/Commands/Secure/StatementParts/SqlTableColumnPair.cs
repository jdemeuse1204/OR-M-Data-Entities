/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    public sealed class SqlTableColumnPair : SqlColumn, IEquatable<SqlTableColumnPair>
    {
        #region Properties
        public Type Table { get; set; }

        public string TableNameAlias { get; set; }
        #endregion

        #region Methods
        public string GetTableName()
        {
            return string.IsNullOrWhiteSpace(TableNameAlias) ? DatabaseSchemata.GetTableName(Table) : TableNameAlias;
        }

        public string GetSelectColumnText()
        {
            return GetColumnText(GetTableName());
        }

        public string GetSelectColumnTextWithAlias()
        {
            return GetColumnTextWithAlias(GetTableName());
        }
        #endregion

        #region IEquatable
        public bool Equals(SqlTableColumnPair other)
        {
            return Table == other.Table && Column == other.Column;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null.
            var hashTableType = Table.GetHashCode();

            //Get hash code for the Code field.
            var hashColumnMemberInfo = Column.GetHashCode();

            //Calculate the hash code for the product.
            return hashColumnMemberInfo ^ hashTableType;
        }
        #endregion
    }
}
