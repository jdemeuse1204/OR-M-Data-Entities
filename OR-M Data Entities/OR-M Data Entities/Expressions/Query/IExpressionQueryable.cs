namespace OR_M_Data_Entities.Expressions.Query
{
    public interface IExpressionQueryable
    {
        DbQuery Query { get; set; }
    }
}
