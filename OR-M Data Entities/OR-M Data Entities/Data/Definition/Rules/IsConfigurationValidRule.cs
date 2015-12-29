using System;
using System.Linq;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition.Rules.Base;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class IsConfigurationValidRule : IRule
    {
        private readonly ConfigurationOptions _configuration;

        public IsConfigurationValidRule(ConfigurationOptions configuration)
        {
            _configuration = configuration;
        }

        public void Process()
        {
            const string errorMessage = "Keys not configured correctly, must have at at least one key for option: {0}";

            if (_configuration.InsertKeys.Int == null || !_configuration.InsertKeys.Int.Any())
            {
                throw new Exception(string.Format(errorMessage, "Integer"));
            }

            if (_configuration.InsertKeys.UniqueIdentifier == null || !_configuration.InsertKeys.UniqueIdentifier.Any())
            {
                throw new Exception(string.Format(errorMessage, "UniqueIdentifier"));
            }
        }
    }
}
