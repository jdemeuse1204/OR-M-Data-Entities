using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition
{
    public class ModificationEntity : Entity
    {
        #region Properties
        public UpdateType UpdateType { get; private set; }

        public EntityState State { get; private set; }

        private IReadOnlyList<ModificationItem> _modificationItems { get; set; }

        private readonly MapsTo _mapsTo;

        public readonly string TableVariable;
        #endregion

        #region Constructor
        public ModificationEntity(object entity, MapsTo mapsTo = null) 
            : base(entity)
        {
            _mapsTo = mapsTo;
            TableVariable = _mapsTo == null ? "ParentTable" : _mapsTo.AsVariable();
            _initialize();
        }
        #endregion

        #region Methods
        public static EntityState GetState(object value)
        {
            var entityStateTrackable = value as EntityStateTrackable;

            if (entityStateTrackable == null) throw new Exception("Entity tracking not turned on");

            var changes = _getChanges(entityStateTrackable);

            return _getState(changes);
        }

        public IReadOnlyList<ModificationItem> Changes()
        {
            return _modificationItems.Where(w => w.IsModified).ToList();
        }

        public IReadOnlyList<ModificationItem> Keys()
        {
            return _modificationItems.Where(w => w.IsPrimaryKey).ToList();
        }

        public IReadOnlyList<ModificationItem> All()
        {
            return _modificationItems;
        }

        private void _setChanges()
        {
            var columns = GetColumns();

            if (!IsEntityStateTrackingOn)
            {
                // mark everything has changed so it will be updated
                _modificationItems = columns.Select(w => new ModificationItem(w)).ToList();
                State = EntityState.Modified;
                return;
            }
            // if _pristineEntity == null then that means a new instance was created and it is an insert

            _modificationItems = _getChanges(EntityTrackable);

            State = _getState(_modificationItems);
        }

        private static EntityState _getState(IReadOnlyList<ModificationItem> changes)
        {
            return changes.Any(w => w.IsModified) ? EntityState.Modified : EntityState.UnChanged;
        }

        private static IReadOnlyList<ModificationItem> _getChanges(EntityStateTrackable entityStateTrackable)
        {
            return (from item in entityStateTrackable.GetType().GetProperties().Where(w => w.IsColumn())
                let current = _getCurrentObject(entityStateTrackable, item.Name)
                let pristineEntity = _getPristineProperty(entityStateTrackable, item.Name)
                let hasChanged = _hasChanged(pristineEntity, current)
                select new ModificationItem(item, hasChanged)).ToList();
        }
        private static bool _hasChanged(object pristineEntity, object entity)
        {
            return ((entity == null && pristineEntity != null) || (entity != null && pristineEntity == null))
                ? entity != pristineEntity
                : entity == null && pristineEntity == null ? false : !entity.Equals(pristineEntity);
        }

        private void _initialize()
        {
            var primaryKeys = GetPrimaryKeys();
            var columns = Properties.Where(w => !IsPrimaryKey(w)).ToList();
            var hasPrimaryKeysOnly = HasPrimaryKeysOnly();
            var areAnyPkGenerationOptionsNone = false;

            UpdateType = UpdateType.Skip;

            // check to see if anything has updated, if not we can skip everything
            _setChanges();

            // if there are no changes then exit
            if (State == EntityState.UnChanged) return;

            UpdateType = UpdateType.Insert;

            _checkIdentityUpdates(EntityTrackable, columns);

            // need to find the Update Type
            for (var i = 0; i < primaryKeys.Count; i++)
            {
                var key = primaryKeys[i];
                var pkValue = key.GetValue(Value);
                var generationOption = GetGenerationOption(key);
                var isUpdating = false;
                var pkValueTypeString = "";
                var pkValueType = "";

                if (generationOption == DbGenerationOption.None) areAnyPkGenerationOptionsNone = true;

                // SET CONFIGURATION FOR ZERO/NOT UPDATED VALUES
                switch (pkValue.GetType().Name.ToUpper())
                {
                    case "INT16":
                        isUpdating = Convert.ToInt16(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT16";
                        break;
                    case "INT32":
                        isUpdating = Convert.ToInt32(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT32";
                        break;
                    case "INT64":
                        isUpdating = Convert.ToInt64(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT64";
                        break;
                    case "GUID":
                        isUpdating = (Guid)pkValue != Guid.Empty;
                        pkValueTypeString = "zero";
                        pkValueType = "INT16";
                        break;
                    case "STRING":
                        isUpdating = !string.IsNullOrWhiteSpace(pkValue.ToString());
                        pkValueTypeString = "null/blank";
                        pkValueType = "STRING";
                        break;
                }

                // break because we are already updating, do not want to set to false
                if (!isUpdating)
                {
                    if (generationOption == DbGenerationOption.None)
                    {
                        // if the db generation option is none and there is no pk value this is an error because the db doesnt generate the pk
                        throw new SqlSaveException(string.Format(
                            "Primary Key cannot be {1} for {2} when DbGenerationOption is set to None.  Primary Key Name: {0}", key.Name,
                            pkValueTypeString, pkValueType));
                    }
                    continue;
                }

                // If we have only primary keys we need to perform a try insert and see if we can try to insert our data.
                // if we have any Pk's with a DbGenerationOption of None we need to first see if a record exists for the pks, 
                // if so we need to perform an update, otherwise we perform an insert
                UpdateType = hasPrimaryKeysOnly
                    ? UpdateType.TryInsert
                    : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;
            }
        }

        /// <summary>
        /// make sure the user is not trying to update an IDENTITY column, these cannot be updated
        /// </summary>
        /// <param name="entityStateTrackable"></param>
        /// <param name="columns"></param>
        private static void _checkIdentityUpdates(EntityStateTrackable entityStateTrackable,
            IReadOnlyList<PropertyInfo> columns)
        {
            foreach (
                var column in
                    columns.Where(
                        w =>
                            w.GetCustomAttribute<DbGenerationOptionAttribute>() != null &&
                            w.GetCustomAttribute<DbGenerationOptionAttribute>().Option ==
                            DbGenerationOption.IdentitySpecification)
                        .Where(
                            column =>
                                entityStateTrackable != null &&
                                _hasColumnChanged(entityStateTrackable, column.Name)))
            {
                throw new SqlSaveException(string.Format("Cannot update value if IDENTITY column.  Column: {0}",
                    column.Name));
            }
        }

        private static bool _hasColumnChanged(EntityStateTrackable entity, string propertyName)
        {
            var pristineEntity = _getPristineProperty(entity, propertyName);
            var current = _getCurrentObject(entity, propertyName);

            return _hasChanged(pristineEntity, current);
        }

        private static object _getCurrentObject(EntityStateTrackable entity, string propertyName)
        {
            return entity.GetType().GetProperty(propertyName).GetValue(entity);
        }

        private static object _getPristineProperty(EntityStateTrackable entity, string propertyName)
        {
            var field = GetPristineEntityFieldInfo();

            // cannot be null here, should never happen
            if (field == null) throw new ArgumentNullException("_pristineEntity");

            var tableOnLoad = field.GetValue(entity);
            return tableOnLoad == null
                ? new object() // need to make sure the current doesnt equal the current if the pristine entity is null
                : tableOnLoad.GetType().GetProperty(propertyName).GetValue(tableOnLoad);
        }

        public static void TrySetPristineEntity(object instance)
        {
            var entityTrackable = instance as EntityStateTrackable;

            if (entityTrackable == null) return;

            var field = GetPristineEntityFieldInfo();

            if (field == null) throw new SqlSaveException("Cannot find Pristine Entity");

            field.SetValue(instance, EntityCloner.Clone(instance));
        }

        public EntitySaveNodeList GetSaveOrder(bool useTransactions)
        {
            var result = new EntitySaveNodeList();

            var entities = GetForeignKeys(Value);

            entities.Insert(0, new ParentChildPair(null, Value, null));

            for (var i = 0; i < entities.Count; i++)
            {
                if (i == 0)
                {
                    // is the base entity, will never have a parent, set it and continue to the next entity
                    result.Add(new ForeignKeySaveNode(null, this, null));
                    continue;
                }

                var e = entities[i];
                int index;
                var foreignKeyIsList = e.Property.IsList();
                var tableInfo = new Table(e.Property.GetPropertyType());

                if (e.Value == null && !useTransactions)
                {
                    throw new SqlSaveException(string.Format("Foreign Key [{0}] cannot be null", tableInfo.TableNameOnly));
                }

                // skip lookup tables
                if (tableInfo.IsLookupTable) continue;

                // skip children(foreign keys) if option is set
                if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.Skip)
                    continue;

                if (e.Value == null)
                {
                    if (foreignKeyIsList) continue;

                    var columnName = e.ChildType.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                    var isNullable = Value.GetType().GetProperty(columnName).PropertyType.IsNullable();

                    // we can skip the foreign key if its nullable and one to one
                    if (isNullable) continue;

                    if (!useTransactions)
                    {
                        // database will take care of this if MARS is enabled
                        // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                        // maintaining the relationship
                        throw new SqlSaveException(string.Format("Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save", e.GetType().Name));
                    }
                }

                // Check for readonly attribute and see if we should throw an error
                if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.ThrowException)
                {
                    throw new SqlSaveException(string.Format("Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys", tableInfo.GetTableName()));
                }

                // doesnt have dependencies
                if (foreignKeyIsList)
                {
                    // e.Value can not be null, above code will catch it
                    foreach (var item in (e.Value as ICollection))
                    {
                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = result.IndexOf(e.Parent);

                        result.Insert(index + 1, new ForeignKeySaveNode(e.Property, item, e.Parent));

                        if (item.HasForeignKeys()) entities.AddRange(GetForeignKeys(item));
                    }
                }
                else
                {
                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = result.IndexOf(e.Parent);

                    result.Insert(index, new ForeignKeySaveNode(e.Property, e.Value, e.Parent));

                    // has dependencies
                    if (e.Value.HasForeignKeys()) entities.AddRange(GetForeignKeys(e.Value));
                }
            }

            return result;
        }
        #endregion
    }
}
