using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityAnalysisTool.Parsers;
using UnityAnalysisTool.Util;
using UnityAnalysisTool.YamlFormats;

namespace UnityAnalysisTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Test with paths with single \ and double \\ (pbt?)
            // TODO: Test this
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ./tool.exe <path-to-unity-project> <path-to-output-directory>");
                return;
            }

            string outputPath = args[1];
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
            Directory.CreateDirectory(outputPath);

            // TODO: Check if there is a project at the given path and if path is valid

            // TODO: This should be Path.DirectorySeparatorChar
            string projectPath = args[0].Trim(' ', '\\');
            Console.WriteLine("Analyzing project at path: " + projectPath);
            Console.WriteLine("Writing ouput to: " + outputPath);

            // TODO: write a test for a project without an Assets folder
            // Check if the Assets directory exists
            string[] directories = Directory.GetDirectories(projectPath, "Assets", SearchOption.TopDirectoryOnly);
            if (directories.Length == 0)
            {
                Console.WriteLine("Couldn't find Assets folder! Are you sure you specified the correct path to the project root?");
                return;
            }

            string assetsPath = Path.Combine(projectPath, "Assets");
            var scriptIdToScript = new ConcurrentDictionary<string, Script>();

            Script.SearchForScripts(assetsPath, scriptIdToScript);
            FindSceneFiles(assetsPath, outputPath, scriptIdToScript);

            CsvWriter.Write(outputPath, scriptIdToScript.Values.ToList());
        }

        /**
         * Finds all the scene files starting from the given root directory.
         * If it encounters a directory, it will recursively search for scene files in it.
         */
        static void FindSceneFiles(
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
                string dirName = dir.Split("\\")[^1];
                FindSceneFiles(dir, Path.Combine(outputPath ?? "", dirName), scriptIdToScript);
            });
        }
    }
}
