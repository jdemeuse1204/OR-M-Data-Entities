using System.Data;

namespace OR_M_Data_Entities.Commands.Transform
{
    public class Cast
    {
        public static object As(object entity, SqlDbType targetTransformType)
        {
            return entity;
        }
    }
}
