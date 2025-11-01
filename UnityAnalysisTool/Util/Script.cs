using System.Collections.Generic;

namespace UnityAnalysisTool.Util;

public class Script
{
    public string id { get; set; }
    public List<string> fields { get; set; }
    public bool isUsed { get; set; } = false;

    public Script(string id, List<string> fields)
    {
        this.id = id;
        this.fields = fields;
    }

    /**
     * Searches for all the scripts in the project and saves their paths and serializable fields in the given dictionaries.
     */
    static void SearchForScripts(
        root,
        ConcurrentDictionary<string, Script> scriptIdToScript,
    )
    {
        string[] files = Directory.GetFiles(root);
        string[] directories = Directory.GetDirectories(root);

        Parallel.ForEach(files, file =>
        {
            if (Path.GetExtension(file) == ".cs")
            {
                var scriptId = MetaFileParser.ParseMetaFile(file);
                var serializableFields = ScriptParser.ParseScript(file, scriptId);
                scriptIdToScript.TryAdd(scriptId, new Script(scriptId, serializableFields));
            }
        });
        Parallel.ForEach(directories, dir =>
        {
            SearchForScripts(dir, scriptIdToPathDictionary, scriptIdToSerializableFields);
        });
    }
}
