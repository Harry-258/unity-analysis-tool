namespace UnityAnalysisTool.YamlFormats;

/// <summary>
///  Represents an entry parsed from a Unity scene file in YAML format. It only contains the fields useful for this tool.
/// </summary>
public class MonoBehaviour
{
    public ScriptReference? m_Script { get; set; }
    public Dictionary<string, object>? OtherFields { get; set; }

    /// <summary>
    /// Finds all the fields that are not prefixed with "m_" and returns them or an empty list if there are none.
    /// </summary>
    /// <returns> The list of fields not prefixed with "_m". </returns>
    public List<string> GetOtherFieldNames()
    {
        if (OtherFields == null)
        {
            return new List<string>();
        }

        return OtherFields.Keys.Where(k => !k.StartsWith("m_")).ToList();
    }
}

/// <summary>
/// Represents an entry parsed from a Unity scene file in YAML format. It only contains the fields useful for this tool.
/// </summary>
public class ScriptReference
{
    public string? guid { get; set; }
}
