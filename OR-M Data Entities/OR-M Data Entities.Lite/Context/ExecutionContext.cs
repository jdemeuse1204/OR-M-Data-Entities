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

        private T Load<T>(SqlQuery query, Func<IPeekDataReader, T> load) where T : class, new()
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(query.Query, connection))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                command.Parameters.AddRange(query.Parameters.ToArray());

                using (var reader = command.ExecutePeekReader())
                {
                    return load(reader);
                }
            }
        }

        public T LoadOne<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            return Load(sql, (reader) =>
            {
                return default(T);
            });
        }

        public T LoadOneDefault<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            return Load(sql, (reader) =>
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

                        if (record.ForeignKeyType == ForeignKeyType.None)
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
                            continue;
                        }

                        // child object
                        // is the foreign key a one to many?  
                        // If so the child object needs to be a list
                        if (record.ForeignKeyType == ForeignKeyType.OneToMany)
                        {
                            // need a new list if there isnt one
                            // check to see if the parent already has a list on it, if so load that list
                            IList listInstance = (IList)record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName];
                            var oneToManyKeyBag = new List<object>();

                            if (listInstance == null)
                            {
                                listInstance = (IList)Activator.CreateInstance(record.ActualType);
                            }

                            // load one record
                            // need to make sure the item isnt already in the list later
                            var singularInstance = Activator.CreateInstance(record.Type);

                            foreach (var column in tableSchematic.Columns)
                            {
                                // keys are always orders to be read first
                                var value = reader.Get(ordinal);

                                if (column.IsKey)
                                {
                                    if (value == null)
                                    {
                                        singularInstance = null;
                                        ordinal += tableSchematic.Columns.Count();
                                        break;
                                    }
                                }
                                else
                                {

                                }

                                record.TypeAccessor[singularInstance, column.PropertyName] = value;
                                ordinal++;
                            }

                            // no reflection needed to add
                            listInstance.Add(singularInstance);

                            record.DataBag = singularInstance;

                            // so we need to set it?
                            record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName] = listInstance;

                            continue;
                        }

                        // check the PK to see if the row exists
                        if (record.ForeignKeyType == ForeignKeyType.NullableOneToOne || record.ForeignKeyType == ForeignKeyType.LeftOneToOne)
                        {
                            if (record.ParentObjectRecord.DataBag != null && record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName] != null)
                            {
                                ordinal += tableSchematic.Columns.Count();
                                continue;
                            }

                            var singularInstance = Activator.CreateInstance(record.Type);

                            foreach (var column in tableSchematic.Columns)
                            {
                                // keys are always orders to be read first
                                var value = reader.Get(ordinal);

                                if (column.IsKey && value == null)
                                {
                                    singularInstance = null;
                                    ordinal += tableSchematic.Columns.Count();
                                    break;
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
                            continue;
                        }

                        if (record.ForeignKeyType == ForeignKeyType.OneToOne)
                        {
                            if (record.ParentObjectRecord.DataBag != null && record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName] != null)
                            {
                                ordinal += tableSchematic.Columns.Count();
                                continue;
                            }

                            var singularInstance = Activator.CreateInstance(record.Type);

                            foreach (var column in tableSchematic.Columns)
                            {
                                // keys are always orders to be read first
                                record.TypeAccessor[singularInstance, column.PropertyName] = reader.Get(ordinal);
                                ordinal++;
                            }

                            record.DataBag = singularInstance;

                            // set the property on the parent with the loaded object
                            record.ParentObjectRecord.TypeAccessor[record.ParentObjectRecord.DataBag, record.FromPropertyName] = singularInstance;
                        }
                    }

                    // reset the index for the next row of reading
                    objectReader.Reset();
                    ordinal = 0;
                    wasFirstRowRead = false;
                }

                return default(T);
            });
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
