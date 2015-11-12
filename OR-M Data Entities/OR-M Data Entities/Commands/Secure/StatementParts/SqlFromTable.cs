/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Extensions;

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    /// <summary>
    /// Base class for all query builders.  Each query builder must have a Table that is being referenced.
    /// </summary>
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
            var tableInfo = new TableInfo(tableType);

            Table(tableInfo.ToString());
        }

        public void Table<T>()
        {
            Table(typeof(T).GetTableNameWithLinkedServer());
        }
        #endregion
    }
}
