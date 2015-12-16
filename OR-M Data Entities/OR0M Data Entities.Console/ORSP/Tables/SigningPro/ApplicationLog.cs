using System;
using OR_M_Data_Entities.Mapping;

namespace ORSigningPro.Common.Data.Tables.SigningPro
{
    [ReadOnly(ReadOnlySaveOption.ThrowException)]
    public class ApplicationLog
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Thread { get; set; }

        public string Level { get; set; }

        public string Logger { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }

    }
}
