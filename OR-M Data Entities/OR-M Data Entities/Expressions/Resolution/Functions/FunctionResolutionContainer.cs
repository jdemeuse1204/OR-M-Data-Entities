/*
 * OR-M Data Entities v2.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

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

        public string Resolve(string viewId = null)
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
