using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UnityAnalysisTool.YamlFormats;

namespace UnityAnalysisTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: find a better way of parsing using the tag in the comment on the 2nd line?
            // TODO: Specify output directory
            
            // TODO: Test this
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ./tool.exe <path-to-unity-project>");
                return;
            }
            
            // TODO: Check if there is a project at the given path and if path is valid

            string projectPath = args[0].Trim(' ', '\\');
            Console.WriteLine("Analyzing project at path: " + projectPath);
            
            // TODO: write a test for a project without an Assets folder
            // Check if the Assets directory exists
            string[] directories = Directory.GetDirectories(projectPath, "Assets", SearchOption.TopDirectoryOnly);
            if (directories.Length == 0)
            {
                // TODO: Change to error
                Console.WriteLine("Couldn't find Assets folder! Are you sure you specified the correct path to the project root?");
                return;
            }
            
            // I decided to keep the same directory structure for the dump files
            // as the scene files in the original project to keep them organized.
            string assetsPath = Path.Combine(projectPath, "Assets");
            findSceneFiles(assetsPath);
        }

        /**
         * Finds all the scene files starting from the given root directory.
         * If it encounters a directory, it will recursively search for scene files in it.
         */
        static void findSceneFiles(string root)
        {
            string[] files = Directory.GetFiles(root);
            string[] directories = Directory.GetDirectories(root);
            Parallel.ForEach(files, file =>
            {
                parseFile(file);
            });
            Parallel.ForEach(directories, dir =>
            {
                findSceneFiles(dir);
            });
        }

        /**
         * Reads a file and parses it using the YAML deserializer.
         * If it finds a GameObject, it creates a dump file to add it to.
         *
         * TODO: path parameter comments?
         */
        static void parseFile(string path)
        {
            // Read file content and split it into different objects using "--- !u!"
            string fileContent = File.ReadAllText(path);
            string[] objects = fileContent.Split("--- !u!");
            
            // TODO: Make them in different directories by checking if there is anything between file name and "Assets"
            string[] pathDirectories = path.Split('\\');
            string fileTitle = pathDirectories[pathDirectories.Length - 1];
            
            // Initialize YAML deserializer such that it ignores tags that are not in the formats defined in UnityAnalysisTool.YamlFormats
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

            // We want to skip the first element of the split array because it's just the YAML header
            bool firstPass = true;
            
            // Store the resulting string and rewrite after iterating through the objects to
            // avoid appending to an existing file if the script is run multiple times.
            // Also faster than opening the same file multiple times and writing to it.
            string result = "";
            foreach (string obj in objects)
            {
                if (firstPass)
                {
                    firstPass = false;
                    continue;
                }

                using var reader = new StringReader(obj);

                // Read the first line to get the object type and id
                string? firstLine = reader.ReadLine();
                if (firstLine is null)
                {
                    // TODO: Is this even possible?
                    break;
                }
                
                string objType = firstLine.Split(" &")[0], objId = firstLine.Split(" &")[1];

                // if it is a GameObject, save the name to the file
                if (objType == "1")
                {
                    string yamlObj = reader.ReadToEnd();
                    var deserializedObj = deserializer.Deserialize<YamlObject>(yamlObj);
                    if (deserializedObj?.GameObject?.m_Name is null)
                    {
                        throw new InvalidDataException("Missing GameObject or m_Name in YAML data.");
                    }

                    result += System.Environment.NewLine + deserializedObj.GameObject.m_Name;
                }
            }
            // TODO: question: if result is empty, don't write?
            // Path.GetDirectoryName? Directory.CreateDirectory?

            Console.WriteLine(result);
            // File.WriteAllText(fileTitle + ".dump", fileContent);
        }
    }
}