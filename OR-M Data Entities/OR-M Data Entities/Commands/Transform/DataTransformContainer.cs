using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public sealed class DataTransformContainer
    {
        public DataTransformContainer(object value, SqlDbType transformType)
        {
            Value = value;
            Transform = transformType;
        }

        public object Value { get; private set; }

        public SqlDbType Transform { get; private set; }
    }
}
