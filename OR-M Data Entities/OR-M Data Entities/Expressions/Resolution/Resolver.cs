/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Linq;
using OR_M_Data_Entities.Expressions.Containers;
using OR_M_Data_Entities.Expressions.ObjectMapping;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public abstract class Resolver
    {
        protected readonly ObjectMap Map;

        protected Resolver(ObjectMap map)
        {
            Map = map;
        }

        protected string ResolveJoins(string viewId)
        {
            return string.IsNullOrWhiteSpace(viewId)
                ? Map.Tables.Aggregate(string.Empty, (current, table) => current + table.GetJoins())
                : Map.Tables.Where(w => w.ViewIds.Contains(viewId))
                    .Aggregate(string.Empty, (current, table) => current + table.GetJoins());
        }

        protected string ResolveSelect()
        {
            var select = string.Format("SELECT {0}", Map.IsDistinct ? "DISTINCT " : string.Empty);

            return Map.Rows.HasValue ? string.Format("{0} TOP {1} ", select, Map.Rows.Value) : select;
        }

        protected string ResolveFrom()
        {
            return Map.FromTableName();
        }

        protected string ResolveColumns(string viewId)
        {
            return string.IsNullOrWhiteSpace(viewId)
                ? Map.Tables.Aggregate(string.Empty, (current, table) => current + table.GetSelectColumns()).TrimEnd(',')
                : Map.Tables.Where(w => w.ViewIds.Contains(viewId))
                    .Aggregate(string.Empty, (current, table) => current + table.GetSelectColumns())
                    .TrimEnd(',');
        }

        public string ResolveOrderBy(string viewId)
        {
            return
                Map.OrderByColumns(viewId)
                    .Aggregate(" ORDER BY",
                        (current, column) =>
                            current + string.Format(" {0} {1},", column.GetText(), _getOrderByText(column.OrderType)))
                    .TrimEnd(',');
        }

        private string _getOrderByText(ObjectColumnOrderType order)
        {
            switch (order)
            {
                case ObjectColumnOrderType.Descending:
                    return " DESC";
                case ObjectColumnOrderType.Ascending:
                    return " ASC";
                default:
                    return string.Empty;
            }
        }

        protected WhereContainer ResolveWheres(string viewId)
        {
            var where = new WhereContainer();
            var list = string.IsNullOrWhiteSpace(viewId)
                ? Map.Tables.Where(w => w.HasValidation())
                : Map.Tables.Where(w => w.HasValidation() && w.ViewIds.Contains(viewId));

            foreach (var objectTable in list)
            {
                objectTable.GetWheres(where);
            }

            return where;
        }

        public abstract BuildContainer Resolve(string viewId);
    }

    #region helpers
    class SelectJoinResolver : Resolver
    {
        public SelectJoinResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);
            var joins = ResolveJoins(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}{3}",
                select,
                columns.TrimEnd(','),
                from,
                joins);

            return result;
        }
    }

    class SelectResolver : Resolver
    {
        public SelectResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}",
                select,
                columns.TrimEnd(','),
                from);

            return result;
        }
    }

    class SelectWhereJoinResolver : Resolver
    {
        public SelectWhereJoinResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var where = ResolveWheres(viewId);
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);
            var joins = ResolveJoins(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}{3}",
                select,
                columns.TrimEnd(','),
                from,
                joins);

            for (var i = 0; i < where.ValidationStatements.Count; i++)
            {
                result.Sql += string.Format(i == 0 ? " WHERE {0} " : "AND {0} ", where.ValidationStatements[i]);
            }

            result.Parameters = where.Parameters;

            return result;
        }
    }

    class SelectWhereResolver : Resolver
    {
        public SelectWhereResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var where = ResolveWheres(viewId);
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}",
                select,
                columns.TrimEnd(','),
                from);

            for (var i = 0; i < where.ValidationStatements.Count; i++)
            {
                result.Sql += string.Format(i == 0 ? " WHERE {0} " : "AND {0} ", where.ValidationStatements[i]);
            }

            result.Parameters = where.Parameters;

            return result;
        }
    }

    class OrderedSelectJoinResolver : Resolver
    {
        public OrderedSelectJoinResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);
            var joins = ResolveJoins(viewId);
            var ordering = ResolveOrderBy(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}{3}{4}",
                select,
                columns.TrimEnd(','),
                from,
                joins,
                ordering);

            return result;
        }
    }

    class OrderedSelectResolver : Resolver
    {
        public OrderedSelectResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);
            var ordering = ResolveOrderBy(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}{3}",
                select,
                columns.TrimEnd(','),
                from,
                ordering);

            return result;
        }
    }

    class OrderedSelectWhereJoinResolver : Resolver
    {
        public OrderedSelectWhereJoinResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var where = ResolveWheres(viewId);
            var whereText = string.Empty;

            for (var i = 0; i < where.ValidationStatements.Count; i++)
            {
                whereText += string.Format(i == 0 ? " WHERE {0} " : "AND {0} ", where.ValidationStatements[i]);
            }

            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);
            var joins = ResolveJoins(viewId);
            var ordering = ResolveOrderBy(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}{3}{4}{5}",
                select,
                columns.TrimEnd(','),
                from,
                joins, 
                whereText,
                ordering);

            result.Parameters = where.Parameters;

            return result;
        }
    }

    class OrderedSelectWhereResolver : Resolver
    {
        public OrderedSelectWhereResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve(string viewId)
        {
            var result = new BuildContainer();
            var where = ResolveWheres(viewId);
            var whereText = string.Empty;

            for (var i = 0; i < where.ValidationStatements.Count; i++)
            {
                whereText += string.Format(i == 0 ? " WHERE {0} " : "AND {0} ", where.ValidationStatements[i]);
            }

            var select = ResolveSelect();
            var from = ResolveFrom();
            var columns = ResolveColumns(viewId);
            var ordering = ResolveOrderBy(viewId);

            result.Sql = string.Format("{0}{1} FROM {2}{3}{4}",
                select,
                columns.TrimEnd(','),
                from,
                whereText,
                ordering);

            result.Parameters = where.Parameters;

            return result;
        }
    }
    #endregion
}
