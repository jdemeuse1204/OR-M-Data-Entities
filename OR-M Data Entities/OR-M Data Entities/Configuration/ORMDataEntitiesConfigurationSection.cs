/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Configuration;
using System.Linq;

namespace OR_M_Data_Entities.Configuration
{
    public class ORMDataEntitiesConfigurationSection
            : ConfigurationSection
    {
        public const string SectionName = "ORMDataEntitiesConfigurationSection";

        private const string _sqlScriptsCollectionName = "SqlScripts";

        [ConfigurationProperty(_sqlScriptsCollectionName)]
        [ConfigurationCollection(typeof (StoredSqlConfigurationCollection), AddItemName = "add")]
        public StoredSqlConfigurationCollection StoredSql
        {
            get
            {
                return (StoredSqlConfigurationCollection)base[_sqlScriptsCollectionName];
            }
        }
    }

    public class StoredSqlConfigurationElement : ConfigurationElement
    {

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
        }

        [ConfigurationProperty("defaultValue", IsRequired = true)]
        public string DefaultValue
        {
            get
            {
                return this["defaultValue"] as string;
            }
        }
    }

    public class StoredSqlConfigurationCollection : ConfigurationElementCollection
    {
        public StoredSqlConfigurationElement First(string name)
        {
            return
                (from object item in this select item as StoredSqlConfigurationElement).FirstOrDefault(
                    w => String.Equals(w.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new StoredSqlConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StoredSqlConfigurationElement)element).DefaultValue;
        }
    }
}
