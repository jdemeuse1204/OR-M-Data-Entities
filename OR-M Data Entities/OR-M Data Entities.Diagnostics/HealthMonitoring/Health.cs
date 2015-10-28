using System.Collections.Generic;

namespace OR_M_Data_Entities.Diagnostics.HealthMonitoring
{
    public class Health
    {
        public Health()
        {
            _fails = new List<HealthError>();
            _passes = new List<HealthCheck>();
        }

        public bool IsHealthy
        {
            get { return _fails.Count == 0; }
        }

        public decimal Percentage
        {
            get
            {
                return _passes.Count == 0 && _fails.Count == 0 || _passes.Count > 0 && _fails.Count == 0
                    ? 100
                    : ((_fails.Count/(_fails.Count + _passes.Count))*100);
            }
        }

        public IEnumerable<HealthError> Fails {
            get { return _fails; }
        }
        public List<HealthError> _fails { get; set; }

        public IEnumerable<HealthCheck> Passes {
            get { return _passes; }
        }
        public List<HealthCheck> _passes { get; set; }

        public void Add(HealthError error)
        {
            _fails.Add(error);
        }

        public void Add(HealthCheck check)
        {
            _passes.Add(check);
        }
    }

    public class HealthCheck
    {
        public HealthCheck(Check check)
        {
            Check = check;
        }

        public readonly Check Check;
    }

    public class HealthError : HealthCheck
    {
        public readonly string Remediation;

        public HealthError(Check check, string remediation)
            : base(check)
        {
            Remediation = remediation;
        }
    }
}
