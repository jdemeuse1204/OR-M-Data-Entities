/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Resolution;

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
            return Reader.ToDynamic();
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
            return new ExpressionQueryResolvable<T>(this);
        }

        public ExpressionQuery<T> FromView<T>(string viewId)
        {
            if (!DatabaseSchemata.IsPartOfView(typeof(T), viewId)) 
            {
                throw new ViewException(string.Format("Type Of {0} Does not contain attribute for View - {1}",
                        typeof(T).Name, viewId));
            }

            return new ExpressionQueryViewResolvable<T>(this, viewId);
        }

        public T Find<T>(params object[] pks)
        {
            var query = From<T>();

            ((ExpressionQueryResolvable<T>)query).ResolveFind(pks);

            return query.FirstOrDefault();  
        } 
        #endregion
    }
}
