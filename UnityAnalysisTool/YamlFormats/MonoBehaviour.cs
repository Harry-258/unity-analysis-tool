namespace UnityAnalysisTool.YamlFormats;

public class MonoBehaviour
{
    public ScriptReference? m_Script { get; set; }
    public Dictionary<string, object> OtherFields { get; set; }

    public List<string> GetOtherFieldNames()
    {
        if (OtherFields == null)
        {
            return new List<string>();
        }

        return OtherFields.Keys.Where(k => !k.StartsWith("m_")).ToList();
    }
}

public class ScriptReference
{
    public string? guid { get; set; }
}
