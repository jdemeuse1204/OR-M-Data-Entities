using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;

namespace OR_M_Data_Entities.Diagnostics.HealthMonitoring
{
    public static class HealthMonitorExtensions
    {
        public static List<Health> GetAllHealth(this DbSqlContext context, DatabaseStoreType databaseStoreType, string nameSpace)
        {
            var tables = _getTypesInNamespace(nameSpace);

            return
                tables.Select(
                    table =>
                        typeof (HealthMonitorExtensions).GetMethod("GetHealth")
                            .MakeGenericMethod(table)
                            .Invoke(context, new[] {context, (object) databaseStoreType}) as Health).ToList();
        }

        public static Health GetHealth<T>(this DbSqlContext context, DatabaseStoreType databaseStoreType)
        {
            var result = new Health();

            // make sure object exists in the database
            if (!_doesTableExist(context, typeof(T)))
            {
                // if no object Id skip other steps because it means the parent table does not exist
                result.Add(new HealthError(Check.DoesTableExist,
                    string.Format(
                        "Table missing, remove the table from your database or delete your POCO.  Table Name: {0}",
                        typeof (T).GetTableName())));

                return result;
            }

            // make sure all columns are correct
            _checkColumns(context, result, typeof (T));

            // make sure object has primary key defined
            _checkPrimaryKeys(context, result, typeof (T));

            var foreignKeys = typeof(T).GetForeignKeys();

            foreach (var foreignKey in foreignKeys)
            {
                // declaring type is the parent
                // make sure all columns are correct
                _checkColumns(context, result, foreignKey.GetPropertyType());

                // make sure object has primary key defined
                _checkPrimaryKeys(context, result, foreignKey.GetPropertyType());

                // Test Joins (if any)
                _checkForeignKeyJoin(context, result, foreignKey.DeclaringType, foreignKey);
            }

            return result;
        }

        private static List<Type> _getTypesInNamespace(string nameSpace)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(t => t.IsClass && t.Namespace == nameSpace).ToList();

            //return assembly.GetTypes().Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToList();
        }

        private static void _checkColumns(DbSqlContext context, Health health, Type type)
        {
            var objectId = _getTableObjectId(context, type);

            // check missing columns
            var dbTableColumns = _getTableColumns(context, type, objectId);
            var pocoTableColumns = type.GetTableFields().Select(w => w.GetColumnName());

            var missingInPoco = dbTableColumns.Where(w => !pocoTableColumns.Contains(w)).ToList();
            var missingInTable = pocoTableColumns.Where(w => !dbTableColumns.Contains(w)).ToList();

            if (missingInPoco.Count == 0 && missingInTable.Count == 0)
            {
                health.Add(new HealthCheck(Check.DoColumnsFromPocoMatchDbTable));
                return;
            }

            foreach (var item in missingInPoco)
            {
                health.Add(new HealthError(Check.DoColumnsFromPocoMatchDbTable,
                    string.Format("Add the following column to your POCO or remove from the database: {0}", item)));
            }

            foreach (var item in missingInTable)
            {
                health.Add(new HealthError(Check.DoColumnsFromPocoMatchDbTable,
                    string.Format("Add to your database table or remove from your POCO: {0}", item)));
            }
        }

        private static void _checkForeignKeyJoin(DbSqlContext context, Health health, Type parent, PropertyInfo child)
        {
            var foreignKeyAttribute = parent.GetProperties().First(w => w.Name == child.Name).GetCustomAttribute<ForeignKeyAttribute>();

            var parentColumnName = child.PropertyType.IsList()
                ? parent.GetPrimaryKeys().First().GetColumnName() : foreignKeyAttribute.ForeignKeyColumnName;
            var childColumnName = child.PropertyType.IsList()
                ? foreignKeyAttribute.ForeignKeyColumnName : child.GetPropertyType().GetPrimaryKeys().First().GetColumnName();
            var parentTableName = _formatTableNameWithLinkedServer(parent.GetTableNameWithLinkedServer());
            var childTableName = _formatTableNameWithLinkedServer(child.GetPropertyType().GetTableNameWithLinkedServer());

            var sql = string.Format("Select Top 1 1 From {0},{1} Where {0}.[{2}] = {1}.[{3}]",
                parentTableName, childTableName, parentColumnName, childColumnName);

            try
            {
                context.ExecuteQuery(sql);

                health.Add(new HealthCheck(Check.ForeignKeyCheck));
            }
            catch (Exception)
            {
                health.Add(new HealthError(Check.ForeignKeyCheck,
                    string.Format("Foreign Key construction has an error - Parent: {0}, Foreign Key: {1}",
                        parent.GetTableName(), child.GetPropertyType().GetTableName())));
            }
        }

        private static string _formatTableNameWithLinkedServer(string tableName)
        {
            return string.Format("[{0}]", tableName.TrimStart('[').TrimEnd(']'));
        }

        private static void _checkPrimaryKeys(DbSqlContext context, Health health, Type type)
        {
            if (type.GetPrimaryKeys().Count == 0)
            {
                health.Add(new HealthError(Check.IsPrimaryKeyDefined,
                    string.Format("Please define your Primary Key in table: {0}", type.GetTableName())));
                return;
            }

            health.Add( new HealthCheck(Check.IsPrimaryKeyDefined));
        }

        #region Sql Server

        public static bool _doesTableExist(DbSqlContext context, Type type)
        {
            return context.ExecuteScript<bool>(new DoesTableExist
            {
                TableName = type.GetTableName(),
                SysTables = type.GetSystemTableSqlString()
            }).First();
        }

        public static int _getTableObjectId(DbSqlContext context, Type type)
        {
            return context.ExecuteScript<int>(new GetTableObjectId
            {
                TableName = type.GetTableName(),
                SysTables = type.GetSystemTableSqlString()
            }).First();
        }

        public static List<string> _getTableColumns(DbSqlContext context, Type type, int objectId)
        {
            return context.ExecuteScript<string>(new GetTableColumns
            {
                ObjectId = objectId,
                SysColumns = type.GetSystemColumnsSqlString()
            }).ToList();
        }

        #region Custom Scripts

        class DoesTableExist : CustomScript<bool>
        {
            public string TableName { get; set; }

            public string SysTables { get; set; }

            protected override string Sql
            {
                get { return string.Format("Select CASE WHEN EXISTS(Select Top 1 1 From {0} Where Name = @TableName) Then Cast(1 as bit) Else Cast(0 as bit) End", SysTables); }
            }
        }

        class GetTableObjectId : CustomScript<int>
        {
            public string TableName { get; set; }

            public string SysTables { get; set; }

            protected override string Sql
            {
                get { return string.Format("Select Top 1 object_id From {0} Where Name = @TableName", SysTables); }
            }
        }

        class GetTableColumns : CustomScript<string>
        {
            public int ObjectId { get; set; }

            public string SysColumns { get; set; }

            protected override string Sql
            {
                get { return string.Format("Select Name From {0} Where object_id = @ObjectId", SysColumns); }
            }
        }
        #endregion
        #endregion
    }


    #region helpers
    public enum Check
    {
        DoesTableExist = 1,
        IsPrimaryKeyDefined,
        DoColumnsFromPocoMatchDbTable,
        ForeignKeyCheck
    }

    public enum DatabaseStoreType
    {
        SqlServer
    }
    #endregion
}
