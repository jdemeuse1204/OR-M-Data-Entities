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
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
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
        public static Type GetUnderlyingType(this object o)
        {
            return GetUnderlyingType(o.GetType());
        }

        public static Type GetUnderlyingType(this Type type)
        {
            return type.IsList() || type.IsNullable()
                ? type.GetGenericArguments()[0]
                : type;
        }
    }
}
