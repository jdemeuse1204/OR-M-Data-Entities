using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Expressions.Resolution.Where;
using OR_M_Data_Entities.Expressions.Resolution.Where.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class WhereResolutionContainer : ResolutionContainerBase, IResolutionContainer
    {
        #region Properties
        public bool HasItems
        {
            get
            {
                return _resolutions != null && _resolutions.Count > 0;
            }
        }

        public void Combine(IResolutionContainer container)
        {
            _resolutions = container.GetType()
                .GetField("_resolutions", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(container) as List<IQueryPart>;

            _parameters = container.GetType()
                .GetField("_parameters", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(container) as List<SqlDbParameter>;
        }

        public IReadOnlyList<IQueryPart> Resolutions { get { return _resolutions; } }
        #endregion

        #region Fields

        private List<IQueryPart> _resolutions;

        private List<SqlDbParameter> _parameters;
        public IReadOnlyList<SqlDbParameter> Parameters { get { return _parameters; } }
        #endregion

        #region Constructor
        public WhereResolutionContainer(Guid expressionQueryId)
            : base(expressionQueryId)
        {
            _resolutions = new List<IQueryPart>();
            _parameters = new List<SqlDbParameter>();
        }
        #endregion

        #region Constructor

        #endregion


        public SqlDbParameter GetAndAddParameter(object value)
        {
            var parameter = new SqlDbParameter
            {
                Name = string.Format("@Param{0}",_parameters.Count),
                Value = value
            };

            // do not add if its an expression query because the parameters will get added to the main query upon resolution
            if (!value.IsExpressionQuery())
            {
                _parameters.Add(parameter);
            }

            return parameter;
        }

        public void AddResolution(WhereResolutionPart resolution)
        {
            resolution.QueryId = ExpressionQueryId;

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
                QueryId = ExpressionQueryId,
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

        public void ClearResolutions()
        {
            _resolutions.Clear();
        }

        public string Resolve()
        {
            var result = "";
            var currentGroupNumber = -1;
            // combine parameters from sub query

            var resolutions = _resolutions.Where(w => w.QueryId == ExpressionQueryId).ToList();

            for (var i = 0; i < resolutions.Count; i++)
            {
                if (i == 0) result += "(";

                var currentResolution = resolutions[i];
                var currentLambdaResolition = currentResolution as WhereResolutionPart;
                var nextLambdaResolution = (i + 1) < resolutions.Count ? resolutions[i + 1] as WhereResolutionPart : null;

                if (currentLambdaResolition != null)
                {
                    currentGroupNumber = currentLambdaResolition.Group;
                    // on current container

                    result += string.Format("[{0}].[{1}] {2} {3}",
                        currentLambdaResolition.TableAlias,
                        currentLambdaResolition.ColumnName,
                        currentLambdaResolition.GetComparisonStringOperator(),
                        currentLambdaResolition.CompareValue is SqlDbParameter && ((SqlDbParameter)currentLambdaResolition.CompareValue).Value.IsExpressionQuery()
                            ? string.Format("({0})", _resolveSubQuery((SqlDbParameter)currentLambdaResolition.CompareValue))
                            : ((SqlDbParameter) currentLambdaResolition.CompareValue).Name);

                    // can be a list of sqldb parameters

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

        private string _resolveSubQuery(SqlDbParameter subQuery)
        {
            var queryable = (IExpressionQueryResolvable)subQuery.Value;

            // add parameters to main here and offset parameters in main
            queryable.ResolveExpression();

            return queryable.Sql;
        }
    }
}
