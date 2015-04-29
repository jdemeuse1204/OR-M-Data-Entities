namespace OR_M_Data_Entities.Data.Definition
{
    public class SqlDbParameter
    {
        public SqlDbParameter()
        {
        }

        public SqlDbParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
