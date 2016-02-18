/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Linq;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition.Rules.Base;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class IsConfigurationValidRule : IRule
    {
        private readonly IConfigurationOptions _configuration;

        public IsConfigurationValidRule(IConfigurationOptions configuration)
        {
            _configuration = configuration;
        }

        public void Process()
        {
            const string errorMessage = "Keys not configured correctly, must have at at least one key for option: {0}";

            if (_configuration.InsertKeys.Int16 == null || !_configuration.InsertKeys.Int16.Any())
            {
                throw new Exception(string.Format(errorMessage, "Integer"));
            }

            if (_configuration.InsertKeys.Guid == null || !_configuration.InsertKeys.Guid.Any())
            {
                throw new Exception(string.Format(errorMessage, "UniqueIdentifier"));
            }
        }
    }
}
