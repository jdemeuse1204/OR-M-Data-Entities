namespace OR_M_Data_Entities.Mock
{
    public abstract class MockDbSqlContext : DbSqlContext
    {
        private readonly Database _database;

        protected MockDbSqlContext()
            : base("MOCK")
        {
            _database = new Database();

            OnDatabaseCreating();
        }

        protected abstract void OnDatabaseCreating();

        protected void Add(object entity)
        {
            _database.Add(DbTableFactory, Configuration, entity);
        }
    }
}
