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

            if (ConstructionType == ExpressionQueryConstructionType.Join)
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
