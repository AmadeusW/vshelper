using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using VSData = System.Collections.Generic.Dictionary<string, string>;

namespace helperapp
{
    public class InvokeOperationCommand : ICommand
    {
        private readonly Operation operation;
        private readonly OperationsViewModel owner;
        private string formattedPath = null;
        private string formattedArguments = null;

        public InvokeOperationCommand(Operation operation, OperationsViewModel owner)
        {
            this.operation = operation;
            this.owner = owner;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            if (formattedPath == null)
                formattedPath = OperationFormatter.Format(operation.Path ?? string.Empty, owner.Data);
            if (formattedArguments == null)
                formattedArguments = OperationFormatter.Format(operation.Arguments ?? string.Empty, owner.Data);
            Invoke(formattedPath, formattedArguments);
        }

        private void Invoke(string path, string args)
        {
            try
            {
                Process.Start(path, args);
            }
            catch (Exception ex)
            {
                owner.ShowError(ex);
            }
        }
    }

    public class OperationViewModel : ViewModelWithPersistingDisplay
    {
        public string FullName => operation.FullName;
        public string ShortName => operation.ShortName;
        public InvokeOperationCommand Command { get; set; }

        private Operation operation;
        private readonly OperationsViewModel owner;

        public OperationViewModel(Operation operation, IDictionary<string, bool> displayPreference, OperationsViewModel owner)
            : base(operation.FullName, displayPreference)
        {
            this.operation = operation;
            this.owner = owner;
            this.Command = new InvokeOperationCommand(operation, owner);
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

        public string Header { get; set; }
        public string HeaderDetails { get; set; }

        private VSData data;
        public VSData Data { get { return this.data; } set { this.data = value; UpdateProperties(); NotifyPropertyChanged(""); } }

        private bool showConfiguration;
        public bool ShowConfiguration { get { return this.showConfiguration; } set { this.showConfiguration = value; NotifyPropertyChanged(); } }

        private void UpdateProperties()
        {
            Properties = Data.Select(kv => new PropertyViewModel(kv.Key, kv.Value, DisplayPreference)).ToList();
            NotifyPropertyChanged(nameof(Properties));
        }

        internal void ShowError(Exception ex)
        {
            this.ShowMessage(ex.Message, ex.ToString());
        }

        internal void ShowMessage(string message, string detail = null)
        {
            this.Header = message ?? string.Empty;
            this.HeaderDetails = detail ?? string.Empty;
        }

        internal void ResetMessage()
        {
            this.Header = string.Empty;
            this.HeaderDetails = string.Empty;
        }

        public OperationsViewModel(IEnumerable<Operation> operations, IDictionary<string, bool> displayPreference)
        {
            DisplayPreference = displayPreference;
            Header = "Use Visual Studio to get started";
            HeaderDetails = "This app updates every time an instance of Visual Studio gets focus";
            Operations = operations.Select(n => new OperationViewModel(n, DisplayPreference, this)).ToList();
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
