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
        protected DbWhereQuery()
            : base()
        {
            WhereResolution = new WhereResolutionContainer(this.Id);
        }

        protected DbWhereQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
            switch (ConstructionType)
            {
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
