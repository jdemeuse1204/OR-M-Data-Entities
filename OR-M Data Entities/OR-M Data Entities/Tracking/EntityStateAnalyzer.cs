/*
 * OR-M Data Entities v2.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Schema;

namespace OR_M_Data_Entities.Tracking
{
    public class EntityStateAnalyzer
    {
        public static EntityStateComparePackage Analyze(EntityStateTrackable entity)
        {
            // if _pristineEntity == null then that means a new instance was created and it is any insert

            var changes = (from item in entity.GetType().GetProperties().Where(w => w.IsColumn())
                let current = item.GetValue(entity)
                let field =
                    typeof(EntityStateTrackable).GetField("_pristineEntity", BindingFlags.Instance | BindingFlags.NonPublic)
                let tableOnLoad = field.GetValue(entity)
                let original =
                    tableOnLoad == null
                        ? current
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
