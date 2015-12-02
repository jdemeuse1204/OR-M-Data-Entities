/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Reflection;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Definition
{
    /// <summary>
    /// Is the common class for getting a tables information
    /// </summary>
    public class Table
    {
        #region Constructor
        public Table(object entity)
            : this(entity.GetType())
        {

        }

        public Table(Type type)
        {
            TableType = type;
            ClassName = type.Name;
            _linkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>();
            _tableAttribute = type.GetCustomAttribute<TableAttribute>();
            _schemaAttribute = type.GetCustomAttribute<SchemaAttribute>();
            _readOnlyAttribute = type.GetCustomAttribute<ReadOnlyAttribute>();

            if (_linkedServerAttribute != null && _schemaAttribute != null)
            {
                throw new Exception(
                    string.Format(
                        "Class {0} cannot have LinkedServerAttribute and SchemaAttribute, use one or the other",
                        ClassName));
            }

            TableNameOnly = _tableAttribute == null ? type.Name : _tableAttribute.Name;
        }

        #endregion

        #region Properties And Fields

        public readonly Type TableType;
        private readonly LinkedServerAttribute _linkedServerAttribute;
        private readonly TableAttribute _tableAttribute;
        private readonly SchemaAttribute _schemaAttribute;
        private readonly ReadOnlyAttribute _readOnlyAttribute;
        private readonly LookupTableAttribute _lookupTableAttribute;

        public bool IsReadOnly
        {
            get { return _readOnlyAttribute != null; }
        }

        public bool IsLookupTable
        {
            get { return _lookupTableAttribute != null; }
        }

        public ReadOnlySaveOption? GetReadOnlySaveOption()
        {
            return _readOnlyAttribute == null ? null : (ReadOnlySaveOption?) _readOnlyAttribute.ReadOnlySaveOption;
        }

        public string ClassName { get; private set; }

        public string TableAttributeName
        {
            get { return _tableAttribute == null ? string.Empty : _tableAttribute.Name; }
        }

        public string ServerName
        {
            get { return _linkedServerAttribute == null ? string.Empty : _linkedServerAttribute.ServerName; }
        }

        public string DatabaseName
        {
            get { return _linkedServerAttribute == null ? string.Empty : _linkedServerAttribute.DatabaseName; }
        }

        public string SchemaName
        {
            get
            {
                return _linkedServerAttribute == null
                    ? _schemaAttribute == null ? "dbo" : _schemaAttribute.SchemaName
                    : _linkedServerAttribute.SchemaName;
            }
        }

        public string TableNameOnly { get; private set; }
        #endregion

        #region Methods

        public string SqlFormattedTableName()
        {
            var tableName = ToString();

            return string.IsNullOrWhiteSpace(tableName) ? "" : tableName.TrimStart('[').TrimEnd(']');
        }

        public override string ToString()
        {
            var tableName = string.Format("{0}",
                _linkedServerAttribute != null ? _linkedServerAttribute.FormattedLinkedServerText : string.Empty);

            tableName += string.Format(_linkedServerAttribute == null ? "[{0}].[{1}]" : "{0}.[{1}]",
                _linkedServerAttribute == null ? SchemaName : string.Empty,
                TableNameOnly);

            return tableName;
        }

        #endregion
    }
}
