using System.Data.Entity;
using OR_M_Data_Entities.Compare.EFTables;

namespace OR_M_Data_Entities.Compare
{
    public class EntityFrameworkContext : DbContext
    {
        static EntityFrameworkContext()
        {
            Database.SetInitializer<EntityFrameworkContext>(null);
        }

        public EntityFrameworkContext()
            : base("name=sqlExpress")
        {
        }

        public IDbSet<Address> Addresses { get; set; }

        public IDbSet<Appointment> Appointments { get; set; }

        public IDbSet<Contact> Contacts { get; set; }

        public IDbSet<PhoneNumber> PhoneNumbers { get; set; }

        public IDbSet<PhoneType> PhoneTypes { get; set; }

        public IDbSet<StateCode> StateCodes { get; set; }

        public IDbSet<User> Users { get; set; }

        public IDbSet<Zip> ZipCodes { get; set; }
    }
}
