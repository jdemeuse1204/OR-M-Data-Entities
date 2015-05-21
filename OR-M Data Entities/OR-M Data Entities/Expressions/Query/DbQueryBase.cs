using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution.Containers;

namespace OR_M_Data_Entities.Expressions.Query
{
    public abstract class DbQueryBase
    {
        public readonly WhereResolutionContainer WhereResolution;
        public readonly JoinResolutionContainer JoinResolution;
        public readonly Type BaseType;
        public readonly SelectInfoResolutionContainer SelectList;

        protected readonly List<Type> _types;
        public IEnumerable<Type> Types {
            get { return _types; }
        }

        public object Shape { get; protected set; }

        public string Sql { get; protected set; }

        protected DbQueryBase(Type baseType = null, 
            WhereResolutionContainer whereResolution = null, 
            SelectInfoResolutionContainer selectInfoCollection = null, 
            JoinResolutionContainer joinResolution = null, 
            List<Type> types = null)
        {
            WhereResolution = whereResolution ?? new WhereResolutionContainer();
            JoinResolution = joinResolution ?? new JoinResolutionContainer();
            SelectList = selectInfoCollection ?? new SelectInfoResolutionContainer();
            BaseType = baseType;
            _types = types ?? new List<Type>();
            Shape = baseType;
        }

        protected DbQueryBase(DbQueryBase query)
        {
            WhereResolution = query.WhereResolution;
            SelectList = query.SelectList;
            JoinResolution = query.JoinResolution;
            BaseType = query.BaseType;
            Shape = query.Shape;
            _types = query._types;
        }       
    }

    class SqlType
    {
        public SqlType(Type type, string tableName = "")
        {
            Type = type;
            TableName = string.IsNullOrWhiteSpace(tableName) ? DatabaseSchemata.GetTableName(type) : tableName;
        }

        public Type Type { get; set; }

        public string TableName { get; set; }
    }
}
