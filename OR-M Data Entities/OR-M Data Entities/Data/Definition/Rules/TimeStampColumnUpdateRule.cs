/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class TimeStampColumnUpdateRule : IRule
    {
        private readonly EntityStateTrackable _entityStateTrackable;

        private readonly IReadOnlyList<PropertyInfo> _columns;

        public TimeStampColumnUpdateRule(EntityStateTrackable entityStateTrackable, IReadOnlyList<PropertyInfo> columns)
        {
            _entityStateTrackable = entityStateTrackable;
            _columns = columns;
        }

        public void Process()
        {
            // cannot check if entity state trackable is null (entity state tracking is off) or if the pristine entity is null (insert)
            if (_entityStateTrackable == null) return;

            List<string> errorColumns;

            var allTimestampColumns = _columns.Where(
                w =>
                    w.GetCustomAttribute<DbTypeAttribute>() != null &&
                    w.GetCustomAttribute<DbTypeAttribute>().Type ==
                    SqlDbType.Timestamp).ToList();

            if (allTimestampColumns.Count == 0) return;

            // entity state tracking is on, check to see if the identity column has been updated
            if (ModificationEntity.GetPristineEntity(_entityStateTrackable) == null)
            {
                // any identity columns should be zero/null or whatever the insert value is
                errorColumns = (from column in allTimestampColumns
                                let value = column.GetValue(_entityStateTrackable)
                                let hasError = value != null
                                where hasError
                                select column.Name).ToList();
            }
            else
            {
                // can only check when entity state tracking is on
                // only can get here when updating, try insert, or try insert update
                errorColumns =
                    allTimestampColumns.Where(column => ModificationEntity.HasColumnChanged(_entityStateTrackable, column.Name))
                        .Select(w => w.Name)
                        .ToList();
            }

            if (!errorColumns.Any()) return;

            const string error = "Cannot update value of TIMESTAMP column.  Column: {0}\r\r";
            var message = errorColumns.Aggregate(string.Empty, (current, item) => string.Concat(current, string.Format(error, item)));

            throw new SqlSaveException(message);
        }
    }
}
