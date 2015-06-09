using System;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Functions
{
    public class FunctionResolutionContainer : ResolutionContainerBase, IResolutionContainer
    {
        public FunctionResolutionContainer(Guid expressionQueryId, FunctionType function)
            : base(expressionQueryId)
        {
        }

        public string Resolve()
        {
            throw new NotImplementedException();
        }

        public bool HasItems { get; private set; }
        public void Combine(IResolutionContainer container)
        {
            throw new NotImplementedException();
        }
    }
}
