using System.Collections.Concurrent;

using UnityAnalysisTool.Util;

namespace UnityAnalysisTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var (outputPath, projectPath) = Utils.Setup(args);

            string assetsPath = Path.Combine(projectPath, "Assets");
            var scriptIdToScript = new ConcurrentDictionary<string, Script>();

            Script.SearchForScripts(assetsPath, scriptIdToScript);
            Utils.FindSceneFiles(assetsPath, outputPath, scriptIdToScript);

            CsvWriter.Write(outputPath, scriptIdToScript.Values.ToList());
        }
    }
}
