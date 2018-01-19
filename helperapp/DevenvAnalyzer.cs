using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helperapp
{
    static class DevenvAnalyzer
    {
        static Dictionary<string, string> PathToIdMap = new Dictionary<string, string>();
        const string installationIdString = "InstallationID";

        public static string GetVsId(string path)
        {
            string installationId;
            if (!PathToIdMap.TryGetValue(path, out installationId))
            {
                var directory = Path.GetDirectoryName(path);
                var configurationFile = Path.Combine(directory, "devenv.isolation.ini");
                foreach (var line in File.ReadAllLines(configurationFile))
                {
                    if (line.StartsWith(installationIdString))
                    {
                        installationId = line.Substring(line.IndexOf('=') + 1);
                        PathToIdMap[path] = installationId;
                        return installationId;
                    }
                }
                throw new Exception($"Could not find {installationIdString} in {configurationFile}");
            }
            return installationId;
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
    }
}
