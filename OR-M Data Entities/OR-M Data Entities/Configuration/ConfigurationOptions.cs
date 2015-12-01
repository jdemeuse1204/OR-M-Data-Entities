namespace OR_M_Data_Entities.Configuration
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions(bool useTransactions)
        {
            ThrowConcurrencyExceptions = true;
            UseTransactions = useTransactions;
        }

        public bool IsLazyLoading { get; set; }

        public bool ThrowConcurrencyExceptions { get; set; }

        public bool UseTransactions { get; set; }

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
