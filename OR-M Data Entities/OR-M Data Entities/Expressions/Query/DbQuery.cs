using System;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Expressions.Query
{
    public abstract class DbQuery<T> : DbSubQuery<T>
    {
        #region Properties
        public string Sql { get; private set; }
        #endregion

        #region Constructor
        protected DbQuery(QueryInitializerType queryInitializerType)
            : base(queryInitializerType)
        {
        }
        #endregion

        public void Resolve()
        {
            // joins can change table names because of Foreign Keys, get name changes
            var changeTables = JoinResolution.GetChangeTableContainers();

            foreach (var changeTable in changeTables)
            {
                SelectList.ChangeTable(changeTable);
            }

            var where = WhereResolution.HasItems ? WhereResolution.Resolve() : string.Empty;

            var select = SelectList.HasItems ? SelectList.Resolve() : string.Empty;

            var join = JoinResolution.HasItems ? JoinResolution.Resolve() : string.Empty;

            var from = DatabaseSchemata.GetTableName(typeof(T));

            Sql = string.Format("SELECT {0}{1} {2} FROM {3} {4} {5} {6}",
                SelectList.IsSelectDistinct ? " DISTINCT" : string.Empty,
                SelectList.TakeRows > 0 ? string.Format("TOP {0}", SelectList.TakeRows) : string.Empty, select,
                from.Contains("[") ? from : string.Format("[{0}]", from),
                join,
                string.Format("WHERE {0}", where),
                "");
        }        
    }
}
