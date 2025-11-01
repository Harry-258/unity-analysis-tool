using UnityAnalysisTool.YamlFormats;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UnityAnalysisTool.Parsers;

public class MetaFileParser
{
    /**
     * Parses the meta file of the given script and returns the GUID of the script.
     */
    public static string ParseMetaFile(string scriptPath)
    {
        string fileContent = File.ReadAllText(scriptPath + ".meta");
        var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
        var deserialzedObj = deserializer.Deserialize<MetaFile>(fileContent);

        string guid = deserialzedObj.guid;

        return guid
    }
}
