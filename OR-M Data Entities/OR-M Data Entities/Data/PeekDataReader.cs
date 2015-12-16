/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Data;
using System.Data.SqlClient;
using OR_M_Data_Entities.Data.Execution;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// Custom data reader to allow for peeking at the next row without losing the positing in the current read
    /// </summary>
    public sealed class PeekDataReader : IDataReader
    {
        #region Fields
        private readonly IDataReader _wrappedReader;
        private bool _lastResult;
        private readonly SqlConnection _connection;
        #endregion

        #region Properties
        public int Depth { get { return _wrappedReader.Depth; } }
        public int RecordsAffected { get { return _wrappedReader.RecordsAffected; } }
        public bool IsClosed { get { return _wrappedReader.IsClosed; } }
        public bool HasRows { get; private set; }
        public int FieldCount { get; private set; }
        public bool WasPeeked { get; private set; }
        public ISqlPayload Payload { get; private set; }

        public object this[int i]
        {
            get { return _wrappedReader[i]; }
        }

        public object this[string name]
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

        public PeekDataReader(SqlCommand cmd, SqlConnection connection)
            : this(cmd, connection, null)
        {
            _connection = connection;
        }

        public PeekDataReader(SqlCommand cmd, SqlConnection connection, ISqlPayload payload)
        {
            try
            {
                var wrappedReader = cmd.ExecuteReader();

                _wrappedReader = wrappedReader;
                HasRows = wrappedReader.HasRows;
                FieldCount = wrappedReader.FieldCount;

                Payload = payload;

                _connection = connection;
            }
            catch (Exception)
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
                throw; // rethrow error after connection is cleaned up
            }
        }
        #endregion

        #region Methods
        public bool Peek()
        {
            // If the previous operation was a peek, do not move...
            if (WasPeeked)
            {
                return _lastResult;
            }

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
            _connection.Close();
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
            _connection.Close();
            _connection.Dispose();
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
