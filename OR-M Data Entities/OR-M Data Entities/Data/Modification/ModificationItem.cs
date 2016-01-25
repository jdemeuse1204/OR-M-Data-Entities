/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Data;
using System.Reflection;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Modification
{
    public sealed class ModificationItem
    {
        #region Properties
        public bool IsModified { get; private set; }

        public string SqlDataTypeString { get; private set; }

        public string PropertyDataType { get; private set; }

        public string PropertyName { get; private set; }

        public string DatabaseColumnName { get; private set; }

        public SqlDbType DbTranslationType { get; private set; }

        public bool IsPrimaryKey { get; private set; }

        public DbGenerationOption Generation { get; private set; }

        public bool TranslateDataType { get; private set; }

        public bool NeedsAlias
        {
            get { return DatabaseColumnName != PropertyName; }
        }
        #endregion

        #region Constructor

        public ModificationItem(PropertyInfo property, bool isModified = true)
        {
            PropertyName = property.Name;
            DatabaseColumnName = property.GetColumnName();
            IsPrimaryKey = property.IsPrimaryKey();
            PropertyDataType = property.PropertyType.Name.ToUpper();
            Generation = IsPrimaryKey
                ? property.GetGenerationOption()
                : property.GetCustomAttribute<DbGenerationOptionAttribute>() != null
                    ? property.GetCustomAttribute<DbGenerationOptionAttribute>().Option
                    : DbGenerationOption.None;

            // set in case entity tracking isnt on
            IsModified = isModified;

            // check for sql data translation, used mostly for datetime2 inserts and updates
            var translation = property.GetCustomAttribute<DbTypeAttribute>();

            if (translation != null)
            {
                DbTranslationType = translation.Type;
                TranslateDataType = true;
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
                case "STRING":
                    SqlDataTypeString = "varchar(max)";
                    break;
                case "DATETIME":
                    SqlDataTypeString = "datetime";
                    break;
            }
        }

        #endregion

        #region Methods

        public string AsOutput(string appendToEnd)
        {
            return string.Format("[INSERTED].[{0}]{1}{2}", DatabaseColumnName,
                NeedsAlias ? string.Format(" as [{0}]", PropertyName) : string.Empty, appendToEnd);
        }

        public string AsField(string appendToEnd)
        {
            return string.Format("[{0}]{1}", DatabaseColumnName, appendToEnd);
        }

        public string AsFieldPropertyName(string appendToEnd)
        {
            return string.Format("[{0}]{1}", PropertyName, appendToEnd);
        }
        #endregion
    }
}
