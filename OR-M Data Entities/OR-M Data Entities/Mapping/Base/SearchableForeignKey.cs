using System;

namespace OR_M_Data_Entities.Mapping.Base
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public abstract class SearchableForeignKeyAttribute : NonSelectableAttribute
    {
    }
}
