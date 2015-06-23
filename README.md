# OR-M-Data-Entities
Speedy Object Relational Mapper

OR-M Data Entities
Overview:  <br><br>
This solution is a micro Object-Relational Mapper aimed at speed.  Others out there are bulky and slow, OR-M Data Entities is much faster than ORM's like Entity Framework.  The catch is there is less "micro managing" of code going on, which makes the framework much faster.  The great thing about OR-M Data Entities are the two different data context's you can use.  DbSqlContext is for those familiar with Sql.  DbSqlContext is a derivative of Entity Framework, the difference is there are no DbSets to declare.  Instead your classes (tables) will perform the direct manipluation (See code below).  Also, there is DbEntityContext, which is almost an exact mirror of Entity Framework, just lighter and different inner workings.  Simple operations are the same such as saving and deleting.  If there is more demand I can add onto it.


######Changes in 2.0
######1. Added support for Foreign Keys
######2. Added support for Linked Servers
######3. Fixed updating of TimeStamps
######4. Added Pseudo Key (On the fly Foreign Key)
######5. Added ReadOnly Attribute for Tables
######6. Added View Attribute for Tables
######7. Changed the way queries are written.  This needed to be done for Foreign Key support.
######8. Better data shaping
######8. Took out recursion in the object loader
######9. Huge refactor, which made the ORM even faster

###Example Classes to be used below:
```C#
	[Table("Contacts")] // Only needed if your table name is not the same as the class name
	public class Contact
	{
		public int Id {get;set;}
		
		public string Name {get;set;}
	}
	
	[Table("Appointments")]
	public class Appointment 
	{
		public int Id {get;set;}
		
		[Column("AppointmentAddress")]  // only needed if you want your property name to be different than the corresponding column name in the table
		public string Address {get;set;}
		
		public int ContactId {get;set;
	}
```

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
			var contact = context.From<Contact>()..Where(w => w.Name == "James").First();
			
        		
        		// FIND
        		
        		var contact = context.Find<Contact>(1); // Finds a contact where ids Primary Key is 1
        		
        		// FROM
        		
        		var expressionQuery = context.From<Contact>(); 
        		// Returns a ExpressionQuery which is custom
        		// Best option to use when selecting data
        		
        		// SAVE CHANGES
        		
        		// ** If the primary key is 0 for integers or MinValue for Guid a record will be inserted, 
        		// else the record will be updated.  Strings not supported for primary keys.
        		// ** If a table has no keys and you would like to insert a record, you must specifiy 
        		// at least one key with the KeyAttribute and set the DbGenerationAttribute to None. 
        		
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

######3.	ExpressionQuery<T> : DbQuery<T>, IEnumerator<T>, IExpressionQuery

#####Namespace: OR_M_Data_Entities.Expressions<br>

#####Methods
```C#
ExpressionQuery Where<T>(Expression<Func<T, bool>> whereExpression)
ExpressionQuery Join<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
            where TParent : class
            where TChild : class
ExpressionQuery Join<TParent, TChild>()
            where TParent : class
            where TChild : class
ExpressionQuery LeftJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> joinExpression)
            where TParent : class
            where TChild : class
ExpressionQuery LeftJoin<TParent, TChild>()
            where TParent : class
            where TChild : class
ExpressionQuery Select<T>(Expression<Func<T, object>> result)
ExpressionQuery Select<T>()
ExpressionQuery Distinct()
ExpressionQuery Take(int rows)
object First()
T First<T>()
ICollection All()
List<T> All<T>()
IEnumerator GetEnumerator<T>()
```

#####Methods Examples:
See _Performing Queries Using ExpressionQuery_ Section (Below)

####Mapping:

######1.	ColumnAttribute : SearchablePrimaryKeyAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Usage: Properties or Fields
```C#
public class Contact
{
	[Column("Name From Database")]
	public string MyName {get;set;}
}
```
#####Notes: Needed if you wish to make your property name different than your database column name
<br><br>


######2.	DbGenerationOptionAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Usage: Properties or Fields
```C#
public class Contact
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


######3.	DbTranslationAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Usage: Properties or Fields
```C#
public class Contact
{
	// If database is datetime2(7)
	[DbTranslationAttribute(SqlDbType.datetime2)]
	public DateTime Date {get;set;}
}
```
#####Notes: If you would like to change your Sql Type you need this attribute.
<br><br>

######4.	KeyAttribute : SearchablePrimaryKeyAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Usage: Properties or Fields
```C#
public class Contact
{
	// Clustered key
	[Key]
	[DbGenerationOption(DbGenerationOption.None)] // set to none if you want to insert into a table with no PK
	public int FkId_1 {get;set;}
	
	[Key]
	[DbGenerationOption(DbGenerationOption.None)]  // set to none if you want to insert into a table with no PK
	public int FkId_2 {get;set;}
}
```
#####Notes: If a column is your primary key and it is not named "Id" or "ID" then you need to add this attribute to identity it
<br><br>


######5.	TableAttribute : Attribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Usage: Class
```C#
[Table("Contacts")]  // only needed if your class name is not the same as your table name
public class Contact
{
	public int Id {get;set;}
}
```
#####Notes: Needed if you wish to make your class name different than your database table name
<br><br>

######6.	UnmappedAttribute : NonSelectableAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Usage: Properties or Fields
```C#
public class Contact
{
	public int Id {get;set;}
	
	[Unmapped] // data is not pulled from or pushed to database
	public string Test {get;set;}
}
```
#####Notes: Needed if you wish to keep this property from being pushed or pulled from the database
<br><br>

######7.	ForeignKeyAttribute : NonSelectableAttribute

#####Namespace: OR_M_Data_Entities.Mapping<br>

#####Usage: Properties or Fields
```C#
public class Contact
{
	public int Id {get;set;}
	
	public int AddressID {get;set;}
	
	// Assumes a one-one relationship between Contact and Address. 
	// Looks for the AddressID in the Contact Class
	[ForeignKey("AddressID")]
	public Address Address {get;set;}
	
	// Assumes a one-many relationship between Contact and PhoneNumber. 
	// Looks for the ContactID in the PhoneNumber table
	[ForeignKey("ContactID")]
	public List<PhoneNumber> PhoneNumbers {get;set;}
}
```
#####Notes: Whether or not your foreign key object is a list or not will determine the relationship.  A List assumes a one-many relationship and a class reference assumes a one-one relationship.
<br><br>



####Performing Queries Using ExpressionQuery:
#####The aim of the ExpressionQuery class is to make writing sql in C# much easier.  Know how to write a join in Lambda without StackOverflow?  Know how to write a left join in Lambda?  Hate using DbFunctions in Entity Framework to truncate time.  Now all of this is easier than ever!

```C#
[Table("Contacts")]
public class Contact
{
	public int Id {get;set;}
	
	public DateTime DateAdded {get;set;}
	
	public string CreatedBy {get;set;}
}

[Table("Appointments")]
public class Appointment
{
	public int Id {get;set;}
	
	public int ContactId {get;set;}
}


	
class Program
{

	DbSqlContext context = new DbSqlContext("your connection string"); 

	static void main(string[] args)
	{
		// Lets write the following with ExpressionQuery syntax
		// Select * From Contacts Where Cast(CreatedBy as date) = Cast(getdate() as date)
		
		var query = context.From<Contact>().Where<Contact>(w => Cast.As(w.CreatedBy,SqlDbType.date)
		== Cast.As(DateTime.Now, SqlDbType.date).Select<Contact>();
		
		// The great thing about ExpressionQuery is order does not matter.  From always needs to come first, 
		// after that it does not matter.
		// The above can also be written as:
		
		var query = context.From<Contact>().Select<Contact>().Where<Contact>(w => Cast.As(w.CreatedBy,SqlDbType.date)
		== Cast.As(DateTime.Now, SqlDbType.date);
		
		// Lets write an Inner Join
		var query = context.From<Contact>().Join<Contact, Appointment>((c,a) => c.Id == a.ContactId).Where<Contact>(w => Cast.As(w.CreatedBy,SqlDbType.date)
		== Cast.As(DateTime.Now, SqlDbType.date).Select<Contact>();
		
		// Lets write an Left Join
		var query = context.From<Contact>().LeftJoin<Contact, Appointment>((c,a) => c.Id == a.ContactId).Where<Contact>(w => Cast.As(w.CreatedBy,SqlDbType.date)
		== Cast.As(DateTime.Now, SqlDbType.date).Select<Contact>();
		
		// Lets write an Select Top 10
		var query = context.From<Contact>().LeftJoin<Contact, Appointment>((c,a) => c.Id == a.ContactId).Where<Contact>(w => Cast.As(w.CreatedBy,SqlDbType.date)
		== Cast.As(DateTime.Now, SqlDbType.date).Select<Contact>().Take(10);
		
		// Dont have a class to return, use Select<dynamic>() and it will return everything in your 
		// select.  Want to return a struct?  Use Select<int>() to return your value.  More examples to come!
	}
}
```

####Noted Issues:
######5.	Linq Joins
_Linq joins are not supported at this time because I simply don't use them.  I use my ExpresssionQuery Joins instead.  If you would like them to be supported please contact me_


Issues/Questions/Comments:

Mail To - james.demeuse@gmail.com
