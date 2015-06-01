using System;
using System.Reflection;

namespace OR_M_Data_Entities.Expressions.Resolution.Select.Info
{
    public class SelectInfo : SelectInfoChanged 
    {
        public SelectInfo(MemberInfo info, Type baseType, string tableName,string tableReadName, int ordinal, bool isPrimaryKey)
        {
            OriginalProperty = info;
            NewProperty = info;
            Ordinal = ordinal;
            BaseType = baseType;
            NewType = baseType;
            TableName = tableName;
            TableReadName = tableReadName;
            IsPrimaryKey = isPrimaryKey;
        }

        private MemberInfo _newProperty;
        public MemberInfo NewProperty
        {
            get { return _newProperty; }
            set
            {
                SetField(ref _newProperty, value);
            }
        }

        private Type _newType;
        public Type NewType
        {
            get { return _newType; }
            set
            {
                SetField(ref _newType, value);
            }
        }

        public Type BaseType { get; private set; } // should never change

        public MemberInfo OriginalProperty { get; private set; } // should never change

        public int Ordinal { get; set; }

        public string TableName { get; private set; }

        public bool IsPrimaryKey { get; private set; }

        public string TableReadName { get; private set; }

        public bool WasTableNameChanged { get; protected set; }

        public void ChangeTableName(string tableName)
        {
            WasTableNameChanged = true;
            TableName = tableName;
        }
    }
}
