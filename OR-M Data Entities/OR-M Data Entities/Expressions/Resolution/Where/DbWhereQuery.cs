/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Join;

namespace OR_M_Data_Entities.Expressions.Resolution.Where
{
    public abstract class DbWhereQuery<T> : DbJoinQuery<T>
    {
        #region Fields
        protected readonly WhereResolutionContainer WhereResolution;
        #endregion

        #region Constructor
        protected DbWhereQuery(string viewId = null)
            : base(viewId)
        {
            WhereResolution = new WhereResolutionContainer(this.Id);
        }

        protected DbWhereQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            switch (ConstructionType)
            {
                case ExpressionQueryConstructionType.Order:
                case ExpressionQueryConstructionType.Select:
                case ExpressionQueryConstructionType.Join:
                    WhereResolution = query.GetType()
                            .GetField("WhereResolution", BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(query) as WhereResolutionContainer;
                    break;
                case ExpressionQueryConstructionType.Main:
                    WhereResolution = new WhereResolutionContainer(this.Id);
                    break;
                case ExpressionQueryConstructionType.SubQuery:
                    {
                        WhereResolution = new WhereResolutionContainer(this.Id);

                        var existingContainer = query.GetType()
                            .GetField("WhereResolution", BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(query) as WhereResolutionContainer;

                        WhereResolution.Combine(existingContainer);
                    }
                    break;
            }
        }
        #endregion

        #region Methods

        protected void ClearWhereQuery()
        {
            WhereResolution.ClearResolutions();
        }
        #endregion
    }
}
