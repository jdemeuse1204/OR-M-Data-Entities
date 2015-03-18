using System;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Resolver;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tests.Context;
using OR_M_Data_Entities.Tests.Tables;

namespace OR0M_Data_Entities.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new SqlContext();
            var c = new Contact
            {
                FirstName = "James",
                LastName = "Demeuse"
            };

            context.SaveChanges(c);

            var a1 = new Appointment
            {
                ContactID = c.ID,
                Description = "Test"
            };

            var a2 = new Appointment
            {
                ContactID = c.ID,
                Description = "Test"
            };

            context.SaveChanges(a1);
            context.SaveChanges(a2);

            var addy = new Address
            {
                Addy = "TEST ADDY",
                AppointmentID = a1.ID
            };

            context.SaveChanges(addy);

            var zip = new Zip
            {
                AddressID = addy.ID,
                Zip4 = "TEST"
            };

            context.SaveChanges(zip);

            RecursiveActivator(c, context);

            context.Delete(c);
            context.Delete(a1);
            context.Delete(a2);
            context.Delete(addy);
            context.Delete(zip);
        }

        static void RecursiveActivator(object parent, DbSqlContext context)
        {
            var autoLoads = parent.GetType().GetProperties().Where(w => w.GetCustomAttribute<AutoLoadAttribute>() != null).ToList();

            foreach (var autoLoad in autoLoads)
            {
                var childInstance = Activator.CreateInstance(autoLoad.PropertyType);

                if (ExpressionTypeTransform.IsList(childInstance))
                {
                    var listItemType = childInstance.GetType().GetGenericArguments()[0];
                    var listItemTable = DatabaseSchemata.GetTableName(listItemType);
                    var listItemProperties = listItemType.GetProperties();
                    var listForeignKeys = listItemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
                        && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

                    var listSelectBuilder = new SqlQueryBuilder();
                    listSelectBuilder.Table(listItemTable);
                    listSelectBuilder.SelectAll();

                    foreach (var item in listForeignKeys)
                    {
                        var columnName = DatabaseSchemata.GetColumnName(item);
                        var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
                        var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
                        listSelectBuilder.AddWhere(listItemTable, columnName, ComparisonType.Equals, compareValue);
                    }

                    var listMethod = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
                        && w.GetParameters().Any() 
                        && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

                    var genericListMethod = listMethod.MakeGenericMethod(new[] {listItemType});
                    var listResult = genericListMethod.Invoke(context, new object[] {listSelectBuilder});
                    var allResults = (listResult as dynamic).All();

                    foreach (var item in allResults)
                    {
                        RecursiveActivator(item, context);
                        (childInstance as dynamic).Add(item);
                    }

                    autoLoad.SetValue(parent, childInstance, null);
                    continue;
                }

                var itemType = childInstance.GetType();
                var itemTable = DatabaseSchemata.GetTableName(itemType);
                var itemProperties = itemType.GetProperties();
                var foreignKeys = itemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
                    && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

                var builder = new SqlQueryBuilder();
                builder.Table(itemTable);
                builder.SelectAll();

                foreach (var item in foreignKeys)
                {
                    var columnName = DatabaseSchemata.GetColumnName(item);
                    var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
                    var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
                    builder.AddWhere(itemTable, columnName, ComparisonType.Equals, compareValue);
                }

                var method = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
                    && w.GetParameters().Any()
                    && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

                var genericMethod = method.MakeGenericMethod(new[] { itemType });
                var query = genericMethod.Invoke(context, new object[] { builder });
                var result = (query as dynamic).Select();

                autoLoad.SetValue(parent, result, null);

                if (result != null)
                {
                    RecursiveActivator(result, context);
                }                
            }
        }
    }
}
