using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Data.Payload.Base
{
    public interface IPayload
    {
        string Sql { get; set; }

        bool IsObjectQuery { get; set; }
    }
}
