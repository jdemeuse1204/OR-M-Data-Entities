﻿/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Query;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class require a READ before data can be retreived
    /// </summary>
    public abstract class DatabaseReading : DatabaseExecution
    {
        #region Constructor
        protected DatabaseReading(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Reader Methods
        /// <summary>
        /// Used for looping through results
        /// </summary>
        /// <returns></returns>
		protected bool Read()
        {
            if (Reader.Read())
            {
                return true;
            }

            // close reader when no rows left
            Reader.Close();
            Reader.Dispose();
            return false;
        }

        /// <summary>
        /// Converts an object to a dynamic
        /// </summary>
        /// <returns></returns>
		protected dynamic Select()
        {
            return Reader.ToObject();
        }

        /// <summary>
        /// Converts a datareader to an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
		protected T Select<T>()
        {
            return Reader.ToObject<T>();
        }
        #endregion

        #region Data Execution
        public ExpressionQuery<T> From<T>()
        {

            //var lst = new ExpressionQuery<Contact>(context);
            //var tableName = DatabaseSchemata.GetTableName<T>();
            //ObjectSchematic schematic;

            //if (!SavedTableSchematics.ContainsKey(tableName))
            //{
            //    schematic = DatabaseSchemata.GetObjectSchematic<T>();

            //    SavedTableSchematics.Add(tableName, schematic);
            //}
            //else
            //{
            //    schematic = SavedTableSchematics[tableName];
            //}
            var query = new DbQuery(typeof (T));

            query.CreateSelectList();

            return new ExpressionQuery<T>(this, query);
        } 
        #endregion
    }
}
