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
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Expressions;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class do not require a READ before data can be retreived
    /// </summary>
    public abstract class DataFetching : DatabaseReading
    {
        protected object Lock = new object();

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
        public T First<T>(Expression<Func<T, bool>> expression)
            where T : class
        {
            lock (Lock)
            {
                var where = Where<T>(expression);

                return where.First<T>();
            }
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> expression)
            where T : class
        {
            lock (Lock)
            {
                var where = Where<T>(expression);

                return where.FirstOrDefault<T>();
            }
        }

        /// <summary>
        /// Converts the first row to type T
        /// </summary>
        /// <returns></returns>
        public T First<T>() where T : class 
        {
            lock (Lock)
            {
                var select = SelectAll<T>();

                return select.First<T>();
            }
        }

        public T FirstOrDefault<T>() where T : class
        {
            lock (Lock)
            {
                var select = SelectAll<T>();

                return select.FirstOrDefault<T>();
            }
        }
		#endregion

		#region All
		/// <summary>
        /// Return list of items
        /// </summary>
        /// <returns>List of type T</returns>
        public List<T> ToList<T>() where T : class
        {
		    lock (Lock)
		    {
		        var select = SelectAll<T>();

                return select.ToList<T>();
		    }
        }
        #endregion

        #region ExpressionQuery
        public ExpressionWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
        {
            lock (Lock)
            {
                var select = SelectAll<T>();

                return select.Where<T>(expression);
            }
        }

		public ExpressionSelectQuery SelectAll<T>() where T : class
		{
		    lock (Lock)
		    {
		        var select = new ExpressionSelectQuery(null, this);

		        select.SelectAll<T>();

		        return select;
		    }
		}

        public ExpressionSelectQuery Select<T>(Expression<Func<T, object>> selector) where T : class
        {
            lock (Lock)
            {
                var select = new ExpressionSelectQuery(null, this);

                select.Select<T>(selector);

                return select;
            }
        }

        public T Find<T>(params object[] pks) where T : class
        {
            lock (Lock)
            {
                var find = new ExpressionFindQuery(null, this);

                return find.Find<T>(pks);
            }
        }
		#endregion
	}
}
