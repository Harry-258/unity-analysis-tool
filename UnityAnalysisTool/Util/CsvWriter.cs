using System.IO;

namespace UnityAnalysisTool.Util;

public class CsvWriter
{
    public static void Write(string outputPath, List<Script> scripts)
    {
        string[] headers = { "Relative Path", "GUID" };

        using (StreamWriter writer = new StreamWriter(Path.Combine(outputPath, "UnusedScripts.csv")))
        {
            writer.WriteLine(string.Join(",", headers));
            foreach (Script script in scripts)
            {
                if (!script.isUsed)
                {
                    string[] row = { script.path, script.id };
                    writer.WriteLine(string.Join(",", row));
                }
            }
        }
    }
}
