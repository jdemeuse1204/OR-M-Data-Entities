using OR_M_Data_Entities.Mapping;

namespace OR0M_Data_Entities.Console
{
    public class Parent
    {
        public int ID { get; set; }

        public int EditedByID { get; set; }

        public int CreatedByID { get; set; }

        [ForeignKey("EditedByID")]
        public Child EditedBy { get; set; }

        [ForeignKey("CreatedByID")]
        public Child CreatedBy { get; set; }
    }
}
