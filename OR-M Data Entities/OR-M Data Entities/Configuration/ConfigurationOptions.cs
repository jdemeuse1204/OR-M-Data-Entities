using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Configuration
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions(bool useTransactions)
        {
            IsLazyLoading = false;
            UseTransactions = useTransactions;

            Concurrency = new ConcurrencyConfiguration
            {
                ViolationRule = ConcurrencyViolationRule.Continue,
                IsOn = true
            };
        }

        public bool IsLazyLoading { get; set; }

        public bool UseTransactions { get; set; }

        public ConcurrencyConfiguration Concurrency { get; private set; }

        private class SqlIntegerOptions
        {
            
        }

        private enum SqlIntegerInsertType
        {
            Zero,
            ZeroMin
        }
    }
}
