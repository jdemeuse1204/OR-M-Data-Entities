﻿/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;

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
            Table(DatabaseSchemata.GetTableNameWithLinkedServer(tableType));
        }

        public void Table<T>()
        {
            Table(DatabaseSchemata.GetTableNameWithLinkedServer(typeof(T)));
        }
        #endregion
    }
}
