/*
 * OR-M Data Entities v3.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Loading
{

    public static class ObjectLoader
    {
        public static void SetPropertyInfoValue(object entity, string propertyName, object value)
        {
            var property = entity.GetType().GetProperty(propertyName) ??
                           entity.GetType()
                               .GetProperties()
                               .First(
                                   w =>
                                       w.GetCustomAttribute<ColumnAttribute>() != null &&
                                       w.GetCustomAttribute<ColumnAttribute>().Name == propertyName);

            SetPropertyInfoValue(entity, property, value);
        }

        public static void SetPropertyInfoValue(object entity, PropertyInfo property, object value)
        {
            // cleanse the value from the database, DBNull is not valid
            var cleansedValue = value is DBNull ? null : value;

            var propertyType = property.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (cleansedValue == null)
                {
                    property.SetValue(entity, null, null);
                    return;
                }

                //Get the underlying type property instead of the nullable generic
                propertyType = new NullableConverter(property.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            property.SetValue(
                entity,
                propertyType.IsEnum // enum cannot be null here, it will be caught above
                    ? Enum.ToObject(propertyType, Convert.ToInt32(cleansedValue))
                    : Convert.ChangeType(cleansedValue, propertyType),
                null);
        }
    }
}
