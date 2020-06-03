using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PInvoke;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;
using VSData = System.Collections.Generic.Dictionary<string, string>;
using System.IO;

namespace helperapp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
        }

        public delegate void WinEventProc(IntPtr hWinEventHook, User32.WindowsEventHookType @event, IntPtr hwnd, int idObject, int idChild, int dwEventThread, uint dwmsEventTime);

        private const string VisibleControlsStoragePath = "visible.txt";
        private const string OperationsDirectory = "operations";
        private const string OperationsPattern = "*.yml";
        private WinEventProc Listener;
        private static MainWindow AppWindow;
        private User32.SafeEventHookHandle Hook;

        public VSData RecentData { get; private set; }
        public string RecentPath { get; private set; }
        public string RecentHive { get; private set; }
        public Dictionary<string, bool> DisplayPreference { get; private set; }
        private OperationsViewModel CurrentViewModel { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializeUi();
                InitializeHook();
            }
            catch (Exception ex)
            {
                AppWindow.Status.Text = ex.Message;
            }
        }

        private void InitializeUi()
        {
            LoadVisibleControls();
            var operations = LoadOperations();
            this.CurrentViewModel = new OperationsViewModel(operations, this.DisplayPreference);
            this.DataContext = this.CurrentViewModel;
        }

        private static void UpdateUI(VSData data)
        {
            AppWindow.CurrentViewModel.Data = data;
        }

        private void InitializeHook()
        {
            Listener = new WinEventProc(target);
            var functionPointer = Marshal.GetFunctionPointerForDelegate(Listener);
            Hook = User32.SetWinEventHook(User32.WindowsEventHookType.EVENT_SYSTEM_FOREGROUND, User32.WindowsEventHookType.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, functionPointer, 0, 0, User32.WindowsEventHookFlags.WINEVENT_OUTOFCONTEXT);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Hook?.Dispose();
            SaveVisibleControls();
        }

        private void SaveVisibleControls()
        {
            var visibleControls = this.DisplayPreference.Where(kv => kv.Value).Select(kv => kv.Key);
            File.WriteAllLines(VisibleControlsStoragePath, visibleControls);
        }

        private void LoadVisibleControls()
        {
            this.DisplayPreference = new Dictionary<string, bool>();
            if (File.Exists(VisibleControlsStoragePath))
            {
                foreach (var line in File.ReadAllLines(VisibleControlsStoragePath))
                {
                    this.DisplayPreference[line] = true;
                }
            }
        }

        private IReadOnlyList<Operation> LoadOperations()
        {
            List<Operation> operations = new List<Operation>();
            Directory.CreateDirectory(OperationsDirectory);
            foreach (var file in Directory.EnumerateFiles(OperationsDirectory, OperationsPattern))
            {
                var operation = LoadOperation(file);
                operations.Add(operation);
            }

            // Temporarily,
            operations.Add(new Operation() { FullName = "MEF Log", ShortName = "mef" });
            operations.Add(new Operation() { FullName = "Activity Log", ShortName = "act" });
            operations.Add(new Operation() { FullName = "Developer command prompt", ShortName = "cmd" });
            operations.Add(new Operation() { FullName = "Installation directory", ShortName = "dir" });
            operations.Add(new Operation() { FullName = "Extension directory", ShortName = "ext" });

            return operations.AsReadOnly();
        }

        private Operation LoadOperation(string file)
        {
            throw new NotImplementedException();
        }

        static void target(IntPtr hWinEventHook, User32.WindowsEventHookType @event, IntPtr hwnd, int idObject, int idChild, int dwEventThread, uint dwmsEventTime)
        {
            try
            {
                var threadId = User32.GetWindowThreadProcessId(hwnd, out int processId);
                if (processId == 0)
                {
                    AppWindow.Status.Text = "Unable to get process data";
                }
                else
                {
                    String[] properties = { "Name", "ExecutablePath", "CommandLine" };
                    SelectQuery s = new SelectQuery("Win32_Process",
                       $"ProcessID = '{processId}' ",
                       properties);

                    ManagementObjectSearcher searcher =
                       new ManagementObjectSearcher(s);

                    foreach (ManagementObject o in searcher.Get())
                    {
                        if (o["Name"].ToString() != "devenv.exe" || o["ExecutablePath"] == null)
                        {
                            return;
                        }

                        var data = DevenvAnalyzer.GetVsData(o["ExecutablePath"].ToString(), o["CommandLine"].ToString());
                        UpdateUI(data);
                    }
                }
            }
            catch (Exception ex)
            {
                AppWindow.Status.Text = ex.Message;
            }
        }

        private void OnMefClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var mefPath = DevenvAnalyzer.GetMefErrorsPath(RecentData, RecentHive);
                Process.Start(mefPath);
            }
            catch(Exception ex)
            {
                Status.Text = ex.Message;
            }
        }

        private void OnActivityLogClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var activityLogPath = DevenvAnalyzer.GetActivityLogPath(RecentData, RecentHive);
                Process.Start(activityLogPath);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        }

        private void OnCommandPromptClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var (commandLinePath, args) = DevenvAnalyzer.GetCommandLinePathAndArgs(RecentData, RecentPath, RecentHive);
                Process.Start(commandLinePath, args);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        }

        private void OnInstallLocationClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var installationPath = DevenvAnalyzer.GetInstallationPath(RecentPath);
                Process.Start(installationPath);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        }

        private void OnExtensionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var extensionPath = DevenvAnalyzer.GetExtensionPath(RecentData, RecentHive);
                Process.Start(extensionPath);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void OnMoreClick(object sender, RoutedEventArgs e)
        {
            var uiElement = sender as Control;
            if (uiElement?.ContextMenu != null)
                uiElement.ContextMenu.IsOpen = true;
        }
    }
}
