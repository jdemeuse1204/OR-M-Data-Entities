/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Collections.Generic;
using System.Linq;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition.Rules;
using OR_M_Data_Entities.Data.Definition.Rules.Base;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data.Definition
{
    public class ModificationEntity : Entity
    {
        #region Properties
        public UpdateType UpdateType { get; private set; }

        public EntityState State { get; private set; }

        protected IReadOnlyList<ModificationItem> ModificationItems { get; set; }

        // table can have two foreign keys of the same time, this will tell them apart
        #endregion

        #region Constructor
        public ModificationEntity(object entity, ConfigurationOptions configuration)
            : this(entity, false, configuration)
        {
        }

        protected ModificationEntity(object entity, bool isDeleting, ConfigurationOptions configuration)
            : base(entity)
        {
            // do not initialize when deleting because we will get unnecessary errors
            if (isDeleting) return;

            _initialize(configuration);
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
            // this is for updates and we should never update a PK
            return ModificationItems.Where(w => w.IsModified && !w.IsPrimaryKey).ToList();
        }

        public IReadOnlyList<ModificationItem> Keys()
        {
            return ModificationItems.Where(w => w.IsPrimaryKey).ToList();
        }

        public IReadOnlyList<ModificationItem> All()
        {
            return ModificationItems;
        }

        private void _setChanges()
        {
            var columns = GetColumns();

            if (!IsEntityStateTrackingOn)
            {
                // mark everything has changed so it will be updated
                ModificationItems = columns.Select(w => new ModificationItem(w)).ToList();
                State = EntityState.Modified;
                return;
            }
            // if _pristineEntity == null then that means a new instance was created and it is an insert

            ModificationItems = _getChanges(EntityTrackable);

            State = _getState(ModificationItems);
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

        private void _initialize(ConfigurationOptions configuration)
        {
            var primaryKeys = GetPrimaryKeys();

            // make sure the table is valid
            RuleProcessor.ProcessRule<IsEntityValidRule>(Value);

            var columns = Properties.Where(w => !IsPrimaryKey(w)).ToList();
            var hasPrimaryKeysOnly = HasPrimaryKeysOnly();
            var areAnyPkGenerationOptionsNone = false;

            UpdateType = UpdateType.Skip;

            // check to see if anything has updated, if not we can skip everything
            _setChanges();

            // if there are no changes then exit
            if (State == EntityState.UnChanged) return;

            // validate all max length attributes
            RuleProcessor.ProcessRule<MaxLengthViolationRule>(Value, Type);

            UpdateType = UpdateType.Insert;

            // need to find the Update Type
            for (var i = 0; i < primaryKeys.Count; i++)
            {
                var key = primaryKeys[i];
                var pkValue = key.GetValue(Value);
                var generationOption = GetGenerationOption(key);

                RuleProcessor.ProcessRule<PkValueNotNullRule>(pkValue, key);

                // check to see if we are updating or not
                var isUpdating = !IsValueInInsertArray(configuration, pkValue);

                if (generationOption == DbGenerationOption.None) areAnyPkGenerationOptionsNone = true;

                // break because we are already updating, do not want to set to false
                if (!isUpdating)
                {
                    if (generationOption != DbGenerationOption.None)
                    {
                        RuleProcessor.ProcessRule<TimeStampColumnInsertRule>(Value, columns);
                        continue;
                    }

                    // if the db generation option is none and there is no pk value this is an error because the db doesnt generate the pk
                    throw new SqlSaveException(string.Format(
                        "Primary Key must not be an insert value when DbGenerationOption is set to None.  Primary Key Name: {0}, Table: {1}",
                        key.Name,
                        ToString(TableNameFormat.Plain)));
                }

                // If we have only primary keys we need to perform a try insert and see if we can try to insert our data.
                // if we have any Pk's with a DbGenerationOption of None we need to first see if a record exists for the pks, 
                // if so we need to perform an update, otherwise we perform an insert
                UpdateType = hasPrimaryKeysOnly
                    ? UpdateType.TryInsert
                    : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;

                if (UpdateType != UpdateType.Update && UpdateType != UpdateType.TryInsertUpdate) continue;

                // make sure identity columns were not updated
                RuleProcessor.ProcessRule<IdentityColumnUpdateRule>(EntityTrackable, configuration, columns);

                // make sure time stamps were not updated if any
                RuleProcessor.ProcessRule<TimeStampColumnUpdateRule>(EntityTrackable, columns);
            }
        }

        public static bool IsValueInInsertArray(ConfigurationOptions configuration, object value)
        {
            // SET CONFIGURATION FOR ZERO/NOT UPDATED VALUES
            switch (value.GetType().Name.ToUpper())
            {
                case "INT16":
                    return configuration.InsertKeys.SmallInt.Contains(Convert.ToInt16(value));
                case "INT32":
                    return configuration.InsertKeys.Int.Contains(Convert.ToInt32(value));
                case "INT64":
                    return configuration.InsertKeys.BigInt.Contains(Convert.ToInt64(value));
                case "GUID":
                    return configuration.InsertKeys.UniqueIdentifier.Contains((Guid)value);
                case "STRING":
                    return configuration.InsertKeys.String.Contains(value.ToString());
            }

            return false;
        }

        public static bool HasColumnChanged(EntityStateTrackable entity, string propertyName)
        {
            // only check when the pristine entity is not null, try insert update will fail otherwise.
            // if the pristine entity is null then there is nothing to compare to
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
            var tableOnLoad = GetPristineEntity(entity);

            return tableOnLoad == null
                ? new object() // need to make sure the current doesnt equal the current if the pristine entity is null
                : tableOnLoad.GetType().GetProperty(propertyName).GetValue(tableOnLoad);
        }

        public static object GetPristineEntity(EntityStateTrackable entity)
        {
            var field = GetPristineEntityFieldInfo();

            // cannot be null here, should never happen
            if (field == null) throw new ArgumentNullException("_pristineEntity");

            return field.GetValue(entity);
        }

        public static void TrySetPristineEntity(object instance)
        {
            var entityTrackable = instance as EntityStateTrackable;

            if (entityTrackable == null) return;

            var field = GetPristineEntityFieldInfo();

            if (field == null) throw new SqlSaveException("Cannot find Pristine Entity");

            field.SetValue(instance, EntityCloner.Clone(instance));
        }
        #endregion

        #region helpers
        private static class EntityCloner
        {
            public static object Clone(object table)
            {
                var instance = Activator.CreateInstance(table.GetType());

                foreach (var item in table.GetType().GetProperties().Where(w => w.IsColumn()))
                {
                    var value = item.GetValue(table);

                    instance.GetType().GetProperty(item.Name).SetValue(instance, value);
                }

                return instance;
            }
        }
        #endregion
    }
}
