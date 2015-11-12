using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    [Schema("ts")]
    [Table("TestUpdateWithKeyDbGenerationOptionNone")]
    public class TestUpdateWithKeyDbGenerationOptionNone_ts
    {
        [DbGenerationOption(DbGenerationOption.None)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
