using System.Collections.Concurrent;

using UnityAnalysisTool.Util;
using UnityAnalysisTool.YamlFormats;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UnityAnalysisTool.Parsers;

public class SceneParser
{
    /**
     * Reads a file and parses it using the YAML deserializer.
     * If it finds a GameObject, it creates a dump file to add it to.
     *
     * TODO: path parameter comments?
     */
    public static void ParseUnitySceneFile(string path, string? outputPath, ConcurrentDictionary<string, Script> scriptIdToScript)
    {
        // Read file content and split it into different objects using "--- !u!"
        string fileContent = File.ReadAllText(path);
        string[] objects = fileContent.Split("--- !u!");

        // TODO: Change to universal divider
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
                    // TODO: Check for monobehavior serializable fields, mark as used if consistent
                    break;
                default:
                    break;
            }
        }

        result = MakeDumpFileContents(gameObjectMap, transformMap, transformMap.Values.ToList(), "");

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
     * Indents the names for children transforms under their parent using the prefix parameter.
     *
     * TODO: parameters?
     */
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
