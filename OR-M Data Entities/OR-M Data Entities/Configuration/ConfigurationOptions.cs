using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Configuration
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions(bool useTransactions)
        {
            CheckConcurrencyViolations = true;
            IsLazyLoading = false;
            UseTransactions = useTransactions;
            ConcurrencyViolationRule = ConcurrencyViolationRule.Continue;
        }

        public bool IsLazyLoading { get; set; }

        public bool CheckConcurrencyViolations { get; set; }

        public bool UseTransactions { get; set; }

        public ConcurrencyViolationRule ConcurrencyViolationRule { get; set; }

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
