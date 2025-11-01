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
            // TODO: Specify output directory as argument
            
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

            // TODO: This should be universal, not with \
            string projectPath = args[0].Trim(' ', '\\');
            Console.WriteLine("Analyzing project at path: " + projectPath);
            Console.WriteLine("Writing ouput at path: " + outputPath);
            
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
            findSceneFiles(assetsPath, outputPath);
        }

        /**
         * Finds all the scene files starting from the given root directory.
         * If it encounters a directory, it will recursively search for scene files in it.
         */
        static void findSceneFiles(string root, string? outputPath)
        {
            string[] files = Directory.GetFiles(root);
            string[] directories = Directory.GetDirectories(root);
            
            
            foreach (string file in files)
            {
                parseFile(file, outputPath);
            }
            foreach (string dir in directories)
            {
                string dirName = dir.Split("\\")[^1];
                findSceneFiles(dir, Path.Combine(outputPath ?? "", dirName));
            }
            
            // Parallel.ForEach(files, file =>
            // {
            //     parseFile(file, outputPath);
            // });
            // Parallel.ForEach(directories, dir =>
            // {
            //     // Get the name of the new directory
            //     string dirName = dir.Split("\\")[^1];
            //     
            //     // Use the new directory as the output path while still searching for scene files in the project 
            //     findSceneFiles(dir, Path.Combine(outputPath ?? "", dirName));
            // });
        }

        /**
         * Reads a file and parses it using the YAML deserializer.
         * If it finds a GameObject, it creates a dump file to add it to.
         *
         * TODO: path parameter comments?
         */
        static void parseFile(string path, string? outputPath)
        {
            // Read file content and split it into different objects using "--- !u!"
            string fileContent = File.ReadAllText(path);
            string[] objects = fileContent.Split("--- !u!");
            
            string[] pathDirectories = path.Split('\\');
            string fileTitle = pathDirectories[pathDirectories.Length - 1];
            
            // Initialize YAML deserializer such that it ignores tags that are not in the formats defined in UnityAnalysisTool.YamlFormats
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            
            string result = "";
            var transformMap = new Dictionary<string, Transform>();
            var gameObjectMap = new Dictionary<string, GameObject>();

            // Iterate through the objects and add them to the dictionaries
            // The first one will always be the YAML header, so we skip it
            foreach (string obj in objects.Skip(1))
            {
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
                if (objType == "1" || objType == "4")
                {
                    string yamlObj = reader.ReadToEnd();
                    var deserializedObj = deserializer.Deserialize<YamlObject>(yamlObj);
                    
                    if (objType == "1")
                    {
                        if (deserializedObj?.GameObject?.m_Name is null)
                        {
                            throw new InvalidDataException("Missing GameObject or m_Name in YAML data.");
                        }
                        gameObjectMap.Add(objId, deserializedObj.GameObject);
                    }
                    else
                    {
                        transformMap.Add(objId, deserializedObj.Transform);
                    }
                }
            }
            
            result = makeFileContents(gameObjectMap, transformMap, transformMap.Values.ToList(), "");

            if (result != "")
            {
                if (outputPath is null)
                {
                    File.WriteAllText(fileTitle + ".dump", result);
                }
                else
                {
                    Directory.CreateDirectory(outputPath);
                    File.WriteAllText(Path.Combine(outputPath, fileTitle + ".dump"), result);
                }
            }
        }

        /**
         * Iterates through the transforms and formats the associated GameObject names into a string.
         *
         * TODO: parameters?
         */
        static string makeFileContents(
            Dictionary<string, GameObject> gameObjectMap,
            Dictionary<string, Transform> transformMap, 
            List<Transform> transforms, 
            string prefix
        )
        {
            var result = new System.Text.StringBuilder();
            
            // Iterate through the Transform map and add the associated GameObject name to the result
            foreach (Transform transform in transforms)
            {
                if (transform.visited)
                {
                    continue;
                }
                string objectName = gameObjectMap[transform.m_GameObject.fileID].m_Name;
                result.AppendLine(prefix + objectName);
                transform.visited = true;

                if (transform.m_Children != null && transform.m_Children.Count() != 0)
                {
                    List<Transform> children = transform.m_Children.Select(child => transformMap[child.fileID]).ToList();
                    result.Append(makeFileContents(gameObjectMap, transformMap, children, prefix + "--"));
                }
            }
            
            return result.ToString();
        }
    }
}