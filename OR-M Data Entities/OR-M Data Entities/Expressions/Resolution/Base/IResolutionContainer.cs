namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public interface IResolutionContainer
    {
        string Resolve();

        bool HasItems { get; }

        void Combine(IResolutionContainer container);
    }
}
