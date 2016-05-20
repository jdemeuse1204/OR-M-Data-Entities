using System;

namespace OR_M_Data_Entities.WebApi
{
    public class QueryParseException : Exception
    {
        public QueryParseException(string exception) : base(exception)
        {

        }

        public QueryParseException(string exception, Exception innerException) : base(exception, innerException)
        {

        }
    }
}
