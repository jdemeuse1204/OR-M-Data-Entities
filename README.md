# OR-M-Data-Entities
Speedy Object Relational Mapper

OR-M Data Entities
Overview:  <br><br>
This solution is a micro Object-Relational Mapper aimed at speed.  Others out there are bulky and slow, OR-M Data Entities is much faster than ORM's like Entity Framework.  The catch is there is less "micro managing" of code going on, which makes the framework much faster.  The great thing about OR-M Data Entities are the two different data context's you can use.  DbSqlContext is for those familiar with Sql.  DbSqlContext is a derivative of Entity Framework, the difference is there are no DbSets to declare.  Instead your classes (tables) will perform the direct manipluation (See code below).  Also, there is DbEntityContext, which is almost an exact mirror of Entity Framework, just lighter and different inner workings.  Simple operations are the same such as saving and deleting.  If there is more demand I can add onto it.

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
Namespace: OR_M_Data_Entities<br>

#####Methods:
```C#
	DbSqlContext context = new DbSqlContext("your connection string"); 
	
	class Program
	{
		static void main(string[] args)
		{
			// ALL
			
			// NOTES - Returns everything from the corresponding table
			var allItems = context.All<T>();  // Returns everything from Persons table
			
			// DELETE
			
			// NOTES - Deletes the entity from the corresponding table, 
			// returns true if deleted and false if no action taken.
			// **Looks up the value to delete based on the primary key
			var person = new Person
			{
				Id = 1
			};
			var success = context.Delete(person);
			
			// DISCONNECT
			
			// Disconnects the existing connection
			context.Disconnect();
			
			// EXECUTE QUERY
			
			// ** All execute queries return a DataReader, which is custom
			
			// This can be unsafe as no parameters are used
			var sql = "Select * From Contacts";
			var reader = ExecuteQuery<Contact>(sql);  
			
			// Safest option for regular sql
			sql = "Select * From Contacts Where Id = @Param1";
			var parameters = new Dictionary<string, object>();
			parameters.Add("Param1",1);
			reader = ExecuteQuery<Contact>(sql, parameters);
			
			// Safe option as well, uses Sql Builders
			var builder = new SqlQueryBuilder();
        		builder.SelectAll<Contact>();
        		
        		reader = ExecuteQuery<Contact>(builder);
        		
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
        		
        		// WHERE
        		
        		// Default is select all records that pass the validation.  Returns ExpressionQuery
        		var expressionQuery = context.Where<Contact>(w => w.Id == 1);
		}
	}
    
```



2.	DataReader<T> : IEnumerable, IDisposable	
a.	Namespace: OR_M_Data_Entities.Data
Constructor – DataReader(SqlDataReader reader)
Methods:
•	IsEOF : returns bool
o	Is end of file, if true no records to fetch
•	All<T>() : returns List<T>
o	Fetches all records
•	Dispose() : disposes of the reader
•	IEnumerator<T> GetEnumerator() : Allows for iteration over results
•	T Select() : selects the current item of the results

3.	ExpressionQuery
a.	Namespace: OR_M_Data_Entities.Expressions
Constructor – ExpressionQuery(string fromTable, DataFetching context)
SQL Methods:
	NOTE: ORDER DOES NOT MATTER!
•	Distinct() : returns type of ExpressionQuery
o	Will select distinct in sql
•	Join<TParent, TChild>( Expression<Func<TParent, TChild, bool>> joinExpression)
o	Performs an inner join
	Example
•	.Join<Class1, Class2>((c1,c2) => c1.Class2Id == c2.Id)
•	Join<TParent, TChild>()
o	If your tables and keys are well formed the join will be chosen for you
	Example:  if we join Appointment onto Contact, Appointment must have a property of ContactId and Contact must have a property of Id. 
•	Left Join<TParent, TChild>( Expression<Func<TParent, TChild, bool>> joinExpression)
o	Performs an left join
	Example
•	.LeftJoin<Class1, Class2>((c1,c2) => c1.Class2Id == c2.Id)
•	LeftJoin<TParent, TChild>()
o	If your tables and keys are well formed the join will be chosen for you
	Example:  if we join Appointment onto Contact, Appointment must have a property of ContactId and Contact must have a property of Id. 
•	Select<T>() : returns type of ExpressionQuery
o	This is a Select All
•	Select<T>(Expression<Func<T, object>> result) : returns type of ExpressionQuery
o	Selects only the columns specified
•	Take(int rows) : returns type of ExpressionQuery
o	Takes only top X rows specified
•	Where<T>(Expression<Func<T, bool>> whereExpression)
o	Performs validation on the query, tells your query which data you want returned


Data Retrieval Methods:
•	All() : returns type of ICollection
•	All<T>() : returns type of List<T>
•	First() : returns type of dynamic
o	This is useful if the data you wish to return is not of a type
•	First<T>() : returns type of T
•	IEnumerator GetEnumerator<T>() : returns iterator so results can be iterated over
o	Choose which fields to select

Examples:
Class Contact
{
	Public int Id {get;set;}
	Public string Name {get;set;}
}

Class Appointment
{
	Public int Id {get;set;}
	Public string Description {get;set;}
	Public int ContactId {get;set;}
}

Select: 
// Grabs the first result
// Corresponding Sql:  Select * From Contact Where Id = 1
Var context = new DbSqlDataContext(“connectionString”);
Var result = context.From<Contact>().Where<Contact>(w => w.Id == 1).Select<Contact>().First<Contact>();
Join:
// Join Contact and Appointment
Var context = new DbSqlDataContext(“connectionString”);
Var result = context.From<Contact>().Join<Contact, Appointment>((c,a) => c.Id == a.ContactId).Select<Contact>().First<Contact>();

Mapping Attributes:
	Namespace: OR_M_Data_Entities.Mapping
•	Column
o	Usage:  Properties or Fields
o	Must pass in column name into the constructor
•	DbGenerationOption
o	Usage: Properties of Fields
o	Must pass in DbGenerationOption in the constructor
	Options: Generate, IdentitySpecification (default), None
•	Key
o	Usage: Properties or Fields
o	If your Primary key is not explicitly named Id or TableId then you must mark your key
•	Table
o	Usage: Class
o	Must mass in table name into the constructor
•	Unmapped
o	Usage: Properties or Fields
o	If you have properties you do not want mapped to and from the database, mark it with this attribute

Notes:  Must always specify T in methods, it does take more code, but it adds to the speed of the query builder.  It is much easier and faster if the type is supplied.

