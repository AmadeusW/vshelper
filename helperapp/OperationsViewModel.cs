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
    public class OperationViewModel : NotifyPropertyChangedBase
    {
        public string FullName => operation.FullName;
        public string ShortName => operation.ShortName;
        public bool Active { get { return this.active; } set { this.active = value; NotifyPropertyChanged(); } }
        public ICommand Command { get; set; }

        private bool active = true;
        private Operation operation;

        public OperationViewModel(Operation operation)
        {
            this.operation = operation;
            // TODO: set up Command
        }
    }

    public class PropertyViewModel : NotifyPropertyChangedBase
    {
        public string Key { get; }
        public string Value { get; }
        public bool Active { get { return this.active; } set { this.active = value; NotifyPropertyChanged(); } }

        private bool active = true;

        public PropertyViewModel(string key, string value)
        {
            this.Key = key;
            this.Value = value;
            this.active = true;
        }
    }

    public class OperationsViewModel : NotifyPropertyChangedBase
    {
        public List<OperationViewModel> Operations { get; }

        public List<PropertyViewModel> Properties { get; private set; }

        public string Header { get; set; } = "Use Visual Studio to get started";

        private VSData data;
        public VSData Data { get { return this.data; } set { this.data = value; UpdateProperties(); NotifyPropertyChanged(""); } }

        private bool showConfiguration;
        public bool ShowConfiguration { get { return this.showConfiguration; } set { this.showConfiguration = value; NotifyPropertyChanged(); } }

        private void UpdateProperties()
        {
            Properties = Data.Select(kv => new PropertyViewModel(kv.Key, kv.Value)).ToList();
            NotifyPropertyChanged(nameof(Properties));
        }

        public OperationsViewModel(IEnumerable<Operation> operations)
        {
            Header = string.Empty;
            Operations = operations.Select(n => new OperationViewModel(n)).ToList();
            Properties = new List<PropertyViewModel>();
            // TODO: apply personalization settings from file
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
