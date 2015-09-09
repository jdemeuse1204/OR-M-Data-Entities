/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Exceptions
{
	public class QueryNotValidException : Exception
	{
		public QueryNotValidException(string message)
			: base(message)
		{

		}
	}
}
