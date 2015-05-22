﻿using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    [Table("PhoneNumbers")]
    public class PhoneNumber
    {
        public int ID { get; set; }

        public string Phone{ get; set; }
    }
}