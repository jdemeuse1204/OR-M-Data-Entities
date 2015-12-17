/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
namespace OR_M_Data_Entities.Data.Modification
{
    public class SqlPartStatement
    {
        public SqlPartStatement(string sql, string declare = null, string set = null)
        {
            Sql = sql;
            Declare = declare;
            Set = set;
        }

        public readonly string Sql;

        public readonly string Declare;

        public readonly string Set;

        public override string ToString()
        {
            return string.Format("{0}{1}{2}",

                string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("{0} \r\r", Declare),

                string.IsNullOrWhiteSpace(Set) ? string.Empty : string.Format("{0} \r\r", Set),
                
                Sql);
        }
    }
}
