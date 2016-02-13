/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OR_M_Data_Entities.Mapping;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class PropertyInfoExtensions
    {
        public static bool IsPropertyTypeList(this PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsList();
        }

        public static Type GetPropertyType(this PropertyInfo propertyInfo)
        {
            return propertyInfo.IsPropertyTypeList()
                ? propertyInfo.PropertyType.GetGenericArguments()[0]
                : propertyInfo.PropertyType;
        }

        public static Type GetUnderlyingType(this Type type)
        {
            return type.IsList()
                ? type.GetGenericArguments()[0]
                : type;
        }
    }

    public static class TypeExtensions
    {
        public static bool IsDynamic(this Type type)
        {
            return type == typeof(IDynamicMetaObjectProvider);
        }

        public static bool IsAnonymousType(this Type type)
        {

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;

        }

        public static Type GetTypeWithListCheck(this Type type)
        {
            return type.IsList()
                ? type.GetGenericArguments()[0]
                : type;
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }
    }

    public static class ListExtensions
    {
        public static bool IsList(this object o)
        {
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsList(this PropertyInfo property)
        {
            return property.PropertyType.IsGenericType &&
                   property.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsList(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
    }

    public static class ObjectExtensions
    {
        public static void SetPropertyInfoValue(this object entity, string propertyName, object value)
        {
            var property = entity.GetType().GetProperty(propertyName) ??
                           entity.GetType()
                               .GetProperties()
                               .First(
                                   w =>
                                       w.GetCustomAttribute<ColumnAttribute>() != null &&
                                       w.GetCustomAttribute<ColumnAttribute>().Name == propertyName);

            entity.SetPropertyInfoValue(property, value);
        }

        public static void SetPropertyInfoValue(this object entity, PropertyInfo property, object value)
        {
            var propertyType = property.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (value == null)
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
                propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType),
                null);
        }

        public static Type GetUnderlyingType(this object o)
        {
            return o.IsList()
                ? o.GetType().GetGenericArguments()[0]
                : o.GetType();
        }
    }

    public static class StringExtension
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);

            if (pos < 0) return text;

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
