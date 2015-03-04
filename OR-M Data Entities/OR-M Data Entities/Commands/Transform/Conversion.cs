using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public class Conversion
    {
        public static object To(object entity, SqlDbType targetTransformType, int style)
        {
            return entity;
        }
    }
}
