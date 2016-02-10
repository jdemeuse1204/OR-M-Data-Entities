namespace OR_M_Data_Entities.Data.Definition
{
    public interface IMappedColumn
    {
        IColumn Column { get; }

        int Ordinal { get; }
    }
}
