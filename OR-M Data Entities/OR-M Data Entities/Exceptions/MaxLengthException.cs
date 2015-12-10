using System;

namespace OR_M_Data_Entities.Exceptions
{
    public class MaxLengthException : Exception
    {
        public MaxLengthException(string message)
            : base(message)
        {
            
        }
    }
}
