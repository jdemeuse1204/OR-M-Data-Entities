/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
namespace OR_M_Data_Entities.Data.Definition
{
    public class SqlExistsString
    {
        public SqlExistsString(bool invert, string joinString, string fromTableName)
        {
            Invert = invert;
            JoinString = joinString;
            FromTableName = fromTableName;
        }

        public readonly bool Invert;

        public readonly string JoinString;

        public readonly string FromTableName;
    }
}
