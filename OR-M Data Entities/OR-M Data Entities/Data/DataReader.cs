﻿/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data
{
	public sealed class DataReader<T> : IEnumerable, IDisposable
    {
        #region Properties and Fields
        private readonly PeekDataReader _reader;

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
        public DataReader(PeekDataReader reader)
		{
			_reader = reader;
		}
        #endregion

        #region Methods
        public T FirstOrDefault(string viewId = null)
	    {
	        _reader.Read();

            var result = _reader.ToObject<T>(viewId);

            Dispose();

            return result == null ? default(T) : result;
	    }

        public T First(string viewId = null)
        {
            _reader.Read();

            var result = _reader.ToObject<T>(viewId);

            Dispose();

            return result;
        }

        public List<T> ToList(string viewId = null)
	    {
	        var result = new List<T>();

	        while (_reader.Read())
	        {
                result.Add(_reader.ToObject<T>(viewId));
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
