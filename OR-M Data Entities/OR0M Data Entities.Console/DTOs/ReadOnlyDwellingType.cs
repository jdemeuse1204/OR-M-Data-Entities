﻿using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("DwellingTypes")]
    public class ReadOnlyDwellingType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}