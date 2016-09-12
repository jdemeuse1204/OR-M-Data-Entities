using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using OR_M_Data_Entities;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Scripts.Base;
using OR_M_Data_Entities.Tracking;
using OR_M_Data_Entities.WebApi;

namespace OR0M_Data_Entities.Console
{
    public class SqlContext : DbSqlContext
    {
        public SqlContext()
            : base("sqlExpress")
        {
            Configuration.IsLazyLoading = true;

            Configuration.UseTransactions = true;
            Configuration.ConcurrencyChecking.IsOn = true;
            Configuration.ConcurrencyChecking.ViolationRule = ConcurrencyViolationRule.Continue;

            OnConcurrencyViolation += OnOnConcurrencyViolation;

            OnSqlGeneration += OnOnSqlGeneration;
        }

        private void OnOnSqlGeneration(string sql, List<SqlDbParameter> parameters)
        {
            //return;
            using (var writetext = File.AppendText("C:\\users\\jdemeuse\\desktop\\OR-M Sql.txt"))
            {
                writetext.WriteLine(sql);
                writetext.WriteLine("\r\r");
            }
        }

        private void OnOnConcurrencyViolation(object entity)
        {

        }
    }

    public class Test
    {
        public int Id { get; set; }

        public string Phone { get; set; }

        public Test2 Item { get; set; }
    }

    public class Test2
    {
        public int TestingId { get; set; }

        public string FirstName { get; set; }
    }



    internal class Program
    {
        public class ApiContext : DbSqlApiContext
        {
            public ApiContext() : base("sqlExpress")
            {
            }

            public override void OnContextCreated()
            {
                RegisterTable<Contact>();
            }
        }

        private static bool Test(int i)
        {
            return false;
        }

        private static void True(DbSqlContext ctx)
        {
        }

        private static void False(DbSqlContext ctx)
        {
        }


        private static void Main(string[] args)
        {
            var context = new ApiContext();
            var c = new SqlContext();
            //c.SaveChanges()

            // Done
            //var result = c.DeleteWhere<Appointment>(w => w.Description.StartsWith("TEST"));

            // Done
            //c.IfExists<Contact>(w => w.FirstName == "Jamessdfsdfs").True(True).False(new CS2 { Changed = "Different", Id = 1 });

            c.UpdateWhere<Contact>(w => w.FirstName == "James")
            .Set(w => new Dictionary<object, object>
            {
                            {w.FirstName, ""},
                            {w.LastName, "James"}
            });


            var sdfs = context.ApiQuery(@"
    {
	    read:{
		    Contacts: {
			    where: ['ContactID = 1'],
			    select: [ 'Contacts.*' ],
			    join: {
				    Appointments: {
					    on: [ 'Id = Contacts.ID' ]
				    }
			    }
		    }
	    }
    }
");

        }

        public class CS1 : CustomScript<Contact>
        {
            public int Id { get; set; }

            protected override string Sql
            {
                get { return @"

                    Select Top 1 * From Contacts Where Id = @Id

                "; }
            }
        }

        public class CS2 : CustomScript
        {
            public int Id { get; set; }

            public string Changed { get; set; }

            protected override string Sql
            {
                get { return @"

                    Update Contacts Set LastName = @Changed Where Id = @Id

                "; }
            }
        }

        [Script("GetLastName")]
        [ScriptPath("../../Scripts2")]
        public class SS1 : StoredScript<Contact>
        {
            public int Id { get; set; }
        }

        [ScriptAttribute("GetFirstName")]
        public class SS2 : StoredScript<Contact>
        {
        }

        [ScriptAttribute("UpdateFirstName")]
        public class SS3 : StoredScript
        {
            public string FirstName { get; set; }

            public int Id { get; set; }
        }

        [ScriptAttribute("GetFirstName")]
        public class SP1 : StoredProcedure<Contact>
        {
            public int Id { get; set; }
        }

        [Script("UpdateFirstName")]
        [Schema("dbo")]
        public class SP2 : StoredProcedure
        {
            [Index(1)]
            public int Id { get; set; }

            [Index(2)]
            public string FirstName { get; set; }
        }

        [ScriptAttribute("GetLastName")]
        public class SF1 : ScalarFunction<string>
        {
            [Index(1)]
            public int Id { get; set; }

            [Index(2)]
            public string FirstName { get; set; }
        }

        public class GetLastName2 : ScalarFunction<string>
        {

            public int Id { get; set; }
        }

        [Table("Contacts")]
        public class Contact : EntityStateTrackable
        {
            [Key]
            [Column("ID")]
            [DbGenerationOption(DbGenerationOption.Generate)]
            public int ContactID { get; set; }

            [DbGenerationOption(DbGenerationOption.Generate)]
            public int Test { get; set; }

            [DbGenerationOption(DbGenerationOption.Generate)]
            public Guid TestUnique { get; set; }

            [MaxLength(25)]
            public string FirstName { get; set; }

            [DbType(SqlDbType.VarChar, "100")]
            public string LastName { get; set; }

            public int? PhoneID { get; set; }

            public int CreatedByUserID { get; set; }

            public int EditedByUserID { get; set; }

            [ForeignKey("CreatedByUserID")]
            public User CreatedBy { get; set; }

            [ForeignKey("EditedByUserID")]
            public User EditedBy { get; set; }

            [ForeignKey("PhoneID")]
            public PhoneNumber Number { get; set; }

            [ForeignKey("ContactID")]
            public List<Appointment> Appointments { get; set; }

            [ForeignKey("ContactID")]
            public List<Name> Names { get; set; }
        }

        public class User : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        [Table("PhoneNumbers")]
        public class PhoneNumber : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Phone { get; set; }

            public int PhoneTypeID { get; set; }

            [ForeignKey("PhoneTypeID")]
            public PhoneType PhoneType { get; set; }
        }

        public class PhoneType : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Type { get; set; }
        }

        [Table("Appointments")]
        public class Appointment : EntityStateTrackable
        {
            [DbGenerationOption(DbGenerationOption.Generate)]
            public Guid ID { get; set; }

            public int ContactID { get; set; }

            public string Description { get; set; }

            public bool IsScheduled { get; set; }

            [ForeignKey("AppointmentID")]
            public List<Address> Address { get; set; }
        }

        public class Address : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Addy { get; set; }

            public Guid AppointmentID { get; set; }

            public int StateID { get; set; }

            [ForeignKey("StateID")]
            public StateCode State { get; set; }

            [ForeignKey("AddressID")]
            public List<Zip> ZipCode { get; set; }
        }

        public class StateCode : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Value { get; set; }
        }

        [Table("ZipCode")]
        public class Zip : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Zip5 { get; set; }

            public string Zip4 { get; set; }

            public int AddressID { get; set; }
        }

        public class Name : EntityStateTrackable
        {
            public int ID { get; set; }

            public string Value { get; set; }

            public int ContactID { get; set; }
        }
    }
}
