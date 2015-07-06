namespace OR_M_Data_Entities.Data.Definition
{
    public class SqlExistsString
    {
        public SqlExistsString(bool invert, string joinString, string fromTableName)
        {
            Invert = invert;
            JoinString = joinString;
            FromTableName = fromTableName;
        }

        public readonly bool Invert;

        public readonly string JoinString;

        public readonly string FromTableName;
    }
}
