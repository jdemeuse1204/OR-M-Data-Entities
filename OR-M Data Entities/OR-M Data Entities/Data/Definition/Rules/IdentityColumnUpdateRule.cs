/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    // make sure the user is not trying to update an IDENTITY column, these cannot be updated
    public sealed class IdentityColumnUpdateRule : IRule
    {
        private readonly EntityStateTrackable _entityStateTrackable;

        private readonly ConfigurationOptions _configuration;

        private readonly IReadOnlyList<PropertyInfo> _columns; 

        public IdentityColumnUpdateRule(EntityStateTrackable entityStateTrackable, ConfigurationOptions configuration, IReadOnlyList<PropertyInfo> columns)
        {
            _entityStateTrackable = entityStateTrackable;
            _configuration = configuration;
            _columns = columns;
        }

        public void Process()
        {
            // cannot check if entity state trackable is null (entity state tracking is off) or if the pristine entity is null (insert)
            if (_entityStateTrackable == null) return;

            List<string> errorColumns;

            var allIdentityColumns = _columns.Where(
                w =>
                    w.GetCustomAttribute<DbGenerationOptionAttribute>() != null &&
                    w.GetCustomAttribute<DbGenerationOptionAttribute>().Option ==
                    DbGenerationOption.IdentitySpecification).ToList();

            if (allIdentityColumns.Count == 0) return;

            // entity state tracking is on, check to see if the identity column has been updated
            if (ModificationEntity.GetPristineEntity(_entityStateTrackable) == null)
            {
                // any identity columns should be zero/null or whatever the insert value is
                errorColumns = (from column in allIdentityColumns
                    let value = column.GetValue(_entityStateTrackable)
                    let hasError = !ModificationEntity.IsValueInInsertArray(_configuration, value)
                    where hasError
                    select column.Name).ToList();
            }
            else
            {
                // can only check when entity state tracking is on
                // only can get here when updating, try insert, or try insert update
                errorColumns =
                    allIdentityColumns.Where(
                        column => ModificationEntity.HasColumnChanged(_entityStateTrackable, column.Name))
                        .Select(w => w.Name)
                        .ToList();
            }

            if (!errorColumns.Any()) return;

            const string error = "Cannot update value of IDENTITY column.  Column: {0}\r\r";
            var message = errorColumns.Aggregate(string.Empty,
                (current, item) => string.Concat(current, string.Format(error, item)));

            throw new SqlSaveException(message);
        }
    }
}
