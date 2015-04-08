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
    public sealed class SqlTable
    {
        #region Constructor
        public SqlTable(Type tableType)
        {
            TableName = DatabaseSchemata.GetTableName(tableType);
            TableType = tableType;
        }
        #endregion

        #region Properties
        public string TableName { get; private set; }

        public Type TableType { get; private set; }
        #endregion
    }
}
