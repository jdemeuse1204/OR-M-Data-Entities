using OR_M_Data_Entities.Lite.Mapping;

namespace NickOfTime.ServiceModels.DataTransferObjects.ORM.Lite
{
    [Table("RoleSettings")]
    public class ReadOnlyRoleSettings
    {
        [Key]
        public int Id { get; set; }
        public bool CanViewManagerJobs { get; set; }
        public bool CanBeAssignedJobs { get; set; }
    }
}
