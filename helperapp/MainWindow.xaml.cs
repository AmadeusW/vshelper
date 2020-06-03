using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VSData = System.Collections.Generic.Dictionary<string, string>;

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
            this.DisplayPreference = Storage.LoadVisibleControls();
            var operations = Storage.LoadOperations();
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
            Storage.SaveVisibleControls(this.DisplayPreference);
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
                    AppWindow.Status.Text = string.Empty;

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
