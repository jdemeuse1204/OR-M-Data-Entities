using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Expressions.Resolution.Containers
{
    public class WhereResolutionContainer
    {
        public WhereResolutionContainer()
        {
            _resolutions = new List<dynamic>();
            _parameters = new List<SqlDbParameter>();
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
    }
}
