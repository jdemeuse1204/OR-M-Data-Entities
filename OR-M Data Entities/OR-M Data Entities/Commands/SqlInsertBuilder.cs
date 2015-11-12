/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Commands
{
    [Obsolete("Expression Query should be used instead.  If using please contact me and I will leave these in.")]
    public class SqlInsertBuilder : SqlFromTable, ISqlBuilder
    {
        #region Properties
        private List<InsertItem> _insertItems { get; set; }
        #endregion

        #region Constructor
        public SqlInsertBuilder()
        {
            _insertItems = new List<InsertItem>();
        }
        #endregion

        #region Methods
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
                                    item.DatabaseColumnName, TableName.FormatTableName());
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

            var sql = BuildSql(package);

            var cmd = new SqlCommand(sql, connection);

            InsertParameters(cmd);

            return cmd;
        }

        protected virtual string BuildSql(SqlInsertPackage package)
        {
            return string.Format("{0} {1} INSERT INTO [{2}] ({3}) VALUES ({4});{5}",
                string.IsNullOrWhiteSpace(package.Declare) ? string.Empty : string.Format("DECLARE {0}", package.Declare.TrimEnd(',')),
                package.Set,
                TableName.FormatTableName(),
                package.Fields.TrimEnd(','),
                package.Values.TrimEnd(','),
                package.SelectColumns.Any()
                    ? package.DoSelectFromForKeyContainer
                        ? string.Format(package.Select, package.SelectColumns.TrimEnd(','),
                            string.Format(package.From, TableName.FormatTableName(), package.Where))
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

    static class SqlInsertBuilderExtensions
    {
        public static string FormatTableName(this string s)
        {
            return s.TrimStart('[').TrimEnd(']');
        }
    }
}
