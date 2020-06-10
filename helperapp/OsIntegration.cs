using System.Diagnostics;

namespace helperapp
{
    internal static class OsIntegration
    {
        internal static void Invoke(string path, string args = null)
        {
            args = args ?? string.Empty;
            Process.Start(path, args);
        }
    }
}
