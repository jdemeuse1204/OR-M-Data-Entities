/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Data.SqlClient;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Data.Query.StatementParts;

namespace OR_M_Data_Entities.Data.Query
{
	public interface ISqlBuilder
	{
		SqlCommand Build(SqlConnection connection);

        SqlTransactionStatement GetTransactionSql(IEnumerable<SqlSecureQueryParameter> parameters);

	    IEnumerable<SqlSecureQueryParameter> GetParameters();
	}
}
