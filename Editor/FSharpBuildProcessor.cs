using Gilzoide.FSharp.Editor.Internal;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            FSharpProjectGenerator.GenerateFsproj();
            FSharpBuilder.Build(
                FSharpPlatform.Player,
                report.summary.options.HasFlag(BuildOptions.Development) ? FSharpConfiguration.Debug : FSharpConfiguration.Release
            );
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            FSharpBuilder.BuildAsync(FSharpPlatform.Editor, FSharpConfiguration.Debug).Forget();
        }
    }
}
