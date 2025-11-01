namespace UnityAnalysisTool.YamlFormats;

/// <summary>
/// Represents an entry parsed from a Unity scene file in YAML format. It only contains the fields useful for this tool.
/// </summary>
public class YamlObject
{
    public GameObject? GameObject { get; set; }
    public Transform? Transform { get; set; }
    public MonoBehaviour? MonoBehaviour { get; set; }
}
