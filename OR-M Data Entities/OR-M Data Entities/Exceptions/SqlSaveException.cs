using System;

namespace OR_M_Data_Entities.Exceptions
{
    public class SqlSaveException : Exception
    {
        public SqlSaveException(string reason)
            : base(string.Format("SAVE CANCELLED!  Reason: {0}", reason))
        {

        }
    }
}
