/*
 * OR-M Data Entities v2.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Schema;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class require a READ before data can be retreived
    /// </summary>
    public abstract class DatabaseReading : DatabaseExecution
    {
        #region Fields
        protected object Lock = new object();
        #endregion

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
        #endregion

        #region Data Execution
        public ExpressionQuery<T> From<T>()
        {
            lock (Lock)
            {
                return new ExpressionQueryResolvable<T>(this);
            }
        }

        public ExpressionQuery<T> FromView<T>(string viewId)
        {
            lock (Lock)
            {
                if (!typeof (T).IsPartOfView(viewId))
                {
                    throw new ViewException(string.Format("Type Of {0} Does not contain attribute for View - {1}",
                        typeof (T).Name, viewId));
                }

                return new ExpressionQueryViewResolvable<T>(this, viewId);
            }
        }

        public T Find<T>(params object[] pks)
        {
            lock (Lock)
            {
                var query = From<T>();

                ((ExpressionQueryResolvable<T>) query).ResolveFind(pks);

                return query.FirstOrDefault();
            }
        } 
        #endregion
    }
}
