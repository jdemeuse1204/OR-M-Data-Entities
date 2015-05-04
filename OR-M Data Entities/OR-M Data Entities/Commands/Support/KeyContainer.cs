/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System.Collections.Generic;

namespace OR_M_Data_Entities.Commands.Support
{
	public sealed class KeyContainer
	{
		public KeyContainer()
		{
			_container = new Dictionary<string, object>();
		}

		private Dictionary<string, object> _container { get; set; }

		public void Add(string columnName, object value)
		{
			_container.Add(columnName, value);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _container.GetEnumerator();
		}
	}
}
