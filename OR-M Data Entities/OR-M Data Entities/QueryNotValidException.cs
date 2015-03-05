/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System;

namespace OR_M_Data_Entities
{
	public class QueryNotValidException : Exception
	{
		public QueryNotValidException(string message)
			: base(message)
		{

		}
	}
}
