/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public static class DatabaseEntity
    {
        public static ModificationState GetState(object entity, List<PropertyInfo> primaryKeys)
        {
            for (var i = 0; i < primaryKeys.Count; i++)
            {
                var key = primaryKeys[i];
                var pkValue = key.GetValue(entity);
                var generationOption = DatabaseSchemata.GetGenerationOption(key);
                var isUpdating = false;

                if (generationOption != DbGenerationOption.None)
                {
                    // If Db generation option is set to none, we always do an insert

                    switch (pkValue.GetType().Name.ToUpper())
                    {
                        case "INT16":
                            isUpdating = Convert.ToInt16(pkValue) != 0;
                            break;
                        case "INT32":
                            isUpdating = Convert.ToInt32(pkValue) != 0;
                            break;
                        case "INT64":
                            isUpdating = Convert.ToInt64(pkValue) != 0;
                            break;
                        case "GUID":
                            isUpdating = (Guid) pkValue != Guid.Empty;
                            break;
                    }
                }

                // break because we are already updating, do not want to set to false
                if (!isUpdating)
                {
                    continue;
                }

                return ModificationState.Update;
            }

            return ModificationState.Insert;
        }

        public static void SetPropertyValue(object entity, string propertyName, object value)
        {
            var found = entity.GetType().GetProperty(propertyName);

            if (found == null)
            {
                return;
            }

            var propertyType = found.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (value == null)
                {
                    found.SetValue(entity, null, null);
                    return;
                }

                //Get the underlying type property instead of the nullable generic
                propertyType = new NullableConverter(found.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            found.SetValue(
                entity,
                propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType),
                null);
        }
    }
}
