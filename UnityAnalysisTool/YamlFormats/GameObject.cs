namespace UnityAnalysisTool.YamlFormats;

/// <summary>
///  Represents a game object entry parsed from a Unity YAML file. It only contains the fields useful for this tool.
/// </summary>
public class GameObject
{
    public string? id { get; set; }
    public string? m_Name { get; set; }
}
