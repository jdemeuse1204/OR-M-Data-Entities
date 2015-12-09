namespace OR_M_Data_Entities.Data.Modification
{
    public class TableLink
    {
        public TableLink(string foreignKeyParentPropertyName, string columnName)
        {
            ForeignKeyParentPropertyName = foreignKeyParentPropertyName;
            ColumnName = columnName;
        }

        public readonly string ForeignKeyParentPropertyName ;

        public readonly string ColumnName;
    }
}
