using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using VSData = System.Collections.Generic.Dictionary<string, string>;

namespace helperapp
{
    public class OperationViewModel : ViewModelWithPersistingDisplay
    {
        public string FullName => operation.FullName;
        public string ShortName => operation.ShortName;
        public ICommand Command { get; set; }

        private Operation operation;

        public OperationViewModel(Operation operation, IDictionary<string, bool> displayPreference)
            : base(operation.FullName, displayPreference)
        {
            this.operation = operation;
        }
    }

    public class PropertyViewModel : ViewModelWithPersistingDisplay
    {
        public string Value { get; }

        public PropertyViewModel(string key, string value, IDictionary<string, bool> displayPreference)
            : base(key, displayPreference)
        {
            this.Value = value;
        }
    }

    public class ViewModelWithPersistingDisplay : NotifyPropertyChangedBase
    {
        public string Key { get; }
        public bool Active
        {
            get => this.active;
            set
            {
                this.active = value;
                DisplayPreference[Key] = value;
                NotifyPropertyChanged();
            }
        }

        private bool active;
        private IDictionary<string, bool> DisplayPreference;

        protected ViewModelWithPersistingDisplay(string key, IDictionary<string, bool> displayPreference)
        {
            this.Key = key;
            this.DisplayPreference = displayPreference;
            bool preference = false;
            if (displayPreference.TryGetValue(key, out preference) == true)
            {
                this.active = preference;
            }
            else
            {
                this.active = false;
            }
        }
    }

    public class OperationsViewModel : NotifyPropertyChangedBase
    {
        public IList<OperationViewModel> Operations { get; }
        public IDictionary<string, bool> DisplayPreference { get; }

        public List<PropertyViewModel> Properties { get; private set; }

        public string Header { get; set; } = "Use Visual Studio to get started";

        private VSData data;
        public VSData Data { get { return this.data; } set { this.data = value; UpdateProperties(); NotifyPropertyChanged(""); } }

        private bool showConfiguration;
        public bool ShowConfiguration { get { return this.showConfiguration; } set { this.showConfiguration = value; NotifyPropertyChanged(); } }

        private void UpdateProperties()
        {
            Properties = Data.Select(kv => new PropertyViewModel(kv.Key, kv.Value, DisplayPreference)).ToList();
            NotifyPropertyChanged(nameof(Properties));
        }

        public OperationsViewModel(IEnumerable<Operation> operations, IDictionary<string, bool> displayPreference)
        {
            DisplayPreference = displayPreference;
            Header = string.Empty;
            Operations = operations.Select(n => new OperationViewModel(n, DisplayPreference)).ToList();
            Properties = new List<PropertyViewModel>();
        }
    }

    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
