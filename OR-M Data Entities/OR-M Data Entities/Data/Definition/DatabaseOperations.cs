﻿/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands.Transform;

namespace OR_M_Data_Entities.Data.Definition
{
    public static class DatabaseOperations
    {
        #region Casting and Converting

        public static bool IsCasting(object expression)
        {
            return _isTransform(expression, "Cast");
        }

        public static bool IsConverting(object expression)
        {
            return _isTransform(expression, "Convert");
        }

        private static bool _isTransform(object expression, string methodName)
        {
            if (!(expression is MethodCallExpression)) return false;

            var methodCallExpression = (MethodCallExpression)expression;

            return methodCallExpression.Method.DeclaringType == typeof(DbFunctions) &&
                   String.Equals(methodCallExpression.Method.Name, methodName, StringComparison.CurrentCultureIgnoreCase);
        }
        #endregion
    }
}