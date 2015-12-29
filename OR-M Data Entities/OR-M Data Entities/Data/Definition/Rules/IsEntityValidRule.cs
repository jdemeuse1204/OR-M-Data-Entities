using System;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Definition.Rules
{
    public sealed class IsEntityValidRule : IRule
    {
        private readonly object _entity;

        public IsEntityValidRule(object entity)
        {
            _entity = entity;
        }

        public void Process()
        {
            var primaryKeys = Entity.GetPrimaryKeys(_entity);
            var plainTableName = _entity.GetTableName();
            var entityType = _entity.GetType();
            var linkedServerAttribute = entityType.GetCustomAttribute<LinkedServerAttribute>();
            var schemaAttribute = entityType.GetCustomAttribute<SchemaAttribute>();

            if (primaryKeys.Count == 0)
            {
                throw new InvalidTableException(string.Format("{0} must have at least one Primary Key defined", plainTableName));
            }

            if (linkedServerAttribute != null && schemaAttribute != null)
            {
                throw new Exception(
                    string.Format(
                        "Class {0} cannot have LinkedServerAttribute and SchemaAttribute, use one or the other",
                        entityType.Name));
            }
        }
    }
}
