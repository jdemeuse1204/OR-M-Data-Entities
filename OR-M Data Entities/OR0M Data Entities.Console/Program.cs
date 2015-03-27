using System;
using System.Data;
using OR_M_Data_Entities.Commands.Transform;
using OR_M_Data_Entities.Tests.Context;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new SqlContext();

            var testItem = context.From<Contact>().Select<Contact>().Join<Contact,Appointment>((p,c) => p.ID == c.ContactID).First<Contact>();

            if (testItem != null)
            {
                
            }

            var currentDateTime = DateTime.Now;
            
            var totalMilliseconds = 0d;
            var max = 1000;
            var ct = 0;

            for (var i = 0; i < max; i++)
            {
                var start = DateTime.Now;
                var item = context.From<Policy>()
                .Where<Policy>(w => DbFunctions.Cast(w.CreatedDate, SqlDbType.Date) == DbFunctions.Cast(currentDateTime, SqlDbType.Date))
                .Select<Policy>(w => DbFunctions.Convert(SqlDbType.VarChar, w.CreatedDate, 101))
                .First<string>();

                if (item != null)
                {
                    
                }

                var end = DateTime.Now;

                totalMilliseconds += (end - start).TotalMilliseconds;
                ct++;
            }

            var final = totalMilliseconds/ct;

            if (final != 0)
            {
                
            }
        }

        //static void RecursiveActivator(object parent, DbSqlContext context)
        //{
        //    var autoLoads = parent.GetType().GetProperties().Where(w => w.GetCustomAttribute<AutoLoadAttribute>() != null).ToList();

        //    foreach (var autoLoad in autoLoads)
        //    {
        //        var childInstance = Activator.CreateInstance(autoLoad.PropertyType);

        //        if (ExpressionTypeTransform.IsList(childInstance))
        //        {
        //            var listItemType = childInstance.GetType().GetGenericArguments()[0];
        //            var listItemTable = DatabaseSchemata.GetTableName(listItemType);
        //            var listItemProperties = listItemType.GetProperties();
        //            var listForeignKeys = listItemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
        //                && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

        //            var listSelectBuilder = new SqlQueryBuilder();
        //            listSelectBuilder.Table(listItemTable);
        //            listSelectBuilder.SelectAll();

        //            foreach (var item in listForeignKeys)
        //            {
        //                var columnName = DatabaseSchemata.GetColumnName(item);
        //                var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
        //                var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
        //                listSelectBuilder.AddWhere(listItemTable, columnName, ComparisonType.Equals, compareValue);
        //            }

        //            var listMethod = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
        //                && w.GetParameters().Any() 
        //                && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

        //            var genericListMethod = listMethod.MakeGenericMethod(new[] {listItemType});
        //            var listResult = genericListMethod.Invoke(context, new object[] {listSelectBuilder});
        //            var allResults = (listResult as dynamic).All();

        //            foreach (var item in allResults)
        //            {
        //                RecursiveActivator(item, context);
        //                (childInstance as dynamic).Add(item);
        //            }

        //            autoLoad.SetValue(parent, childInstance, null);
        //            continue;
        //        }

        //        var itemType = childInstance.GetType();
        //        var itemTable = DatabaseSchemata.GetTableName(itemType);
        //        var itemProperties = itemType.GetProperties();
        //        var foreignKeys = itemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
        //            && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

        //        var builder = new SqlQueryBuilder();
        //        builder.Table(itemTable);
        //        builder.SelectAll();

        //        foreach (var item in foreignKeys)
        //        {
        //            var columnName = DatabaseSchemata.GetColumnName(item);
        //            var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
        //            var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
        //            builder.AddWhere(itemTable, columnName, ComparisonType.Equals, compareValue);
        //        }

        //        var method = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
        //            && w.GetParameters().Any()
        //            && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

        //        var genericMethod = method.MakeGenericMethod(new[] { itemType });
        //        var query = genericMethod.Invoke(context, new object[] { builder });
        //        var result = (query as dynamic).Select();

        //        autoLoad.SetValue(parent, result, null);

        //        if (result != null)
        //        {
        //            RecursiveActivator(result, context);
        //        }                
        //    }
        //}
    }
}
