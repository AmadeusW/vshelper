using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace helperapp
{
    internal static class Storage
    {
        private const string VisibleControlsStoragePath = "visible.txt";
        private const string OperationsDirectory = "Operations";
        private const string OperationsPattern = "*.yaml";

        internal static void SaveVisibleControls(Dictionary<string, bool> settings)
        {
            if (settings == null)
                return;

            var visibleControls = settings.Where(kv => kv.Value).Select(kv => kv.Key);
            File.WriteAllLines(VisibleControlsStoragePath, visibleControls);
        }

        internal static Dictionary<string, bool> LoadVisibleControls()
        {
            var settings = new Dictionary<string, bool>();
            if (File.Exists(VisibleControlsStoragePath))
            {
                foreach (var line in File.ReadAllLines(VisibleControlsStoragePath))
                {
                    settings[line] = true;
                }
            }
            else
            {
                // Seed default settings for new installations
                settings["InstallationVersion"] = true;
                settings["InstallationDirectory"] = true;
                settings["Developer command prompt"] = true;
            }
            return settings;
        }

        internal static IReadOnlyList<Operation> LoadOperations()
        {
            List<Operation> operations = new List<Operation>();
            Directory.CreateDirectory(OperationsDirectory);
            foreach (var file in Directory.EnumerateFiles(OperationsDirectory, OperationsPattern))
            {
                try
                {
                    var operation = LoadOperation(file);
                    operations.Add(operation);
                }
                catch
                {
                    Debugger.Break();
                }
            }
            return operations.AsReadOnly();
        }

        internal static Operation LoadOperation(string file)
        {
            var streamReader = File.OpenText(file);
            var operation = new Deserializer().Deserialize<Operation>(streamReader);
            return operation;
        }

        internal static string GetOperationsPath()
        {
            return Path.GetFullPath(OperationsDirectory);
        }
    }
}
