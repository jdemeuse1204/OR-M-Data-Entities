/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition.Base;

namespace OR_M_Data_Entities.Data.Query.StatementParts
{
    /// <summary>
    /// Base class for all query builders.  Each query builder must have a Table that is being referenced.
    /// </summary>
    public abstract class SqlFromTable : SqlStatement
    {
        #region Properties
        public string TableNameOnly { get; private set; }

        protected string TableName { get; private set; }

        protected string FormattedTableName
        {
            get
            {
                return string.IsNullOrWhiteSpace(TableName) ? "" : TableName.TrimStart('[').TrimEnd(']');
            }
        }
        #endregion

        #region Constructor
        protected SqlFromTable(ConfigurationOptions configuration)
            : base(configuration) 
        {
            TableName = string.Empty;
        }
        #endregion

        #region Methods
        public void Table(TableInfo info)
        {
            TableName = info.ToString();
        }

        public void Table(Type tableType)
        {
            var tableInfo = new TableInfo(tableType);

            Table(tableInfo);

            TableNameOnly = tableInfo.TableNameOnly;
        }
        #endregion
    }
}
