using System.Collections;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionQuery<T> : IEnumerable
    {
        public readonly SqlQuery Query;
        public readonly DatabaseReading Context;

        public ExpressionQuery(DatabaseReading context)
        {
            var tableName = DatabaseSchemata.GetTableName<T>();

            Query = new SqlQuery
            {
                ReturnType = typeof(T),
                IsLazyLoading = context.IsLazyLoadEnabled,
                From = string.Format("[{0}]", tableName)
            };

            Context = context;

            //if (Context.)
        }

        public ExpressionQuery(SqlQuery query, DatabaseReading context)
        {
            Query = query;
            Context = context;
        }

        public IEnumerator GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}
