/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Extensions;

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    public abstract class SqlFromTable : SqlSecureExecutable
    {
        #region Properties
        protected string TableName { get; set; }
        #endregion

        #region Constructor
        protected SqlFromTable()
        {
            TableName = string.Empty;
        }
        #endregion

        #region Methods
        public void Table(string tableName)
        {
            TableName = tableName;
        }

        public void Table(Type tableType)
        {
            Table(tableType.GetTableNameWithLinkedServer());
        }

        public void Table<T>()
        {
            Table(typeof(T).GetTableNameWithLinkedServer());
        }
        #endregion
    }
}
