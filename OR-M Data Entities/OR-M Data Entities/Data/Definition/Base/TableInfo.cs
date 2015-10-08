/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Reflection;
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
        }

        private readonly LinkedServerAttribute _linkedServerAttribute;
        private readonly TableAttribute _tableAttribute;

        public string ClassName { get; private set; }

        public string TableAttributeName {
            get { return _tableAttribute == null ? string.Empty : _tableAttribute.Name; }
        }

        public string ServerName {
            get { return _linkedServerAttribute == null ? string.Empty : _linkedServerAttribute.ServerName; }
        }

        public string DatabaseName {
            get { return _linkedServerAttribute == null ? string.Empty : _linkedServerAttribute.DatabaseName; }
        }

        public string SchemaName {
            get { return _linkedServerAttribute == null ? string.Empty : _linkedServerAttribute.SchemaName; }
        }

        public override string ToString()
        {
            var tableName = string.Format("{0}",
                _linkedServerAttribute != null ? _linkedServerAttribute.FormattedLinkedServerText : string.Empty);

            tableName += string.Format(_linkedServerAttribute == null ? "[{0}]" : ".[{0}]",
                string.IsNullOrWhiteSpace(TableAttributeName) ? ClassName : TableAttributeName);

            return tableName;
        }
    }
}
