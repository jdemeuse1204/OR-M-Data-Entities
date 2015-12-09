namespace OR_M_Data_Entities.Data.Modification
{
    public interface ISqlContainer
    {
        string Resolve();

        SqlPartStatement Split();
    }
}
