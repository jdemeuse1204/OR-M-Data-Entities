namespace OR_M_Data_Entities.Data
{
    public sealed class SecurityDataReader<T> : DataReader<T>
    {
        public SecurityDataReader(PeekDataReader reader) 
            : base(reader)
        {
        }
    }
}
