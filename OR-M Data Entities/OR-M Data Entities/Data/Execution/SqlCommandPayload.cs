using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Data.Execution
{
    public class SqlCommandPayload
    {
        public SqlCommandPayload(DbQuery dbQuery, bool isLazyLoading)
        {
            DbQuery = dbQuery;
            IsLazyLoading = isLazyLoading;
        }

        public bool IsLazyLoading { get; private set; }

        public DbQuery DbQuery { get; private set; }
    }
}
