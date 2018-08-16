using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Expressions.Query;
using OR_M_Data_Entities.Lite.Extensions;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
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
                var index = 0;
                var ordinal = 0;
                var instance = new T();
                object lastInstance = null;

                while (reader.Read())
                {
                    while (objectReader.Read())
                    {
                        var record = objectReader.GetRecord();
                        var tableSchematic = tableSchemas[record.Type];

                        if (index == 0)
                        {
                            // base object
                            foreach (var column in tableSchematic.Columns)
                            {
                                record.TypeAccessor[instance, column.PropertyName] = reader.Get(ordinal);
                                lastInstance = instance;

                                ordinal++;
                            }

                            index++;
                            continue;
                        }

                        // child object
                        // is the foreign key a one to many?  
                        // If so the child object needs to be a list
                        if (record.ForeignKeyType == ForeignKeyType.OneToMany)
                        {
                            // need a new list if there isnt one
                            // check to see if the parent already has a list on it, if so load that list

                            continue;
                        }

                        // check the PK to see if the row exists
                        if (record.ForeignKeyType == ForeignKeyType.NullableOneToOne || record.ForeignKeyType == ForeignKeyType.LeftOneToOne)
                        {
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
                                record.ParentObjectRecord.TypeAccessor[lastInstance, record.FromPropertyName] = singularInstance;
                            }
                        }

                        if (record.ForeignKeyType == ForeignKeyType.OneToOne)
                        {
                            var singularInstance = Activator.CreateInstance(record.Type);

                            foreach (var column in tableSchematic.Columns)
                            {
                                // keys are always orders to be read first
                                record.TypeAccessor[singularInstance, column.PropertyName] = reader.Get(ordinal);
                                ordinal++;
                            }

                            // set the property on the parent with the loaded object
                            record.ParentObjectRecord.TypeAccessor[lastInstance, record.FromPropertyName] = singularInstance;
                        }

                        index++;
                    }
                    // read ordinals
                    // load main object
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
