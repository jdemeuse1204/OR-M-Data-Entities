using FastMember;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Lite.Data
{
    public interface IObjectRecord
    {
        Type FromType { get; set; }
        object DataBag { get; set; }
        List<object> DataBags { get; set; }

        Type Type { get; }
        Type ActualType { get; }
        string FromPropertyName { get; }
        string ForeignKeyProperty { get; }
        ForeignKeyType ForeignKeyType { get; set; }
        MemberSet Members { get; }
        string LevelId { get; }
        string ParentLevelId { get; }

        TypeAccessor TypeAccessor { get; }
        IObjectRecord ParentObjectRecord { get; }
    }
}
