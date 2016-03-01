/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Reflection;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IEntity : ITable
    {
        object GetPropertyValue(PropertyInfo property);

        object GetPropertyValue(string propertyName);

        bool IsPristineEntityNull();

        object GetPristineEntityPropertyValue(string propertyName);

        object TryGetPristineEntityPropertyValue(string propertyName);
    }
}
