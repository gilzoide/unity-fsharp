using System.Collections.Generic;
using System.Threading.Tasks;
using Gilzoide.FSharp.Editor.Internal;

namespace Gilzoide.FSharp.Editor
{
    public static class DotnetRunner
    {
        public static Task<bool> Run(string progressMessage, bool async, params string[] args)
        {
            return Run(progressMessage, async, (IEnumerable<string>) args);
        }

        public static async Task<bool> Run(string progressMessage, bool async, IEnumerable<string> args)
        {
            // Install .NET SDK if necessary
            bool installedDotnet = await DotnetInstaller.InstallDotnetSdk(async);
            if (!installedDotnet)
            {
                return false;
            }

            return await ProcessRunner.Run(progressMessage, async, $"{DotnetInstaller.DotnetInstallDir}/dotnet", args);
        }
    }
}
