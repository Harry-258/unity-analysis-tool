using UnityAnalysisTool.YamlFormats;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UnityAnalysisTool.Parsers;

public class MetaFileParser
{
    /// <summary> Parses the meta file of the given script and returns the GUID of the script. </summary>
    /// <param name="scriptPath"> The path to the script. </param>
    /// <returns> The GUID of the script. </returns>
    public static string ParseMetaFile(string scriptPath)
    {
        string fileContent = File.ReadAllText(scriptPath + ".meta");
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var deserialzedObj = deserializer.Deserialize<MetaFile>(fileContent);

        string guid = deserialzedObj.guid ?? "";
        if (guid == "")
        {
            Console.Error.WriteLine($"Warning: Missing GUID in script meta file at path {scriptPath + ".meta"}.");
        }

        return guid;
    }
}
