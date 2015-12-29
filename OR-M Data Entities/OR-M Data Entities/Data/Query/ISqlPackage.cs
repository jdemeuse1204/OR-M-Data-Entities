/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System.Data.SqlClient;
using OR_M_Data_Entities.Data.Modification;

namespace OR_M_Data_Entities.Data.Query
{
    public interface ISqlBuilder
    {
        string GetSql();

        ISqlContainer BuildContainer();

        void InsertParameters(SqlCommand cmd);
    }
}
