/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Expressions.Support;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses all data fetching functions to create their own functions
    /// </summary>
    public abstract class DataModifiable : DataFetching
    {
        #region Constructor
        protected DataModifiable(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Methods
        public virtual bool Delete<T>(T entity)
            where T : class
        {
            // Check to see if the PK is defined
            var tableName = DatabaseSchemata.GetTableName(entity);

            // ID is the default primary key name
            var primaryKeys = DatabaseSchemata.GetPrimaryKeys(entity);

            // delete Data
            var builder = new SqlDeleteBuilder();
            builder.Table(tableName);

            // Loop through all mapped properties
            foreach (var property in primaryKeys)
            {
                var value = property.GetValue(entity);
                var columnName = DatabaseSchemata.GetColumnName(property);
                builder.AddWhere(tableName, columnName, ComparisonType.Equals, value);
            }

            // Execute the insert statement
            Execute(builder);

            if (!Reader.HasRows) return false;

            Read();

            var rowsAffected = Reader.GetInt32(0);

            // close our readers
            Reader.Close();
            Reader.Dispose();

            // return if anything was deleted
            return rowsAffected > 0;
        }

        public virtual void SaveChanges<T>(T entity)
            where T : class
        {
            if (DatabaseSchemata.HasForeignKeys<T>())
            {
                var savableObjects = new List<ForeignKeySaveNode> {new ForeignKeySaveNode(null, entity, null)};
                _analyzeChangesWithForeignKeys(entity, savableObjects);

                foreach (var savableObject in savableObjects)
                {
                    _saveChangesWithNoForeignKeys(savableObject.Value);

                    // insert keys into parent
                    _setValue(savableObject.Parent, savableObject.Value);
                }

                // loop through the saved list in reverse and set all ids that came in from saving
                _setKeysWithForeignKeysAfterSave(entity);
                return;
            }

            _saveChangesWithNoForeignKeys(entity);
        }

        private void _setValue(object parent, object child)
        {
            var foreignKeyProperty =
                DatabaseSchemata.GetForeignKeys(parent)
                    .First(
                        w =>
                            (w.PropertyType.IsList()
                                ? w.PropertyType.GetGenericArguments()[0]
                                : w.PropertyType) == child.GetType());

            var childProperties = child.GetType().GetProperties();
            var foreignKeyAttribute = foreignKeyProperty.GetCustomAttribute<ForeignKeyAttribute>();
            var childProperty = childProperties.FirstOrDefault(w => w.Name == foreignKeyAttribute.ForeignKeyColumnName);

            if (childProperty != null)
            {
                //// correct FK
                //var pkValue = parent.GetType().GetProperty(foreignKeyAttribute.PrimaryKeyColumnName).GetValue(parent);

                //DatabaseEntity.SetPropertyValue(child, childProperty, pkValue);
            }
            else
            {
                // check for backwards FK
            }
        }

        private void _setKeysWithForeignKeysAfterSave<T>(T entity)
            where T : class
        {
            // make sure FKs have values before saving, if they dont you need to throw an error
            foreach (var foreignKey in DatabaseSchemata.GetForeignKeys(entity))
            {
                var foreignKeyValue = foreignKey.GetValue(entity);

                if (foreignKeyValue == null)
                {
                    throw new Exception(string.Format("Foreign Key Has No Value - Foreign Key Property Name: {0}", foreignKey.Name));
                }

                if (foreignKeyValue.IsList())
                {
                    foreach (var item in (foreignKeyValue as dynamic))
                    {
                        if (DatabaseSchemata.HasForeignKeys(item))
                        {
                            // set object here
                            _setValue(entity, item);
                            _setKeysWithForeignKeysAfterSave(item);
                        }
                        else
                        {
                            _setValue(entity, item);
                        }
                    }
                    continue;
                }

                if (DatabaseSchemata.HasForeignKeys(foreignKeyValue))
                {
                    // set object here
                    _setValue(entity, foreignKeyValue);
                    _setKeysWithForeignKeysAfterSave(foreignKeyValue as dynamic);
                }
            }
        }

        private void _analyzeChangesWithForeignKeys<T>(T entity, List<ForeignKeySaveNode> savableObjects)
            where T : class
        {
            // make sure FKs have values before saving, if they dont you need to throw an error
            foreach (var foreignKey in DatabaseSchemata.GetForeignKeys(entity))
            {
                var index = 0;
                var foreignKeyValue = foreignKey.GetValue(entity);

                if (foreignKeyValue == null)
                {
                    throw new Exception(string.Format("SAVE CANCELLED!  Reason: Foreign Key Has No Value - Foreign Key Property Name: {0}", foreignKey.Name));
                }

                // doesnt have dependencies
                if (foreignKeyValue.IsList())
                {
                    foreach (var item in (foreignKeyValue as dynamic))
                    {
                        index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                        if (DatabaseSchemata.HasForeignKeyDependencies(item))
                        {
                            savableObjects.Insert(index, new ForeignKeySaveNode(foreignKey, item, foreignKeyValue));
                        }
                        else
                        {
                            savableObjects.Insert(index + 1, new ForeignKeySaveNode(foreignKey, item, foreignKeyValue)); 
                        }

                        if (DatabaseSchemata.HasForeignKeys(item))
                        {
                            _analyzeChangesWithForeignKeys(item, savableObjects);
                        }
                    }
                    continue;
                }

                index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                savableObjects.Insert(index, new ForeignKeySaveNode(foreignKey, foreignKeyValue, entity));

                // has dependencies
                if (DatabaseSchemata.HasForeignKeys(foreignKeyValue))
                {
                    _analyzeChangesWithForeignKeys(foreignKeyValue as dynamic, savableObjects);
                }
            }
        }

        private void _saveChangesWithNoForeignKeys<T>(T entity)
        {
            // Check to see if the PK is defined
            var tableName = DatabaseSchemata.GetTableName(entity);

            // ID is the default primary key name
            var primaryKeys = DatabaseSchemata.GetPrimaryKeys(entity);

            // all table properties
            var tableColumns = DatabaseSchemata.GetTableFields(entity);

            // check to see whether we have an insert or update
            var state = DatabaseEntity.GetState(entity, primaryKeys);

            // Update Or Insert data
            switch (state)
            {
                case ModificationState.Update:
                    {
                        // Update Data
                        var update = new SqlUpdateBuilder();
                        update.Table(tableName);

                        foreach (var property in from property in tableColumns let columnName = DatabaseSchemata.GetColumnName(property) where !primaryKeys.Select(w => w.Name).Contains(property.Name) select property)
                        {
                            // Skip unmapped fields
                            update.AddUpdate(property, entity);
                        }

                        // add validation to only update the row
                        foreach (var primaryKey in primaryKeys)
                        {
                            update.AddWhere(tableName, primaryKey.Name, ComparisonType.Equals, primaryKey.GetValue(entity));
                        }

                        Execute(update);
                    }
                    break;
                case ModificationState.Insert:
                    {
                        // Insert Data
                        var insert = new SqlInsertBuilder();

                        insert.Table(tableName);

                        // Loop through all mapped properties
                        foreach (var property in tableColumns)
                        {
                            insert.AddInsert(property, entity);
                        }

                        // Execute the insert statement
                        Execute(insert);

                        // set the resulting pk(s) and db generated columns in the entity object
                        foreach (var item in SelectIdentity())
                        {
                            // find the property first in case the column name change attribute is used
                            // Key is property name, value is the db value
                            DatabaseEntity.SetPropertyValue(
                                entity,
                                item.Key,
                                item.Value);
                        }
                    }
                    break;
            }

            // close our readers
            Reader.Close();
            Reader.Dispose();
        }

        /// <summary>
        /// Finds a data object by looking for PK matches
        /// </summary>
        /// <typeparam name="T">Must be a Class</typeparam>
        /// <param name="pks"></param>
        /// <returns>Class Object</returns>
        public T Find<T>(params object[] pks)
            where T : class
        {
            var result = Activator.CreateInstance<T>();

            // get the database table name
            var tableName = DatabaseSchemata.GetTableName(result);

            var builder = new SqlQueryBuilder();
            builder.SelectAll<T>();
            builder.Table(tableName);

            // Get All PKs
            var keyProperties = DatabaseSchemata.GetPrimaryKeys(result);

            for (var i = 0; i < keyProperties.Count(); i++)
            {
                var key = keyProperties[i];

                // check to see if the column is renamed
                var name = DatabaseSchemata.GetColumnName(key);

                builder.AddWhere(tableName, name, ComparisonType.Equals, pks[i]);
            }

            Execute(builder);

            if (Reader.HasRows)
            {
                Reader.Read();
            }

            return Select<T>();
        }
        #endregion
    }
}
