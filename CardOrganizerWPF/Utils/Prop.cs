using System;
using System.ComponentModel;

namespace CardOrganizerWPF
{
    public class Prop<T> : INotifyPropertyChanged
    {
        public Prop()
        {
            Value = default(T);
        }

        public Prop(T value)
        {
            Value = value;
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set { _value = value; NotifyPropertyChanged(nameof(Value)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void NotifyPropertyChanged(String propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
