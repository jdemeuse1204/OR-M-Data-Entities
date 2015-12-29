using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class TimeStampColumnInsertRule : IRule
    {
        private readonly object _value;

        private readonly IReadOnlyList<PropertyInfo> _columns;

        public TimeStampColumnInsertRule(object value, IReadOnlyList<PropertyInfo> columns)
        {
            _value = value;
            _columns = columns;
        }

        public void Process()
        {
            var allTimestampColumns = _columns.Where(
                w =>
                    w.GetCustomAttribute<DbTypeAttribute>() != null &&
                    w.GetCustomAttribute<DbTypeAttribute>().Type ==
                    SqlDbType.Timestamp).ToList();

            if (allTimestampColumns.Count == 0) return;

            var errorColumns = (from column in allTimestampColumns
                let value = column.GetValue(_value)
                let hasError = value != null
                where hasError
                select column.Name).ToList();

            if (!errorColumns.Any()) return;

            const string error = "Cannot insert a value into TIMESTAMP column.  Column: {0}\r\r";
            var message = errorColumns.Aggregate(string.Empty, (current, item) => string.Concat(current, string.Format(error, item)));

            throw new SqlSaveException(message);
        }
    }
}
