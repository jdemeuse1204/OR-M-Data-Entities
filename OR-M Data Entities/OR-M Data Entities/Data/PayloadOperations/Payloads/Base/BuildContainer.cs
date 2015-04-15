using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base
{
    public class BuildContainer
    {
        public string Sql { get; set; }

        public Dictionary<string,object> Parameters { get; set; } 
    }
}
