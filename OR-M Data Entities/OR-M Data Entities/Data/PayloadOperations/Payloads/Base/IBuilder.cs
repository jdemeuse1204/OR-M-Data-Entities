using System.Data.SqlClient;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base
{
    public interface IBuilder
    {
        SqlCommand ExecuteBuilder();
    }
}
