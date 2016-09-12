/*
 * OR-M Data Entities v3.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Scripts;

namespace OR_M_Data_Entities.Expressions
{
    public interface IConditionalExpressionQuery<out T> : IExpressionQuery<T>
    {
        IConditionalExpressionQuery<T> True(string sqlToExecute);

        IConditionalExpressionQuery<T> True(OnConditionHandler handler);

        IConditionalExpressionQuery<T> True(CustomScript script);


        IConditionalExpressionQuery<T> False(string sqlToExecute);

        IConditionalExpressionQuery<T> False(OnConditionHandler handler);

        IConditionalExpressionQuery<T> False(CustomScript script);
    }
}
