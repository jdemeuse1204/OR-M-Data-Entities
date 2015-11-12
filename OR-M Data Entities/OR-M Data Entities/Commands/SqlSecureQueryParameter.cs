using OR_M_Data_Entities.Commands.Secure;

namespace OR_M_Data_Entities.Commands
{
    public class SqlSecureQueryParameter
    {
        public string Key { get; set; }

        public string DbColumnName { get; set; }

        public SqlSecureObject Value { get; set; }
    }
}
