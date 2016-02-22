/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Xml;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses DataFetching functions to Save, Delete
    /// </summary>
    public abstract partial class DatabaseModifiable : DatabaseQuery
    {
        #region Events And Delegates

        protected delegate void OnBeforeSaveHandler(object entity, UpdateType updateType);

        protected event OnBeforeSaveHandler OnBeforeSave;

        protected delegate void OnAfterSaveHandler(object entity, UpdateType actualUpdateType);

        protected event OnAfterSaveHandler OnAfterSave;

        protected delegate void OnSavingHandler(object entity);

        protected event OnSavingHandler OnSaving;

        protected delegate void OnConcurrencyViolated(object entity);

        protected event OnConcurrencyViolated OnConcurrencyViolation;

        #endregion

        #region Constructor

        protected DatabaseModifiable(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }

        #endregion

        #region Save Methods

        public virtual IPersistResult SaveChanges<T>(T entity) where T : class
        {
            return Configuration.UseTransactions ? _saveChangesUsingTransactions(entity) : _saveChanges(entity);
        }

        public virtual List<IPersistResult> SaveChanges<T>(List<T> entities) where T : class
        {
            return
                entities.Select(
                    entity => Configuration.UseTransactions ? _saveChangesUsingTransactions(entity) : _saveChanges(entity))
                    .ToList();
        }

        public virtual List<IPersistResult> SaveChanges<T>(IEnumerable<T> entities) where T : class
        {
            return SaveChanges(entities.ToList());
        }
        #endregion

        #region Delete Methods

        public virtual IPersistResult Delete<T>(T entity) where T : class
        {
            return Configuration.UseTransactions ? _deleteUsingTransactions(entity) : _delete(entity);
        }

        public virtual List<IPersistResult> Delete<T>(List<T> entities) where T : class
        {
            return
                entities.Select(
                    entity => Configuration.UseTransactions ? _deleteUsingTransactions(entity) : _delete(entity))
                    .ToList();
        }

        public virtual List<IPersistResult> Delete<T>(IEnumerable<T> entities) where T : class
        {
            return Delete(entities.ToList());
        }
        #endregion

        #region Reference Mapping
        private class ReferenceMap : IEnumerable<Reference>
        {
            public Reference this[int i]
            {
                get { return _internal[i] as Reference; }
            }

            public int Count { get { return _internal.Count; } }

            private readonly List<object> _internal;

            private readonly IConfigurationOptions _configuration;

            public ReferenceMap(IConfigurationOptions configuration)
            {
                _internal = new List<object>();
                _configuration = configuration;
            }

            public void Add(ModificationEntity entity)
            {
                _internal.Add(new Reference(entity, _nextAlias()));
            }

            public void AddOneToManySaveReference(ForeignKeyAssociation association, object value, bool isDeleting = false)
            {
                var parentIndex = _indexOf(association.Parent);

                _insert(parentIndex + 1, value, association, _configuration, isDeleting);

                // add the references
                var index = _indexOf(value);
                var oneToManychildIndex = _indexOf(association.Parent);
                var oneToManyChild = this[oneToManychildIndex];
                var oneToManyParent = this[index];
                var oneToOneForeignKeyAttribute = association.Property.GetCustomAttribute<ForeignKeyAttribute>();
                var oneToOneParentProperty = association.ChildType.GetProperty(oneToOneForeignKeyAttribute.ForeignKeyColumnName);
                var oneToOneChildProperty = ReflectionCacheTable.GetPrimaryKeys(association.ParentType)[0];

                oneToManyParent.References.Add(new ReferenceNode(association.Parent, oneToManyChild.Alias, RelationshipType.OneToMany, new Link(oneToOneParentProperty, oneToOneChildProperty)));
            }

            public void AddOneToOneSaveReference(ForeignKeyAssociation association, bool isDeleting = false)
            {
                var oneToOneParentIndex = _indexOf(association.Parent);

                _insert(oneToOneParentIndex, association.Value, association, _configuration, isDeleting);

                var oneToOneIndex = _indexOf(association.Parent);
                var childIndex = _indexOf(association.Value);
                var child = this[childIndex];
                var parent = this[oneToOneIndex];
                var oneToOneForeignKeyAttribute = association.Property.GetCustomAttribute<ForeignKeyAttribute>();
                var oneToOneParentProperty = association.ParentType.GetProperty(oneToOneForeignKeyAttribute.ForeignKeyColumnName);
                var oneToOneChildProperty = ReflectionCacheTable.GetPrimaryKeys(association.ChildType)[0];

                parent.References.Add(new ReferenceNode(association.Value, child.Alias, RelationshipType.OneToOne, new Link(oneToOneParentProperty, oneToOneChildProperty)));
            }

            private int _indexOf(object entity)
            {
                return _internal.IndexOf(entity);
            }

            private void _insert(int index, object entity, ForeignKeyAssociation association, IConfigurationOptions configuration, bool isDeleting)
            {
                _internal.Insert(index, new Reference(entity, _nextAlias(), configuration, association, isDeleting));
            }

            private string _nextAlias()
            {
                return string.Format("Tbl{0}", _internal.Count);
            }

            public void Reverse()
            {
                _internal.Reverse();
            }

            public IEnumerator<Reference> GetEnumerator()
            {
                return (dynamic)_internal.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class Link
        {
            public Link(PropertyInfo parentProperty, PropertyInfo childProperty)
            {
                ParentPropertyName = parentProperty.Name;
                ParentColumnName = Table.GetColumnName(parentProperty);
                ChildPropertyName = childProperty.Name;
                ChildColumnName = Table.GetColumnName(childProperty);
            }

            public readonly string ParentPropertyName;

            public readonly string ParentColumnName;

            public bool IsParentColumnRenamed
            {
                get { return ParentPropertyName != ParentColumnName; }
            }

            public readonly string ChildPropertyName;

            public readonly string ChildColumnName;

            public bool IsChildColumnRenamed
            {
                get { return ChildPropertyName != ChildColumnName; }
            }
        }

        private class ReferenceNode
        {
            public ReferenceNode(object value, string alias, RelationshipType relationshipType, Link link)
            {
                Value = value;
                Alias = alias;
                Relationship = relationshipType;
                Link = link;
            }

            public readonly object Value;

            public readonly string Alias;

            public readonly RelationshipType Relationship;

            public readonly Link Link;

            public string GetOutputFieldValue()
            {
                return string.Format("(SELECT TOP 1 {0} FROM @{1})", Link.ChildPropertyName, Alias);
            }
        }

        private class Reference : IEquatable<Reference>
        {
            public Reference(ModificationEntity entity, string alias, ForeignKeyAssociation association = null)
            {
                Entity = entity;
                Alias = alias;
                Parent = association == null ? null : association.Parent;
                Property = association == null ? null : association.Property;
                References = new List<ReferenceNode>();
            }

            public Reference(object entity, string alias, IConfigurationOptions configuration, ForeignKeyAssociation association, bool isDeleting = false) :
                this(new ModificationEntity(entity, configuration, isDeleting), alias, association)
            {
            }

            /// <summary>
            /// References needed for saving the object.  Refences are needed because they have values that are needed for insertion
            /// </summary>
            public readonly List<ReferenceNode> References;

            public readonly ModificationEntity Entity;

            public readonly string Alias;

            public readonly object Parent;

            public readonly PropertyInfo Property;

            public bool Equals(Reference other)
            {
                //Check whether the compared object is null.
                if (ReferenceEquals(other, null)) return false;

                //Check whether the compared object references the same data.
                return ReferenceEquals(this, other) || Equals(Entity, other.Entity);
            }

            public override bool Equals(object obj)
            {
                return Entity.Equals(obj);
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.
            public override int GetHashCode()
            {
                //Calculate the hash code for the product.
                return Entity.GetHashCode();
            }
        }

        private static class EntityMapper
        {
            public static ReferenceMap GetReferenceMap(ModificationEntity entity, IConfigurationOptions configuration, bool isDeleting)
            {
                var result = new ReferenceMap(configuration);
                var useTransactions = configuration.UseTransactions;
                var entities = _getForeignKeys(entity.Value);

                entities.Insert(0, new ForeignKeyAssociation(null, entity.Value, null));

                for (var i = 0; i < entities.Count; i++)
                {
                    var e = entities[i];
                    var foreignKeyIsList = e.Property == null ? false : e.Property.IsList();
                    var tableType = e.Property == null ? e.Value.GetType() : e.Property.GetPropertyType();
                    var tableInfo = new Table(tableType, configuration);
                     
                    // this is ok as long as the parent is not an insert
                    if (e.Value == null)
                    {
                        // check to see if nulls are allowed.  Are allowed on a list and nullable one to one FK
                        if (foreignKeyIsList) continue;

                        var columnName = e.Property.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                        var isNullable = e.Parent.GetType().GetProperty(columnName).PropertyType.IsNullable();

                        // we can skip the foreign key if its nullable and one to one
                        if (isNullable) continue;

                        if (e.Parent == null) throw new SqlSaveException("Cannot save a null object");

                        var modifcationEntity = new ModificationEntity(e.Parent, configuration);

                        if (!useTransactions && (modifcationEntity.UpdateType == UpdateType.Insert ||
                            modifcationEntity.UpdateType == UpdateType.TryInsert ||
                            modifcationEntity.UpdateType == UpdateType.TryInsertUpdate))
                        {
                            throw new SqlSaveException(string.Format("Foreign Key violation.  {0} cannot be null", Table.GetTableName(e.Property.PropertyType.GetUnderlyingType())));
                        }

                        continue;
                    }

                    if (tableInfo.IsReadOnly)
                    {
                        switch (tableInfo.GetReadOnlySaveOption())
                        {
                            case ReadOnlySaveOption.ThrowException:
                                // Check for readonly attribute and see if we should throw an error
                                throw new SqlSaveException(string.Format("Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys", tableInfo.PlainTableName));
                            case ReadOnlySaveOption.Skip:
                                // skip children(foreign keys) if option is set 
                                continue;
                        }
                    }

                    // check to see if its the base entity
                    if (i == 0)
                    {
                        // is the base entity, will never have a parent, set it and continue to the next entity
                        result.Add(entity);
                        continue;
                    }

                    // skip lookup tables only when its not the parent
                    if (tableInfo.IsLookupTable) continue;

                    // doesnt have dependencies
                    if (foreignKeyIsList)
                    {
                        // e.Value can not be null, above code will catch it
                        foreach (var item in (ICollection)e.Value)
                        {
                            // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                            // the value property
                            result.AddOneToManySaveReference(e, item, isDeleting);

                            // add any dependencies
                            if (ReflectionCacheTable.HasForeignKeys(item)) entities.AddRange(_getForeignKeys(item));
                        }
                    }
                    else
                    {
                        // must be saved before the parent
                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        result.AddOneToOneSaveReference(e, isDeleting);

                        // add any dependencies
                        if (ReflectionCacheTable.HasForeignKeys(e.Value)) entities.AddRange(_getForeignKeys(e.Value));
                    }
                }

                return result;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Used with insert statements only, gets the value if the id's that were inserted
        /// </summary>
        /// <returns></returns>
        private OutputContainer GetOutput(ModificationEntity entity, ChangeManager changeManager)
        {
            if (Reader.HasRows)
            {
                Reader.Read();
                var keyContainer = new OutputContainer();
                var dataRecord = (IDataRecord)Reader;

                for (var i = 0; i < dataRecord.FieldCount; i++)
                {
                    var databaseColumnName = dataRecord.GetName(i);
                    var oldValue = _getPropertyValue(databaseColumnName, entity);
                    var dbValue = dataRecord.GetValue(i);
                    var newValue = dbValue is DBNull ? null : dbValue;

                    if (ObjectComparison.HasChanged(oldValue, newValue)) changeManager.AddChange(databaseColumnName, entity.PlainTableName, oldValue, newValue);

                    keyContainer.Add(databaseColumnName, newValue);
                }

                // set the keys on the change manager
                foreach (var key in entity.Keys())
                {
                    var value = entity.GetPropertyValue(key.PropertyName);

                    changeManager.AddKey(key.DatabaseColumnName, entity.PlainTableName, value);
                }

                // clean up the database connection
                Disconnect();

                return keyContainer;
            }

            // clean up the database connection
            Disconnect();

            return new OutputContainer();
        }

        private object _getPropertyValue(string databaseColumnName, ModificationEntity entity)
        {
            var property = entity.GetProperty(databaseColumnName);

            if (entity.UpdateType == UpdateType.Insert ||
                entity.UpdateType == UpdateType.TryInsert ||
                entity.UpdateType == UpdateType.TryInsertUpdate)
            {
                return entity.GetPropertyValue(property);
            }

            return entity.GetPristineEntityPropertyValue(property.Name);
        }

        private static List<ForeignKeyAssociation> _getForeignKeys(object entity)
        {
            return
                Table.GetForeignKeys(entity)
                    .OrderBy(w => w.PropertyType.IsList())
                    .Select(w => new ForeignKeyAssociation(entity, w.GetValue(entity), w))
                    .ToList();
        }

        public override void Dispose()
        {
            OnBeforeSave = null;
            OnAfterSave = null;
            OnSaving = null;
            OnConcurrencyViolation = null;

            base.Dispose();
        }

        #endregion

        #region shared
        private class SqlPartStatement : ISqlPartStatement
        {
            public SqlPartStatement(string sql, string declare = null, string set = null)
            {
                Sql = sql;
                Declare = declare;
                Set = set;
            }

            public string Sql { get; private set; }

            public string Declare { get; private set; }

            public string Set { get; private set; }

            public override string ToString()
            {
                return string.Format("{0}{1}{2}",

                    string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("{0} \r\r", Declare),

                    string.IsNullOrWhiteSpace(Set) ? string.Empty : string.Format("{0} \r\r", Set),

                    Sql);
            }
        }

        /// <summary>
        /// Provides us a way to get the execution plan for an entity
        /// </summary>
        private abstract class SqlExecutionPlan : ISqlExecutionPlan
        {
            #region Constructor

            protected SqlExecutionPlan(ModificationEntity entity, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> parameters)
            {
                Entity = entity;
                Parameters = parameters;
                Configuration = configurationOptions;
            }

            #endregion

            #region Properties and Fields

            public IModificationEntity Entity { get; private set; }

            protected readonly List<SqlSecureQueryParameter> Parameters;

            protected readonly IConfigurationOptions Configuration;
            #endregion

            #region Methods

            public abstract ISqlBuilder GetBuilder();

            public SqlCommand BuildSqlCommand(SqlConnection connection)
            {
                // build the sql package
                var package = GetBuilder();

                // generate the sql command
                var command = new SqlCommand(package.GetSql(), connection);

                // insert the parameters
                package.InsertParameters(command);

                return command;
            }

            #endregion
        }

        private abstract class SqlModificationBuilder : SqlSecureExecutable, ISqlBuilder
        {
            #region Constructor


            protected SqlModificationBuilder(ISqlExecutionPlan plan, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> parameters) 
                : base(parameters)
            {
                Entity = plan.Entity;
                Configuration = configurationOptions;
            }

            #endregion

            #region Properties

            protected readonly IModificationEntity Entity;

            protected readonly IConfigurationOptions Configuration;

            #endregion

            #region Methods

            protected abstract ISqlContainer NewContainer();

            public abstract ISqlContainer BuildContainer();

            public string GetSql()
            {
                var container = BuildContainer();

                return container.Resolve();
            }

            #endregion
        }

        private class ForeignKeyAssociation
        {
            public ForeignKeyAssociation(object parent, object value, PropertyInfo property)
            {
                Parent = parent;
                Value = value;
                Property = property;
            }

            public object Parent { get; private set; }

            public PropertyInfo Property { get; private set; }

            public Type ParentType
            {
                get { return Parent == null ? null : Parent.GetUnderlyingType(); }
            }

            public object Value { get; private set; }

            public Type ChildType
            {
                get { return Value == null ? null : Value.GetUnderlyingType(); }
            }
        }

        private abstract class SqlSecureExecutable
        {
            #region Fields

            private readonly List<SqlSecureQueryParameter> _parameters;

            #endregion

            #region Constructor

            protected SqlSecureExecutable(List<SqlSecureQueryParameter> parameters)
            {
                _parameters = parameters;
            }

            #endregion

            #region Methods

            // key where the data will be insert into the secure command
            private string _getNextKey()
            {
                return string.Format("@DATA{0}", _parameters.Count);
            }

            protected string AddParameter(IModificationItem item, object value)
            {
                return _addParameter(item, value, false);
            }

            protected string AddPristineParameter(IModificationItem item, object value)
            {
                return _addParameter(item, value, true);
            }

            private string _addParameter(IModificationItem item, object value, bool addPristineParameter)
            {
                var parameterKey = _getNextKey();

                _parameters.Add(new SqlSecureQueryParameter
                {
                    Key = parameterKey,
                    DbColumnName =
                        addPristineParameter
                            ? string.Format("Pristine{0}", item.DatabaseColumnName)
                            : item.DatabaseColumnName,
                    TableName = item.ToString(),
                    ForeignKeyPropertyName = Table.GetTableName(item),
                    Value =
                        item.TranslateDataType
                            ? new SqlSecureObject(value, item.DbTranslationType)
                            : new SqlSecureObject(value)
                });

                return parameterKey;
            }

            public void InsertParameters(SqlCommand cmd)
            {
                foreach (var item in _parameters)
                {
                    cmd.Parameters.Add(cmd.CreateParameter()).ParameterName = item.Key;
                    cmd.Parameters[item.Key].Value = item.Value.Value;

                    if (item.Value.TranslateDataType)
                    {
                        cmd.Parameters[item.Key].SqlDbType = item.Value.DbTranslationType;
                    }
                }
            }

            #endregion
        }

        // helps return data from insertions
        private class OutputContainer : IEnumerable<KeyValuePair<string, object>>
        {
            #region Constructor
            public OutputContainer()
            {
                _container = new Dictionary<string, object>();
            }
            #endregion

            #region Properties
            private Dictionary<string, object> _container { get; set; }

            public int Count { get { return _container == null ? 0 : _container.Count; } }
            #endregion

            #region Methods
            public void Add(string columnName, object value)
            {
                _container.Add(columnName, value);
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return _container.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }

        #region Change Management
        private class ChangeManager
        {
            private readonly XmlDocument _doc;
            private readonly Dictionary<string, ChangeContainer> _tableChangeResultsXml;
            private readonly List<ITableChangeResult> _tableChangeResults;
            private readonly XmlElement _root;

            public ChangeManager()
            {
                _doc = new XmlDocument();
                _tableChangeResultsXml = new Dictionary<string, ChangeContainer>();
                _tableChangeResults = new List<ITableChangeResult>();

                var xmlDeclaration = _doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                var root = _doc.DocumentElement;
                _doc.InsertBefore(xmlDeclaration, root);

                _root = _doc.CreateElement(string.Empty, "procedure", string.Empty);
            }

            private ChangeContainer _findChangeContainer(string tableName)
            {
                ChangeContainer container;

                _tableChangeResultsXml.TryGetValue(tableName, out container);

                if (container == null) throw new Exception(string.Format("Table not found for Xml change document creation.  Table Name- {0}", tableName));

                return container;
            }

            public void AddChange(string columnName, string tableName, object oldValue, object newValue)
            {
                var container =_findChangeContainer(tableName);

                // create column element
                var columnElement = _doc.CreateElement(string.Empty, "column", string.Empty);

                // add name attribute to column
                var nameAttribute = _doc.CreateAttribute("name");
                nameAttribute.InnerText = columnName;
                columnElement.SetAttributeNode(nameAttribute);

                // add old value node
                var oldValueElement = _doc.CreateElement(string.Empty, "old", string.Empty);
                var oldValueNode = _doc.CreateTextNode(oldValue == null ? "NULL" : oldValue.ToString());
                oldValueElement.AppendChild(oldValueNode);
                columnElement.AppendChild(oldValueElement);

                // add new value node
                var newValueElement = _doc.CreateElement(string.Empty, "new", string.Empty);
                var newValueNode = _doc.CreateTextNode(newValue == null ? "NULL" : newValue.ToString());
                newValueElement.AppendChild(newValueNode);
                columnElement.AppendChild(newValueElement);

                container.Columns.AppendChild(columnElement);
                container.AddChange();
            }

            public void AddKey(string columnName, string tableName, object value)
            {
                var container = _findChangeContainer(tableName);

                // create column element
                var columnElement = _doc.CreateElement(string.Empty, "column", string.Empty);

                // add name attribute to column
                var nameAttribute = _doc.CreateAttribute("name");
                nameAttribute.InnerText = columnName;
                columnElement.SetAttributeNode(nameAttribute);

                // set value of the key
                var oldValueElement = _doc.CreateElement(string.Empty, "value", string.Empty);
                var oldValueNode = _doc.CreateTextNode(value.ToString());
                oldValueElement.AppendChild(oldValueNode);
                columnElement.AppendChild(oldValueElement);

                container.Keys.AppendChild(columnElement);
            }

            public void AddTable(string tableName, UpdateType updateType)
            {
                if (_tableChangeResultsXml.ContainsKey(tableName)) return;

                var change = new ChangeContainer(_doc);
                var tableElement = _doc.CreateElement(string.Empty, "table", string.Empty);

                var nameAttribute = _doc.CreateAttribute("name");
                nameAttribute.InnerText = tableName;

                tableElement.SetAttributeNode(nameAttribute);

                change.SetTableElement(tableElement);

                // set the action taken on the table
                var actionAttribute = _doc.CreateAttribute("action");
                actionAttribute.InnerText = updateType.ToString();
                tableElement.SetAttributeNode(actionAttribute);

                _tableChangeResultsXml.Add(tableName, change);
                _tableChangeResults.Add(new TableChangeResult(updateType, tableName));
            }

            public void ChangeUpdateType(string tableName, UpdateType updateType)
            {
                var tableChangeResult = _tableChangeResults.FirstOrDefault(w => w.TableName == tableName);

                if (tableChangeResult == null) throw new Exception(string.Format("Could not find change result for table {0}", tableName));

                tableChangeResult.ChangeAction(updateType);
            }

            public IPersistResult Compile()
            {
                // compile the changes
                foreach (var change in _tableChangeResultsXml) change.Value.Finalize(_doc, _root);

                _doc.AppendChild(_root);

                _doc.Save("C:\\users\\jdemeuse\\desktop\\document.xml");

                return new SaveResult(_doc, _tableChangeResults);
            }

            #region helpers
            private class ChangeContainer
            {
                public ChangeContainer(XmlDocument doc)
                {
                    Count = 0;
                    TableElement = null;

                    Columns = doc.CreateElement(string.Empty, "columns", string.Empty);
                    Keys = doc.CreateElement(string.Empty, "keys", string.Empty);
                }

                public int Count { get; private set; }

                public XmlElement TableElement { get; private set; }

                public XmlElement Columns { get; private set; }

                public XmlElement Keys { get; private set; }

                public void AddChange()
                {
                    Count++;
                }

                public void SetTableElement(XmlElement element)
                {
                    TableElement = element;
                }

                public void Finalize(XmlDocument doc, XmlElement root)
                {
                    var totalAttribute = doc.CreateAttribute("total");
                    totalAttribute.InnerText = Count.ToString();
                    TableElement.SetAttributeNode(totalAttribute);

                    TableElement.AppendChild(Columns);
                    TableElement.AppendChild(Keys);

                    root.AppendChild(TableElement);
                }
            }

            private class SaveResult : IPersistResult
            {
                public SaveResult(XmlDocument resultsXml, List<ITableChangeResult> results)
                {
                    ResultsXml = resultsXml;
                    Results = results;
                }

                public XmlDocument ResultsXml { get; private set; }

                public IReadOnlyList<ITableChangeResult> Results { get; private set; }

                public bool WereChangesPersisted { get { return Results.Any(w => w.Action != UpdateType.Skip); } }
            }

            private class TableChangeResult : ITableChangeResult
            {
                public TableChangeResult(UpdateType action, string tableName)
                {
                    Action = action;
                    TableName = tableName;
                }

                public UpdateType Action { get; private set; }

                public string TableName { get; private set; }

                public void ChangeAction(UpdateType action)
                {
                    Action = action;
                }
            }
            #endregion
        }
        #endregion

        #endregion
    }
}
