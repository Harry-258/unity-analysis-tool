namespace UnityAnalysisTool.YamlFormats;

public class Transform
{
    public FileReference? m_GameObject { get; set; }
    public List<FileReference>? m_Children { get; set; }
	public FileReference? m_Father { get; set; }
    public bool visited { get; set; } = false;
}

public class FileReference
{
    public string fileID { get; set; } = "";
}