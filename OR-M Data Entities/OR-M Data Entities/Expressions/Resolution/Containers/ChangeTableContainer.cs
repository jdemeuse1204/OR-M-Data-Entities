using System;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class ChangeTableContainer
    {
        public ChangeTableContainer(Type type, string tableName)
        {
            Type = type;

            TableName = tableName;
        }

        public Type Type { get; private set; }

        public string TableName { get; private set; }
    }
}
