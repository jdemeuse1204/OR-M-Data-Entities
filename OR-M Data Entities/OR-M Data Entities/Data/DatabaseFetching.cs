/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Query.StatementParts;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Extensions;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class do not require a READ before data can be retreived
    /// </summary>
    public abstract partial class DatabaseFetching : DatabaseReading
    {
        #region Constructor
        protected DatabaseFetching(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Identity
        /// <summary>
        /// Used with insert statements only, gets the value if the id's that were inserted
        /// </summary>
        /// <returns></returns>
        protected KeyContainer SelectIdentity()
        {
            if (Reader.HasRows)
            {
                Reader.Read();
                var keyContainer = new KeyContainer();
                var rec = (IDataRecord)Reader;

                for (var i = 0; i < rec.FieldCount; i++)
                {
                    keyContainer.Add(rec.GetName(i), rec.GetValue(i));
                }

                Reader.Close();
                Reader.Dispose();

                return keyContainer;
            }

            Reader.Close();
            Reader.Dispose();

            return new KeyContainer();
        }
        #endregion
    }

    /// <summary>
    /// Created partial class to split off query builders
    /// </summary>
    public partial class DatabaseFetching
    {
        #region Query Builders
        protected class SqlInsertBuilder : SqlFromTable, ISqlBuilder
        {
            #region Properties
            private readonly List<InsertItem> _insertItems;

            private bool _useTransaction { get; set; }
            #endregion

            #region Constructor
            public SqlInsertBuilder(ConfigurationOptions configuration, bool useTransaction)
                : base (configuration)
            {
                _insertItems = new List<InsertItem>();
                _useTransaction = useTransaction;
            }
            #endregion

            #region Methods
            public SqlTransactionStatement GetTransactionSql(IEnumerable<SqlSecureQueryParameter> parameters)
            {
                // concurrency check

                if (string.IsNullOrWhiteSpace(TableName)) throw new QueryNotValidException("INSERT statement needs Table Name");

                if (_insertItems.Count == 0) throw new QueryNotValidException("INSERT statement needs VALUES");

                var fields = string.Empty;
                var values = string.Empty;
                var declare = string.Empty;
                var set = string.Empty;
                var output = "OUTPUT ";

                //  NOTE:  Alias any Identity specification and generate columns with their property
                // name not db column name so we can set the property when we return the values back.

                for (var i = 0; i < _insertItems.Count; i++)
                {
                    var item = _insertItems[i];

                    switch (item.Generation)
                    {
                        case DbGenerationOption.None:
                            {
                                //Value is simply inserted
                                var data = item.TranslateDataType
                                    ? AddParameter(item.DatabaseColumnName, item.Value, item.DbTranslationType)
                                    : AddParameter(item.DatabaseColumnName, item.Value);

                                fields += string.Format("[{0}],", item.DatabaseColumnName);
                                values += string.Format("{0},", data);
                                output += string.Format("INSERTED.{0} as '{1}.{2}'", item.DatabaseColumnName,
                                    TableNameOnly, item.PropertyName);
                            }
                            break;
                        case DbGenerationOption.Generate:
                            {
                                // Value is generated from the database
                                var key = string.Format("@{0}", item.PropertyName);

                                // make our set statement
                                if (item.SqlDataTypeString.ToUpper() == "UNIQUEIDENTIFIER")
                                {
                                    // GUID
                                    set += string.Format("SET {0} = NEWID();", key);
                                }
                                else
                                {
                                    // INTEGER
                                    set += string.Format("SET {0} = (Select ISNULL(MAX([{1}]),0) + 1 From [{2}]);", key,
                                        item.DatabaseColumnName, FormattedTableName);
                                }

                                fields += string.Format("[{0}],", item.DatabaseColumnName);
                                values += string.Format("{0},", key);
                                declare += string.Format("{0} as {1},", key, item.SqlDataTypeString);
                                output += string.Format("INSERTED.{0} as '{1}.{2}'", item.DatabaseColumnName,
                                    TableNameOnly, item.PropertyName);

                                // create output
                            }
                            break;
                        case DbGenerationOption.IdentitySpecification:
                            {
                                // create output
                            }
                            break;
                        case DbGenerationOption.DbDefault:
                            {
                                // create output
                            }
                            break;
                    }
                }

                return new SqlTransactionStatement("","","");
            }

            public SqlCommand Build(SqlConnection connection)
            {
                if (string.IsNullOrWhiteSpace(TableName)) throw new QueryNotValidException("INSERT statement needs Table Name");

                if (_insertItems.Count == 0) throw new QueryNotValidException("INSERT statement needs VALUES");

                var package = new SqlInsertPackage
                {
                    DoSelectFromForKeyContainer = _insertItems.Any(w => w.DbTranslationType == SqlDbType.Timestamp) ||
                                                  _insertItems.Any(w => w.Generation == DbGenerationOption.DbDefault)
                };

                //  NOTE:  Alias any Identity specification and generate columns with their property
                // name not db column name so we can set the property when we return the values back.

                for (var i = 0; i < _insertItems.Count; i++)
                {
                    var item = _insertItems[i];

                    switch (item.Generation)
                    {
                        case DbGenerationOption.None:
                            {
                                if (item.DbTranslationType == SqlDbType.Timestamp)
                                {
                                    package.SelectColumns += item.PropertyName == item.DatabaseColumnName
                                        ? string.Format("[{0}],", item.DatabaseColumnName)
                                        : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                    continue;
                                }

                                //Value is simply inserted

                                var data = item.TranslateDataType
                                    ? AddParameter(item.DatabaseColumnName, item.Value, item.DbTranslationType)
                                    : AddParameter(item.DatabaseColumnName, item.Value);

                                package.Fields += string.Format("[{0}],", item.DatabaseColumnName);
                                package.Values += string.Format("{0},", data);

                                if (!item.IsPrimaryKey)
                                {
                                    // should never update the pk
                                    package.Update += string.Format("[{0}] = {1},", item.DatabaseColumnName, data);
                                    continue;
                                }

                                package.Where +=
                                    string.Format(
                                        string.IsNullOrEmpty(package.Where) ? "[{0}] = {1} " : "AND [{0}] = {1} ",
                                        item.DatabaseColumnName, data);
                            }
                            break;
                        case DbGenerationOption.Generate:
                            {
                                // Value is generated from the database
                                var key = string.Format("@{0}", item.PropertyName);

                                // make our set statement
                                if (item.SqlDataTypeString.ToUpper() == "UNIQUEIDENTIFIER")
                                {
                                    // GUID
                                    package.Set += string.Format("SET {0} = NEWID();", key);
                                }
                                else
                                {
                                    // INTEGER
                                    package.Set += string.Format("SET {0} = (Select ISNULL(MAX([{1}]),0) + 1 From [{2}]);", key,
                                        item.DatabaseColumnName, FormattedTableName);
                                }

                                package.Fields += string.Format("[{0}],", item.DatabaseColumnName);
                                package.Values += string.Format("{0},", key);
                                package.Declare += string.Format("{0} as {1},", key, item.SqlDataTypeString);
                                package.Keys += string.Format("{0} as [{1}],", key, item.PropertyName);
                                package.SelectColumns += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);

                                if (!item.IsPrimaryKey)
                                {
                                    var data = FindParameterKey(item.DatabaseColumnName);

                                    if (string.IsNullOrEmpty(data))
                                    {
                                        data = item.TranslateDataType
                                        ? AddParameter(item.DatabaseColumnName, item.Value, item.DbTranslationType)
                                        : AddParameter(item.DatabaseColumnName, item.Value);
                                    }

                                    package.Update += string.Format("[{0}] = {1},", item.DatabaseColumnName, data);
                                    continue;
                                }

                                package.Where +=
                                    string.Format(
                                        string.IsNullOrEmpty(package.Where) ? "[{0}] = {1} " : "AND [{0}] = {1} ",
                                        item.DatabaseColumnName, key);

                                // Do not add as a parameter because the parameter will be converted to a string to
                                // be inserted in to the database
                            }
                            break;
                        case DbGenerationOption.IdentitySpecification:
                            {
                                package.SelectColumns += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                package.Keys += string.Format("@@IDENTITY as [{0}],", item.PropertyName);

                                if (!item.IsPrimaryKey) continue;

                                package.Where +=
                                    string.Format(
                                        string.IsNullOrEmpty(package.Where) ? "[{0}] = @@IDENTITY " : "AND [{0}] = @@IDENTITY ",
                                        item.DatabaseColumnName, item.DatabaseColumnName);
                            }
                            break;
                        case DbGenerationOption.DbDefault:
                            {
                                package.SelectColumns += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                package.Keys += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                            }
                            break;
                    }
                }

                var nonTransactionSql = BuildSql(package);
                var cmd = new SqlCommand(nonTransactionSql, connection);

                InsertParameters(cmd);

                return cmd;
            }

            protected virtual string BuildSql(SqlInsertPackage package)
            {
                return string.Format("{0} {1} INSERT INTO [{2}] ({3}) VALUES ({4});{5}",
                    string.IsNullOrWhiteSpace(package.Declare) ? string.Empty : string.Format("DECLARE {0}", package.Declare.TrimEnd(',')),
                    package.Set,
                    FormattedTableName,
                    package.Fields.TrimEnd(','),
                    package.Values.TrimEnd(','),
                    package.SelectColumns.Any()
                        ? package.DoSelectFromForKeyContainer
                            ? string.Format(package.Select, package.SelectColumns.TrimEnd(','),
                                string.Format(package.From, FormattedTableName, package.Where))
                            : string.Format(package.Select, package.Keys.TrimEnd(','), string.Empty)
                        : string.Empty
                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );
            }

            public void AddInsert(PropertyInfo property, object entity)
            {
                _insertItems.Add(new InsertItem(property, entity));
            }

            #endregion

            #region Helpers
            protected class SqlInsertPackage
            {
                public SqlInsertPackage()
                {
                    Fields = string.Empty;
                    Values = string.Empty;
                    Declare = string.Empty;
                    Keys = string.Empty;
                    SelectColumns = string.Empty;
                    Where = string.Empty;
                    Set = string.Empty;
                }

                public string Fields { get; set; }

                public string Values { get; set; }

                public string Declare { get; set; }

                public readonly string Select = "SELECT TOP 1 {0}{1}";

                public readonly string From = " FROM [{0}] WHERE {1}";

                public string Keys { get; set; }

                public string SelectColumns { get; set; }

                public string Where { get; set; }

                public string Set { get; set; }

                public string Update { get; set; }

                public bool DoSelectFromForKeyContainer { get; set; }
            }
            #endregion
        }

        protected class SqlTryInsertBuilder : SqlInsertBuilder
        {
            public SqlTryInsertBuilder(ConfigurationOptions configuration, bool useTransaction)
                : base(configuration, useTransaction)
            {
            }

            protected override string BuildSql(SqlInsertPackage package)
            {
                return string.Format(@"
{0}
{1}
IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{2}] WHERE {6}))) 
    BEGIN
        INSERT INTO [{2}] ({3}) VALUES ({4});{5}
    END

",
                    string.IsNullOrWhiteSpace(package.Declare) ? string.Empty : string.Format("DECLARE {0}", package.Declare.TrimEnd(',')),

                    package.Set,

                    FormattedTableName,

                    package.Fields.TrimEnd(','),

                    package.Values.TrimEnd(','),

                    package.SelectColumns.Any()
                        ? package.DoSelectFromForKeyContainer
                            ? string.Format(package.Select, package.SelectColumns.TrimEnd(','),
                                string.Format(package.From, FormattedTableName, package.Where))
                            : string.Format(package.Select, package.Keys.TrimEnd(','), string.Empty)
                        : string.Empty,
                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    package.Where);
            }
        }

        protected class SqlTryInsertUpdateBuilder : SqlInsertBuilder
        {
            public SqlTryInsertUpdateBuilder(ConfigurationOptions configuration, bool useTransaction) 
                : base(configuration, useTransaction)
            {
            }

            protected override string BuildSql(SqlInsertPackage package)
            {
                return string.Format(@"
{0}
{1}
IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{2}] WHERE {6}))) 
    BEGIN
        INSERT INTO [{2}] ({3}) VALUES ({4});{5}
    END
ELSE
    BEGIN
        UPDATE [{2}] SET {7} WHERE {6}
    END
",
                    string.IsNullOrWhiteSpace(package.Declare) ? string.Empty : string.Format("DECLARE {0}", package.Declare.TrimEnd(',')),

                    package.Set,

                    FormattedTableName,

                    package.Fields.TrimEnd(','),

                    package.Values.TrimEnd(','),

                    package.SelectColumns.Any()
                        ? package.DoSelectFromForKeyContainer
                            ? string.Format(package.Select, package.SelectColumns.TrimEnd(','),
                                string.Format(package.From, FormattedTableName, package.Where))
                            : string.Format(package.Select, package.Keys.TrimEnd(','), string.Empty)
                        : string.Empty,
                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    package.Where,

                    package.Update.TrimEnd(','));
            }
        }

        protected class SqlDeleteBuilder : SqlValidation, ISqlBuilder
        {
            #region Properties
            private string _delete { get; set; }

            private bool _useTransaction { get; set; }
            #endregion

            #region Constructor
            public SqlDeleteBuilder(ConfigurationOptions configuration, bool useTransaction)
                : base(configuration)
            {
                _delete = string.Empty;
                _useTransaction = useTransaction;
            }
            #endregion

            #region Methods
            public SqlTransactionStatement GetTransactionSql(IEnumerable<SqlSecureQueryParameter> parameters)
            {
                throw new System.NotImplementedException();
            }

            public SqlCommand Build(SqlConnection connection)
            {
                if (string.IsNullOrWhiteSpace(TableName))
                {
                    throw new QueryNotValidException("Table statement missing");
                }

                _delete = string.Format(" DELETE FROM [{0}] ", FormattedTableName);

                var nonTransactionSql = _delete + GetValidation() + ";Select @@ROWCOUNT as 'int';";
                var cmd = new SqlCommand(nonTransactionSql, connection);

                InsertParameters(cmd);

                return cmd;
            }
            #endregion
        }

        protected class SqlUpdateBuilder : SqlValidation, ISqlBuilder
        {
            #region Properties
            private string _set { get; set; }
            private bool _useTransaction { get; set; }
            #endregion

            #region Constructor
            public SqlUpdateBuilder(ConfigurationOptions configuration, bool useTransaction)
                : base (configuration)
            {
                _set = string.Empty;
                _useTransaction = useTransaction;
            }
            #endregion

            #region Methods
            public SqlTransactionStatement GetTransactionSql(IEnumerable<SqlSecureQueryParameter> parameters)
            {
                throw new System.NotImplementedException();
            }

            public SqlCommand Build(SqlConnection connection)
            {
                if (string.IsNullOrWhiteSpace(TableName))
                {
                    throw new QueryNotValidException("UPDATE table missing");
                }

                if (string.IsNullOrWhiteSpace(_set))
                {
                    throw new QueryNotValidException("UPDATE SET values missing");
                }

                var nonTransactionSql = string.Format("UPDATE [{0}] SET {1} {2}", FormattedTableName, _set.TrimEnd(','), GetValidation());
                var cmd = new SqlCommand(nonTransactionSql, connection);

                InsertParameters(cmd);

                return cmd;
            }

            public void AddUpdate(PropertyInfo property, object entity)
            {
                // check if its a timestamp, we need to skip the update

                var datatype = property.GetCustomAttribute<DbTypeAttribute>();

                // never update a timestamp
                if (datatype != null && datatype.Type == SqlDbType.Timestamp) return;

                //string fieldName, object value
                var value = property.GetValue(entity);
                var fieldName = property.GetColumnName();

                // check for sql data translation, used mostly for datetime2 inserts and updates
                var translation = property.GetCustomAttribute<DbTypeAttribute>();

                if (translation != null)
                {
                    var key = AddParameter(property.GetColumnName(), value, translation.Type);

                    _set += string.Format("[{0}] = {1},", fieldName, key);
                }
                else
                {
                    var key = AddParameter(property.GetColumnName(), value);

                    _set += string.Format("[{0}] = {1},", fieldName, key);
                }
            }

            public void AddUpdate(string column, object newValue)
            {
                //string fieldName, object value
                var data = AddParameter(column, newValue);
                _set += string.Format("[{0}] = {1},", column, data);
            }
            #endregion
        }
        #endregion
    }
}
