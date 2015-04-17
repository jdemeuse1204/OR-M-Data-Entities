using System.Collections.Generic;

namespace OR_M_Data_Entities.Expressions.Operations.Payloads.Base
{
    public class WhereContainer
    {
        public WhereContainer()
        {
            ValidationStatements = new List<string>();
            Parameters = new Dictionary<string, object>();
        }

        public List<string> ValidationStatements { get; set; } 

        public Dictionary<string, object> Parameters { get; set; } 
    }
}
