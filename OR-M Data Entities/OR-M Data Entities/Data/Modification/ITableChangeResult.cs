namespace OR_M_Data_Entities.Data.Modification
{
    public interface ITableChangeResult
    {
        UpdateType Action { get; }

        string TableName { get; }

        void ChangeAction(UpdateType action);
    }
}
