﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Scripts.Base;
using System.Collections;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseExecution : DatabaseSchematic
    {
        #region Constructor
        protected DatabaseExecution(string connectionStringOrName)
            : base(connectionStringOrName)
        {
            
        }
        #endregion

        #region Query Execution
        public IDataTranslator<T> ExecuteQuery<T>(string sql)
        {
            ExecuteReader(sql);

            return new DataTranslator<T>(Reader);
        }

        public IDataTranslator<T> ExecuteQuery<T>(string sql, List<SqlDbParameter> parameters, IQuerySchematic schematic = null)
        {
            ExecuteReader(sql, parameters, schematic);

            return new DataTranslator<T>(Reader);
        }

        public IDataTranslator<T> ExecuteQuery<T>(string sql, params SqlDbParameter[] parameters)
        {
            return ExecuteQuery<T>(sql, parameters.ToList());
        }

        public IDataTranslator<T> ExecuteQuery<T>(ISqlExecutionPlan builder)
        {
            ExecuteReader(builder);

            return new DataTranslator<T>(Reader);
        }


        public void ExecuteQuery(string sql, List<SqlDbParameter> parameters)
        {
            ExecuteReader(sql, parameters);

            Disconnect();
        }

        public void ExecuteQuery(string sql, params SqlDbParameter[] parameters)
        {
            ExecuteQuery(sql, parameters.ToList());
        }
        #endregion

        #region Stored Procedures And Functions
        public IDataTranslator<T> ExecuteScript<T>(IReadScript<T> script)
        {
            return _executeScript<T>(script as dynamic);
        }

        public void ExecuteScript(IWriteScript script)
        {
            _executeScript(script as dynamic);
        }

        private IDataTranslator<T> _executeScript<T>(StoredProcedure<T> script)
        {
            var package = _getStoredProcedureParametersAndSql(script);

            return ExecuteQuery<T>(package.Key, package.Value);
        }


        private IDataTranslator<T> _executeScript<T>(CustomScript<T> script)
        {
            var package = _getCustomScriptParametersAndSql(script);

            return ExecuteQuery<T>(package.Key, package.Value);
        }

        private IDataTranslator<T> _executeScript<T>(StoredScript<T> script)
        {
            var package = _getStoredScriptParametersAndSql(script);

            return ExecuteQuery<T>(package.Key, package.Value);
        }

        private IDataTranslator<T> _executeScript<T>(ScalarFunction<T> script)
        {
            var package = _getStoredProcedureParametersAndSql(script);

            return ExecuteQuery<T>(package.Key, package.Value);
        }

        private void _executeScript(StoredProcedure script)
        {
            var package = _getStoredProcedureParametersAndSql(script);

            ExecuteQuery(package.Key, package.Value);

            Disconnect();
        }


        private void _executeScript(CustomScript script)
        {
            var package = _getCustomScriptParametersAndSql(script);

            ExecuteQuery(package.Key, package.Value);

            Disconnect();
        }

        private void _executeScript(StoredScript script)
        {
            var package = _getStoredScriptParametersAndSql(script);

            ExecuteQuery(package.Key, package.Value);

            Disconnect();
        }

        private KeyValuePair<string, List<SqlDbParameter>> _getCustomScriptParametersAndSql(CustomScript script)
        {
            return new KeyValuePair<string, List<SqlDbParameter>>(script.GetSql(), _getParameters(script));
        }

        private KeyValuePair<string, List<SqlDbParameter>> _getStoredScriptParametersAndSql(StoredScript script)
        {
            return new KeyValuePair<string, List<SqlDbParameter>>(_getSqlFromFile(script), _getParameters(script));
        }

        private KeyValuePair<string, List<SqlDbParameter>> _getStoredProcedureParametersAndSql(StoredProcedure script)
        {
            var scriptType = script.GetType();
            var schemaAttribute = scriptType.GetCustomAttribute<SchemaAttribute>();
            var scriptAttribute = scriptType.GetCustomAttribute<ScriptAttribute>();
            var config = ConfigurationManager.GetSection(ORMDataEntitiesConfigurationSection.SectionName) as ORMDataEntitiesConfigurationSection;
            var schema = schemaAttribute != null
                ? schemaAttribute.SchemaName
                : config != null ? config.StoredSql.First("schema").DefaultValue : "dbo";
            bool wasIndexUsed;
            var parameters = _getParameters(script, out wasIndexUsed);
            var scriptName = scriptAttribute != null ? scriptAttribute.ScriptName : scriptType.Name;
            var scriptSplit = scriptName.Split(' ');
            var scriptHasParameters = scriptSplit.Count() >= 2 && scriptSplit[1].Contains("@");
            var baseType = script.GetType().BaseType;
            var isScalarFunction = baseType != null && (baseType.IsGenericType &&
                                                        baseType.GetGenericTypeDefinition()
                                                            .IsAssignableFrom(typeof(ScalarFunction<>)));

            if (parameters.Count > 1 && !scriptHasParameters && !wasIndexUsed)
            {
                throw new Exception(
                    "ScriptName has no parameters when parameters are specified.  Please either use the index attribute on your parameters denoting the order they should be in OR include the attributes in your script name");
            }

            if ((wasIndexUsed) || (parameters.Count == 1 && !scriptHasParameters))
            {
                if (scriptHasParameters) throw new Exception("Script Name cannot have parameters if Index is being used.");

                // add parameters into the script
                scriptName = isScalarFunction
                    ? _getScalarFunctionScriptName(scriptName, parameters)
                    : _getStoredProcedureScriptName(scriptName, parameters);
            }

            return
                new KeyValuePair<string, List<SqlDbParameter>>(
                    string.Format(isScalarFunction ? "Select {0}.{1}" : "{0}.{1}", schema, scriptName), parameters);
        }

        private string _getStoredProcedureScriptName(string scriptName, List<SqlDbParameter> parameters)
        {
            return
                parameters.Aggregate(scriptName,
                    (current, sqlDbParameter) => current + string.Format(" @{0},", sqlDbParameter.Name)).TrimEnd(',');
        }

        private string _getScalarFunctionScriptName(string scriptName, List<SqlDbParameter> parameters)
        {
            var parametersString = parameters.Aggregate(string.Empty,
                (current1, parameter) => current1 + string.Format("@{0},", parameter.Name)).TrimEnd(',');

            return string.Format("{0}({1})", scriptName, parametersString);
        }

        private List<SqlDbParameter> _getParameters(IScript script)
        {
            return script.GetType().GetProperties().Select(w => new SqlDbParameter(w.Name, w.GetValue(script))).ToList();
        }

        private List<SqlDbParameter> _getParameters(StoredProcedure script, out bool wasIndexUsed)
        {
            var namesToSkip = new string[] { "ScriptName", "SchemaName" };
            var properties = script.GetType().GetProperties().Where(w => !namesToSkip.Contains(w.Name)).ToList();
            wasIndexUsed = properties.Any(w => w.GetCustomAttribute<Index>() != null);

            if (wasIndexUsed && !properties.All(w => w.GetCustomAttribute<Index>() != null))
            {
                throw new Exception("If Index attribute is used, all properties must have the index attribute!");
            }

            return wasIndexUsed
                ? properties.OrderBy(w => w.GetCustomAttribute<Index>().Value)
                    .Select(w => new SqlDbParameter(w.Name, w.GetValue(script)))
                    .ToList()
                : properties.Select(w => new SqlDbParameter(w.Name, w.GetValue(script)))
                    .ToList();
        }

        private string _getSqlFromFile(StoredScript storedScript)
        {
            var scriptType = storedScript.GetType();
            var scriptAttribute = scriptType.GetCustomAttribute<ScriptAttribute>();
            var scriptPathAttribute = scriptType.GetCustomAttribute<ScriptPathAttribute>();
            var config = ConfigurationManager.GetSection(ORMDataEntitiesConfigurationSection.SectionName) as ORMDataEntitiesConfigurationSection;
            var pathFromScript = scriptPathAttribute != null ? scriptPathAttribute.Path : string.Empty;
            var path = !string.IsNullOrWhiteSpace(pathFromScript)
                ? pathFromScript
                : config != null ? config.StoredSql.First("path").DefaultValue : "../../Scripts/";
            var fileName = scriptAttribute == null ? storedScript.GetType().Name : scriptAttribute.ScriptName;
            var fullScriptPath = Path.Combine(path,
                string.Format("{0}{1}", fileName, ".sql"));
            var result = string.Empty;

            // open the file as readonly and read line by line in case the script is large
            using (var fs = new FileStream(fullScriptPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        result += string.Format("{0} ", line);
                    }
                }
            }

            return result;
        }

        #endregion

        #region Data Translator
        private class DataTranslator<T> : IDataTranslator<T>
        {
            #region Properties and Fields
            private readonly IPeekDataReader _reader;

            public bool HasRows
            {
                get { return _reader.HasRows; }
            }
            #endregion

            #region Constructor
            public DataTranslator(IPeekDataReader reader)
            {
                _reader = reader;
            }
            #endregion

            #region Methods
            public T FirstOrDefault()
            {
                _reader.Read();

                var result = _reader.ToObjectDefault<T>();

                Dispose();

                return result;
            }

            public T First()
            {
                _reader.Read();

                var result = _reader.ToObject<T>();

                Dispose();

                return result;
            }

            public List<T> ToList()
            {
                var result = new List<T>();

                while (_reader.Read()) result.Add(_reader.ToObject<T>());

                Dispose();

                return result;
            }

            public IEnumerator<T> GetEnumerator()
            {
                while (_reader.Read()) yield return _reader.ToObject<T>();

                // close when done enumerating
                Dispose();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Dispose()
            {
                _reader.Close();
                _reader.Dispose();
            }
            #endregion
        }
        #endregion
    }
}
