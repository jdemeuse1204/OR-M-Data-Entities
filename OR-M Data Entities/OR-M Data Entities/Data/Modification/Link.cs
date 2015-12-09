namespace OR_M_Data_Entities.Data.Modification
{
    public class Link
    {
        public Link(TableLink parent, TableLink goesTo)
        {
            Parent = parent;
            GoesTo = goesTo;
        }

        public readonly TableLink Parent;

        public readonly TableLink GoesTo;
    }
}
