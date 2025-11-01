namespace UnityAnalysisTool.YamlFormats;

/// <summary>
///  Represents an entry parsed from a Unity script meta file in YAML format. It only contains the fields useful for this tool.
/// </summary>
public class MetaFile
{
    public string? guid { get; set; }
}
