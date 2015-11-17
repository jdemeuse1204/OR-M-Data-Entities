namespace OR_M_Data_Entities.Configuration
{
    public class ConfigurationOptions
    {
        public ConfigurationOptions(bool useMultipleActiveResultSets)
        {
            ThrowConcurrencyExceptions = true;
            UseMultipleActiveResultSets = useMultipleActiveResultSets;
        }

        public bool IsLazyLoading { get; set; }

        public bool ThrowConcurrencyExceptions { get; set; }

        public bool UseMultipleActiveResultSets { get; private set; }
    }
}
