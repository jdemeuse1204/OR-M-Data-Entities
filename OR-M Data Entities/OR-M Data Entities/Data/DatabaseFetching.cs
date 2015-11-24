/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Data;
using System.Linq;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Query.StatementParts;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

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
        #region Non Transaction Insert Builders
        protected class SqlNonTransactionInsertBuilder : SqlInsertBuilder, ISqlBuilder
        {
            public SqlNonTransactionInsertBuilder(object entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionInsertPackage(this);

                package.CreatePackage();

                return package;
            }
        }

        protected class SqlNonTransactionTryInsertBuilder : SqlInsertBuilder, ISqlBuilder
        {
            public SqlNonTransactionTryInsertBuilder(object entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionTryInsertPackage(this);

                package.CreatePackage();

                return package;
            }
        }

        protected class SqlNonTransactionTryInsertUpdateBuilder : SqlInsertBuilder, ISqlBuilder
        {
            public SqlNonTransactionTryInsertUpdateBuilder(object entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionTryInsertUpdatePackage(this);

                package.CreatePackage();

                return package;
            }
        }
        #endregion

        #region Non Transaction Packages
        protected class SqlNonTransactionInsertPackage : SqlSecureExecutable, ISqlPackage
        {
            #region Constructor
            public SqlNonTransactionInsertPackage(SqlInsertBuilder builder)
            {
                Fields = string.Empty;
                Values = string.Empty;
                Declare = string.Empty;
                Keys = string.Empty;
                SelectColumns = string.Empty;
                Where = string.Empty;
                Set = string.Empty;
                FormattedTableName = builder.SqlFormattedTableName();
                InsertItems = builder.GetInsertItems();
            }
            #endregion

            #region Properties

            protected readonly List<InsertItem> InsertItems;

            protected string FormattedTableName { get; set; }

            protected string Fields { get; set; }

            protected string Values { get; set; }

            protected string Declare { get; set; }

            protected readonly string Select = "SELECT TOP 1 {0}{1}";

            protected readonly string From = " FROM [{0}] WHERE {1}";

            protected string Keys { get; set; }

            protected string SelectColumns { get; set; }

            protected string Where { get; set; }

            protected string Set { get; set; }

            protected string Update { get; set; }

            protected bool DoSelectFromForKeyContainer { get; set; }
            #endregion

            #region Methods
            public virtual void CreatePackage()
            {
                if (InsertItems.Count == 0) throw new QueryNotValidException("INSERT statement needs VALUES");

                DoSelectFromForKeyContainer = InsertItems.Any(w => w.DbTranslationType == SqlDbType.Timestamp) ||
                                               InsertItems.Any(w => w.Generation == DbGenerationOption.DbDefault);

                for (var i = 0; i < InsertItems.Count; i++)
                {
                    var item = InsertItems[i];

                    //  NOTE:  Alias any Identity specification and generate columns with their property
                    // name not db column name so we can set the property when we return the values back.
                    switch (item.Generation)
                    {
                        case DbGenerationOption.None:
                            {
                                if (item.DbTranslationType == SqlDbType.Timestamp)
                                {
                                    SelectColumns += item.PropertyName == item.DatabaseColumnName
                                        ? string.Format("[{0}],", item.DatabaseColumnName)
                                        : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                    continue;
                                }

                                //Value is simply inserted

                                var data = item.TranslateDataType
                                    ? AddParameter(item.DatabaseColumnName, item.Value, item.DbTranslationType)
                                    : AddParameter(item.DatabaseColumnName, item.Value);

                                Fields += string.Format("[{0}],", item.DatabaseColumnName);
                                Values += string.Format("{0},", data);

                                if (!item.IsPrimaryKey)
                                {
                                    // should never update the pk
                                    Update += string.Format("[{0}] = {1},", item.DatabaseColumnName, data);
                                    continue;
                                }

                                Where +=
                                    string.Format(
                                        string.IsNullOrEmpty(Where) ? "[{0}] = {1} " : "AND [{0}] = {1} ",
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
                                    Set += string.Format("SET {0} = NEWID();", key);
                                }
                                else
                                {
                                    // INTEGER
                                    Set += string.Format("SET {0} = (Select ISNULL(MAX([{1}]),0) + 1 From [{2}]);", key,
                                        item.DatabaseColumnName, FormattedTableName);
                                }

                                Fields += string.Format("[{0}],", item.DatabaseColumnName);
                                Values += string.Format("{0},", key);
                                Declare += string.Format("{0} as {1},", key, item.SqlDataTypeString);
                                Keys += string.Format("{0} as [{1}],", key, item.PropertyName);
                                SelectColumns += item.PropertyName == item.DatabaseColumnName
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

                                    Update += string.Format("[{0}] = {1},", item.DatabaseColumnName, data);
                                    continue;
                                }

                                Where +=
                                    string.Format(
                                        string.IsNullOrEmpty(Where) ? "[{0}] = {1} " : "AND [{0}] = {1} ",
                                        item.DatabaseColumnName, key);

                                // Do not add as a parameter because the parameter will be converted to a string to
                                // be inserted in to the database
                            }
                            break;
                        case DbGenerationOption.IdentitySpecification:
                            {
                                SelectColumns += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                Keys += string.Format("@@IDENTITY as [{0}],", item.PropertyName);

                                if (!item.IsPrimaryKey) continue;

                                Where +=
                                    string.Format(
                                        string.IsNullOrEmpty(Where) ? "[{0}] = @@IDENTITY " : "AND [{0}] = @@IDENTITY ",
                                        item.DatabaseColumnName, item.DatabaseColumnName);
                            }
                            break;
                        case DbGenerationOption.DbDefault:
                            {
                                SelectColumns += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                Keys += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                            }
                            break;
                    }
                }
            }

            public virtual string GetSql()
            {
                return string.Format("{0} {1} INSERT INTO [{2}] ({3}) VALUES ({4});{5}",

                    string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("DECLARE {0}", Declare.TrimEnd(',')),

                    Set,

                    FormattedTableName,

                    Fields.TrimEnd(','),

                    Values.TrimEnd(','),

                    SelectColumns.Any()
                        ? DoSelectFromForKeyContainer
                            ? string.Format(Select, SelectColumns.TrimEnd(','),
                                string.Format(From, FormattedTableName, Where))
                            : string.Format(Select, Keys.TrimEnd(','), string.Empty)
                        : string.Empty

                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );
            }
            #endregion
        }

        protected class SqlNonTransactionTryInsertPackage : SqlNonTransactionInsertPackage
        {
            public SqlNonTransactionTryInsertPackage(SqlInsertBuilder builder)
                : base(builder)
            {
            }

            public override string GetSql()
            {
                return string.Format(@"
{0}
{1}
IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{2}] WHERE {6}))) 
    BEGIN
        INSERT INTO [{2}] ({3}) VALUES ({4});{5}
    END

",
                    string.IsNullOrWhiteSpace(Declare)
                        ? string.Empty
                        : string.Format("DECLARE {0}", Declare.TrimEnd(',')),

                    Set,

                    FormattedTableName,

                    Fields.TrimEnd(','),

                    Values.TrimEnd(','),

                    SelectColumns.Any()
                        ? DoSelectFromForKeyContainer
                            ? string.Format(Select, SelectColumns.TrimEnd(','),
                                string.Format(From, FormattedTableName, Where))
                            : string.Format(Select, Keys.TrimEnd(','), string.Empty)
                        : string.Empty,
                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    Where);
            }
        }

        protected class SqlNonTransactionTryInsertUpdatePackage : SqlNonTransactionInsertPackage
        {
            public SqlNonTransactionTryInsertUpdatePackage(SqlInsertBuilder builder)
                : base(builder)
            {
            }

            public override string GetSql()
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
                    string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("DECLARE {0}", Declare.TrimEnd(',')),

                    Set,

                    FormattedTableName,

                    Fields.TrimEnd(','),

                    Values.TrimEnd(','),

                    SelectColumns.Any()
                        ? DoSelectFromForKeyContainer
                            ? string.Format(Select, SelectColumns.TrimEnd(','),
                                string.Format(From, FormattedTableName, Where))
                            : string.Format(Select, Keys.TrimEnd(','), string.Empty)
                        : string.Empty,
                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    Where,

                    Update.TrimEnd(','));
            }
        }
        #endregion

        #region Transaction Insert Builders

        #endregion

        #region Transaction Packages

        #endregion
    }
}
