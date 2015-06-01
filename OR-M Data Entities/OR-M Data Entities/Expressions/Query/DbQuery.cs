using System;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;

namespace OR_M_Data_Entities.Expressions.Query
{
    public abstract class DbQuery<T> : DbSubQuery<T>
    {
        #region Properties
        public string Sql { get; private set; }

        protected readonly Type Type;
        #endregion

        #region Constructor
        protected DbQuery(QueryInitializerType queryInitializerType)
            : base(queryInitializerType)
        {
            Type = typeof (T);
        }

        protected DbQuery(IExpressionQueryResolvable query)
            : base(query)
        {
            Type =
                query.GetType()
                    .GetField("Type", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(query) as Type;
        }
        #endregion

        protected void ResolveQuery()
        {
            var where = WhereResolution.HasItems ? WhereResolution.Resolve() : string.Empty;

            var select = SelectList.HasItems ? SelectList.Resolve() : string.Empty;

            var join = JoinResolution.HasItems ? JoinResolution.Resolve() : string.Empty;

            var from = string.Format("[{0}] As [{1}]", DatabaseSchemata.GetTableName(Type),
                Tables.FindAlias(Type));

            var order = SelectList.GetOrderStatement();

            Sql = string.Format("SELECT {0}{1} {2} FROM {3} {4} {5} {6}",
                SelectList.IsSelectDistinct ? " DISTINCT" : string.Empty,
                SelectList.TakeRows > 0 ? string.Format("TOP {0}", SelectList.TakeRows) : string.Empty, select,
                from.Contains("[") ? from : string.Format("[{0}]", from),
                join,
                string.IsNullOrWhiteSpace(where) ? string.Empty : string.Format("WHERE {0}", where),
                order);
        }        
    }
}
