/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Expressions.Support;

namespace OR_M_Data_Entities.Expressions.Types.Base
{
    public abstract class SqlExpressionResolvableBase : ExpressionResolver
    {
        #region Abstract Methods
        public abstract SqlExpressionType Resolve();
        #endregion

        #region Fields
        protected readonly ExpressionQuery Query;
        #endregion

        #region Constructor
        protected SqlExpressionResolvableBase(ExpressionQuery query)
        {
            Query = query;
        }
        #endregion

        #region Methods
        protected List<SqlWhere> ResolveWheresList()
        {
            var where = new List<SqlWhere>();

            foreach (var resolution in Query.WheresList.Select(item => GetWheres(item as dynamic)))
            {
                @where.AddRange(resolution);
            }

            return where;
        }

        protected List<SqlTableColumnPair> ResolveSelectsList()
        {
            var select = new List<SqlTableColumnPair>();

            foreach (var resolution in Query.SelectsList)
            {
                GetSelects(resolution as dynamic, select);
            }

            return select;
        }

        protected SqlJoinCollection ResolveJoinsList()
        {
            var joinCollection = new SqlJoinCollection();

            foreach (var leftJoin in Query.LeftJoinsList)
            {
                var join = leftJoin as SqlJoin;

                if (join != null)
                {
                    joinCollection.Add(join);
                    continue;
                }

                GetJoins(leftJoin as dynamic, JoinType.Left, joinCollection);
            }

            foreach (var innerJoin in Query.InnerJoinsList)
            {
                var join = innerJoin as SqlJoin;

                if (join != null)
                {
                    joinCollection.Add(join);
                    continue;
                }

                GetJoins(innerJoin as dynamic, JoinType.Inner, joinCollection);
            }


            return joinCollection;
        }
        #endregion
    }
}
