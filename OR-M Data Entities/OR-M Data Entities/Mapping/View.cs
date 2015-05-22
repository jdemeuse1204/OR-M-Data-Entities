using System;

namespace OR_M_Data_Entities.Mapping
{
     [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewAttribute : Attribute
    {
        public ViewAttribute(params string[] viewIds)
        {
            ViewIds = viewIds;
        }

        public string[] ViewIds { get; private set; }
    }
}
