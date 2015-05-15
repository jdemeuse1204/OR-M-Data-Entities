using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class WhereResolutionContainer : IResolutionContainer
    {
        public WhereResolutionContainer()
        {
            _resolutions = new List<dynamic>();
            _parameters = new List<SqlDbParameter>();
        }

        public bool HasItems
        {
            get
            {
                return _resolutions != null && _resolutions.Count > 0;
            }
        }

        private readonly List<dynamic> _resolutions;
        public IEnumerable<dynamic> Resolutions { get { return _resolutions; } }

        private readonly List<SqlDbParameter> _parameters;
        public IEnumerable<SqlDbParameter> Parameters { get { return _parameters; } }

        public string AddParameter(object value)
        {
            var parameter = string.Format("@Param{0}", _parameters.Count);

            _parameters.Add(new SqlDbParameter
            {
                Name = parameter,
                Value = value
            });

            return parameter;
        }

        public void AddResolution(LambdaResolution resolution)
        {
            _resolutions.Add(resolution);
        }

        public void AddConnector(SqlConnector connector)
        {
            _resolutions.Add(connector);
        }

        public int NextGroupNumber()
        {
            return _resolutions.Count == 0 ? 0 : _resolutions.Where(w => w.GetType() == typeof(LambdaResolution)).Select(w => w.Group).Max() + 1;
        }

        public string Resolve()
        {
            var result = "";
            var currentGroupNumber = -1;

            for (var i = 0; i < _resolutions.Count; i++)
            {
                if (i == 0) result += "(";

                var currentResolution = _resolutions[i];
                var currentLambdaResolition = currentResolution as LambdaResolution;
                var nextLambdaResolution = (i + 1) < _resolutions.Count ? _resolutions[i + 1] as LambdaResolution : null;

                if (currentLambdaResolition != null)
                {
                    currentGroupNumber = currentLambdaResolition.Group;
                    // on current container

                    // is expression query?

                    result += string.Format("[{0}].[{1}] {2} {3}", currentLambdaResolition.TableName,
                        currentLambdaResolition.ColumnName, currentLambdaResolition.GetComparisonStringOperator(),
                        currentLambdaResolition.CompareValue.IsExpressionQuery()
                            ? string.Format("({0})", _resolveSubQuery(currentLambdaResolition.CompareValue))
                            : currentLambdaResolition.CompareValue);
                    continue;
                }

                if (nextLambdaResolution != null)
                {
                    // is a connector
                    result += nextLambdaResolution.Group != currentGroupNumber
                        ? string.Format(") {0} (", currentResolution) // switching groups
                        : string.Format(" {0} ", currentResolution);
                }
            }

            result += ")";

            return result;
        }

        private string _resolveSubQuery(object subQuery)
        {
            // resolve sub query
            var eq = ((dynamic)subQuery);
            eq.Query.Resolve();
            return eq.Query.Sql;
        }
    }
}
