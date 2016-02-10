/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

namespace OR_M_Data_Entities.Configuration
{
    public sealed class ConfigurationOptions
    {
        public ConfigurationOptions(bool useTransactions, string defaultSchema)
        {
            IsLazyLoading = false;
            UseTransactions = useTransactions;

            ConcurrencyChecking = new ConcurrencyConfiguration
            {
                ViolationRule = ConcurrencyViolationRule.OverwriteAndContinue,
                IsOn = true
            };

            InsertKeys = new KeyConfiguration();
            DefaultSchema = defaultSchema;
        }

        public bool IsLazyLoading { get; set; }
        
        public bool UseTransactions { get; set; }

        public ConcurrencyConfiguration ConcurrencyChecking { get; private set; }

        public KeyConfiguration InsertKeys { get; private set; }

        public string DefaultSchema { get; private set; }
    }
}
