using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Gilzoide.FSharp.Editor
{
    public class FSharpBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            FSharpProjectGenerator.GenerateAndBuild(false).Wait();
        }
    }
}
