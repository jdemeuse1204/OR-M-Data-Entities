/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Data
{
    public abstract partial class Database : IDisposable
    {
        #region Properties

        protected string ConnectionString { get; private set; }

        protected IPeekDataReader Reader { get; private set; }

        protected IConfigurationOptions Configuration { get; private set; }

        private SqlConnection _connection { get; set; }

        private SqlCommand _command { get; set; }
        #endregion

        #region Constructor
        protected Database(string connectionStringOrName)
        {
            if (connectionStringOrName.Contains(";") || connectionStringOrName.Contains("="))
            {
                ConnectionString = connectionStringOrName;
            }
            else
            {
                var conn = ConfigurationManager.ConnectionStrings[connectionStringOrName];

                if (conn == null) throw new ConfigurationErrorsException("Connection string not found in config");

                ConnectionString = conn.ConnectionString;
            }

            // check to see if MARS is enabled, it is needed for transactions
            Configuration = new ConfigurationOptions(_isMARSEnabled(ConnectionString), "dbo");

            _connection = new SqlConnection(ConnectionString);
        }
        #endregion

        #region Methods
        private bool _isMARSEnabled(string connectionString)
        {
            return connectionString.ToUpper().Contains("MULTIPLEACTIVERESULTSETS=TRUE");
        }

        /// <summary>
        /// Connect our SqlConnection
        /// </summary>
        protected void Connect()
        {
            const string errorMessage = "Data Context in the middle of an operation, consider locking your threads to avoid this.  Operation: {0}";

            switch (_connection.State)
            {
                case ConnectionState.Closed:
                case ConnectionState.Broken:

                    // if the connection was opened before we need to renew it
                    if (_wasConnectionPreviouslyOpened())
                    {
                        // connection needs to be renewed
                        _connection.Dispose();
                        _connection = null;
                        _connection = new SqlConnection(ConnectionString);
                    }

                    _connection.Open();
                    return;
                case ConnectionState.Connecting:
                    throw new Exception(string.Format(errorMessage,"Connecting to database"));
                case ConnectionState.Executing:
                    throw new Exception(string.Format(errorMessage, "Executing Query"));
                case ConnectionState.Fetching:
                    throw new Exception(string.Format(errorMessage, "Fetching Data"));
                case ConnectionState.Open:
                    return;
            }
        }

        /// <summary>
        /// Disconnect the SqlConnection
        /// </summary>
        public void Disconnect()
        {
            // disconnect our db reader
            if (Reader != null)
            {
                Reader.Close();
                Reader.Dispose();
            }

            // dispose of our sql command
            if (_command != null)
            {
                _command.Dispose();
            }

            // close our connection
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }

        protected void TryDisposeCloseReader()
        {
            if (Reader == null) return;

            Reader.Close();
            Reader.Dispose();
        }

        /// <summary>
        /// Checks to see if the connection was previously closed.  
        /// </summary>
        /// <returns></returns>
        private bool _wasConnectionPreviouslyOpened()
        {
            var innerConnection = _connection.GetType().GetField("_innerConnection", BindingFlags.Instance | BindingFlags.NonPublic);

            if (innerConnection == null) throw new Exception("Cannot connect to database, inner connection not found");

            var connection = innerConnection.GetValue(_connection).GetType().Name;

            return connection.EndsWith("PreviouslyOpened");
        }

        public virtual void Dispose()
        {
            // clean up the command, connection and reader
            Disconnect();

            Configuration = null;
            ConnectionString = null;
        }


        #endregion

        #region Execution
        protected void ExecuteReader(string sql, List<SqlDbParameter> parameters, IQuerySchematic schematic)
        {
            _preprocessExecution();

            _command = new SqlCommand(sql, _connection);

            _addParameters(parameters);

            Reader = new PeekDataReader(_command, _connection, schematic);
        }

        protected void ExecuteReader(string sql)
        {
            ExecuteReader(sql, new List<SqlDbParameter>());
        }

        protected void ExecuteReader(string sql, List<SqlDbParameter> parameters)
        {
            ExecuteReader(sql, parameters, null);
        }

        protected void ExecuteReader(ISqlExecutionPlan builder)
        {
            _preprocessExecution();

            _command = builder.BuildSqlCommand(_connection);

            Reader = new PeekDataReader(_command, _connection);
        }

        private void _addParameters(List<SqlDbParameter> parameters)
        {
            foreach (var item in parameters)
            {
                _command.Parameters.Add(_command.CreateParameter()).ParameterName = item.Name;
                _command.Parameters[item.Name].Value = item.Value;
            }
        }

        private void _preprocessExecution()
        {
            TryDisposeCloseReader();

            Connect();
        }
        #endregion

        #region Configuration
        private class ConfigurationOptions : IConfigurationOptions
        {
            public ConfigurationOptions(bool useTransactions, string defaultSchema)
            {
                IsLazyLoading = false;
                UseTransactions = useTransactions;

                ConcurrencyChecking = new ConcurrencyConfiguration
                {
                    ViolationRule = ConcurrencyViolationRule.OverwriteAndContinue,
                    IsOn = true
                };

                InsertKeys = new KeyConfiguration();
                DefaultSchema = defaultSchema;
            }

            public bool IsLazyLoading { get; set; }

            public bool UseTransactions { get; set; }

            public ConcurrencyConfiguration ConcurrencyChecking { get; private set; }

            public KeyConfiguration InsertKeys { get; private set; }

            public string DefaultSchema { get; private set; }
        }
        #endregion
    }

    public abstract partial class Database
    {
        private class PeekDataReader : IPeekDataReader
        {
            #region Fields
            private readonly IDataReader _wrappedReader;
            private bool _lastResult;
            private readonly SqlConnection _connection;
            private readonly IQuerySchematic _schematic;
            #endregion

            #region Properties
            public int Depth { get { return _wrappedReader.Depth; } }

            public int RecordsAffected { get { return _wrappedReader.RecordsAffected; } }

            public bool IsClosed { get { return _wrappedReader.IsClosed; } }

            public bool HasRows { get; private set; }

            public int FieldCount { get; private set; }

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

            public PeekDataReader(SqlCommand cmd, SqlConnection connection)
                : this(cmd, connection, null)
            {
                _connection = connection;
            }

            public PeekDataReader(SqlCommand cmd, SqlConnection connection, IQuerySchematic schematic)
            {
                try
                {
                    var wrappedReader = cmd.ExecuteReader();

                    _wrappedReader = wrappedReader;
                    HasRows = wrappedReader.HasRows;
                    FieldCount = wrappedReader.FieldCount;

                    _schematic = schematic;

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

            #region Data Loading Methods
            public dynamic ToDynamic()
            {
                if (!HasRows) return null;

                var result = new ExpandoObject() as IDictionary<string, object>;

                var rec = (IDataRecord)this;

                for (var i = 0; i < rec.FieldCount; i++) result.Add(rec.GetName(i), rec.GetValue(i));

                return result;
            }

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

                    throw new DataException("Query contains no records");
                }

                if (typeof(T).IsValueType || typeof(T) == typeof(string)) return this[0] == DBNull.Value ? default(T) : (T)this[0];

                if (typeof(T) == typeof(object)) return ToDynamic();

                // if its an anonymous type, use the correct loader
                return typeof(T).IsAnonymousType() ? (T)_getAnonymousObject(typeof(T))

                       // if the payload is null, load by column names
                       : _schematic == null ? _getObjectFromReaderUsingDatabaseColumnNames<T>()

                       // if the payload has foreign keys, use the foreign key loader
                       : _schematic.AreForeignKeysSelected() ? _getObjectFromReaderWithForeignKeys<T>()

                       // default if all are false
                       : _getObjectFromReader<T>();
            }

            private T _getObjectFromReaderUsingDatabaseColumnNames<T>()
            {
                // Create instance
                var instance = Activator.CreateInstance<T>();

                // load the instance
                _loadObjectByColumnNames(instance);

                // set the table on load if possible
                DatabaseSchematic.TrySetPristineEntity(instance);

                return instance;
            }

            private bool _loadObjectFromSchematic(object instance, IDataLoadSchematic schematic)
            {
                try
                {
                    foreach (var property in schematic.MappedTable.SelectedColumns)
                    {
                        var dbValue = this[property.Ordinal];
                        var dbNullValue = dbValue as DBNull;

                        // the rest of the object will be null.  No data exists for the object
                        if (property.Column.IsPrimaryKey && dbNullValue != null) return false;

                        instance.SetPropertyInfoValue(property.Column.PropertyName, dbNullValue != null ? null : dbValue);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw new DataLoadException(string.Format("PeekDataReader could not load object {0}.  Message: {1}",
                        instance.GetType().Name, ex.Message));
                }
            }

            private T _getObjectFromReader<T>()
            {
                // Create instance
                var instance = Activator.CreateInstance<T>();

                // load the instance
                _loadObjectFromSchematic(instance, _schematic.DataLoadSchematic);

                // set the table on load if possible, we don't care about foreign keys
                DatabaseSchematic.TrySetPristineEntity(instance);

                return instance;
            }

            private T _getObjectFromReaderWithForeignKeys<T>()
            {
                // Create instance
                var instance = Activator.CreateInstance<T>();

                // get the key so we can look at the key of each row
                var beginningSchematic = _schematic.DataLoadSchematic;

                // get beginning composite key array
                var compositeKeyArray = _getCompositKeyArray(beginningSchematic);

                // grab the starting composite key
                var compositeKey = _getCompositKey(compositeKeyArray);

                // load the instance
                _loadObjectFromSchematic(instance, _schematic.DataLoadSchematic);

                // set the table on load if possible, we don't care about foreign keys
                DatabaseSchematic.TrySetPristineEntity(instance);

                // load first row, do not move next.  While loop will move next 
                _loadObjectWithForeignKeys(instance);

                // Loop through the dataset and fill our object.  Check to see if the next PK is the same as the starting PK
                // if it is then we need to stop and return our object
                while (Peek() && compositeKey.Equals(_getCompositKey(compositeKeyArray)) && Read())
                {
                    _loadObjectWithForeignKeys(instance);
                }

                // Rows with a PK from the initial object are done loading.  
                // Clear Schematics, selected columns, and ordered columns
                // clear is recursive and will clear all children
                _schematic.ClearReadCache();

                return instance;
            }

            private long _getCompositKey(int[] compositeKeyArray)
            {
                return compositeKeyArray.Sum(w => this[w].GetHashCode());
            }

            private int[] _getCompositKeyArray(IDataLoadSchematic dataLoadSchematic)
            {
                return dataLoadSchematic.MappedTable.SelectedColumns.Where(w => w.Column.IsPrimaryKey).Select(w => w.Ordinal).OrderBy(w => w).ToArray();
            }

            private void _loadObjectWithForeignKeys(object startingInstance)
            {
                // after this method is completed we need to make sure we can read the next set.  This method should go in a loop
                // load the instance before it comes into thos method

                var schematic = _schematic.DataLoadSchematic;
                var schematicsToScan = new List<IDataLoadSchematic>();
                var parentInstance = startingInstance;

                // initialize the list
                schematicsToScan.AddRange(schematic.Children);

                // set the original count so we know wether to look in the parent or reference to parent
                var originalCount = schematicsToScan.Count - 1;

                for (var i = 0; i < schematicsToScan.Count; i++)
                {
                    var currentSchematic = schematicsToScan[i];

                    // if the current table is not included in the selection we skip it
                    if (!currentSchematic.MappedTable.IsIncluded) continue;

                    var compositeKeyArray = _getCompositKeyArray(currentSchematic);
                    var compositeKey = _getCompositKey(compositeKeyArray);
                    var schematicKey = new OSchematicKey(compositeKey, compositeKeyArray);

                    // if ReferenceToCurrent is null then its from the parent and we need to check the composite key.  If its not from the 
                    // parent we need to check the Reference to current and see if the property has a value.  If not we need to load
                    // the instance.  is null property check should only be for a single instance.  If its a list we need 
                    // to fall back to checking the composite key to see if it was loaded.  The property is the list, that 
                    // is the incorrect check
                    var wasLoaded = currentSchematic.ReferenceToCurrent == null || currentSchematic.ActualType.IsList()
                        ? currentSchematic.LoadedCompositePrimaryKeys.Contains(schematicKey)
                        : currentSchematic.ReferenceToCurrent.GetType()
                            .GetProperty(currentSchematic.PropertyName)
                            .GetValue(currentSchematic.ReferenceToCurrent) != null;

                    // add children of current instance so they can be scanned
                    schematicsToScan.AddRange(currentSchematic.Children);

                    // if it was already loaded, continue to next schematic
                    if (wasLoaded) continue;

                    // create the instance
                    var newInstance = Activator.CreateInstance(currentSchematic.Type);

                    // mark the object as loaded
                    currentSchematic.LoadedCompositePrimaryKeys.Add(schematicKey);

                    // load the data into new instance
                    // If load returns false, then its a left join, everything might be null
                    if (!_loadObjectFromSchematic(newInstance, currentSchematic)) continue;

                    // set the table on load if possible, we don't care about foreign keys
                    DatabaseSchematic.TrySetPristineEntity(newInstance);

                    // List
                    if (currentSchematic.ActualType.IsList())
                    {
                        // check and see if the list was created
                        var foundInstanceForListGetValue = _getInstance(i, originalCount, currentSchematic, parentInstance);

                        var list =
                            foundInstanceForListGetValue
                                .GetType()
                                .GetProperty(currentSchematic.PropertyName)
                                .GetValue(foundInstanceForListGetValue);

                        if (list == null)
                        {
                            var foundInstanceForListSetValue = _getInstance(i, originalCount, currentSchematic, parentInstance);

                            // create new list
                            list = Activator.CreateInstance(currentSchematic.ActualType);

                            // set the new list on the parent
                            foundInstanceForListSetValue.GetType()
                                .GetProperty(currentSchematic.PropertyName)
                                .SetValue(foundInstanceForListSetValue, list);
                        }

                        // add object to list
                        _add(list, newInstance);

                        // store references to the current instance so we can load the objects,
                        // otherwise we will have to search through the object and look for the instance
                        foreach (var child in currentSchematic.Children) child.ReferenceToCurrent = newInstance;

                        // continue processing next record
                        continue;
                    }

                    var foundInstanceForSingleSetValue = _getInstance(i, originalCount, currentSchematic, parentInstance);

                    // Single Instance
                    foundInstanceForSingleSetValue.GetType()
                        .GetProperty(currentSchematic.PropertyName)
                        .SetValue(foundInstanceForSingleSetValue, newInstance);

                    // store references to the current instance so we can load the objects,
                    // otherwise we will have to search through the object and look for the instance
                    foreach (var child in currentSchematic.Children) child.ReferenceToCurrent = newInstance;
                }
            }

            private void _add(object list, object valueToAdd)
            {
                list.GetType().GetMethod("Add").Invoke(list, new[] { valueToAdd });
            }

            private object _getInstance(int index, int originalCount, IDataLoadSchematic schematic, object parentInstance)
            {
                return index <= originalCount ? parentInstance : schematic.ReferenceToCurrent;
            }

            private void _loadObjectByColumnNames(object instance)
            {
                try
                {
                    var properties =
                        instance.GetType()
                            .GetProperties()
                            .Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null)
                            .ToList();

                    foreach (var property in properties)
                    {
                        var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                        var columnName = columnAttribute == null ? property.Name : columnAttribute.Name;
                        var dbValue = this[columnName];


                        // todo change
                        instance.SetPropertyInfoValue(property, dbValue is DBNull ? null : dbValue);
                    }
                }
                catch (Exception ex)
                {
                    throw new DataLoadException(string.Format("PeekDataReader could not load object {0}.  Message: {1}", instance.GetType().Name, ex.Message));
                }
            }

            private object _getAnonymousObject(Type type)
            {
                var constructorParameters = new Queue<object>();
                var properties = type.GetProperties();

                foreach (var property in properties)
                {
                    var propertyType = property.PropertyType;

                    if (propertyType.IsSerializable)
                    {
                        if (propertyType.IsList())
                        {
                            // load the object
                            constructorParameters.Enqueue(_getValue(type, property.Name));
                            continue;
                        }

                        // the we assume its a value type
                        constructorParameters.Enqueue(_getValue(type, property.Name));
                        continue;
                    }

                    var propertyInstance = _getAnonymousObject(propertyType);

                    constructorParameters.Enqueue(propertyInstance);
                }

                // Do last because the constructor needs the premade properties to go into it
                var instance = Activator.CreateInstance(type, constructorParameters.ToArray());

                return instance;
            }

            private object _getValue(Type instanceType, string propertyName)
            {
                var table = _schematic.MappedTables.FirstOrDefault(w => w.Table.Type == instanceType);

                if (table == null) throw new Exception(string.Format("Table not found for type.  Type of {0}", instanceType.Name));

                var property = table.SelectedColumns.FirstOrDefault(w => w.Column.PropertyName == propertyName);

                if (property == null) throw new Exception(string.Format("Property name not found for type.  Type of {0}.  Property - {1}", instanceType.Name, propertyName));

                var ordinal = property.Ordinal;

                return this[ordinal];
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

        protected class ParameterCollection : IEnumerable<SqlDbParameter>
        {
            private readonly HashSet<SqlDbParameter> _internal;

            public ParameterCollection()
            {
                _internal = new HashSet<SqlDbParameter>();
            }

            public void Add(object value, out string parameterKey)
            {
                parameterKey = string.Format("@DATA{0}", _internal.Count);

                _internal.Add(new SqlDbParameter(parameterKey, value));
            }

            public void AddRange(ParameterCollection collection)
            {
                foreach (var parameter in collection) _internal.Add(parameter);
            }

            public IEnumerator<SqlDbParameter> GetEnumerator()
            {
                return _internal.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
