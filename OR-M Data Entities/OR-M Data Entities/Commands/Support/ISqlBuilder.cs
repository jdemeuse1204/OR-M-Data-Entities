/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Data.SqlClient;
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.Support
{
	public interface ISqlBuilder
	{
		void Table(string tableName);
		SqlCommand Build(SqlConnection connection, out DataQueryType dataQueryType);
	}
}
