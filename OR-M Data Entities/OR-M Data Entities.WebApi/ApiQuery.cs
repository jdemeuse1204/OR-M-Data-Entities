using System;
using Newtonsoft.Json;

namespace OR_M_Data_Entities.WebApi
{
    public static class ApiQuery
    {
        public static object ProcessQuery(this DbSqlContext context, string query)
        {
            if (string.IsNullOrEmpty(query)) return false;

            try
            {
                var parsedQuery = JsonConvert.DeserializeObject<Query>(query);

                return false;
            }
            catch (JsonReaderException ex)
            {
                throw new Exception("query is not valid, please see documentation for query formation.", ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private class Query
        {
            public Read read { get; set; }

            public Save save { get; set; }

            public Delete delete { get; set; }
        }

        private class Read : IQuery
        {
            #region Select
            public string first { get; set; }

            public string tolist { get; set; }

            public string min { get; set; }

            public string max { get; set; }

            public string count { get; set; }
            #endregion

            public string from { get; set; }

            public string where { get; set; }

            public string include { get; set; }
        }

        private class Save : IQuery
        {
            public string from { get; set; }

            public string entity { get; set; }
        }

        private class Delete : IQuery
        {
            public string from { get; set; }

            public string entity { get; set; }
        }

        private interface IQuery
        {
             string from { get; set; }
        }
    }
}
