using System.Collections.Concurrent;
using System.Collections.Generic;

using UnityAnalysisTool.Parsers;

namespace UnityAnalysisTool.Util;

public class Script
{
    public string id { get; set; }
    public string path { get; set; }
    public List<string> fields { get; set; }
    public bool isUsed { get; set; } = false;

    public Script(string id, List<string> fields, string path)
    {
        this.id = id;
        this.fields = fields;
        this.path = path;
    }

    /**
     * Searches for all the scripts in the project and saves their paths and serializable fields in the given dictionaries.
     */
    public static void SearchForScripts(
        string root,
        ConcurrentDictionary<string, Script> scriptIdToScript
    )
    {
        string[] files = Directory.GetFiles(root);
        string[] directories = Directory.GetDirectories(root);

        Parallel.ForEach(files, file =>
        {
            if (Path.GetExtension(file) == ".cs")
            {
                var scriptId = MetaFileParser.ParseMetaFile(file);
                var serializableFields = ScriptParser.ParseScript(file, scriptId, scriptIdToScript);
                scriptIdToScript.TryAdd(scriptId, new Script(scriptId, serializableFields, file));
            }
        });
        Parallel.ForEach(directories, dir =>
        {
            SearchForScripts(dir, scriptIdToScript);
        });
    }
}
