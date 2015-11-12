using System;
using System.Linq;

namespace OR_M_Data_Entities.Commands
{
    [Obsolete("Expression Query should be used instead.  If using please contact me and I will leave these in.")]
    public class SqlTryInsertBuilder : SqlInsertBuilder
    {
        protected override string BuildSql(SqlInsertPackage package)
        {
            return string.Format( @"
{0}
{1}
IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{2}] WHERE {6}))) 
    BEGIN
        INSERT INTO [{2}] ({3}) VALUES ({4});{5}
    END

",
                string.IsNullOrWhiteSpace(package.Declare) ? string.Empty : string.Format("DECLARE {0}", package.Declare.TrimEnd(',')),

                package.Set,

                TableName.FormatTableName(),

                package.Fields.TrimEnd(','),

                package.Values.TrimEnd(','),

                package.SelectColumns.Any()
                    ? package.DoSelectFromForKeyContainer
                        ? string.Format(package.Select, package.SelectColumns.TrimEnd(','),
                            string.Format(package.From, TableName.FormatTableName(), package.Where))
                        : string.Format(package.Select, package.Keys.TrimEnd(','), string.Empty)
                    : string.Empty,
                // we want to select everything back from the database in case the model relies on db generation for some fields.
                // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                // where the column is not the PK
                package.Where);
        }
    }
}
