using System;

namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public abstract class ResolutionContainerBase
    {
        public readonly Guid ExpressionQueryId;

        protected ResolutionContainerBase(Guid expressionQueryId)
        {
            ExpressionQueryId = expressionQueryId;
        }
    }
}
