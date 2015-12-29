using System;

namespace OR_M_Data_Entities.Exceptions
{
    public class InvalidTableException : Exception
    {
        public InvalidTableException(string message)
            : base(message)
        {
            
        }
    }
}
