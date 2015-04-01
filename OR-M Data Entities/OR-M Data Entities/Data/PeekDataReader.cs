using System;
using System.Data;

namespace OR_M_Data_Entities.Data
{
    public sealed class PeekDataReader : IDataReader
    {
        #region Fields
        private readonly IDataReader _wrappedReader;
        private bool _wasPeeked;
        private bool _lastResult;
        #endregion

        #region Properties
        public int Depth { get; private set; }
        public bool IsClosed { get; private set; }
        public int RecordsAffected { get; private set; }
        public int FieldCount { get; private set; }

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
        public PeekDataReader(IDataReader wrappedReader)
        {
            _wrappedReader = wrappedReader;
        }
        #endregion

        #region Methods
        public bool Peek()
        {
            // If the previous operation was a peek, do not move...
            if (_wasPeeked)
            {
                return _lastResult;
            }

            // This is the first peek for the current position, so read and tag
            var result = Read();
            _wasPeeked = true;
            return result;
        }

        public bool Read()
        {
            // If last operation was a peek, do not actually read
            if (_wasPeeked)
            {
                _wasPeeked = false;
                return _lastResult;
            }

            // Remember the result for any subsequent peeks
            _lastResult = _wrappedReader.Read();
            return _lastResult;
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            _wasPeeked = false;
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
