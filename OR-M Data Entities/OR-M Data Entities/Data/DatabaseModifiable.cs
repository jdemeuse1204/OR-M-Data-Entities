/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses DataFetching functions to Save, Delete
    /// </summary>
    public abstract class DatabaseModifiable : DatabaseFetching
    {
        #region Events And Delegates
        public delegate void OnBeforeSaveHandler(DatabaseModifiable context, object entity);

        public event OnBeforeSaveHandler OnBeforeSave;
        #endregion

        #region Constructor
        protected DatabaseModifiable(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion
    }
}
