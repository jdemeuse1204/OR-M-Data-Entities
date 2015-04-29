using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Reflection;

namespace OR_M_Data_Entities.Data.Definition
{
    public sealed class ObjectSchematic
    {
        public ObjectSchematic()
        {
            ColumnSchematics = new List<SqlColumnSchematic>();
        }

        public ObjectSchematic(ObjectSchematic detail)
            : this()
        {
            ParentType = detail.ParentType;
            Type = detail.Type;
            PropertyName = detail.PropertyName;
            IsList = detail.IsList;
            ListType = detail.ListType;
            PrimaryKeyDatabaseNames = detail.PrimaryKeyDatabaseNames;
            ChildTypes = detail.ChildTypes;
            IsLazyLoading = detail.IsLazyLoading;
            TableName = detail.TableName;
            TableAlias = detail.TableAlias;
            JoinString = detail.JoinString;
        }

        public string JoinString { get; set; }

        public Type ParentType { get; set; }

        public Type Type { get; set; }

        public string PropertyName { get; set; }

        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public List<SqlColumnSchematic> ColumnSchematics { get; set; }

        public bool IsList { get; set; }

        public Type ListType { get; set; }

        public string[] PrimaryKeyDatabaseNames { get; set; }

        public List<ObjectSchematic> ChildTypes { get; set; }

        public bool IsLazyLoading { get; set; }

        public SqlColumnSchematic FindColumnSchematic(string tableName,string columName, Type type)
        {
            var lookupSkeleton = new SqlColumnSchematic(tableName, columName, type);

            return _findColumnSchematic(this, lookupSkeleton);
        }

        public void RemoveAllColumnSchematics()
        {
            _removeAllColumnSchematics(this);
        }

        private void _removeAllColumnSchematics(ObjectSchematic schematic)
        {
            schematic.ColumnSchematics = new List<SqlColumnSchematic>();

                // not in current look through children
            foreach (var child in schematic.ChildTypes)
            {
                _removeAllColumnSchematics(child);
            }
        }

        public void AddColumnSchematic(string tableName, string columName, Type type)
        {
            AddColumnSchematic(new SqlColumnSchematic(tableName, columName, type));
        }

        public void AddColumnSchematic(SqlColumnSchematic columnSchematic)
        {
            _addColumnSchematic(this, columnSchematic, columnSchematic.TableName);
        }

        private void _addColumnSchematic(ObjectSchematic schematic, SqlColumnSchematic columnSchematic, string tableName = "")
        {
            if (string.IsNullOrWhiteSpace(tableName) && schematic.TableName == columnSchematic.TableName)
            {
                schematic.ColumnSchematics.Add(columnSchematic);
                return;
            }

            if (tableName == columnSchematic.TableName)
            {
                schematic.ColumnSchematics.Add(columnSchematic);
                return;
            }

            foreach (var child in schematic.ChildTypes)
            {
                _addColumnSchematic(child, columnSchematic);
            }
        }

        private SqlColumnSchematic _findColumnSchematic(ObjectSchematic schematic, SqlColumnSchematic lookupSkeleton)
        {
            var index = schematic.ColumnSchematics.IndexOf(lookupSkeleton);

            if (index == -1)
            {
                // not in current look through children
                foreach (var child in schematic.ChildTypes)
                {
                    return _findColumnSchematic(child, lookupSkeleton);
                }
            }

            return ColumnSchematics[index];
        }
    }

    public sealed class PulledForeignKeyDetail : IEquatable<PulledForeignKeyDetail>
    {
        public PulledForeignKeyDetail(PropertyInfo property)
        {
            Type = property.PropertyType;
            ParentType = property.DeclaringType;
            PropertyName = property.Name;
        }

        public Type Type { get; set; }

        public Type ParentType { get; set; }

        public string PropertyName { get; set; }

        public bool Equals(PulledForeignKeyDetail other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return Type == other.Type &&
                ParentType == other.ParentType &&
                PropertyName == other.PropertyName;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + Type.GetHashCode();
            hash = (hash * 7) + ParentType.GetHashCode();
            hash = (hash * 7) + PropertyName.GetHashCode();

            return hash;
        }
    }
}
