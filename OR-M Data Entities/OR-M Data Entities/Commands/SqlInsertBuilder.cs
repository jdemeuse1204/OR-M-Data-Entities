/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Commands
{
    public sealed class SqlInsertBuilder : SqlFromTable, ISqlBuilder
    {
        #region Properties
        private List<InsertItem> _insertItems { get; set; }
        private bool _isTryInsert { get; set; }
        private string _tryInsertStatement { get; set; }
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

            var fields = string.Empty;
            var values = string.Empty;
            var declare = string.Empty;
            const string select = "SELECT TOP 1 {0}{1}";
            const string from = " FROM [{0}] WHERE {1}";
            var keys = string.Empty;
            var selectColumns = string.Empty;
            var where = string.Empty;
            var set = string.Empty;
            var hasTimeStamp = _insertItems.Any(w => w.DbTranslationType == SqlDbType.Timestamp);

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
                                selectColumns += item.PropertyName == item.DatabaseColumnName
                                    ? string.Format("[{0}],", item.DatabaseColumnName)
                                    : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                continue;
                            }

                            //Value is simply inserted
                            var data = GetNextParameter();
                            fields += string.Format("[{0}],", item.DatabaseColumnName);
                            values += string.Format("{0},", data);

                            if (item.TranslateDataType)
                            {
                                AddParameter(item.Value, item.DbTranslationType);
                            }
                            else
                            {
                                AddParameter(item.Value);
                            }

                            if (!item.IsPrimaryKey) continue;

                            where += string.Format("[{0}] = {1} ", item.DatabaseColumnName, data);
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
                                set += string.Format("SET {0} = (Select ISNULL(MAX({1}),0) + 1 From {2});", key,
                                    item.DatabaseColumnName, TableName);
                            }

                            fields += string.Format("[{0}],", item.DatabaseColumnName);
                            values += string.Format("{0},", key);
                            declare += string.Format("{0} as {1},", key, item.SqlDataTypeString);
                            keys += string.Format("{0} as [{1}],", key, item.PropertyName);
                            selectColumns += item.PropertyName == item.DatabaseColumnName
                                ? string.Format("[{0}],", item.DatabaseColumnName)
                                : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);

                            if (!item.IsPrimaryKey) continue;

                            where += string.Format("[{0}] = {1} ", item.DatabaseColumnName, key);

                            // Do not add as a parameter because the parameter will be converted to a string to
                            // be inserted in to the database
                        }
                        break;
                    case DbGenerationOption.IdentitySpecification:
                        {
                            selectColumns += item.PropertyName == item.DatabaseColumnName
                                ? string.Format("[{0}],", item.DatabaseColumnName)
                                : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                            keys += string.Format("@@IDENTITY as [{0}],", item.PropertyName);

                            if (!item.IsPrimaryKey) continue;

                            where += string.Format("[{0}] = @@IDENTITY ", item.DatabaseColumnName);
                        }
                        break;
                }
            }

            var tryInsertSplit = string.IsNullOrWhiteSpace(_tryInsertStatement)
                ? new[] { "", "" }
                : _tryInsertStatement.Split('#');
            var tryInsertSplitOne = tryInsertSplit[0];
            var tryinsertSplitTwo = tryInsertSplit[1];
            var sql = string.Format("{0} {1} {2} INSERT INTO [{3}] ({4}) VALUES ({5});{6}{7}",
                string.IsNullOrWhiteSpace(declare) ? string.Empty : string.Format("DECLARE {0}", declare.TrimEnd(',')),
                set,
                tryInsertSplitOne,
                TableName.TrimStart('[').TrimEnd(']'),
                fields.TrimEnd(','),
                values.TrimEnd(','),
                selectColumns.Any()
                    ? hasTimeStamp
                        ? string.Format(select, selectColumns.TrimEnd(','),
                            string.Format(from, TableName.TrimStart('[').TrimEnd(']'), where))
                        : string.Format(select, keys.TrimEnd(','), string.Empty)
                    : string.Empty,
                // we want to select everything back from the database in case the model relies on db generation for some fields.
                // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                // where the column is not the PK
                tryinsertSplitTwo);

            var cmd = new SqlCommand(sql, connection);

            InsertParameters(cmd);

            return cmd;
        }

        public void AddInsert(PropertyInfo property, object entity)
        {
            _insertItems.Add(new InsertItem(property, entity));
        }

        public void MakeTryInsert(IEnumerable<PropertyInfo> primaryKeys, object entitty)
        {
            var where = string.Empty;

            foreach (var propertyInfo in primaryKeys)
            {
                var parameterKey = GetNextParameter();
                var parameterValue = propertyInfo.GetValue(entitty);
                var columnName = propertyInfo.GetColumnName();

                AddParameter(parameterKey, parameterValue);

                where += string.Format("{0}{1} = {2}", (string.IsNullOrWhiteSpace(where) ? string.Empty : " AND "),
                    columnName, parameterKey);
            }

            _tryInsertStatement =
                string.Format("IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{0}] WHERE {1}))) BEGIN # END",
                    entitty.GetTableNameWithLinkedServer().TrimStart('[').TrimEnd(']'), where);
            _isTryInsert = true;
        }
        #endregion
    }
}
