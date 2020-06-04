using SharpYaml.Tokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace helperapp
{
    public class OperationFormatter
    {
        private const string SpecialFolder = "SpecialFolder.";

        public static string Format(string format, Dictionary<string, string> properties)
        {
            var pattern = new Regex(@"\(.*?\)");
            var formatted = pattern.Replace(format, new MatchEvaluator(EvaluateGroup));
            return formatted;

            string EvaluateGroup(Match match)
            {
                var special = match.Groups[0].ToString().Trim('(', ')');
                if (special.StartsWith(SpecialFolder))
                {
                    var folderName = special.Substring(SpecialFolder.Length);
                    if (Enum.TryParse<Environment.SpecialFolder>(folderName, out var member))
                    {
                        var expanded = Environment.GetFolderPath(member);
                        return expanded;
                    }
                }
                else if (special.StartsWith("%"))
                {
                    var expanded = Environment.ExpandEnvironmentVariables(special);
                    return expanded;
                }
                else if (properties.TryGetValue(special, out string value))
                {
                    return value;
                }
                else
                {
                    // We were unable to expand this match
                    Debugger.Break();
                }
                return match.Groups[0].ToString();
            }
        }
    }
}
