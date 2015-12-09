using System.Collections.Generic;
using System.Data.SqlClient;

namespace OR_M_Data_Entities.Data.Query.StatementParts
{
    public sealed class SqlTransaction
    {
        #region Constructor
        public SqlTransaction() 
        {
            _statements = new List<SqlStatement>();
        }
        #endregion

        #region Properties and Fields
        private readonly List<SqlStatement> _statements;

        public IEnumerable<SqlStatement> Statements
        {
            get { return _statements; }
        }

        #endregion

        #region Methods
        public void Add(SqlStatement statement)
        {
            _statements.Add(statement);
        }

        public SqlCommand BuildCommand(SqlConnection connection)
        {
            // reorder the parameters

            return new SqlCommand();
        }
        #endregion
    }
}
