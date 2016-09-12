/*
 * OR-M Data Entities v3.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public enum UpdateType
    {
        Insert,
        TryInsert,
        TryInsertUpdate,
        Update,
        Delete,
        RowNotFound,
        TransactionalSave,
        TransactionalDelete,
        Skip
    }
}
