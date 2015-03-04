using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Connection;

namespace OR_M_Data_Entities.Entity
{
    public abstract class DbEntityContext : IDisposable
    {
        // do not want to expose sql context's methods
        private readonly DbSqlContext _context;
        private List<PropertyInfo> _tables { get; set; }

        protected DbEntityContext(string connectionString)
        {
            _context = new DbSqlContext(connectionString);
            OnModelCreating();
        }

        protected DbEntityContext(IConnectionBuilder connection)
            : this(connection.BuildConnectionString())
        {
        }

        public void SaveChanges()
        {
            foreach (var propertyInfo in _tables.Where(w => ((dynamic)w.GetValue(this)).HasChanges))
            {
                var table = propertyInfo.GetValue(this) as dynamic;

                foreach (var entity in table.Local)
                {
                    switch ((SaveAction)entity.Value)
                    {
                        case SaveAction.Remove:
                            _context.Delete(entity.Key);
                            break;
                        case SaveAction.Save:
                            _context.SaveChanges(entity.Key);
                            break;
                    }
                }
            }
        }

        protected void OnModelCreating()
        {
            // Get all IDbTables
            _tables = _getDbTables();

            foreach (var property in _tables)
            {
                // Get the generic type 
                var generic = property.PropertyType.GetGenericArguments()[0];

                // get the typeof DbTable<>
                var dbTable = typeof(DbTable<>);

                // Get the type of DbTable with its corresponding generic
                var creationType = dbTable.MakeGenericType(generic);

                // Create the Db Table
                var instance = Activator.CreateInstance(creationType, _context);

                // Set the property
                property.SetValue(this, instance, null);
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            _tables = null;
        }

        private List<PropertyInfo> _getDbTables()
        {
            return GetType().GetProperties()
                .Where(w => w.PropertyType.IsInterface
                    && w.PropertyType.GetGenericTypeDefinition() == typeof(IDbTable<>)).ToList();
        }
    }
}
