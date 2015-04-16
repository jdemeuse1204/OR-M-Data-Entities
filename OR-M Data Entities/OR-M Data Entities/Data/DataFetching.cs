/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data.PayloadOperations;
using OR_M_Data_Entities.Expressions;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class do not require a READ before data can be retreived
    /// </summary>
    public abstract class DataFetching : DatabaseReading
    {
        #region Constructor
        protected DataFetching(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Identity
        /// <summary>
        /// Used with insert statements only, gets the value if the id's that were inserted
        /// </summary>
        /// <returns></returns>
        protected KeyContainer SelectIdentity()
        {
            if (Reader.HasRows)
            {
                Reader.Read();
                var keyContainer = new KeyContainer();
                var rec = (IDataRecord)Reader;

                for (var i = 0; i < rec.FieldCount; i++)
                {
                    keyContainer.Add(rec.GetName(i), rec.GetValue(i));
                }

                Reader.Close();
                Reader.Dispose();

                return keyContainer;
            }

            Reader.Close();
            Reader.Dispose();

            return new KeyContainer();
        }
        #endregion

        #region First
        protected T First<T>(Expression<Func<T, bool>> expression)
            where T : class
        {
			var where = Where<T>(expression);

            return where.First<T>();
        }

        /// <summary>
        /// Converts the first row to type T
        /// </summary>
        /// <returns></returns>
        protected T First<T>() where  T : class 
        {
			var select = SelectAll<T>();

			return select.First<T>();
        }
		#endregion

		#region All
		/// <summary>
        /// Return list of items
        /// </summary>
        /// <returns>List of type T</returns>
        public List<T> All<T>() where T : class
        {
			var select = SelectAll<T>();

            return select.All<T>();
        }

        public ExpressionWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
        {
			var select = SelectAll<T>();

            return select.Where<T>(expression);
        }

		public ExpressionSelectQuery SelectAll<T>() where T : class
		{
			var select = new ExpressionSelectQuery(null, this);

			select.SelectAll<T>();

			return select;
		}

		public ExpressionUpdateSetQuery Update<T>() where T : class
		{
			var update = new ExpressionUpdateQuery(null, this);

			return update.Update<T>();
		}

		public ExpressionDeleteWhereQuery Delete<T>() where T : class
		{
			var delete = new ExpressionDeleteQuery(null, this);

			return delete.Delete<T>();
		}

		public ExpressionInsertValueQuery Insert<T>() where T : class
		{
			var insert = new ExpressionInsertQuery(null, this);

			return insert.Insert<T>();
		}
		#endregion
	}
}
