﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Data
{
    internal class PeekDataReader : IPeekDataReader
    {
        #region Fields
        private readonly IDataReader _wrappedReader;
        private bool _lastResult;
        #endregion

        #region Properties
        public int Depth { get { return _wrappedReader.Depth; } }

        public int RecordsAffected { get { return _wrappedReader.RecordsAffected; } }

        public bool IsClosed { get { return _wrappedReader.IsClosed; } }

        public bool HasRows { get { return _wrappedReader == null ? false : ((SqlDataReader)_wrappedReader).HasRows; } }

        public int FieldCount { get { return _wrappedReader == null ? 0 : _wrappedReader.FieldCount; } }

        public bool WasPeeked { get; private set; }

        private object this[int i]
        {
            get { return _wrappedReader[i]; }
        }

        private object this[string name]
        {
            get { return _wrappedReader[name]; }
        }

        object IDataRecord.this[int i]
        {
            get { return _wrappedReader[i]; }
        }

        object IDataRecord.this[string name]
        {
            get { return _wrappedReader[name]; }
        }
        #endregion

        #region Constructor

        public PeekDataReader(IDataReader reader)
        {
            _wrappedReader = reader;
        }
        #endregion

        #region Data Loading Methods
        public T ToObjectDefault<T>()
        {
            if (HasRows) return ToObject<T>();

            // clean up reader
            Dispose();

            // return the default
            return default(T);
        }

        public T ToObject<T>()
        {
            if (!HasRows)
            {
                // clean up reader
                Dispose();

                throw new DataException("Query contains no rows");
            }

            if (typeof(T).IsValueType || typeof(T) == typeof(string)) return this[0] == DBNull.Value ? default(T) : (T)this[0];

            // if its an anonymous type, use the correct loader
            return default(T);
        }
        #endregion

        #region Methods
        public bool Peek()
        {
            // If the previous operation was a peek, do not move...
            if (WasPeeked) return _lastResult;


            // This is the first peek for the current position, so read and tag
            var result = Read();
            WasPeeked = true;
            return result;
        }

        public bool Read()
        {
            // If last operation was a peek, do not actually read
            if (WasPeeked)
            {
                WasPeeked = false;
                return _lastResult;
            }

            // Remember the result for any subsequent peeks
            _lastResult = _wrappedReader.Read();
            return _lastResult;
        }

        public void Close()
        {
            _wrappedReader.Close();
        }

        public DataTable GetSchemaTable()
        {
            return _wrappedReader.GetSchemaTable();
        }

        public bool NextResult()
        {
            WasPeeked = false;
            return _wrappedReader.NextResult();
        }

        public void Dispose()
        {
            _wrappedReader.Dispose();
        }

        public string GetName(int i)
        {
            return _wrappedReader.GetName(i);
        }

        public string GetDataTypeName(int i)
        {
            return _wrappedReader.GetDataTypeName(i);
        }

        public Type GetFieldType(int i)
        {
            return _wrappedReader.GetFieldType(i);
        }

        public object GetValue(int i)
        {
            return _wrappedReader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return _wrappedReader.GetValues(values);
        }

        public int GetOrdinal(string name)
        {
            return _wrappedReader.GetOrdinal(name);
        }

        public bool GetBoolean(int i)
        {
            return _wrappedReader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return _wrappedReader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _wrappedReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return _wrappedReader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _wrappedReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public Guid GetGuid(int i)
        {
            return _wrappedReader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return _wrappedReader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return _wrappedReader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return _wrappedReader.GetInt64(i);
        }

        public float GetFloat(int i)
        {
            return _wrappedReader.GetFloat(i);
        }

        public double GetDouble(int i)
        {
            return _wrappedReader.GetDouble(i);
        }

        public string GetString(int i)
        {
            return _wrappedReader.GetString(i);
        }

        public decimal GetDecimal(int i)
        {
            return _wrappedReader.GetDecimal(i);
        }

        public DateTime GetDateTime(int i)
        {
            return _wrappedReader.GetDateTime(i);
        }

        public IDataReader GetData(int i)
        {
            return _wrappedReader.GetData(i);
        }

        public bool IsDBNull(int i)
        {
            return _wrappedReader.IsDBNull(i);
        }
        #endregion
    }
}
