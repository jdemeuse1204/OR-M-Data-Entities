using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    public enum ForeignKeyType
    {
        NullableOneToOne,
        LeftOneToOne,
        OneToOne,
        OneToMany,
        None
    }
}
