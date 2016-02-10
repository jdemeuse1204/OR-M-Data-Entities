﻿using System.Collections;
using System.Collections.Generic;
using OR_M_Data_Entities.Configuration;

namespace OR_M_Data_Entities.Data.Definition
{
    /// <summary>
    /// Acts like an IQueryable, no actions taken until the list is enumerated.  
    /// Once it is enumerated it will always read from its internal cache
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DelayedEnumerationCachedList<T> : IEnumerable<T>
    {
        protected readonly ITable ParentTable;

        protected readonly int Count;

        protected readonly HashSet<T> Internal;

        protected readonly ConfigurationOptions Configuration;

        protected DelayedEnumerationCachedList(ITable table, ConfigurationOptions configuration, int count)
        {
            Internal = new HashSet<T>();
            ParentTable = table;
            Count = count;
            Configuration = configuration;
        }

        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}