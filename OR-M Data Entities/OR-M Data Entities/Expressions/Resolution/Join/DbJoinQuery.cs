using System;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Containers;
using OR_M_Data_Entities.Expressions.Resolution.Select;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public abstract class DbJoinQuery<T> : DbSelectQuery<T>
    {
        public readonly JoinResolutionContainer JoinResolution;

        protected DbJoinQuery()
            : base()
        {
            
        }

        protected void InitializeJoins()
        {
            
        }

        private JoinGroupInfo _getJoinGroupInfo(PropertyInfo property)
        {
            var isList = property.PropertyType.IsList();
            return new JoinGroupInfo
            {
                IsList = isList,
                Type = isList
                ? property.PropertyType.GetGenericArguments()[0]
                : property.PropertyType
            };
        }

        protected JoinGroup GetJoinGroup(PseudoKeyAttribute pseudoKeyAttribute, PropertyInfo property, string parentTableName, bool isParentSearch)
        {
            var result = new JoinGroup();
            var info = _getJoinGroupInfo(property);
            var tableName = DatabaseSchemata.GetTableName(info.Type);

            result.JoinType = info.IsList ? JoinType.PseudoKeyLeft : JoinType.PseudoKeyInner;

            result.Left = new LeftJoinNode
            {
                ColumnName = pseudoKeyAttribute.ParentTableColumnName,
                TableName = tableName,
                TableAlias = property.Name,
                Change = SelectList.TableAliases.FirstOrDefault(w => w.ForeignKeyTableName == property.Name)
            };

            result.Right = new RightJoinNode
            {
                ColumnName = pseudoKeyAttribute.ChildTableColumnName,
                TableName = parentTableName
            };

            return result;
        }

        protected JoinGroup GetJoinGroup(ForeignKeyAttribute foreignKeyAttribute, PropertyInfo property, string parentTableName, bool isParentSearch)
        {
            var result = new JoinGroup();
            var info = _getJoinGroupInfo(property);
            string tableName;

            if (info.IsList)
            {
                result.JoinType = JoinType.ForeignKeyLeft;
                tableName = DatabaseSchemata.GetTableName(info.Type);

                result.Left = new LeftJoinNode
                {
                    ColumnName = foreignKeyAttribute.ForeignKeyColumnName,
                    TableName = tableName,
                    TableAlias = property.Name,
                    Change = SelectList.TableAliases.FirstOrDefault(w => w.ForeignKeyTableName == property.Name)
                };

                var rightChange = SelectList.TableAliases.FirstOrDefault(w => w.TableName == parentTableName);

                rightChange = rightChange ?? SelectList.TableAliases.FirstOrDefault(w => w.ForeignKeyTableName == parentTableName);

                result.Right = new RightJoinNode
                {
                    ColumnName = DatabaseSchemata.GetColumnName(DatabaseSchemata.GetPrimaryKeys(info.Type).First()),
                    TableName = parentTableName,
                    Change = rightChange
                };
            }
            else
            {
                result.JoinType = JoinType.ForeignKeyInner;
                tableName = DatabaseSchemata.GetTableName(info.Type);

                result.Left = new LeftJoinNode
                {
                    ColumnName = DatabaseSchemata.GetColumnName(DatabaseSchemata.GetPrimaryKeys(info.Type).First()),
                    TableName = tableName,
                    TableAlias = property.Name,
                    Change = SelectList.TableAliases.FirstOrDefault(w => w.ForeignKeyTableName == property.Name)
                };

                var rightChange = SelectList.TableAliases.FirstOrDefault(w => w.TableName == parentTableName);

                rightChange = rightChange ?? SelectList.TableAliases.FirstOrDefault(w => w.ForeignKeyTableName == parentTableName);

                result.Right = new RightJoinNode
                {
                    ColumnName = foreignKeyAttribute.ForeignKeyColumnName,
                    TableName = parentTableName,
                    Change = rightChange
                };
            }

            return result;
        }
    }

    class JoinGroupInfo
    {
        public bool IsList { get; set; }

        public Type Type { get; set; }
    }
}
