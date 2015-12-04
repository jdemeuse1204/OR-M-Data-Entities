using System.Data.SqlClient;

namespace OR_M_Data_Entities.Data.Query
{
    public interface ISqlPackage
    {
        string GetSql();

        ISqlContainer CreatePackage();

        void InsertParameters(SqlCommand cmd);
    }
}
