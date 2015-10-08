/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Select;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public abstract class DbJoinQuery<T> : DbSelectQuery<T>
    {
        #region Fields
        protected readonly JoinResolutionContainer JoinResolution;
        #endregion

        #region Constructor
        protected DbJoinQuery(string viewId = null)
            : base(viewId)
        {
            JoinResolution = new JoinResolutionContainer(ForeignKeyJoinPairs, this.Id);
        }

        protected DbJoinQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            JoinResolution = new JoinResolutionContainer(ForeignKeyJoinPairs, this.Id);

            if (ConstructionType == ExpressionQueryConstructionType.Join || ConstructionType == ExpressionQueryConstructionType.Order)
            {
                JoinResolution =
                    query.GetType()
                        .GetField("JoinResolution", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(query) as JoinResolutionContainer;
            }
        }
        #endregion

        #region Methods
        protected void ClearJoinQuery()
        {
            JoinResolution.ClearJoins();
        }
        #endregion
    }
}
