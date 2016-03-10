/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Definition.Base
{
    public class TableInfo
    {
        public TableInfo(Type type)
        {
            ClassName = type.Name;
            _linkedServerAttribute = type.GetCustomAttribute<LinkedServerAttribute>();
            _tableAttribute = type.GetCustomAttribute<TableAttribute>();
            _schema = type.GetCustomAttribute<SchemaAttribute>();

            if (_linkedServerAttribute != null && _schema != null)
            {
                throw new Exception(
                    string.Format(
                        "Class {0} cannot have LinkedServerAttribute and SchemaAttribute, use one or the other",
                        ClassName));
            }
        }

        private readonly LinkedServerAttribute _linkedServerAttribute;
        private readonly TableAttribute _tableAttribute;
        private readonly SchemaAttribute _schema;

        public string ClassName { get; private set; }

        public string TableAttributeName
        {
            get { return _tableAttribute == null ? string.Empty : _tableAttribute.Name; }
        }

        public bool IsUsingLinkedServer
        {
            get { return _linkedServerAttribute != null; }
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
                    ? _schema == null ? "dbo" : _schema.SchemaName
                    : _linkedServerAttribute.SchemaName;
            }
        }

        public override string ToString()
        {
            var tableName = string.Format("{0}",
                _linkedServerAttribute != null ? _linkedServerAttribute.FormattedLinkedServerText : string.Empty);

            tableName += string.Format(_linkedServerAttribute == null ? "[{0}].[{1}]" : "{0}.[{1}]",
                _linkedServerAttribute == null ? SchemaName : string.Empty,
                string.IsNullOrWhiteSpace(TableAttributeName) ? ClassName : TableAttributeName);

            return tableName;
        }
    }
}
