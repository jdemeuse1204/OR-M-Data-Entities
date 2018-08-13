using FastMember;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;

namespace OR_M_Data_Entities.Lite.Data
{
    public interface IObjectRecord
    {
        Type FromType { get; set; }

        string Location { get; }
        Type Type { get; }
        string FromPropertyName { get; }
        string ForeignKeyProperty { get; }
        ForeignKeyType ForeignKeyType { get; set; }
        MemberSet Members { get; }
        int LevelId { get; }
    }
}
