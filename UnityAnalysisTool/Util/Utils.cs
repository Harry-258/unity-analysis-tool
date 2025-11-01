using System.Collections.Concurrent;
using System.IO;

using UnityAnalysisTool.Parsers;

namespace UnityAnalysisTool.Util;

public class Utils
{
    /// <summary>
    /// Checks if the given arguments are valid and sets up the directory for the output.
    /// </summary>
    /// <param name="args"> The arguments passed to the tool. </param>
    /// <returns> The path to the output directory. </returns>
    public static (string outputPath, string projectPath) Setup(string[] args)
    {
        if (args.Length != 2)
        {
            throw new ArgumentException("Usage: ./tool.exe <path-to-unity-project> <path-to-output-directory>");
        }

        string outputPath = Path.Combine(args[1], "output");
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }
        Directory.CreateDirectory(outputPath);

        string projectPath = args[0].Trim(' ', Path.DirectorySeparatorChar);
        Console.WriteLine("Analyzing project at path: " + projectPath);
        Console.WriteLine("Writing ouput to: " + outputPath);

        // Check if the Assets directory exists
        string[] directories = Directory.GetDirectories(projectPath, "Assets", SearchOption.TopDirectoryOnly);
        if (directories.Length == 0)
        {
            throw new ArgumentException("Couldn't find Assets folder! Are you sure you specified the correct path to the project root?");
        }

        return (outputPath, projectPath);
    }

    /// <summary>
    ///  Finds all the scene files starting from the given root directory and parses them in parallel.
    ///  If it encounters a directory, it will recursively search for scene files in it.
    /// </summary>
    /// <param name="root"> The root directory to search for scene files in. </param>
    /// <param name="outputPath"> The path to the output directory. </param>
    /// <param name="scriptIdToScript"> The concurrent dictionary containing information about the found scripts. </param>
    public static void FindSceneFiles(
        string root,
        string? outputPath,
        ConcurrentDictionary<string, Script> scriptIdToScript
    )
    {
        string[] files = Directory.GetFiles(root);
        string[] directories = Directory.GetDirectories(root);

        Parallel.ForEach(files, file =>
        {
            if (Path.GetExtension(file) == ".unity")
            {
                SceneParser.ParseUnitySceneFile(file, outputPath, scriptIdToScript);
            }
        });
        Parallel.ForEach(directories, dir =>
        {
            // Use the new directory as the output path while still searching for scene files in the project
            string dirName = dir.Split(Path.DirectorySeparatorChar)[^1];
            FindSceneFiles(dir, Path.Combine(outputPath ?? "", dirName), scriptIdToScript);
        });
    }
}
