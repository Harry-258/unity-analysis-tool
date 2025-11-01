using System.Collections.Concurrent;
using System.Collections.Generic;

using UnityAnalysisTool.Parsers;

namespace UnityAnalysisTool.Util;

public class Script
{
    public string id { get; set; }
    public string path { get; set; }
    public List<string> SerializableFields { get; set; }
    public bool isUsed { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Script"/> class.
    /// </summary>
    /// <param name="id">The GUID related to the script.</param>
    /// <param name="SerializableFields">A list of serializable field names found in the script.</param>
    /// <param name="path">The relative path to the script.</param>
    public Script(string id, List<string> SerializableFields, string path)
    {
        this.id = id;
        this.SerializableFields = SerializableFields;
        this.path = path;
    }

    /// <summary>
    /// Searches for all the scripts in the project and saves their paths and serializable fields in the given dictionaries.
    /// </summary>
    /// <param name="root">The root directory to search for scripts in.</param>
    /// <param name="scriptIdToScript">The dictionary to save the scripts in.</param>
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
                int relativePathIndex = file.IndexOf("Assets");
                var relativePath = file.Substring(relativePathIndex);

                var scriptId = MetaFileParser.ParseMetaFile(file);
                var fields = ScriptParser.ParseScript(file, scriptId);

                scriptIdToScript.TryAdd(scriptId, new Script(scriptId, fields, relativePath));
            }
        });
        Parallel.ForEach(directories, dir =>
        {
            SearchForScripts(dir, scriptIdToScript);
        });
    }
}
