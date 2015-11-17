/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.Query
{
    public sealed class KeyContainer : IEnumerable<KeyValuePair<string, object>>
	{
        #region Constructor
        public KeyContainer()
		{
			_container = new Dictionary<string, object>();
		}
        #endregion

        #region Properties
        private Dictionary<string, object> _container { get; set; }
        #endregion

        #region Methods
        public void Add(string columnName, object value)
		{
			_container.Add(columnName, value);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _container.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
	}
}
