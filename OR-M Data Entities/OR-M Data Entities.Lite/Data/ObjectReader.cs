using FastMember;
using OR_M_Data_Entities.Lite.Extensions;
using OR_M_Data_Entities.Lite.Mapping;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Lite.Data
{
    public class ObjectReader
    {
        private List<IObjectRecord> objectTypes;
        private int index;
        private string lastFromObjectName;
        private int nextLevelId;
        private int stepId;
        private readonly Type type;
        private readonly bool readFromCache;

        private static Dictionary<Type, List<IObjectRecord>> results;
        private static Dictionary<Type, TypeAccessor> typeAccessors;
        private static Dictionary<Type, MemberSet> memberSets;

        public ObjectReader(Type type)
        {
            readFromCache = false;

            if (typeAccessors == null)
            {
                typeAccessors = new Dictionary<Type, TypeAccessor>();
            }

            if (memberSets == null)
            {
                memberSets = new Dictionary<Type, MemberSet>();
            }

            if (results == null)
            {
                results = new Dictionary<Type, List<IObjectRecord>>();
            }

            nextLevelId = 0;
            stepId = 0;
            index = 0;
            lastFromObjectName = string.Empty;
            this.type = type;

            if (results.ContainsKey(type))
            {
                readFromCache = true;
            }
            else
            {
                objectTypes = new List<IObjectRecord>
                {
                    new ObjectRecord(type, string.Empty, string.Empty, "0", "0", null, type)
                };
            }
        }

        private static TypeAccessor GetTypeAccessor(Type type)
        {
            if (typeAccessors.ContainsKey(type))
            {
                return typeAccessors[type];
            }

            var typeAccessor = TypeAccessor.Create(type);

            typeAccessors.Add(type, typeAccessor);

            return typeAccessor;
        }

        public bool Read()
        {
            if (readFromCache)
            {
                objectTypes = results[type];

                while (index < objectTypes.Count)
                {
                    index++;
                    return true;
                }

                return false;
            }
            else
            {
                while (index < objectTypes.Count)
                {
                    var objectType = objectTypes[index];

                    nextLevelId++;

                    foreach (var member in objectType.Members)
                    {
                        var foreignKey = member.GetAttribute<ForeignKeyAttribute>();
                        var memberType = member.Type.Resolve();

                        if (foreignKey != null)
                        {
                            var objectRecord = CreateObjectRecord(member, objectType.Members, objectType.Type, memberType, foreignKey, $"{nextLevelId}_{stepId}", objectType.LevelId, objectType.ForeignKeyType, objectType, member.Type);
                            stepId++;
                            objectTypes.Add(objectRecord);
                        }
                    }
                    index++;
                    stepId = 0;
                    return true;
                }

                // store result
                results.Add(type, objectTypes);

                return false;
            }
        }

        public IObjectRecord GetRecord()
        {
            return objectTypes[index - 1];
        }

        public IObjectRecord Find(string levelId)
        {
            return objectTypes.FirstOrDefault(w => w.LevelId == levelId);
        }

        private ObjectRecord CreateObjectRecord(Member member, MemberSet allMembers, Type fromType, Type resolvedMemberType, ForeignKeyAttribute foreignKeyAttribute, string levelId, string parentLevelId, ForeignKeyType parentJoinType, IObjectRecord parentObjectRecord, Type actualType)
        {
            var result = new ObjectRecord(resolvedMemberType, member.Name, foreignKeyAttribute.ForeignKeyColumnName, levelId, parentLevelId, parentObjectRecord, actualType);
            var isList = member.Type.IsList();
            var foreignKeyType = ForeignKeyType.OneToOne;

            // if the parent is left joined, then the 
            // children need to be left joined,
            // inner joined will cause the query to fail if 
            // no results are found
            if (!isList)
            {
                var isParentNullableOrLeftJoin = parentJoinType == ForeignKeyType.LeftOneToOne || parentJoinType == ForeignKeyType.NullableOneToOne || parentJoinType == ForeignKeyType.OneToMany;

                foreignKeyType = isParentNullableOrLeftJoin ? ForeignKeyType.LeftOneToOne : ForeignKeyType.OneToOne;

                // Is not a list
                var foundMember = allMembers.First(w => w.Name == foreignKeyAttribute.ForeignKeyColumnName);

                if (foundMember.Type.IsNullableType())
                {
                    foreignKeyType = isParentNullableOrLeftJoin ? ForeignKeyType.LeftOneToOne : ForeignKeyType.NullableOneToOne;
                }
            }
            else
            {
                foreignKeyType = ForeignKeyType.OneToMany;
            }

            result.ForeignKeyType = foreignKeyType;
            result.FromType = fromType;

            return result;
        }

        public static IObjectRecord GetFuzzyObjectRecord(Type baseType, Type type)
        {
            return results[baseType].First(w => w.Type == type);
        }

        private class ObjectRecord : IObjectRecord
        {
            public ObjectRecord(Type type, string fromPropertyName, string foreignKeyProperty, string levelId, string parentLevelId, IObjectRecord parentObjectRecord, Type actualType)
            {
                Type = type;
                FromPropertyName = fromPropertyName;
                ForeignKeyProperty = foreignKeyProperty;
                TypeAccessor = GetTypeAccessor(type);
                Members = TypeAccessor.GetMembers();
                ForeignKeyType = ForeignKeyType.None;
                LevelId = levelId;
                ParentLevelId = parentLevelId;
                ParentObjectRecord = parentObjectRecord;
                ActualType = actualType;
            }

            public Type FromType { get; set; }

            public Type Type { get; }
            public Type ActualType { get; }
            public string FromPropertyName { get; }
            public string ForeignKeyProperty { get; }
            public ForeignKeyType ForeignKeyType { get; set; }
            public MemberSet Members { get; }
            public string LevelId { get; }
            public string ParentLevelId { get; }

            public TypeAccessor TypeAccessor { get; }
            public IObjectRecord ParentObjectRecord { get; }
        }
    }
}
