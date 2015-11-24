using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Data.Query.StatementParts;

namespace OR_M_Data_Entities.Data.Query
{
    public abstract class SqlInsertBuilder : EntityInfo
    {
        public virtual List<InsertItem> GetInsertItems()
        {
            return GetAllColumns().Select(property => new InsertItem(property, Entity)).ToList();
        }

        protected SqlInsertBuilder(object entity) 
            : base(entity)
        {
        }

        public abstract ISqlPackage Build();

        public SqlCommand BuildSqlCommand(SqlConnection connection)
        {
            // build the sql package
            var package = Build();

            // generate the sql command
            var command = new SqlCommand(package.GetSql(), connection);

            // insert the parameters
            package.InsertParameters(command);

            return command;
        }
    }
}
