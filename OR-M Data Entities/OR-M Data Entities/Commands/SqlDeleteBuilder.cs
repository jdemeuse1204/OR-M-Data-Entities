/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Data.SqlClient;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Exceptions;

namespace OR_M_Data_Entities.Commands
{
    public sealed class SqlDeleteBuilder : SqlValidation, ISqlBuilder
    {
        #region Properties
        private string _delete { get; set; }
        #endregion

        #region Constructor
        public SqlDeleteBuilder()
        {
            _delete = string.Empty;
        }
        #endregion

        #region Methods
        public SqlCommand Build(SqlConnection connection)
        {
            if (string.IsNullOrWhiteSpace(TableName))
            {
                throw new QueryNotValidException("Table statement missing");
            }

            _delete = string.Format(" DELETE FROM [{0}] ", TableName.TrimStart('[').TrimEnd(']'));

            var sql = _delete + GetValidation() + ";Select @@ROWCOUNT as 'int';";
            var cmd = new SqlCommand(sql, connection);

            InsertParameters(cmd);

            return cmd;
        }
        #endregion
    }
}
