/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Commands.StatementParts
{
    public sealed class SqlJoin
    {
        public SqlTableColumnPair ParentEntity { get; set; }

        public SqlTableColumnPair JoinEntity { get; set; }

        public JoinType Type { get; set; }

        public string GetJoinText()
        {
            var parentTableName = DatabaseSchemata.GetTableName(ParentEntity.Table);
            var parentColumnName = DatabaseSchemata.GetColumnName(ParentEntity.Column);
            var joinTableName = DatabaseSchemata.GetTableName(JoinEntity.Table);
            var joinColumnName = DatabaseSchemata.GetColumnName(JoinEntity.Column);

            switch (Type)
            {
                case JoinType.Equi:
                    return string.Format("[{0}].[{1}] = [{2}].[{3}]", 
                        parentTableName,
                        parentColumnName,
                        joinTableName,
                        joinColumnName);
                case JoinType.Inner:
                    return string.Format("INNER JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}]",
                        joinTableName,
                        joinColumnName,
                        parentTableName,
                        parentColumnName);
                case JoinType.Left:
                    return string.Format("LEFT JOIN [{0}] On [{0}].[{1}] = [{2}].[{3}]",
                        joinTableName,
                        joinColumnName,
                        parentTableName,
                        parentColumnName);
                default:
                    return "";
            }
        }
    }
}
