using System;

namespace OR_M_Data_Entities.Exceptions
{
    public class SqlReferenceConstraintException : Exception
    {
        public SqlReferenceConstraintException(string message)
            : base(message)
        {

        }
    }
}
