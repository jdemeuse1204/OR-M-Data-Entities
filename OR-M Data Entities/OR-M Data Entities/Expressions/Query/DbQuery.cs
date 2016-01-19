/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.SubQuery;

namespace OR_M_Data_Entities.Expressions.Query
{
    public abstract class DbQuery<T> : DbSubQuery<T>
    {
        #region Fields
        protected readonly DatabaseReading Context;
        #endregion

        #region Properties
        public bool IsSubQuery { get; private set; }

        public string Sql { get; private set; }

        protected FunctionType Function { get; set; }

        // load either by ordinal or column name
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
                    // if we are counting just select the PK and count that column
                    var column = Columns.Infos.Where(w => w.IsSelected).OrderBy(w => w.Ordinal).First();

                    Columns.UnSelectAll();

                    column.IsSelected = true;

                    select = string.Format("COUNT({0})", Columns.Resolve());
                    break;
            }

            var join = JoinResolution.HasItems ? JoinResolution.Resolve() : string.Empty;
            var fromType = Tables.GetTableType(Type, this.Id);
            var tableInfo = new Table(fromType);
            var from = string.Format("{0} As [{1}]", tableInfo,
                Tables.FindAlias(Type, this.Id));

            var order = IsSubQuery || Function != FunctionType.None
                ? string.Empty
                : HasForeignKeys ? Columns.GetPrimaryKeyOrderStatement() : Columns.GetOrderByStatement();

            Sql = string.Format("SELECT {0}{1} {2} FROM {3} {4} {5} {6}",

                Columns.IsSelectDistinct ? " DISTINCT" : string.Empty,

                Columns.TakeRows > 0 ? string.Format("TOP {0}", Columns.TakeRows) : string.Empty, 
                
                select,

                from.Contains("[") ? from : string.Format("[{0}]", from),

                join,

                string.IsNullOrWhiteSpace(where) ? string.Empty : string.Format("WHERE {0}", where),

                order);
        }
    }
}
