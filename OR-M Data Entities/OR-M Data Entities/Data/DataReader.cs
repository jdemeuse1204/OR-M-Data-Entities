/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OR_M_Data_Entities.Data
{
	public sealed class DataReader<T> : IEnumerable, IDisposable
    {
        #region Properties and Fields
        private readonly SqlDataReader _reader;
        private bool _useTableColumnFetch { get; set; }

        public bool HasRows 
        {
            get { return _reader.HasRows; }
        }

        [Obsolete("Use HasRows instead.  NOTE: Do not use with while loop, use foreach iteration.")]
        public bool IsEOF
        {
            get { return _reader.HasRows; }
        }
        #endregion

        #region Constructor
        public DataReader(SqlDataReader reader, bool useTableColumnFetch = false)
		{
			_reader = reader;
            _useTableColumnFetch = useTableColumnFetch;
		}
        #endregion

        #region Methods
        public T Select()
	    {
	        _reader.Read();

            return _reader.ToObject<T>(_useTableColumnFetch);
	    }

	    public List<T> All()
	    {
	        var result = new List<T>();

	        while (_reader.Read())
	        {
                result.Add(_reader.ToObject<T>(_useTableColumnFetch));
	        }

            Dispose();

	        return result;
	    } 

		// IEnumerable Member
		public IEnumerator<T> GetEnumerator()
		{
			while (_reader.Read())
			{
                yield return _reader.ToObject<T>(_useTableColumnFetch);
			}

            // close when done enumerating
            Dispose();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			// Lets call the generic version here
			return GetEnumerator();
		}

	    public void Dispose()
	    {
	        _reader.Close();
            _reader.Dispose();
        }
        #endregion
    }
}
