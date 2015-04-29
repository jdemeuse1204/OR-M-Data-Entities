using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Expressions
{
    public class SqlQuery
    {
        public SqlQuery()
        {
            From = String.Empty;
            Where = new List<string>();
            Join = new List<string>();
            OrderBy = new List<string>();
            _parameters = new List<SqlDbParameter>();
        }

        public ObjectSchematic Schematic { get; set; }

        public bool IsLazyLoading { get; set; }

        public Type ReturnType { get; set; }

        public int Take { get; set; }

        public bool Distinct { get; set; }

        public string From { get; set; }

        public List<string> Where { get; set; }

        public List<string> Join { get; set; }

        public List<string> OrderBy { get; set; }

        public bool IsFirstOrDefault { get; set; }

        private readonly List<SqlDbParameter> _parameters;

        public IEnumerable<SqlDbParameter> Parameters
        {
            get { return _parameters; }
        }

        public string GetNextParameter()
        {
            return String.Format("@Param{0}", _parameters.Count);
        }

        public void AddParameter(string parameter, object value)
        {
            _parameters.Add(new SqlDbParameter(parameter, value));
        }

        public string ToSql()
        {
            var select = string.Format("SELECT {0}{1}", Take > 0 ? string.Format("TOP {0} ", Take) : "",
                Distinct ? "DISTINCT " : "");

            var columns = Schematic.GetColumnSql();

            var where = Where.Aggregate(string.Empty,
                (current, column) =>
                    string.IsNullOrWhiteSpace(current)
                        ? string.Format("WHERE {0} ", column)
                        : current + string.Format("AND {0} ", column));

            var joins = Schematic.HasJoins() ? Schematic.GetJoinSql() : string.Empty;

            var sql = string.Format("{0}{1} FROM {2}{3}", select, columns, joins, where);

            return sql;
        }
    }
}
