﻿using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.QueryResolution
{
    public class SelectWhereResolver : Resolver
    {
        public SelectWhereResolver(ObjectMap map)
            : base(map)
        {
        }

        public override BuildContainer Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
