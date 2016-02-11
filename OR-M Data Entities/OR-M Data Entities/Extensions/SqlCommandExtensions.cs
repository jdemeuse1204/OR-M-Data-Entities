/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Data.SqlClient;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class SqlCommandExtensions
    {
        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd, SqlConnection connection)
        {
            return new PeekDataReader(cmd, connection);
        }

        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd, SqlConnection connection, IQuerySchematic schematic)
        {
            return new PeekDataReader(cmd, connection, schematic);
        }
    }
}
