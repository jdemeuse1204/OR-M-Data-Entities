namespace OR_M_Data_Entities.Data.Definition
{
    public interface IAutoLoadKeyRelationship
    {
        IColumn ChildColumn { get; }

        IColumn ParentColumn { get; }

        IColumn AutoLoadPropertyColumn { get; }

        JoinType JoinType { get; }
    }
}