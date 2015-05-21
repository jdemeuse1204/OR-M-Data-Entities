using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Data.Execution
{
    public class SqlCommandPayload
    {
        public SqlCommandPayload(DbQueryBase dbQuery, bool isLazyLoading)
        {
            DbQuery = dbQuery;
            IsLazyLoading = isLazyLoading;
        }

        public bool IsLazyLoading { get; private set; }

        public DbQueryBase DbQuery { get; private set; }
    }
}
