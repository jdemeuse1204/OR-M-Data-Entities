/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class MaxLengthViolationRule : IRule
    {
        public readonly Type _type;

        public readonly object _entity;

        public MaxLengthViolationRule(object entity, Type type)
        {
            _entity = entity;
            _type = type;
        }

        public void Process()
        {
            var properties =
                _type.GetProperties()
                    .Where(w => w.GetCustomAttribute<MaxLengthAttribute>() != null && w.PropertyType == typeof (string))
                    .ToList();

            foreach (var property in properties)
            {
                var value = (string)property.GetValue(_entity);
                var maxLengthAttribute = property.GetCustomAttribute<MaxLengthAttribute>();

                if (value == null || value.Length <= maxLengthAttribute.Length) continue;

                if (maxLengthAttribute.ViolationType == MaxLengthViolationType.Truncate)
                {
                    _entity.SetPropertyInfoValue(property, value.Substring(0, maxLengthAttribute.Length));
                    continue;
                }

                throw new MaxLengthException(string.Format("Max Length violated on column: {0}", property.Name));
            }
        }
    }
}
