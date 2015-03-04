using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using OR_M_Data_Entities.Commands.Secure;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Commands
{
    public sealed class SqlInsertBuilder : SqlSecureExecutable, ISqlBuilder
	{
		#region Properties
		private string _table { get; set; }
		private List<InsertItem> _insertItems { get; set; }

		#endregion

		#region Constructor
		public SqlInsertBuilder()
		{
			_table = string.Empty;  
			_insertItems = new List<InsertItem>();
		}
		#endregion

		#region Methods
		public SqlCommand Build(SqlConnection connection)
		{
			if (string.IsNullOrWhiteSpace(_table))
			{
				throw new QueryNotValidException("INSERT statement needs Table Name");
			}

			if (_insertItems.Count == 0)
			{
				throw new QueryNotValidException("INSERT statement needs VALUES");
			}

			var fields = string.Empty;
			var values = string.Empty;
			var identity = string.Empty;
			var declare = string.Empty;
			var set = string.Empty;

			//  NOTE:  Alias any Identity specification and generate columns with their property
			// name not db column name so we can set the property when we return the values back.

		    for (var i = 0; i < _insertItems.Count; i++)
		    {
		        var item = _insertItems[i];

		        switch (item.Generation)
		        {
		            case DbGenerationOption.None:
		            {
		                //Value is simply inserted
		                var data = GetNextParameter();
		                fields += string.Format("[{0}],", item.DatabaseColumnName);
		                values += string.Format("{0},", data);

		                if (item.TranslateDataType)
		                {
		                    AddParameter(item.Value, item.DbTranslationType);
		                }
		                else
		                {
		                    AddParameter(item.Value);
		                }
		            }
		                break;
		            case DbGenerationOption.Generate:
		            {
		                // Value is generated from the database
		                var key = string.Format("@{0}", item.PropertyName);

		                // alias as the property name so we can set the property
		                var variable = string.Format("{0} as {1}", key, item.PropertyName);

		                // make our set statement
		                if (item.SqlDataTypeString.ToUpper() == "UNIQUEIDENTIFIER")
		                {
		                    // GUID
		                    set += string.Format("SET {0} = NEWID();", key);
		                }
		                else
		                {
		                    // INTEGER
		                    set += string.Format("SET {0} = (Select ISNULL(MAX({1}),0) + 1 From {2});", key,
		                        item.DatabaseColumnName, _table);
		                }

		                fields += string.Format("[{0}],", item.DatabaseColumnName);
		                values += string.Format("{0},", key);
		                declare += string.Format("{0} as {1},", key, item.SqlDataTypeString);
		                identity += variable + ",";

		                // Do not add as a parameter because the parameter will be converted to a string to
		                // be inserted in to the database
		            }
		                break;
		            case DbGenerationOption.IdentitySpecification:
		            {
		                identity = string.Format("@@IDENTITY as {0},", item.PropertyName);
		            }
		                break;
		        }
		    }

		    var sql = string.Format("{0} {1} INSERT INTO {2} ({3}) VALUES ({4});{5}",
				string.IsNullOrWhiteSpace(declare) ? "" : string.Format("DECLARE {0}", declare.TrimEnd(',')),
				set,
				_table, fields.TrimEnd(','),
				values.TrimEnd(','),
				string.IsNullOrWhiteSpace(identity) ? "" : string.Format("Select {0}", identity.TrimEnd(',')));

			var cmd = new SqlCommand(sql, connection);

			InsertParameters(cmd);

			return cmd;
		}

		public void Table(string tableName)
		{
			_table = tableName;
		}

		public void AddInsert(PropertyInfo property, object entity)
		{
			_insertItems.Add(new InsertItem(property, entity));
		}
		#endregion
	}
}
