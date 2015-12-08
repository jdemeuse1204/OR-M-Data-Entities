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

                string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("DECLARE {0} \r\r", string.Concat(Declare.TrimEnd(','), ";")),

                string.IsNullOrWhiteSpace(Set) ? string.Empty : string.Format("{0} \r\r", Set),
                
                Sql);
        }
    }
}
