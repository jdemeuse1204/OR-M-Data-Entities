using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OR_M_Data_Entities.Expressions.Resolution.Select.Info
{
    public abstract class SelectInfoChanged : INotifyPropertyChanged
    {
        public bool WasModified { get; protected set; }

        public bool IsSelected { get; set; } // if all are false then its select all or else only whats true

        // always look up by new type because type might have changed

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
    }
}
