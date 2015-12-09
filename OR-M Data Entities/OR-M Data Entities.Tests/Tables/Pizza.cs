using System;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Tests.Tables
{
    public class Pizza
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CookTime { get; set; }

        [Column("ToppingId")]
        public int ToppingRenameId { get; set; }

        [ForeignKey("ToppingRenameId")]
        public Topping Topping { get; set; }

        public int CrustId { get; set; }

        [ForeignKey("CrustId")]
        public Crust Crust { get; set; }

        public Guid DeliveryManId { get; set; }

        [ForeignKey("DeliveryManId")]
        public DeliveryMan DeliveryMan { get; set; }
    }
}
