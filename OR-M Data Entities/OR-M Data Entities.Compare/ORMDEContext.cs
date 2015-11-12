namespace OR_M_Data_Entities.Compare
{
    public class ORMDEContext : DbSqlContext
    {
        public ORMDEContext()
            : base("sqlExpress")
        {
        }
    }
}
