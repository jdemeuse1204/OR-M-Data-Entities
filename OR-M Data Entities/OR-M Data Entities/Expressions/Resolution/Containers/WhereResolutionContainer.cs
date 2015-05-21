using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Where;
using OR_M_Data_Entities.Expressions.Resolution.Where.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class WhereResolutionContainer : IResolutionContainer
    {
        public Guid Id { get; private set; }

        public WhereResolutionContainer()
        {
            _resolutions = new List<IQueryPart>();
            Id = Guid.NewGuid();
        }

        public bool HasItems
        {
            get
            {
                return _resolutions != null && _resolutions.Count > 0;
            }
        }

        private readonly List<IQueryPart> _resolutions;
        public IReadOnlyList<IQueryPart> Resolutions { get { return _resolutions; } }

        public SqlDbParameter GetParameter(object value)
        {
            return new SqlDbParameter
            {
                Name = string.Format("@Param{0}", _resolutions.Count(w => w is WhereResolutionPart && ((WhereResolutionPart)w).CompareValue is SqlDbParameter)),
                Value = value
            };
        }

        public SqlDbParameter GetParameter(object value, string parameterName)
        {
            return new SqlDbParameter
            {
                Name = parameterName,
                Value = value
            };
        }

        public void AddResolution(WhereResolutionPart resolution)
        {
            resolution.QueryId = Id;

            _resolutions.Add(resolution);
        }

        public void AddGhostResolution(WhereResolutionPart resolution)
        {
            _resolutions.Add(resolution);
        }

        public void AddConnector(ConnectorType connector)
        {
            _resolutions.Add(new SqlConnector
            {
                QueryId = Id,
                Type = connector
            });
        }

        public int NextGroupNumber()
        {
            return _resolutions.Count == 0 ? 0 : _resolutions.Where(w => w is WhereResolutionPart).Select(w => ((WhereResolutionPart)w).Group).Max() + 1;
        }

        public IEnumerable<SqlDbParameter> GetParameters()
        {
            return
                _resolutions.Where(w => w is WhereResolutionPart && ((WhereResolutionPart)w).CompareValue is SqlDbParameter).Select(w => ((WhereResolutionPart)w).CompareValue)
                    .Cast<SqlDbParameter>();
        } 

        public string Resolve()
        {
            var result = "";
            var currentGroupNumber = -1;
            var list =
                _resolutions.Where(
                    w =>
                        (w is WhereResolutionPart) && w.QueryId == Id &&
                        ((WhereResolutionPart) w).CompareValue.IsExpressionQuery()).ToList();

            for (var i = 0; i < list.Count; i++)
            {
                Combine(((WhereResolutionPart)list[i]).CompareValue);
            }

            for (var i = 0; i < _resolutions.Count(w => w.QueryId == Id); i++)
            {
                if (i == 0) result += "(";

                var currentResolution = _resolutions[i];
                var currentLambdaResolition = currentResolution as WhereResolutionPart;
                var nextLambdaResolution = (i + 1) < _resolutions.Count ? _resolutions[i + 1] as WhereResolutionPart : null;

                if (currentLambdaResolition != null)
                {
                    currentGroupNumber = currentLambdaResolition.Group;
                    // on current container

                    // is expression query?

                    result += string.Format("[{0}].[{1}] {2} {3}",
                        currentLambdaResolition.TableName,
                        currentLambdaResolition.ColumnName,
                        currentLambdaResolition.GetComparisonStringOperator(),
                        currentLambdaResolition.CompareValue.IsExpressionQuery()
                            ? string.Format("({0})", _resolveSubQuery(currentLambdaResolition.CompareValue))
                            : ((SqlDbParameter) currentLambdaResolition.CompareValue).Name);

                    continue;
                }

                if (nextLambdaResolution != null)
                {
                    var connector = (SqlConnector) currentResolution;

                    result += nextLambdaResolution.Group != currentGroupNumber
                        ? string.Format(") {0} (", connector.Type) // switching groups
                        : string.Format(" {0} ", connector.Type);
                }
            }

            result += ")";

            return result;
        }

        private void _addConnector(ref string result, SqlConnector connector, bool isChangingGroup)
        {
            // is a connector

        }

        private string _resolveSubQuery(object subQuery)
        {
            var queryable = subQuery as IExpressionQueryable;

            queryable.Query.Resolve();

            return queryable.Query.Sql;
        }

        public void Combine(object subQuery)
        {
            var queryable = subQuery as IExpressionQueryable;

            for (var i = 0; i < queryable.Query.WhereResolution.Resolutions.Count; i++)
            {
                var item = queryable.Query.WhereResolution.Resolutions[i] as WhereResolutionPart;
                var sqlDbParameter = item != null ? item.CompareValue as SqlDbParameter : null;

                if (item == null || sqlDbParameter == null) continue;

                item.CompareValue = GetParameter(sqlDbParameter.Value);

                AddGhostResolution(item);
            }
        }
    }
}
