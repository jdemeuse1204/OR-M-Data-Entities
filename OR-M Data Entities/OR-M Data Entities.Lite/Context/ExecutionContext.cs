using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Expressions.Query;
using OR_M_Data_Entities.Lite.Extensions;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace OR_M_Data_Entities.Lite.Context
{
    internal class ExecutionContext
    {
        private readonly string connectionString;

        public ExecutionContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public T LoadOne<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            return default(T);
        }

        public T LoadOneDefault<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(sql.Query, connection))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                command.Parameters.AddRange(sql.Parameters.ToArray());

                using (var reader = command.ExecutePeekReader())
                {
                    if (!reader.HasRows) { return default(T); }

                    var objectReader = new ObjectReader(typeof(T));
                    var ordinal = 0;
                    var instance = new T();

                    while (reader.Read())
                    {
                          while (objectReader.Read())
                        {
                            var record = objectReader.GetRecord();
                            var tableSchematic = tableSchemas[record.Type];

                            switch (record.ForeignKeyType)
                            {
                                case ForeignKeyType.None:
                                    {
                                        var compositeKeyPieces = new List<object>();

                                        // base object
                                        for (var i = 0; i < tableSchematic.Columns.Count; i++)
                                        {
                                            var column = tableSchematic.Columns[i];
                                            var value = reader.Get(ordinal);

                                            if (column.IsKey)
                                            {
                                                compositeKeyPieces.Add(value);
                                                ordinal++;
                                                continue;
                                            }

                                            if (column.IsFirstNonKey)
                                            {
                                                // check to see if item has already been loaded
                                                // This is where Activator.CreateInstance should go
                                                var currentCompositeKey = compositeKeyPieces.Aggregate(string.Empty, (current, next) => $"{current}{next}");

                                                if (record.DataBags.Contains(currentCompositeKey))
                                                {
                                                    ordinal += (tableSchematic.Columns.Count - tableSchematic.KeyCount);
                                                    break;
                                                }
                                                else
                                                {
                                                    record.DataBags.Add(currentCompositeKey);

                                                    // load keys into the object
                                                    for (var j = 0; j < compositeKeyPieces.Count; j++)
                                                    {
                                                        var keyValue = compositeKeyPieces[j];
                                                        var keyColumn = tableSchematic.Columns[j];
                                                        record.TypeAccessor[instance, keyColumn.PropertyName] = keyValue;
                                                    }
                                                }
                                            }

                                            // composite key should have been compared by now
                                            // set wasCompositeKeyCompared so we can more easily terminate the above if statement
                                            record.TypeAccessor[instance, column.PropertyName] = value;
                                            ordinal++;
                                        }

                                        record.DataBag = instance;
                                    }
                                    break;
                                case ForeignKeyType.OneToOne:
                                    {
                                        object singularInstance = null;
                                        var oneToOneKeyBag = new List<object>();

                                        for (var i = 0; i < tableSchematic.Columns.Count; i++)
                                        {
                                            var column = tableSchematic.Columns[i];
                                            var value = reader.Get(ordinal);

                                            if (column.IsKey)
                                            {
                                                oneToOneKeyBag.Add(value);
                                                ordinal++;
                                                continue;
                                            }

                                            if (column.IsFirstNonKey)
                                            {
                                                var currentCompositeKey = oneToOneKeyBag.Aggregate(string.Empty, (current, next) => $"{current}{next}");

                                                if (record.DataBags.Contains(currentCompositeKey))
                                                {
                                                    ordinal += (tableSchematic.Columns.Count - tableSchematic.KeyCount);
                                                    break;
                                                }
                                                else
                                                {
                                                    record.DataBags.Add(currentCompositeKey);
                                                    singularInstance = Activator.CreateInstance(record.Type);

                                                    // load keys into the object
                                                    for (var j = 0; j < oneToOneKeyBag.Count; j++)
                                                    {
                                                        var keyValue = oneToOneKeyBag[j];
                                                        var keyColumn = tableSchematic.Columns[j];
                                                        record.TypeAccessor[singularInstance, keyColumn.PropertyName] = keyValue;
                                                    }
                                                }
                                            }

                                            // keys are always orders to be read first
                                            record.TypeAccessor[singularInstance, column.PropertyName] = value;
                                            ordinal++;
                                        }

                                        if (singularInstance != null)
                                        {
                                            record.DataBag = singularInstance;

                                            // set the property on the parent with the loaded object
                                            record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName] = singularInstance;
                                        }
                                    }
                                    break;
                                case ForeignKeyType.LeftOneToOne:
                                case ForeignKeyType.NullableOneToOne:
                                    {
                                        var leftAndNullableOneToOneKeyBag = new List<object>();
                                        object singularInstance = null;

                                        for (var i = 0; i < tableSchematic.Columns.Count; i++)
                                        {
                                            // keys are always orders to be read first
                                            var column = tableSchematic.Columns[i];
                                            var value = reader.Get(ordinal);

                                            if (column.IsKey)
                                            {
                                                if (value == null)
                                                {
                                                    ordinal += tableSchematic.Columns.Count;
                                                    break;
                                                }
                                                else
                                                {
                                                    leftAndNullableOneToOneKeyBag.Add(value);
                                                    ordinal++;
                                                    continue;
                                                }
                                            }

                                            if (column.IsFirstNonKey)
                                            {
                                                // check to see if item has already been loaded
                                                // This is where Activator.CreateInstance should go
                                                var currentCompositeKey = leftAndNullableOneToOneKeyBag.Aggregate(string.Empty, (current, next) => $"{current}{next}");

                                                if (record.DataBags.Contains(currentCompositeKey))
                                                {
                                                    ordinal += (tableSchematic.Columns.Count - tableSchematic.KeyCount);
                                                    break;
                                                }
                                                else
                                                {
                                                    record.DataBags.Add(currentCompositeKey);
                                                    singularInstance = Activator.CreateInstance(record.Type);

                                                    // load keys into the object
                                                    for (var j = 0; j < leftAndNullableOneToOneKeyBag.Count; j++)
                                                    {
                                                        var keyValue = leftAndNullableOneToOneKeyBag[j];
                                                        var keyColumn = tableSchematic.Columns[j];
                                                        record.TypeAccessor[singularInstance, keyColumn.PropertyName] = keyValue;
                                                    }
                                                }
                                            }

                                            record.TypeAccessor[singularInstance, column.PropertyName] = value;
                                            ordinal++;
                                        }

                                        // set the property on the parent with the loaded object
                                        if (singularInstance != null)
                                        {
                                            record.DataBag = singularInstance;
                                            record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName] = singularInstance;
                                        }
                                    }
                                    break;
                                case ForeignKeyType.OneToMany:
                                    {
                                        // need a new list if there isnt one
                                        // check to see if the parent already has a list on it, if so load that list
                                        var oneToManyKeyBag = new List<object>();
                                        IList listInstance = null;
                                        object singularInstance = null;

                                        for (var i = 0; i < tableSchematic.Columns.Count; i++)
                                        {
                                            // keys are always orders to be read first
                                            var column = tableSchematic.Columns[i];
                                            var value = reader.Get(ordinal);

                                            if (column.IsKey)
                                            {
                                                if (value == null)
                                                {
                                                    singularInstance = null;
                                                    ordinal += tableSchematic.Columns.Count;
                                                    break;
                                                }
                                                else
                                                {
                                                    oneToManyKeyBag.Add(value);
                                                    ordinal++;
                                                    continue;
                                                }
                                            }

                                            if (column.IsFirstNonKey)
                                            {
                                                // check to see if item has already been loaded
                                                // This is where Activator.CreateInstance should go
                                                var currentCompositeKey = oneToManyKeyBag.Aggregate(string.Empty, (current, next) => $"{current}{next}");

                                                if (record.DataBags.Contains(currentCompositeKey))
                                                {
                                                    ordinal += (tableSchematic.Columns.Count - tableSchematic.KeyCount);
                                                    break;
                                                }
                                                else
                                                {
                                                    record.DataBags.Add(currentCompositeKey);
                                                    listInstance = (IList)record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName];

                                                    if (listInstance == null)
                                                    {
                                                        listInstance = (IList)Activator.CreateInstance(record.ActualType);
                                                    }

                                                    singularInstance = Activator.CreateInstance(record.Type);

                                                    // load keys into the object
                                                    for (var j = 0; j < oneToManyKeyBag.Count; j++)
                                                    {
                                                        var keyValue = oneToManyKeyBag[j];
                                                        var keyColumn = tableSchematic.Columns[j];
                                                        record.TypeAccessor[singularInstance, keyColumn.PropertyName] = keyValue;
                                                    }
                                                }
                                            }

                                            record.TypeAccessor[singularInstance, column.PropertyName] = value;
                                            ordinal++;
                                        }

                                        if (tableSchematic.HasKeysOnly)
                                        {
                                            // if nothing was loaded in the key bag,
                                            // then we need to break out of the switch
                                            // otherwise an empty object will be loaded
                                            if (oneToManyKeyBag.Count == 0)
                                            {
                                                break;
                                            }

                                            var currentCompositeKey = oneToManyKeyBag.Aggregate(string.Empty, (current, next) => $"{current}{next}");

                                            if (record.DataBags.Contains(currentCompositeKey))
                                            {
                                                // ordinal already changed from reading above
                                                break;
                                            }
                                            else
                                            {
                                                record.DataBags.Add(currentCompositeKey);
                                            }

                                            // if there are only keys, none were loaded from above
                                            // load them here
                                            // load keys into the object
                                            listInstance = (IList)record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName];

                                            if (listInstance == null)
                                            {
                                                listInstance = (IList)Activator.CreateInstance(record.ActualType);
                                            }

                                            singularInstance = Activator.CreateInstance(record.Type);

                                            // load keys into the object
                                            for (var j = 0; j < oneToManyKeyBag.Count; j++)
                                            {
                                                var keyValue = oneToManyKeyBag[j];
                                                var keyColumn = tableSchematic.Columns[j];
                                                record.TypeAccessor[singularInstance, keyColumn.PropertyName] = keyValue;
                                            }
                                        }

                                        if (singularInstance != null)
                                        {
                                            // no reflection needed to add
                                            listInstance.Add(singularInstance);

                                            record.DataBag = singularInstance;

                                            // so we need to set it?
                                            record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName] = listInstance;
                                        }
                                    }
                                    break;
                            }
                        }

                        // reset the index for the next row of reading
                        objectReader.Reset();
                        ordinal = 0;
                    }

                    return instance;
                }
            }
        }

        public List<T> LoadAll<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(sql.Query, connection))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                command.Parameters.AddRange(sql.Parameters.ToArray());

                using (var reader = command.ExecutePeekReader())
                {
                    var result = new List<T>();
                    while (reader.Read())
                    {
                        result.Add(default(T));
                    }
                    return result;
                }
            }
        }

        //public object Load(Type typeToLoad, List<ColumnSchema> columns)
        //{
        //    //for(var i = )
        //}
    }
}
