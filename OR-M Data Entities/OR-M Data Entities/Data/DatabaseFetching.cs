/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class do not require a READ before data can be retreived
    /// </summary>
    public abstract class DatabaseFetching : DatabaseReading
    {
        #region Constructor
        protected DatabaseFetching(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Identity
        /// <summary>
        /// Used with insert statements only, gets the value if the id's that were inserted
        /// </summary>
        /// <returns></returns>
        protected OutputContainer GetOutput()
        {
            if (Reader.HasRows)
            {
                Reader.Read();
                var keyContainer = new OutputContainer();
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

            return new OutputContainer();
        }

        #endregion

        #region helpers
        // helps return data from insertions
        protected sealed class OutputContainer : IEnumerable<KeyValuePair<string, object>>
        {
            #region Constructor
            public OutputContainer()
            {
                _container = new Dictionary<string, object>();
            }
            #endregion

            #region Properties
            private Dictionary<string, object> _container { get; set; }

            public int Count { get { return _container == null ? 0 : _container.Count; } }
            #endregion

            #region Methods
            public void Add(string columnName, object value)
            {
                _container.Add(columnName, value);
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return _container.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }
        #endregion
    }
}
