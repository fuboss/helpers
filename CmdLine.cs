using System.Collections.Generic;
using System.Linq;

namespace Assets.Code.Tools
{
    public static class CmdLine
    {
        public static IEnumerable<string> GetCmdLineParameters()
        {
            return System.Environment.GetCommandLineArgs().Skip(1).Select(s => s.TrimStart('-').ToLowerInvariant());
        }
    }
}
