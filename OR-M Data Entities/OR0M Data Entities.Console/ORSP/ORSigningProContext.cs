using OR_M_Data_Entities;

namespace ORSigningPro.Common.Data
{
    public class ORSigningProContext : DbSqlContext
    {
        #region Constructor
        public ORSigningProContext()
            : base("ORTSigningProEntities")
        {
        }
        #endregion
    }
}
