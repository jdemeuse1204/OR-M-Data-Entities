# OR-M-Data-Entities
Object Relational Mapper aimed at speed.

OR-M Data Entities
Overview:  <br><br>
This solution is a micro Object-Relational Mapper aimed at speed.  Others out there are bulky and slow, OR-M Data Entities is much faster and reading and inserting.

####Objects:
######1.	DbSqlContext
Namespace: OR_M_Data_Entities<br>

Constructor:
  
    DbSqlContext(string connectionString)
    
Methods:
	
    All<T>() 
    
    - Returns everything from Table T
    
    Delete<T>(T entity) where T : class
    
    -Deletes the entity from the corresponding table, returns true if deleted and false if no action taken.
    *Note:  Looks up the value to delete based on the primary key
    
    Disconnnect()
    
    -Disconnects from the server
    
    ExecuteQuery<T>(string sql) : returns typeof DataReader
    ExecuteQuery<T>(string sql, Dictionary<string,object> parameters) : returns typeof DataReader
    ExecuteQuery<T>(ISqlBuilder builder) : returns typeof DataReader
    
    -Executes a query and returns a DataReader
    
    Find<T>(params object[] pks) : returns entity of type T
    From<T>() : Returns type of ExpressionQuery
    
    SaveChanges<T>(T entity) where T : class
    
    -Saves changes of the entity
    *Note: If the primary key is 0 for integers or MinValue for Guid a record will be inserted, else the record will be updated.  Strings not supported for primary keys.
	If a table has no keys and you would like to insert a record, you must specifiy at least one key with the KeyAttribute and set the DbGenerationAttribute to None. 

    Where<T>(Expression<Func<T,bool>> lambdaExpression)
    -Default is select all records that pass the Where validation

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

