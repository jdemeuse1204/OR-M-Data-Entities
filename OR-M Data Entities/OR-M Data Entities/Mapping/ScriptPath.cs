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
	public class ScriptPathAttribute : Attribute
	{
		public ScriptPathAttribute(string path)
		{
            Path = path;
		}

        public string Path { get; private set; }
	}
}
