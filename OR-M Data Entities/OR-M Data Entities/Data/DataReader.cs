/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data
{
	public class DataReader<T> : IEnumerable, IDisposable
    {
        #region Properties and Fields
        private readonly PeekDataReader _reader;

        public bool HasRows 
        {
            get { return _reader.HasRows; }
        }
        #endregion

        #region Constructor
        public DataReader(PeekDataReader reader)
		{
			_reader = reader;
		}
        #endregion

        #region Methods
        public T FirstOrDefault()
	    {
	        _reader.Read();

            var result = _reader.ToObjectDefault<T>();

            Dispose();

            return result;
	    }

        public T First()
        {
            _reader.Read();

            var result = _reader.ToObject<T>();

            Dispose();

            return result;
        }

	    public List<T> ToList()
	    {
	        var result = new List<T>();

	        while (_reader.Read())
	        {
                result.Add(_reader.ToObject<T>());
	        }

            Dispose();

	        return result;
	    } 

		public IEnumerator<T> GetEnumerator()
		{
			while (_reader.Read())
			{
                yield return _reader.ToObject<T>();
			}

            // close when done enumerating
            Dispose();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
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
