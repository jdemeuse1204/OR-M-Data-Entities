/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Data;
using System.Reflection;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Commands.Secure.StatementParts
{
    /// <summary>
    /// Used with ISqlBuilder for data insertion into the specified table
    /// </summary>
	public sealed class InsertItem
	{
		#region Properties
		public string SqlDataTypeString { get; private set; }

		public string PropertyDataType { get; private set; }

		public string PropertyName { get; private set; }

		public string DatabaseColumnName { get; private set; }

		public string KeyName { get; private set; }

		public SqlDbType DbTranslationType { get; private set; }

		public bool IsPrimaryKey { get; private set; }

        public DbGenerationOption Generation { get; private set; }

		public object Value { get; private set; }

		public bool TranslateDataType { get; private set; }
		#endregion

        #region Constructor
        public InsertItem(PropertyInfo property, object entity)
		{
			PropertyName = property.Name;
            DatabaseColumnName = property.GetColumnName();
            IsPrimaryKey = property.IsPrimaryKey();
			Value = property.GetValue(entity);
			PropertyDataType = property.PropertyType.Name.ToUpper();
            Generation = IsPrimaryKey
                ? property.GetGenerationOption()
                : property.GetCustomAttribute<DbGenerationOptionAttribute>() != null
                    ? property.GetCustomAttribute<DbGenerationOptionAttribute>().Option
                    : DbGenerationOption.None;

			// check for sql data translation, used mostly for datetime2 inserts and updates
			var translation = property.GetCustomAttribute<DbTypeAttribute>();

			if (translation != null)
			{
				DbTranslationType = translation.Type;
				TranslateDataType = true;
			}

			switch (Generation)
			{
				case DbGenerationOption.None:
					KeyName = "";
					break;
                case DbGenerationOption.IdentitySpecification:
					KeyName = "@@IDENTITY";
					break;
                case DbGenerationOption.Generate:
					KeyName = string.Format("@{0}", PropertyName);
					// set as the property name so we can pull the value back out
					break;
			}

			// for auto generation
			switch (property.PropertyType.Name.ToUpper())
			{
				case "INT16":
					SqlDataTypeString = "smallint";
					break;
				case "INT64":
					SqlDataTypeString = "bigint";
					break;
				case "INT32":
					SqlDataTypeString = "int";
					break;
				case "GUID":
					SqlDataTypeString = "uniqueidentifier";
					break;
			}
        }
        #endregion
    }
}
