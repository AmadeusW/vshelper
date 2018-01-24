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
            ThisWindow = this;
        }

        public delegate void WinEventProc(IntPtr hWinEventHook, User32.WindowsEventHookType @event, IntPtr hwnd, int idObject, int idChild, int dwEventThread, uint dwmsEventTime);
        WinEventProc Listener;
        static MainWindow ThisWindow;

        public string RecentId { get; private set; }
        public string RecentPath { get; private set; }
        public string RecentHive { get; private set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Listener = new WinEventProc(target);
                var functionPointer = Marshal.GetFunctionPointerForDelegate(Listener);
                User32.SetWinEventHook(User32.WindowsEventHookType.EVENT_SYSTEM_FOREGROUND, User32.WindowsEventHookType.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, functionPointer, 0, 0, User32.WindowsEventHookFlags.WINEVENT_OUTOFCONTEXT);
            }
            catch (Exception ex)
            {
                ThisWindow.Status.Text = ex.Message;
            }
        }

        static void target(IntPtr hWinEventHook, User32.WindowsEventHookType @event, IntPtr hwnd, int idObject, int idChild, int dwEventThread, uint dwmsEventTime)
        {
            try
            {
                var threadId = User32.GetWindowThreadProcessId(hwnd, out int processId);
                if (processId == 0)
                {
                    ThisWindow.Status.Text = "Unable to get process data";
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
                        if (o["Name"].ToString() != "devenv.exe")
                        {
                            return;
                        }

                        var id = DevenvAnalyzer.GetVsId(o["ExecutablePath"].ToString());
                        UpdateUI(id, o["CommandLine"].ToString(), o["ExecutablePath"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                ThisWindow.Status.Text = ex.Message;
            }
        }

        private static void UpdateUI(VSData data, string arguments, string path)
        {
            var rootSuffix = arguments.Split('/').FirstOrDefault(n => n.ToLower().StartsWith("rootsuffix"))?.Substring("rootsuffix".Length + 1);
            ThisWindow.Status.Text = data.InstallationChannel + " " + rootSuffix;
            ThisWindow.Version.Text = data.InstallationVersion;
            ThisWindow.RecentId = data.InstallationId;
            ThisWindow.RecentPath = path;
            ThisWindow.RecentHive = rootSuffix;
            ThisWindow.AllUI.Visibility = Visibility.Visible;
        }

        private void OnMefClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var mefPath = DevenvAnalyzer.GetMefErrorsPath(RecentId, RecentHive);
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
                var activityLogPath = DevenvAnalyzer.GetActivityLogPath(RecentId, RecentHive);
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
                var (commandLinePath, args) = DevenvAnalyzer.GetCommandLinePathAndArgs(RecentId, RecentPath, RecentHive);
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
                var extensionPath = DevenvAnalyzer.GetExtensionPath(RecentId, RecentHive);
                Process.Start(extensionPath);
            }
            catch (Exception ex)
            {
                Status.Text = ex.Message;
            }
        }
    }
}
