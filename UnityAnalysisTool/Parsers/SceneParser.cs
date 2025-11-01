using System.Collections.Concurrent;

using UnityAnalysisTool.Util;
using UnityAnalysisTool.YamlFormats;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UnityAnalysisTool.Parsers;

public class SceneParser
{
    /// <summary>
    /// Reads a file and parses it using the YAML deserializer.
    /// Writes the formatted result to a dump file.
    /// </summary>
    /// <param name="path"> The path to the scene file. </param>
    /// <param name="outputPath"> The path where the dump file should be paced at. </param>
    /// <param name="scriptIdToScript"> The dictionary containing all the scripts in the project. </param>
    public static void ParseUnitySceneFile(string path, string? outputPath, ConcurrentDictionary<string, Script> scriptIdToScript)
    {
        // Read file content and split it into different objects using "--- !u!"
        string fileContent = File.ReadAllText(path);
        string[] objects = fileContent.Split("--- !u!");

        string[] pathDirectories = path.Split(Path.DirectorySeparatorChar);
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

            string? firstLine = reader.ReadLine();
            if (firstLine is null)
            {
                // This should not be possible
                break;
            }

            string objType = firstLine.Split(" &")[0];
            string objId = firstLine.Split(" &")[1];
            string yamlObj = reader.ReadToEnd();
            var deserializedObj = deserializer.Deserialize<YamlObject>(yamlObj);

            switch (objType)
            {
                case "1":
                    if (deserializedObj?.GameObject?.m_Name is null)
                    {
                        throw new InvalidDataException("Missing GameObject or m_Name in YAML data.");
                    }
                    gameObjectMap.Add(objId, deserializedObj.GameObject);
                    break;
                case "4":
                    transformMap.Add(objId, deserializedObj.Transform);
                    break;
                case "114":
                    string guid = deserializedObj.MonoBehaviour.m_Script.guid;

                    var yamlFields = deserializedObj.MonoBehaviour.GetOtherFieldNames();
                    var scriptSerializedFields = scriptIdToScript[guid].SerializableFields;

                    if (yamlFields.Count == 0)
                    {
                        scriptIdToScript[guid].isUsed = true;
                        break;
                    }

                    // TODO: This is never reached ------------------------------------------------------------------------------------------------------

                    // --------------------
                    Console.WriteLine("GUID " + guid);
                    foreach (var field in yamlFields)
                    {
                        Console.WriteLine("yaml field " + field);
                    }
                    foreach (var field in scriptSerializedFields)
                    {
                        Console.WriteLine("script field " + field);
                    }
                    Console.WriteLine("---");
                    // --------------------

                    bool consistent = yamlFields.Any(fieldName => scriptSerializedFields.Contains(fieldName));
                    if (consistent)
                    {
                        scriptIdToScript[guid].isUsed = true;
                    }
                    break;
                default:
                    break;
            }
        }

        result = MakeDumpFileContents(gameObjectMap, transformMap, transformMap.Values.ToList(), "");
        WriteToDumpFile(outputPath, fileTitle, result);
    }

    /// <summary>
    /// Writes the given content to a dump file.
    /// </summary>
    /// <param name="outputPath"> The path where the dump file should be paced at. </param>
    /// <param name="fileTitle"> The title of the dump file, without the ".dump" extension. If it's null, it writes to the directory at the same level as the tool.</param>
    /// <param name="content"> The content to be placed in the dump file. </param>
    /// <returns></returns>
    static void WriteToDumpFile(string outputPath, string fileTitle, string content)
    {
        if (content != "")
        {
            if (outputPath is null)
            {
                File.WriteAllText(fileTitle + ".dump", content);
            }
            else
            {
                Directory.CreateDirectory(outputPath);
                File.WriteAllText(Path.Combine(outputPath, fileTitle + ".dump"), content);
            }
        }
    }

    /// <summary>
    /// Iterates through the transforms and formats the associated GameObject names into a string.
    /// Indents the names for children objects under their parent using "--" for each hierarchy level.
    /// </summary>
    /// <param name="gameObjectMap"> The map containing all the GameObject objects in the scene file. </param>
    /// <param name="transformMap"> The map containing all the Transform objects in the scene file. </param>
    /// <param name="transforms"> The list of all the Transform objects that should be taken into account. </param>
    /// <returns> A string containing the formatted GameObject names. </returns>
    static string MakeDumpFileContents(
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
            // Skip if we already visited this transform or if it has a parent transform that wasn't visited
            if (transform.visited || transform.m_Father.fileID != "0" && !transformMap[transform.m_Father.fileID].visited)
            {
                continue;
            }
            string objectName = gameObjectMap[transform.m_GameObject.fileID].m_Name;
            result.AppendLine(prefix + objectName);
            transform.visited = true;

            if (transform.m_Children != null && transform.m_Children.Count() != 0)
            {
                List<Transform> children = transform.m_Children.Select(child => transformMap[child.fileID]).ToList();
                result.Append(MakeDumpFileContents(gameObjectMap, transformMap, children, prefix + "--"));
            }
        }

        return result.ToString();
    }
}
