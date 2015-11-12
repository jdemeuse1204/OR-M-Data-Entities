/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Data;
using System.Reflection;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Extensions;

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    public class SqlColumn : SqlFunctionTextBuilder
    {
        #region Constructor
        public SqlColumn(MemberInfo columnInfo)
            : this()
        {
            Column = columnInfo;
        }

        public SqlColumn()
        {
        }
        #endregion

        #region Properties
        public MemberInfo Column { get; set; }

        public SqlDbType DataType { get; set; }
        #endregion

        #region Methods
        public string GetColumnName()
        {
            return Column.GetColumnName();
        }
        #endregion
    }
}
