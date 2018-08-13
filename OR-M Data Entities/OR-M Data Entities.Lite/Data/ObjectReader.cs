using FastMember;
using OR_M_Data_Entities.Lite.Extensions;
using OR_M_Data_Entities.Lite.Mapping;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OR_M_Data_Entities.Lite.Data
{
    public class ObjectReader<T> where T : class
    {
        private readonly List<ObjectRecord> objectTypes;
        private int index;
        private int currentLevel;
        private string lastFromObjectName;
        private string location;
        private static Dictionary<Type, Dictionary<Type, Type>> results;
        private static Dictionary<Type, TypeAccessor> typeAccessors;
        private static Dictionary<Type, MemberSet> memberSets;
        private int nextLevelId;

        public ObjectReader()
        {
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
                results = new Dictionary<Type, Dictionary<Type, Type>>();
            }

            nextLevelId = 0;
            index = 0;
            currentLevel = 0;
            location = "Main";
            lastFromObjectName = string.Empty;

            objectTypes = new List<ObjectRecord>
            {
                new ObjectRecord(typeof(T), string.Empty, string.Empty, string.Empty, nextLevelId)
            };
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
            while (index < objectTypes.Count)
            {
                var objectType = objectTypes[index];

                if (objectType.LevelId > currentLevel)
                {
                    currentLevel++;

                    // change location
                    location = string.Join(".", location, objectType.FromPropertyName);
                }

                nextLevelId++;

                foreach (var member in objectType.Members)
                {
                    var foreignKey = member.GetAttribute<ForeignKeyAttribute>();
                    var memberType = member.Type.Resolve();

                    if (foreignKey != null)
                    {
                        var objectRecord = CreateObjectRecord(member, objectType.Members, objectType.Type, memberType, foreignKey, nextLevelId);

                        objectTypes.Add(objectRecord);
                    }
                }
                index++;

                return true;
            }

            return false;
        }

        public IObjectRecord GetRecord()
        {
            return objectTypes[index - 1];
        }

        private ObjectRecord CreateObjectRecord(Member member, MemberSet allMembers, Type fromType, Type resolvedMemberType, ForeignKeyAttribute foreignKeyAttribute, int splitId)
        {
            var result = new ObjectRecord(resolvedMemberType, member.Name, foreignKeyAttribute.ForeignKeyColumnName, $"{location}.{member.Name}", splitId);
            var isList = member.Type.IsList();
            var foreignKeyType = ForeignKeyType.OneToOne;

            if (!isList)
            {
                // Is not a list
                var foundMember = allMembers.First(w => w.Name == foreignKeyAttribute.ForeignKeyColumnName);

                if (foundMember.Type.IsNullableType())
                {
                    foreignKeyType = ForeignKeyType.NullableOneToOne;
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

        private class ObjectRecord : IObjectRecord
        {
            public ObjectRecord(Type type, string fromPropertyName, string foreignKeyProperty, string location, int levelId)
            {
                Type = type;
                FromPropertyName = fromPropertyName;
                ForeignKeyProperty = foreignKeyProperty;
                TypeAccessor = GetTypeAccessor(type);
                Members = TypeAccessor.GetMembers();
                ForeignKeyType = ForeignKeyType.None;
                Location = location;
                LevelId = levelId;
            }

            public Type FromType { get; set; }

            public string Location { get; }
            public Type Type { get; }
            public string FromPropertyName { get; }
            public string ForeignKeyProperty { get; }
            public ForeignKeyType ForeignKeyType { get; set; }
            public MemberSet Members { get; }
            public int LevelId { get; }

            private TypeAccessor TypeAccessor { get; }
        }
    }
}
