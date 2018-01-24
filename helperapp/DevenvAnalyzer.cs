using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helperapp
{
    struct VSData
    {
        public string InstallationId;
        public string InstallationChannel;
        public string InstallationVersion;
    }

    static class DevenvAnalyzer
    {
        static Dictionary<string, VSData> PathToDataMap = new Dictionary<string, VSData>();
        const string installationIdString = "InstallationID";
        const string installationChannelString = "ChannelTitle";
        const string installationVersionString = "DisplayVersion";

        public static VSData GetVsId(string path)
        {
            VSData vsData;
            if (!PathToDataMap.TryGetValue(path, out vsData))
            {
                var directory = Path.GetDirectoryName(path);
                var configurationFile = Path.Combine(directory, "devenv.isolation.ini");

                string installationId = String.Empty;
                string installationChannel = String.Empty;
                string installationVersion = String.Empty;
                foreach (var line in File.ReadAllLines(configurationFile))
                {
                    if (line.StartsWith(installationIdString))
                    {
                        installationId = line.Substring(line.IndexOf('=') + 1);
                    }
                    if (line.StartsWith(installationChannelString))
                    {
                        installationChannel = line.Substring(line.IndexOf('=') + 1);
                    }
                    if (line.StartsWith(installationVersionString))
                    {
                        installationVersion = line.Substring(line.IndexOf('=') + 1);
                    }

                }
                if (String.IsNullOrEmpty(installationId))
                    throw new Exception($"Could not find {installationIdString} in {configurationFile}");
                if (String.IsNullOrEmpty(installationChannel))
                    throw new Exception($"Could not find {installationChannel} in {configurationFile}");
                if (String.IsNullOrEmpty(installationVersion))
                    throw new Exception($"Could not find {installationVersion} in {configurationFile}");
                vsData = new VSData
                {
                    InstallationId = installationId,
                    InstallationChannel = installationChannel.Trim('"'),
                    InstallationVersion = installationVersion.Trim('"'),
                };
                PathToDataMap[path] = vsData;
            }
            return vsData;
        }

        internal static string GetMefErrorsPath(string id, string hive)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft\\VisualStudio\\15.0_" + id + hive,
                "ComponentModelCache",
                "Microsoft.VisualStudio.Default.err");
        }

        internal static (string, string) GetCommandLinePathAndArgs(string id, string path, string hive)
        {
            var cmdPath = Environment.ExpandEnvironmentVariables("%comspec%");
            var args = "/k \"" + Path.Combine(
                Path.GetDirectoryName(path),
                "..\\Tools",
                "VsDevCmd.bat")
                + "\"";
            return (cmdPath, args);
        }

        internal static string GetActivityLogPath(string id, string hive)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Microsoft\\VisualStudio\\15.0_" + id + hive,
                "ActivityLog.xml");
        }

        internal static string GetInstallationPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        internal static string GetExtensionPath(string id, string hive)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft\\VisualStudio\\15.0_" + id + hive,
                "Extensions");
        }
    }
}
