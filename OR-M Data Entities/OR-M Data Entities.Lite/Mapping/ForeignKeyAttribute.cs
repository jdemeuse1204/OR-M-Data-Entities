using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Mapping
{
    /// <summary>
    /// One-Many - Reference Column in Other Table which links to PK of This Table
    /// One-One - Reference Column in This Table which links to PK of Other Table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// One-Many - Reference Column in Other Table which links to PK of This Table
        /// One-One - Reference Column in This Table which links to PK of Other Table
        /// </summary>
        /// <param name="foreignKeyColumnName">ONE-MANY: Reference Column in Other Table which links to PK of This Table, ONE-ONE: Reference Column in This Table which links to PK of Other Table</param>
        public ForeignKeyAttribute(string foreignKeyColumnName)
        {
            ForeignKeyColumnName = foreignKeyColumnName;
        }

        /// <summary>
        /// Parent property name, not the column name if used
        /// </summary>
        public string ForeignKeyColumnName { get; private set; }
    }
}
