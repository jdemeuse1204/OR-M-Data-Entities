# OR-M-Data-Entities
Speedy Object Relational Mapper

OR-M Data Entities
Overview:  <br><br>
This solution is a micro Object-Relational Mapper aimed at speed.  Others out there are bulky and slow, OR-M Data Entities is much faster than ORM's like Entity Framework.  The catch is there is less "micro managing" of code going on, which makes the framework much faster.  The great thing about OR-M Data Entities are the two different data context's you can use.  DbSqlContext is for those familiar with Sql.  DbSqlContext is a derivative of Entity Framework, the difference is there are no DbSets to declare.  Instead your classes (tables) will perform the direct manipluation (See code below).  Also, there is DbEntityContext, which is almost an exact mirror of Entity Framework, just lighter and different inner workings.  Simple operations are the same such as saving and deleting.  If there is more demand I can add onto it.

OR-M Data Entities is now even better. Support was added for ForeignKeys, Pseudo Keys, ReadOnly Tables, and VIEWS!  Yes thats right, Views now exist!  When writing the new code I realized some models that have foreign keys can get pretty big and you might not want to always select everything.  Sure you could shape your data with a select, but that can be a lot of code for bigger models.  Enter views!  Just put the ViewAttribute on each Foreign Key class and specify which view you want it to be a part of.  Then in the context do FromView<T>(string viewId) to select your data. 

#####Changes in 2.0
######1. Added support for Foreign Keys
######2. Added support for Linked Servers
######3. Fixed updating of TimeStamps
######4. Added Pseudo Key (On the fly Foreign Key)
######5. Added ReadOnly Attribute for Tables
######6. Added View Attribute for Tables
######7. Changed the way queries are written.  This needed to be done for Foreign Key support
######8. Better data shaping
######9. Took out recursion in the object loader
######10. Huge refactor, which made the ORM even faster with large queries
######11. Fixed null issue in where statements
######12. First will now throw an error if no data rows exist from the query
######13. Because of added support for foreign keys, all queries must now use Linq

#####Changes in 2.1
######1. Added entity state tracking, see below how to turn on/off

#####Changes in 2.2
######1. Added LookupTable Attribute, see below

#####Changes in 2.3
######1. Added support for Stored Procedures, Scalar Functions, Custom Sql, and .Sql files located in the project. See Stored Sql below

#####Changes in 2.3.1
######1. Added support for one-to-one nullable foreign keys


###Example Classes to be used below:
```C#
    [View("ContactOnly")]
    [Table("Contacts")]  <----- ONLY NEEDED IF CLASS NAME DIFFERENT FROM DB TABLE NAME
    public class Contact
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int PhoneID { get; set; }

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
	
    public class User
    {
        public int ID { get; set; }

        public string Name { get; set; }
    }
    
    [View("ContactOnly")]
    [Table("PhoneNumbers")]
    public class PhoneNumber
    {
        public int ID { get; set; }

        public string Phone{ get; set; }

        public int PhoneTypeID { get; set; }

        [ForeignKey("PhoneTypeID")]
        public PhoneType PhoneType { get; set; }
    }
    
    public class PhoneType
    {
        public int ID { get; set; }

        public string Type { get; set; }
    }
    
    [Table("Appointments")]
    public class Appointment
    {   
        [DbGenerationOption(DbGenerationOption.Generate)]
        public Guid ID { get; set; }

        public int ContactID { get; set; }

        public string Description { get; set; }

        [ForeignKey("AppointmentID")]
        public List<Address> Address { get; set; }
    }
        
    public class Address
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
    
    public class StateCode
    {
        public int ID { get; set; }

        public string Value { get; set; }
    }
    
    [Table("ZipCode")]
    public class Zip
    {
        public int ID { get; set; }

        public string Zip5 { get; set; }

        public string Zip4 { get; set; }

        public int AddressID { get; set; }
    }
    
    public class Name
    {
        public int ID { get; set; }

        public string Value { get; set; }

        public int ContactID { get; set; }
    }
```

####Entity State Tracking:
######How To Use<br/><br/>
Entity State Tracking has the ability to be turned on/off.  To turn on simply inherit from EntityStateTrackable on your class, to turn off either remove EntityStateTrackable or do not inherit from it.<br/><br/>
When Entity State Tracking is on, the entity has an underlying 'pristine' state that it is compared to on save.  If there are no changes the save will be skipped, otherwise an update on only the changed columns will be performed.  Without Entity State Tracking an insert or update will always be performed.<br/><br/>
The state can be checked by calling GetState() on your table class.

######Methods Added<br/><br/>
```C#
public EntityState GetState();
```

#####Example:
```C#
    // Entity Tracking is on
    public class MyClass : EntityStateTrackable
    {
        public int Id { get; set; }

        public string MyProperty { get; set; }
    }
   
    // Entity Tracking is off
    public class MyClass
    {
        public int Id { get; set; }

        public string MyProperty { get; set; }
    }
```

####Stored Sql:
######How To Use<br/><br/>
The idea of Stored Sql is the ability to use Stored Procedures, Scalar Functions, Custom Sql, and .sql files easier within the ORM.  All types listed above rely on properties to act as the parameters, see below for usage examples.

###### Execution Methods
```C#
DataReader<T> ExecuteScript<T>(IReadScript<T> script);
void ExecuteScript(IWriteScript script);
````

####Objects:
######1.	CustomScript : IWriteScript<br>
This script is used to write data to the server, not to read.  Updates/Inserts/Deletes should only be used because it has no return type.

#####Namespace: OR_M_Data_Entities.Scripts<br>

#####Example:
```C#
public class MyCustomScript : CustomScript
{
    public int Id { get; set; } // refers to the parameter @Id

    public string Changed { get; set; } // refers to the parameter @Changed

    protected override string Sql
    {
        get
        {
            return @"

            Update Contacts Set LastName = @Changed Where Id = @Id

        ";
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        DbSqlContext context = new DbSqlContext("your connection string"); 
        
        context.ExecuteScript(new MyCustomScript
        {
            Id = 10,
            Changed = "LastName"
        }); // returns a void
    }
}
````

######2.	CustomScript<T> : CustomScript
This script is used to read data from the server, should be used with Select statements, or queries that return data.

#####Namespace: OR_M_Data_Entities.Scripts<br>

#####Example:
```C#
public class MyOtherCustomScript : CustomScript<int>
{
    public int Id { get; set; } // refers to the parameter @Id

    protected override string Sql
    {
        get
        {
            return @"

            Select Top 1 Id From Contacts Where Id = @Id

        ";
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        DbSqlContext context = new DbSqlContext("your connection string"); 
        
        var id = context.ExecuteScript(new MyOtherCustomScript
        {
            Id = 10,
            Changed = "LastName"
        }).First();
    }
}
````

######3.	StoredProcedure : IWriteScript
This script is used to write data to the server and refers to a stored procedure located in the database.  Updates/Inserts/Deletes should only be used because it has no return type. Schema is always defaulted to DBO.  This can be changed through attributes or the config file.  See the configuration below to see how to change the default schema.

Parameters can be used two ways, you may include them in the script name (not advisable) OR you can give an index to each property in your class indication where the parameter goes in the overload sequence.  For example if we have a stored procedure dbo.GetName(@Id, @CurrentTime) then your class should be decorated as follows:
```C#
public class GetName : StoredProcedure
{
    [Index(1)]
    public int Id { get; set; } // refers to the parameter @Id
    
    [Index(2)]
    public DateTime CurrentTime { get; set; } // refers to the parameter @Id
}
````
NOTE: If you have one parameter it is not necessary to decorate your properties with an Index attribute.

#####Namespace: OR_M_Data_Entities.Scripts<br>

#####Example:
The class name is the name of the stored procedure.  You can use attributes to rename the name of the script or change schemas.  See below.
```C#
public class MyStoredProcedure : StoredProcedure
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("sp_FindName")] // name from the database
public class SomeCoolProcedure : StoredProcedure
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("sp_FindMyId")] // name from the database
[ScriptSchema("myschema")] // name from the schema
public class DifferentSchemaStoredProcedure : StoredProcedure
{
    public int Id { get; set; } // refers to the parameter @Id
}

class Program
{
    static void Main(string[] args)
    {
        DbSqlContext context = new DbSqlContext("your connection string"); 
        
        context.ExecuteScript(new MyStoredProcedure
        {
            Id = 5
        });
        
        context.ExecuteScript(new SomeCoolProcedure
        {
            Id = 5
        });
        
        context.ExecuteScript(new DifferentSchemaStoredProcedure
        {
            Id = 5
        });
    }
}
````

######4.	StoredProcedure<T> : StoredProcedure, IReadScript<T>
This script is used to read data from the server and refers to a stored procedure located in the database.  Select should be used because it has a return type. Schema is always defaulted to DBO.  This can be changed through attributes or the config file.  See the configuration below to see how to change the default schema.

Parameters can be used two ways, you may include them in the script name (not advisable) OR you can give an index to each property in your class indication where the parameter goes in the overload sequence.  For example if we have a stored procedure dbo.GetName(@Id, @CurrentTime) then your class should be decorated as follows:
```C#
public class GetName : StoredProcedure<int>
{
    [Index(1)]
    public int Id { get; set; } // refers to the parameter @Id
    
    [Index(2)]
    public DateTime CurrentTime { get; set; } // refers to the parameter @Id
}
````
NOTE: If you have one parameter it is not necessary to decorate your properties with an Index attribute.

#####Namespace: OR_M_Data_Entities.Scripts<br>

#####Example:
The class name is the name of the stored procedure.  You can use attributes to rename the name of the script or change schemas.  See below.
```C#
public class MyStoredProcedure : StoredProcedure<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("sp_FindName")] // name from the database
public class SomeCoolProcedure : StoredProcedure<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("sp_FindMyId")] // name from the database
[ScriptSchema("myschema")] // name from the schema
public class DifferentSchemaStoredProcedure : StoredProcedure<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

class Program
{
    static void Main(string[] args)
    {
        DbSqlContext context = new DbSqlContext("your connection string"); 
        
        var id = context.ExecuteScript(new MyStoredProcedure
        {
            Id = 100
        }).First();
        
        var id = context.ExecuteScript(new SomeCoolProcedure
        {
            Id = 100
        }).First();
        
        var id = context.ExecuteScript(new DifferentSchemaStoredProcedure
        {
            Id = 100
        }).First();
    }
}
````

######5.	StoredScript : IWriteScript
This script is used to write data to the server and refers to a (.sql) file stored within the project or a directory of your chosing.  Updates/Inserts/Deletes should only be used because it has no return type. The file name and path can be changed through attributes, however the default path can be changed through the config file.  See the configuration below to see how to change the default path. The default path is always the Scripts folder under your main project.

#####Namespace: OR_M_Data_Entities.Scripts<br>

#####Example:
The class name is the name of the stored (.sql) file.  You can use attributes to rename the name of the script or change the path.  See below.
```C#
public class MyStoredScript : StoredScript
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("TheActualScriptName")]
public class SomeCoolScript : StoredScript
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("GetNameById")] // name from the database
[ScriptPath("../../OtherScripts")] // new path
public class DifferentPathScript: StoredScript
{
    public int Id { get; set; } // refers to the parameter @Id
}

class Program
{
    static void Main(string[] args)
    {
        DbSqlContext context = new DbSqlContext("your connection string"); 
        
        context.ExecuteScript(new MyStoredScript
        {
            Id = 5
        });
        
        context.ExecuteScript(new SomeCoolScript
        {
            Id = 5
        });
        
        context.ExecuteScript(new DifferentPathScript
        {
            Id = 5
        });
    }
}
````

######6.	StoredScript<T> : StoredScript
This script is used to read data from the server and refers to a (.sql) file stored within the project or a directory of your chosing.  Select statements should be used because it returns data. The file name and path can be changed through attributes, however the default path can be changed through the config file.  See the configuration below to see how to change the default path. The default path is always the Scripts folder under your main project.

#####Namespace: OR_M_Data_Entities.Scripts<br>

#####Example:
The class name is the name of the stored (.sql) file.  You can use attributes to rename the name of the script or change the path.  See below.
```C#
public class MyStoredScript : StoredScript<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("TheActualScriptName")]
public class SomeCoolScript : StoredScript<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("GetNameById")] // name from the database
[ScriptPath("../../OtherScripts")] // new path
public class DifferentPathScript: StoredScript<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

class Program
{
    static void Main(string[] args)
    {
        DbSqlContext context = new DbSqlContext("your connection string"); 
        
        var id = context.ExecuteScript(new MyStoredScript
        {
            Id = 5
        }).First();
        
        var id = context.ExecuteScript(new SomeCoolScript
        {
            Id = 5
        }).First();
        
        var id = context.ExecuteScript(new DifferentPathScript
        {
            Id = 5
        }).First();
    }
}
````

######7.	ScalarFunction<T> : StoredProcedure, IReadScript<T>
This script is used to read data from the server and refers to a scalar function located in the database.  Select should be used because it has a return type. Schema is always defaulted to DBO.  This can be changed through attributes or the config file.  See the configuration below to see how to change the default schema.

Parameters can be used two ways, you may include them in the script name (not advisable) OR you can give an index to each property in your class indication where the parameter goes in the overload sequence.  For example if we have a scalar function dbo.GetName(@Id, @CurrentTime) then your class should be decorated as follows:
```C#
public class GetName : StoredProcedure<int>
{
    [Index(1)]
    public int Id { get; set; } // refers to the parameter @Id
    
    [Index(2)]
    public DateTime CurrentTime { get; set; } // refers to the parameter @Id
}
````
NOTE: If you have one parameter it is not necessary to decorate your properties with an Index attribute.

#####Namespace: OR_M_Data_Entities.Scripts<br>

#####Example:
The class name is the name of the stored (.sql) file.  You can use attributes to rename the name of the script or change the path.  See below.
```C#
public class MyScalarFunction : ScalarFunction<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("sp_FindFirstUserName")] // name from the database
public class SomeCoolScalarFunction : ScalarFunction<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

[Script("sp_FindSomeId")] // name from the database
[ScriptSchema("myschema")] // name from the schema
public class DifferentSchemaScalarFunction : ScalarFunction<int>
{
    public int Id { get; set; } // refers to the parameter @Id
}

class Program
{
    static void Main(string[] args)
    {
        DbSqlContext context = new DbSqlContext("your connection string"); 
        
        var id = context.ExecuteScript(new MyScalarFunction
        {
            Id = 100
        }).First();
        
        var id = context.ExecuteScript(new SomeCoolScalarFunction
        {
            Id = 100
        }).First();
        
        var id = context.ExecuteScript(new DifferentSchemaScalarFunction
        {
            Id = 100
        }).First();
    }
}
````

#####Web Configuration
With no configuration the default values are <br>
1. Default Schema: DBO <br>
2. Default Script Path: ../../Scripts (Scripts folder under your project)
```XML 
    <!-- IMPORTANT!! -->
    <!-- configSections must go before the startup section -->
  <configSections>
    <section name="ORMDataEntitiesConfigurationSection" type="OR_M_Data_Entities.Configuration.ORMDataEntitiesConfigurationSection, OR-M Data Entities" />
  </configSections>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5" />
    </startup>
    
    <!-- IMPORTANT!! -->
    <!-- ORMDataEntitiesConfigurationSection must go after the startup section -->
  <ORMDataEntitiesConfigurationSection>
    <SqlScripts>
      <add name="path" defaultValue="../../Scripts"/>
      <add name="schema" defaultValue="dbo"/>
    </SqlScripts>
  </ORMDataEntitiesConfigurationSection>
````

####Objects:
######1.	DbSqlContext

#####Namespace: OR_M_Data_Entities<br>

#####Methods:
```C#
bool Delete<T>(T entity) where T : class
void Disconnect()
DataReader<T> ExecuteQuery<T>(string sql)
DataReader<T> ExecuteQuery<T>(string sql, List<SqlDbParameter> parameters)
DataReader<T> ExecuteQuery<T>(string sql, params SqlDbParameter[] parameters)
DataReader<T> ExecuteQuery<T>(ISqlBuilder builder)
DataReader<T> ExecuteQuery<T>(ExpressionQuery<T> query)
DataReader<T> ExecuteScript<T>(IReadScript<T> script);
void ExecuteScript(IWriteScript script);
T Find<T>(params object[] pks)
ExpressionQuery<T> From<T>()
ExpressionQuery<T> FromView<T>(string viewId)
ChangeStateType SaveChanges<T>(T entity) where T : class
````
#####Method Examples
```C#

	class Program
	{
	
		DbSqlContext context = new DbSqlContext("your connection string"); 
	
		static void main(string[] args)
		{
			// DELETE
			
			// NOTES - Deletes the entity from the corresponding table, 
			// returns true if deleted and false if no action taken.
			// **Looks up the value to delete based on the primary key
			
			// *** For foreign keys, everything will be delete in order
			var contact = new Contact
			{
				Id = 1
			};
			var success = context.Delete(contact);
			
			// DISCONNECT
			
			// Disconnects the existing connection
			context.Disconnect();
			
			// EXPRESSION QUERY
			
			// Expression queries are intented to be just like linq queries. 
			
			// see below for more examples
			var contact = context.From<Contact>().Where(w => w.Name == "James").First();
			
				// Sub Queries
				var contact = context.From<Contact>().Where(w => w.Name == context.From<Appointment>().First(x => x.Description == "James").Description).First();
				
				// Query from foreign key list
        			var contact = context.From<Contact>().Where(w => w.Name == "James" && w.Appointments.Any(x => x.Description == "James").First();
        		// FIND
        		
        		var contact = context.Find<Contact>(1); // Finds a contact where ids Primary Key is 1
        		
        		// VIEW
        		
        		var expressionQuery = context.FromView<Contact>("ContactOnly").FirstOrDefault(); 
        		
        		// Returns a contact and any corresponding foreign key that has the "ContactOnly" view attribute
        		// ** Will throw an error if you try to select a property from a foreign key
        		// that does not have the "ContactOnly" view attribute
        		
        		// SAVE CHANGES
        		
        		// ** If the primary key is 0 for integers or MinValue for Guid a record will be inserted, 
        		// else the record will be updated.  Strings not supported for primary keys.
        		// ** If a table has no keys and you would like to insert a record, you must specifiy 
        		// at least one key with the KeyAttribute and set the DbGenerationAttribute to None. 
        		// ** All foreign keys will be inserted/updated in the correct order and their 
        		// corresponding keys will be loaded into their respective models
        		var contact = new Contact();
        		context.SaveChanges(contact);
        		// Insert contact because the Primary Key is 0.  the Contact object will automatically have 
        		// the Primary Key set after it is saved
		}
	}
    
```

######2.	DataReader<T> : IEnumerable, IDisposable

#####Namespace: OR_M_Data_Entities.Data<br>

#####Methods
```C#
bool HasRows {get;}
T FirstOrDefault()
T First()
List<T> ToList()
void Dispose()
```

#####Methods Examples:
```C#
	DbSqlContext context = new DbSqlContext("your connection string"); 
	
	class Program
	{
		static void main(string[] args)
		{
			var builder = new SqlQueryBuilder();
        		builder.SelectAll<Contact>();
        		
        		reader = ExecuteQuery<Contact>(builder);
        		
        		// Iteration Example
        		foreach(var item in reader)
        		{
        		
        		}
        		
        		// Get All example
        		var allItems = context.ToList();
		}
	}
```

######3.	ExpressionQuery<T> : DbQuery<T>, IExpressionQuery

#####Namespace: OR_M_Data_Entities.Expressions<br>

#####Extension Methods
```C#
TSource First<TSource>(this ExpressionQuery<TSource> source)
TSource First<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source)
TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
decimal? Max(this ExpressionQuery<decimal?> source)
decimal Max(this ExpressionQuery<decimal> source)
double? Max(this ExpressionQuery<double?> source)
double Max(this ExpressionQuery<double> source)
float? Max(this ExpressionQuery<float?> source)
float Max(this ExpressionQuery<float> source)
int? Max(this ExpressionQuery<int?> source)
int Max(this ExpressionQuery<int> source)
long? Max(this ExpressionQuery<long?> source)
long Max(this ExpressionQuery<long> source)
decimal? Min(this ExpressionQuery<decimal?> source)
decimal Min(this ExpressionQuery<decimal> source)
double? Min(this ExpressionQuery<double?> source)
double Min(this ExpressionQuery<double> source)
float? Min(this ExpressionQuery<float?> source)
float Min(this ExpressionQuery<float> source)
int? Min(this ExpressionQuery<int?> source)
int Min(this ExpressionQuery<int> source)
long? Min(this ExpressionQuery<long?> source)
long Min(this ExpressionQuery<long> source)
ExpressionQuery<T> Distinct<T>(this ExpressionQuery<T> source)
ExpressionQuery<T> Take<T>(this ExpressionQuery<T> source, int rows)
TSource Count<TSource>(this ExpressionQuery<TSource> source)
TSource Count<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
List<TSource> ToList<TSource>(this ExpressionQuery<TSource> source)
List<TSource> ToList<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
OrderedExpressionQuery<TSource> OrderBy<TSource, TKey>(this ExpressionQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
OrderedExpressionQuery<TSource> OrderByDescending<TSource, TKey>(this ExpressionQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
OrderedExpressionQuery<TSource> ThenBy<TSource, TKey>(this OrderedExpressionQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
OrderedExpressionQuery<TSource> ThenByDescending<TSource, TKey>(this OrderedExpressionQuery<TSource> source, Expression<Func<TSource, TKey>> keySelector)
bool Any<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
bool Any<TSource>(this ExpressionQuery<TSource> source)
ExpressionQuery<TSource> Where<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
ExpressionQuery<TResult> InnerJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
            ExpressionQuery<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
ExpressionQuery<TResult> Select<TSource, TResult>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TResult>> selector)
```

#####Methods Examples:
See _Performing Queries Using ExpressionQuery_ Section (Below)

####Mapping:

######1.	ColumnAttribute : SearchablePrimaryKeyAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public ColumnAttribute(string name) : base(SearchablePrimaryKeyType.Column)

#####Usage: Properties or Fields
```C#
public class MyClass
{
	[Column("Name From Database")]
	public string MyName {get;set;}
}
```
#####Notes: Needed if you wish to make your property name different than your database column name
<br><br>


######2.	DbGenerationOptionAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public DbGenerationOptionAttribute(DbGenerationOption option)

#####Usage: Properties or Fields
```C#
public class MyClass
{
	// example if Identity Specification is not set to true
	[DbGenerationOption(DbGenerationOption.Generate)]
	public int Id {get;set;}
	
	// example if you always want to set your key and insert it
	[DbGenerationOption(DbGenerationOption.None)]
	public int Id {get;set;}
	
	// example if identity insert is on, (Default for Primary Keys)
	public int Id {get;set;}
}
```
#####Notes: Needed to specify the key generation type.  A key can be auto generated if Generate is chosen.  If None is chosen a record will always be inserted
<br><br>


######3.	DbTypeAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public DbTypeAttribute(SqlDbType type) 

#####Usage: Properties or Fields
```C#
public class MyClass
{
	// If database is datetime2(7)
	[DbType(SqlDbType.datetime2)]
	public DateTime Date {get;set;}
}
```
#####Notes: If you have a time stamp you need to make sure you put this attribute on it and say it is a SqlDbType.Timestamp.  If you do not do this you will get errors when you try to update because timestamps cannot be updated
<br><br>

######4.	KeyAttribute : SearchablePrimaryKeyAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public KeyAttribute()

#####Usage: Properties or Fields
```C#
public class MyClass
{
	// Clustered key
	[Key]
	[DbGenerationOption(DbGenerationOption.None)] // set to none if you want to insert into a table with no PK
	public int PkId_1 {get;set;}
	
	[Key]
	[DbGenerationOption(DbGenerationOption.None)]  // set to none if you want to insert into a table with no PK
	public int PkId_2 {get;set;}
}
```
#####Notes: If a column is your primary key and it is not named "Id" or "ID" then you need to add this attribute to identity it
<br><br>


######5.	TableAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public TableAttribute(string name)

#####Usage: Class
```C#
[Table("MyTableName")]  // only needed if your class name is not the same as your table name
public class MyClass
{
	public int Id {get;set;}
}
```
#####Notes: Needed if you wish to make your class name different than your database table name
<br><br>

######6.	UnmappedAttribute : NonSelectableAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public UnmappedAttribute()

#####Usage: Properties or Fields
```C#
public class MyClass
{
	public int Id {get;set;}
	
	[Unmapped] // data is not pulled from or pushed to database
	public string Test {get;set;}
}
```
#####Notes: Needed if you wish to keep this property from being pushed or pulled from the database
<br><br>

######7.	ForeignKeyAttribute : AutoLoadKeyAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public ForeignKeyAttribute(string foreignKeyColumnName)

#####Usage: Properties or Fields
```C#
public class Contact
{
	public int ID {get;set;}
	
	public int PhoneID {get;set;}
	
	public int? PersonID {get;set;} 
	
	... See above classes ...
	
	// Assumes a one-one relationship between Contact and PhoneNumber. 
	// Looks for the PhoneID in the Contact Class.  Inner Join is performed.
	
	// In your database your foreign key would be from Contact/PhoneID and point to PhoneNumber/(PrimaryKey)
	[ForeignKey("PhoneID")]
        public PhoneNumber Number { get; set; }

	// Assumes a one-many relationship between Contact and PhoneNumber. 
	// Looks for the ContactID in the PhoneNumber table.  Left Join is performed.
	// ** NOTE:  All subsequent joins after the left join will also be a left join regardless of the relationship.
	
	// In your database your foreign key would be from Appointment/ContactID and point to Contact/(PrimaryKey)
        [ForeignKey("ContactID")]
        public List<Appointment> Appointments { get; set; }
}
```
Nullable one-one foreign keys
```C#
public class Contact
{
	public int ID {get;set;}
	
	public int? PhoneID {get;set;}
	
	... See above classes ...
	
	// Assumes NULLABLE a one-one relationship between Contact and PhoneNumber. 
	// Looks for the PhoneID in the Contact Class.  Left Join is performed since it is nullable.
	
	// In your database your foreign key would be from Contact/PhoneID and point to PhoneNumber/(PrimaryKey)
	[ForeignKey("PhoneID")]
        public PhoneNumber Number { get; set; }
}
```
#####Notes: Whether or not your foreign key object is a list or not will determine the relationship.  A List assumes a one-many relationship and a class reference assumes a one-one relationship.
<br><br>

######8.	PseudoKeyAttribute : AutoLoadKeyAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public PseudoKeyAttribute(string parentTableColumnName, string childTableColumnName)

#####Usage: Properties or Fields
```C#
public class Contact
{
	public int ID {get;set;}
	
	public int PhoneID {get;set;}
	
	... See above classes ...
	
	// Assumes a one-one relationship between Contact and Address. 
	// Looks for the AddressID in the Contact Class.  Inner Join is performed.
	
	// In your database your foreign key would be from Contacts/PhoneID and point to PhoneNumber/(PrimaryKey)
	[PseudoKey("PhoneID")]
        public PhoneNumber Number { get; set; }

	// Assumes a one-many relationship between Contact and PhoneNumber. 
	// Looks for the ContactID in the PhoneNumber table.  Left Join is performed.
	// ** NOTE:  All subsequent joins after the left join will also be a left join regardless of the relationship.
	
	// In your database your foreign key would be from Appointment/ContactID and point to Contacts/(PrimaryKey)
        [PseudoKey("ContactID")]
        public List<Appointment> Appointments { get; set; }
}
```

<br/><br/>

######9.	LinkedServerAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public LinkedServerAttribute(string serverName, string databaseName, string schemaName)

#####Usage: Class
```C#
[LinkedServer("MyServer", "MyDatabase", "MySchema")]
public class Contact
{

}
```
#####Notes: Linked Server Attribute will format your sql for any linked server queries.
<br><br>

######10.	ViewAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public ViewAttribute(params string[] viewIds)

#####Usage: Class
```C#
[View("ContactOnly", "OtherView")]
public class Contact
{

}
```
#####Notes: You can make as many views as you want on each class.  Only classes with the same view will be returned in the query, everything else will be null.  BE CAREFUL SAVING!  Should be used for data shaping only.
<br><br>

######11.	ReadOnlyAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public ReadOnlyAttribute()

#####Usage: Class
```C#
[ReadOnly]
public class Contact
{

}
```
#####Notes: Like the name suggests, this table is a read only table, nothing can be saved or deleted
<br><br>

######12.	LookupTableAttribute : TableAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public LookupTableAttribute(string name)

#####Usage: Properties or Fields
```C#
[LookupTable("MyTableName")]  // Always put in your table name
public class MyClass
{
	public int Id {get;set;}
}
```
#####Notes: LookupTable Attribute is for use on tables that are use primary for lookup data.  When using Foriegn Key Attributes and you wish to save/delete a record and its children, if you save/delete the record, but one or more if its children are being used in another table we do not want to remove that entity.  If we mark that table as a Lookup Table it will be skipped when saving/deleting the parent record.  You can still delete from a lookup table if you have the actual entity.  The only time deletion will be skipped is if it's a child record.
#####Example:
```C#
[LookupTable("State")]  // Always put in your table name
public class State
{
	public int Id {get;set;}
	
	public string Name {get;set;}
}

public class PropertyAddress
{
	public int Id {get;set;}
	
	public int StateId {get;set;}
	
	[ForeignKey("StateId")]
	public State State {get;set;}
}

public class HomeAddress 
{
	public int Id {get;set;}
	
	public int StateId {get;set;}
	
	[ForeignKey("StateId")]
	public State State {get;set;}
}

class program(string[] args) 
{
	using (var ctx = new MyContext())
	{
		// DELETE
		var homeAddress = ctx.Find<HomeAddress>(1);
		
		// since state is a lookup table it will be skipped on delete only.  Save works as normal
		ctx.Delete(homeAddress);
		
		var state = ctx.Find<State>(1);
		
		// since its not a child record, state will be deleted as normal
		ctx.Delete(state);
		
		// SAVE
		
		var propertyAddress = ctx.Find<PropertyAddress>(2);
		
		// since state is a lookup table it will be skipped on delete only.  Save works as normal
		ctx.SaveChanges(propertyAddress);
		
		var state = ctx.Find<State>(2);
		
		// since its not a child record, state will be saved as normal
		ctx.SaveChanges(state);
	}
}
```
<br><br>
######13.	Script : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public Script(string scriptName)

#####Usage: Class <br>
Only use with Stored Procedures, Scalar Functions, and StoredScripts
```C#
[Script("sp_FindName")]
public class Contact : StoredProcedure
{

}
```
#####Notes: Gives you the ability to change the name of your script
<br><br>
######14.	ScriptPath : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public ScriptPath(string path)

#####Usage: Class <br>
Only use with StoredScripts
```C#
[ScriptPath("../../SomeOtherFolder")]
public class Contact : StoredProcedure
{

}
```
#####Notes: Gives you the ability to change lookup path of your StoredScripts
<br><br>
######15.	ScriptSchema : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public ScriptSchema(string schemaName)

#####Usage: Class <br>
Only use with Stored Procedures and Scalar Functions
```C#
[ScriptSchema("myschema")]
public class Contact : StoredProcedure
{

}
```
#####Notes: Gives you the ability to change schema of your Stored Procedures or Scalar Functions
<br><br>
######16.	Index : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Constructor: public Index(string value)

#####Usage: Property <br>
Only use with Stored Procedures and Scalar Functions.  Denotes the order the parameters should be inserted into the script.
```C#
public class Contact : StoredProcedure
{
    [Index(1)]
    public int Id { get;set; }
    
    [Index(2)]
    public string FirstName { get;set }
}
```
#####Notes: Gives you the ability to specify what the order of parameters is for the Stored Procedure or Scalar Function.  If we have a Scalar Function dbo.FindName(@Id, @FirstName), labeling the index on your propertes will ensure they are inserted into the function in the correct order
<br><br>
####ExpressionQuery:

Note: ExpressionQuery is based off of Entity Framework's lambda queries. However, when it comes to complex queries it is not as robust.  If you are writing a complex query it is best to write it in sql not linq or lambda, it takes a lot longer to compile a complex query and run it than running straight sql.  The aim of this ORM is speed, not complexity.  If you use ForeignKeys you should not need complex queries because your object will be constructed for you.  See ForeignKeys

#####Joins

######Helper Classes
```C#
public class Policy : EntityStateTrackable
    {
        [Column("PolicyID")]
        public int Id { get; set; }

        public int FileNumber { get; set; }

        [Column("PolicyTypeID")] // changing the name for my join
        public int PolicyInfoId { get; set; }

        public int? StateID { get; set; }

        public string County { get; set; }

        public DateTime CreatedDate { get; set; }

        public string FeeOwnerName { get; set; }

        public string InsuredName { get; set; }

        public decimal? PolicyAmount { get; set; }

        public DateTime? PolicyDate { get; set; }

        public string PolicyNumber { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }
    }
    
    public class PolicyInfo
    {
        [DbGenerationOption(DbGenerationOption.Generate)]
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Description { get; set; }

        public Guid Stamp { get; set; }
    }
```
######Examples: Inner Join
```C#
class program(string[] args) 
{
	using (var ctx = new MyContext())
	{
		// find policy where id is 20 and return its data
		var policy = ctx.From<Policy>()
	                .InnerJoin(
	                    ctx.From<PolicyInfo>(),
	                    p => p.PolicyInfoId, // parent table join column
	                    pi => pi.Id,  // child table join column
	                    (p, pi) => p) // selector
	                    .FirstOrDefault(w => w.Id == 20);
	}
}
```

######Examples: Left Join<br>
Written the same as an Inner Join, only a Left Join is performed
```C#
class program(string[] args) 
{
	using (var ctx = new MyContext())
	{
		// find policy where id is 20 and return its data
		var policy = ctx.From<Policy>()
	                .InnerJoin(
	                    ctx.From<PolicyInfo>(),
	                    p => p.PolicyInfoId, // parent table join column
	                    pi => pi.Id,  // child table join column
	                    (p, pi) => p) // selector
	                    .FirstOrDefault(w => w.Id == 20);
	}
}
```

Issues/Questions/Comments:

Mail To - james.demeuse@gmail.com
