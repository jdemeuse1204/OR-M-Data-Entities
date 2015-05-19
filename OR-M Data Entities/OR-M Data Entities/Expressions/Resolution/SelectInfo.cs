using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class SelectInfo : INotifyPropertyChanged
    {
        public SelectInfo(MemberInfo info, Type baseType, string tableName, int ordinal)
        {
            OriginalProperty = info;
            NewProperty = info;
            Ordinal = ordinal;
            BaseType = baseType;
            NewType = baseType;
            TableName = tableName;
        }

        private MemberInfo _newProperty;
        public MemberInfo NewProperty
        {
            get { return _newProperty; }
            set
            {
                SetField(ref _newProperty, value);
            }
        }

        private Type _newType;
        public Type NewType
        {
            get { return _newType; }
            set
            {
                SetField(ref _newType, value);
            }
        }

        public Type BaseType { get; private set; } // should never change

        public MemberInfo OriginalProperty { get; private set; } // should never change

        public int Ordinal { get; set; }

        public string TableName { get; private set; }

        public bool WasModified { get; private set; }

        public bool IsSelected { get; set; } // if all are false then its select all or else only whats true

        public void ChangeTableName(string tableName)
        {
            WasTableNameChanged = true;
            TableName = tableName;
        }

        public bool WasTableNameChanged { get; private set; }

        // always look up by new type because type might have changed

        #region Modification State
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            WasModified = true;
            IsSelected = true;

            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false; // if its the same or default it has not changed
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
