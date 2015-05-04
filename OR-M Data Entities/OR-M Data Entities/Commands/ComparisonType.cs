/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

namespace OR_M_Data_Entities.Commands
{
	public enum ComparisonType
	{
		Contains,
		BeginsWith,
		EndsWith,
		Equals,
		EqualsIgnoreCase,
		EqualsTruncateTime,
		GreaterThan,
		GreaterThanEquals,
		LessThan,
		LessThanEquals,
		NotEqual,
        Between
	}
}
