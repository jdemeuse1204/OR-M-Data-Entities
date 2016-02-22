namespace OR_M_Data_Entities.Data.Modification
{
    public interface ISqlPartStatement
    {
        string Sql { get; }

        string Declare { get; }

        string Set { get; }

        string ToString();
    }
}
