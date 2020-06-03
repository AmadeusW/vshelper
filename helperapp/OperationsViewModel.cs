using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace helperapp
{
    public class OperationViewModel : NotifyPropertyChangedBase
    {
        public string FullName => operation.FullName;
        public string ShortName => operation.ShortName;
        public bool Visible { get { return this.visible; } set { this.visible = value; NotifyPropertyChanged(); } }
        public ICommand Command { get; set; }

        private bool visible = true;
        private Operation operation;

        public OperationViewModel(Operation operation)
        {
            this.operation = operation;
            // TODO: set up Command
        }
    }

    public class OperationsViewModel : NotifyPropertyChangedBase
    {
        public List<OperationViewModel> Operations { get; }

        public string Header { get; set; } = "Use Visual Studio to get started";

        private VSData data;
        public VSData Data { get { return this.data; } set { this.data = value; UpdateHeader(); NotifyPropertyChanged(""); } }

        private bool showConfiguration;
        public bool ShowConfiguration { get { return this.showConfiguration; } set { this.showConfiguration = value; NotifyPropertyChanged(); } }

        private void UpdateHeader()
        {
            var sb = new StringBuilder();
            ConditionalAppend(ShowInstallationId, InstallationId);
            ConditionalAppend(ShowInstallationChannel, InstallationChannel);
            ConditionalAppend(ShowInstallationVersion, InstallationVersion);
            ConditionalAppend(ShowSKU, SKU);
            ConditionalAppend(ShowMajorVersion, MajorVersion);
            ConditionalAppend(ShowPath, Path);
            ConditionalAppend(ShowHive, Hive);
            ConditionalAppend(ShowRootFolderName, RootFolderName);
            this.Header = sb.ToString();
            //NotifyPropertyChanged(nameof(Header));

            void ConditionalAppend(bool condition, string value)
            {
                if (condition)
                {
                    sb.Append(value);
                    sb.Append(" ");
                }
            }
        }

        public OperationsViewModel(IEnumerable<Operation> operations)
        {
            Header = string.Empty;
            Operations = operations.Select(n => new OperationViewModel(n)).ToList();
            // TODO: apply personalization settings from file
        }

        public string InstallationId => this.Data?.InstallationId ?? string.Empty;
        public string InstallationChannel => this.Data?.InstallationChannel ?? string.Empty;
        public string InstallationVersion => this.Data?.InstallationVersion ?? string.Empty;
        public string SKU => this.Data?.SKU ?? string.Empty;
        public string MajorVersion => this.Data?.MajorVersion ?? string.Empty;
        public string Path => this.Data?.Path ?? string.Empty;
        public string Hive => this.Data?.Hive ?? string.Empty;
        public string RootFolderName => this.Data?.RootFolderName ?? string.Empty;

        public bool ShowInstallationId { get { return this._showInstallationId; } set { this._showInstallationId = value; UpdateHeader(); } }
        public bool ShowInstallationChannel { get { return this._showInstallationChannel; } set { this._showInstallationChannel = value; UpdateHeader(); } }
        public bool ShowInstallationVersion { get { return this._showInstallationVersion; } set { this._showInstallationVersion = value; UpdateHeader(); } }
        public bool ShowSKU { get { return this._showSKU; } set { this._showSKU = value; UpdateHeader(); } }
        public bool ShowMajorVersion { get { return this._showMajorVersion; } set { this._showMajorVersion = value; UpdateHeader(); } }
        public bool ShowPath { get { return this._showPath; } set { this._showPath = value; UpdateHeader(); } }
        public bool ShowHive { get { return this._showHive; } set { this._showHive = value; UpdateHeader(); } }
        public bool ShowRootFolderName { get { return this._showRootFolderName; } set { this._showRootFolderName = value; UpdateHeader(); } }

        private bool _showInstallationId = true;
        private bool _showInstallationChannel;
        private bool _showInstallationVersion;
        private bool _showSKU;
        private bool _showMajorVersion;
        private bool _showPath;
        private bool _showHive = true;
        private bool _showRootFolderName = true;
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
