using System;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data.PayloadOperations.LambdaResolution;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectSelectBuilder : ObjectQueryBuilder
    {
        public void SelectAll<T>() where T : class
        {
            // rename for asethetics
            if (Map != null && Map.BaseType != null && Map.BaseType == typeof(T))
            {
                throw new Exception("Cannot return more than one data type");
            }

            Table<T>();
        }

        public void Take(int rows)
        {
            Map.Rows = rows;
        }

        public void AddWhere<T>(Expression<Func<T, bool>> expression) where T : class
        {
            LambdaResolver.ResolveWhereExpression(expression, Map);
        }

        public void AddInnerJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
            where TParent : class
            where TChild : class
        {
            LambdaResolver.ResolveJoinExpression(expression, JoinType.Inner, Map);
        }

        public void AddLeftJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
            where TParent : class
            where TChild : class
        {
            LambdaResolver.ResolveJoinExpression(expression, JoinType.Left, Map);
        }

        private bool _hasJoins()
        {
            return Map.Tables.Any(w => w.Columns.Any(x => x.HasJoins));
        }

        private bool _hasWheres()
        {
            return Map.Tables.Any(w => w.Columns.Any(x => x.IsPartOfValidation));
        }

        private T _createInstance<T>() where T : Resolver
        {
            return
                Activator.CreateInstance(typeof (T), new object[] {Map}) as T;
        }

        public override BuildContainer Build()
        {
            var resolver = _getResolver();

            return resolver.Resolve();
        }

        private Resolver _getResolver()
        {
            var hasJoins = _hasJoins();
            var hasWheres = _hasWheres();

            // Check for foreign keys
            if (DatabaseSchemata.HasForeignKeys(Map.BaseType))
            {
                if (hasJoins)
                {
                    if (hasWheres)
                    {
                        return _createInstance<ForeignKeySelectWhereJoinResolver>();
                    }

                    return _createInstance<ForeignKeySelectJoinResolver>();
                }

                if (hasWheres)
                {
                    return _createInstance<ForeignKeySelectWhereResolver>();
                }

                return _createInstance<ForeignKeySelectResolver>();
            }

            // no foreign keys exist
            if (hasJoins)
            {
                if (hasWheres)
                {
                    return _createInstance<SelectWhereJoinResolver>();
                }

                return _createInstance<SelectJoinResolver>();
            }

            if (hasWheres)
            {
                return _createInstance<SelectWhereResolver>();
            }

            return _createInstance<SelectResolver>();
        }
    }
}
