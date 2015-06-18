/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;

namespace OR_M_Data_Entities.Expressions.Query
{
    public abstract class DbQuery<T> : DbSubQuery<T>
    {
        #region Fields
        protected readonly DatabaseReading Context;
        #endregion

        #region Properties
        public bool IsLazyLoadEnabled
        {
            get { return Context == null || Context.IsLazyLoadEnabled; }
        }

        public bool IsSubQuery { get; private set; }

        public string Sql { get; private set; }

        protected FunctionType Function { get; set; }
        #endregion

        #region Constructor
        protected DbQuery(DatabaseReading context, string viewId = null)
            : base(viewId)
        {
            Context = context;
            Function = FunctionType.None;
            IsSubQuery = context == null && ConstructionType == ExpressionQueryConstructionType.SubQuery;
        }

        protected DbQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            Context =
                query.GetType().GetField("Context", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(query) as
                    DatabaseReading;
            Function =
                (FunctionType)
                    query.GetType()
                        .GetProperty("Function", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(query);
            IsSubQuery = (this.ConstructionType !=
                          (ExpressionQueryConstructionType.Main | ExpressionQueryConstructionType.Order)) &&
                         this.Id != query.Id;
        }
        #endregion

        // if tables are coming into the function then we need to change all table aliases (AkA) in this query before we resolve it
        protected void ResolveQuery()
        {
            // views
            var where = WhereResolution.HasItems ? WhereResolution.Resolve() : string.Empty;

            var select = Columns.HasItems ? Columns.Resolve() : string.Empty;

            switch (Function)
            {
                case FunctionType.Max:
                    select = string.Format("MAX({0})", select);
                    break;
                case FunctionType.Min:
                    select = string.Format("MIN({0})", select);
                    break;
                case FunctionType.Count:
                    select = string.Format("COUNT({0})", select);
                    break;
            }

            var join = JoinResolution.HasItems ? JoinResolution.Resolve() : string.Empty;
            var fromType = Tables.GetTableType(Type, this.Id);
            var tableInfo = new TableInfo(fromType);
            var from = string.Format("{0} As [{1}]", tableInfo,
                Tables.FindAlias(Type, this.Id, null));

            var order = IsSubQuery || Function != FunctionType.None
                ? string.Empty
                : HasForeignKeys ? Columns.GetPrimaryKeyOrderStatement() : Columns.GetOrderByStatement();

            Sql = string.Format("SELECT {0}{1} {2} FROM {3} {4} {5} {6}",
                Columns.IsSelectDistinct ? " DISTINCT" : string.Empty,
                Columns.TakeRows > 0 ? string.Format("TOP {0}", Columns.TakeRows) : string.Empty, select,
                from.Contains("[") ? from : string.Format("[{0}]", from),
                join,
                string.IsNullOrWhiteSpace(where) ? string.Empty : string.Format("WHERE {0}", where),
                order);
        }
    }
}
