using System;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class IsTableValidRule : IRule
    {
        private readonly Type _type;

        public IsTableValidRule(Type type)
        {
            _type = type;
        }

        public void Process()
        {
            var linkedServerAttribute = _type.GetCustomAttribute<LinkedServerAttribute>();
            var schemaAttribute = _type.GetCustomAttribute<SchemaAttribute>();

            if (linkedServerAttribute != null && schemaAttribute != null)
            {
                throw new Exception(
                    string.Format(
                        "Class {0} cannot have LinkedServerAttribute and SchemaAttribute, use one or the other",
                        _type.Name));
            }
        }
    }
}
