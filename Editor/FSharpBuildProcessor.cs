using Gilzoide.FSharp.Editor.Internal;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            FSharpProjectGenerator.GenerateAndBuild(true, true).Forget();
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            FSharpProjectGenerator.GenerateAndBuild(false, false).Wait();
        }
    }
}
