/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System.Data.SqlClient;

namespace OR_M_Data_Entities.Commands.Support
{
	public interface ISqlBuilder
	{
		void Table(string tableName);

		SqlCommand Build(SqlConnection connection);
	}
}
