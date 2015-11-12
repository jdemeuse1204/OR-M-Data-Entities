/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Extensions;

namespace OR_M_Data_Entities.Tracking
{
    public class EntityStateAnalyzer
    {
        public static bool HasColumnChanged(EntityStateTrackable entity, string propertyName)
        {
            var pristineEntity = typeof (EntityStateTrackable).GetField("_pristineEntity",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (pristineEntity == null) return false;

            var tableOnLoad = pristineEntity.GetValue(entity);
            var original = tableOnLoad == null
                ? new object() // need to make sure the current doesnt equal the current if the pristine entity is null
                : tableOnLoad.GetType().GetProperty(propertyName).GetValue(tableOnLoad);
            var current = entity.GetType().GetProperty(propertyName).GetValue(entity);

            return ((current == null && original != null) || (current != null && original == null))
                ? current != original
                : current == null && original == null ? false : !current.Equals(original);
        }

        public static EntityStateComparePackage Analyze(EntityStateTrackable entity)
        {
            // if _pristineEntity == null then that means a new instance was created and it is any insert

            var changes = (from item in entity.GetType().GetProperties().Where(w => w.IsColumn())
                let current = item.GetValue(entity)
                let field =
                    typeof (EntityStateTrackable).GetField("_pristineEntity",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                let tableOnLoad = field.GetValue(entity)
                let original =
                    tableOnLoad == null
                        ? new object() // need to make sure the current doesnt equal the current if the pristine entity is null
                        : tableOnLoad.GetType().GetProperty(item.Name).GetValue(tableOnLoad)
                where
                    ((current == null && original != null) || (current != null && original == null))
                        ? current != original
                        : current == null && original == null ? false : !current.Equals(original)
                select item.Name).ToList();

            return new EntityStateComparePackage(changes.Count == 0 ? EntityState.UnChanged : EntityState.Modified,
                changes);
        }

        public static void TrySetPristineEntity(object instance)
        {
            var entityTrackable = instance as EntityStateTrackable;

            if (entityTrackable == null) return;

            var field = typeof(EntityStateTrackable).GetField("_pristineEntity", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null) throw new Exception("Try Set Table On Load Error");

            field.SetValue(instance, EntityCloner.Clone(instance));
        }
    }
}
