using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace helperapp
{
    static class DevenvAnalyzer
    {
        static Dictionary<string, VSData> PathToDataMap = new Dictionary<string, VSData>();
        const string installationIdString = "InstallationID";
        const string installationChannelString = "ChannelTitle";
        const string installationVersionString = "DisplayVersion";
        const string skuString = "SKU";

        public static VSData GetVsData(string path, string arguments)
        {
            VSData vsData;
            if (!PathToDataMap.TryGetValue(path, out vsData))
            {
                var directory = Path.GetDirectoryName(path);
                var configurationFile = Path.Combine(directory, "devenv.isolation.ini");

                string installationId = string.Empty;
                string installationChannel = string.Empty;
                string installationVersion = string.Empty;
                string sku = string.Empty;
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
                    if (line.StartsWith(skuString))
                    {
                        sku = line.Substring(line.IndexOf('=') + 1);
                    }
                }
                if (string.IsNullOrEmpty(installationId))
                    throw new Exception($"Could not find {installationIdString} in {configurationFile}");
                if (string.IsNullOrEmpty(installationChannel))
                    throw new Exception($"Could not find {installationChannelString} in {configurationFile}");
                if (string.IsNullOrEmpty(installationVersion))
                    throw new Exception($"Could not find {installationVersionString} in {configurationFile}");
                if (string.IsNullOrEmpty(sku))
                    throw new Exception($"Could not find {skuString} in {configurationFile}");

                var rootSuffix = arguments.Split('/').FirstOrDefault(n => n.ToLower().StartsWith("rootsuffix"))?.Substring("rootsuffix".Length + 1) ?? string.Empty;
                var rootFolder = path.Split('\\').Reverse().Skip(3).FirstOrDefault() ?? string.Empty;

                vsData = new VSData
                {
                    InstallationId = installationId,
                    InstallationChannel = installationChannel.Trim('"'),
                    InstallationVersion = installationVersion.Trim('"'),
                    SKU = sku,
                    MajorVersion = installationVersion.Trim('"').Split('.').First(),
                    Hive = rootSuffix,
                    RootFolderName = rootFolder,
                    Path = path
                };
                PathToDataMap[path] = vsData;
            }
            return vsData;
        }

        internal static string GetMefErrorsPath(VSData data, string hive)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                $"Microsoft\\VisualStudio\\{data.MajorVersion}.0_{data.InstallationId}{hive}",
                "ComponentModelCache",
                "Microsoft.VisualStudio.Default.err");
        }

        internal static (string, string) GetCommandLinePathAndArgs(VSData data, string path, string hive)
        {
            var cmdPath = Environment.ExpandEnvironmentVariables("%comspec%");
            var args = "/k \"" + Path.Combine(
                Path.GetDirectoryName(path),
                "..\\Tools",
                "VsDevCmd.bat")
                + "\"";
            return (cmdPath, args);
        }

        internal static string GetActivityLogPath(VSData data, string hive)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                $"Microsoft\\VisualStudio\\{data.MajorVersion}.0_{data.InstallationId}{hive}",
                "ActivityLog.xml");
        }

        internal static string GetInstallationPath(string path)
        {
            return Path.GetDirectoryName(path);
        }

        internal static string GetExtensionPath(VSData data, string hive)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                $"Microsoft\\VisualStudio\\{data.MajorVersion}.0_{data.InstallationId}{hive}",
                "Extensions");
        }
    }
}
