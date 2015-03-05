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

#####Namespace: OR_M_Data_Entities<br>

#####Methods:
```C#
List<T> All<T>()
bool Delete<T>(T entity) where T : class
void Disconnect()
DataReader<T> ExecuteQuery<T>(string sql)
DataReader<T> ExecuteQuery<T>(string sql, Dictionary<string, object> parameters)
DataReader<T> ExecuteQuery<T>(ISqlBuilder builder)
T Find<T>(params object[] pks)
ExpressionQuery From<T>()
void SaveChanges<T>(T entity) where T : class
ExpressionQuery Where<T>(Expression<Func<T, bool>> propertyLambda) where T : class
````
#####Method Examples
```C#
	DbSqlContext context = new DbSqlContext("your connection string"); 
	
	class Program
	{
		static void main(string[] args)
		{
			// ALL
			
			// NOTES - Returns everything from the corresponding table
			var allItems = context.All<Contact>();  // Returns everything from Contacts table
			
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

######2.	DataReader<T> : IEnumerable, IDisposable

#####Namespace: OR_M_Data_Entities.Data<br>

#####Methods
```C#
bool IsEOF {get;}
T Select()
List<T> All()
IEnumerator<T> GetEnumerator()
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
        		
        		// Example 1  (Select() and IsEOF
        		while(!reader.IsEOF)
        		{
        			var item = reader.Select();
        		}
        		
        		// Example 2 (Enumeration)
        		foreach(var item in reader)
        		{
        		
        		}
        		
        		// Example 3 (All)
        		var allItems = context.All();
		}
	}
```

######3.	ExpressionQuery : DataTransform, IEnumerable

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
```C#

```

####Mapping:

######1.	ColumnAttribute : SearchablePrimaryKeyAttribute

#####Namespace: Mapping.Expressions<br>

#####Usage: Properties or Fields

######2.	DbGenerationOptionAttribute : Attribute

#####Namespace: Mapping.Expressions<br>

#####Usage: Properties or Fields

######3.	DbTranslationAttribute : Attribute

#####Namespace: Mapping.Expressions<br>

#####Usage: Properties or Fields

######4.	KeyAttribute : SearchablePrimaryKeyAttribute

#####Namespace: Mapping.Expressions<br>

#####Usage: Properties or Fields

######5.	TableAttribute : Attribute

#####Namespace: Mapping.Expressions<br>

#####Usage: Class

######6.	UnmappedAttribute : Attribute

#####Namespace: Mapping.Expressions<br>

#####Usage: Properties or Fields


