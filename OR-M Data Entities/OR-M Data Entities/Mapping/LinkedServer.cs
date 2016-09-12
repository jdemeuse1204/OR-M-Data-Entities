/*
 * OR-M Data Entities v3.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class LinkedServerAttribute : Attribute
	{
		public LinkedServerAttribute(string serverName, string databaseName, string schemaName)
		{
            ServerName = serverName;
            DatabaseName = databaseName;
            SchemaName = schemaName;
		}

        public string ServerName { get; private set; }

        public string DatabaseName { get; private set; }

        public string SchemaName { get; private set; }

        public string FormattedLinkedServerText
	    {
            get { return string.Format("[{0}].[{1}].[{2}]", ServerName, DatabaseName, SchemaName); }
	    }
	}
}
