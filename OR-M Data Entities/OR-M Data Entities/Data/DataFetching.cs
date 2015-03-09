/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Expressions;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class do not require a READ before data can be retreived
    /// </summary>
    public abstract class DataFetching : DatabaseReader
    {
        protected DataFetching(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }

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

		#region First
        protected T First<T>(Expression<Func<T, bool>> propertyLambda)
            where T : class
        {
			var result = Execute(propertyLambda, this);

            // Select All
            result.Select<T>();

            return result.First<T>();
        }

        /// <summary>
        /// Converts the first row to type T
        /// </summary>
        /// <returns></returns>
        protected T First<T>()
        {
            var builder = new SqlQueryBuilder();
            builder.SelectTopOneAll<T>();

            Execute(builder);

            if (Reader.HasRows)
            {
                var result = Reader.ToObject<T>();

                Reader.Close();
                Reader.Dispose();

                return result;
            }

            Reader.Close();
            Reader.Dispose();

            return default(T);
        }
		#endregion

		#region All
		/// <summary>
        /// Return list of items
        /// </summary>
        /// <returns>List of type T</returns>
        public List<T> All<T>()
        {
            var builder = new SqlQueryBuilder();
            builder.SelectAll<T>();

            Execute(builder);

            var result = new List<T>();

            while (Reader.Read())
            {
                result.Add(Reader.ToObject<T>());
            }

            Reader.Close();
            Reader.Dispose();

            return result;
        }

        public ExpressionQuery Where<T>(Expression<Func<T, bool>> propertyLambda) where T : class
        {
            var result = Execute(propertyLambda, this);

            // Select All
            result.Select<T>();

            return result;
        }

        public ExpressionQuery From<T>() where T : class
        {
            return new ExpressionQuery(DatabaseSchemata.GetTableName<T>(), this);
        }
		#endregion
	}
}
