using System.Collections.Concurrent;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using UnityAnalysisTool.Util;

namespace UnityAnalysisTool.Parsers;

public class ScriptParser
{
    /// <summary>
    ///  Parses the given script and returns a list of all serialized fields.
    /// </summary>
    /// <param name="scriptPath"> The path to the script. </param>
    /// <param name="scriptId"> The GUID of the script. </param>
    /// <returns> A list of the serialized fields inside the script. </returns>
    public static List<string> ParseScript(string scriptPath, string scriptId)
    {
        // TODO: also look at the objects used in the script. They could be instances of other scripts => still used
        var code = File.ReadAllText(scriptPath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetCompilationUnitRoot();

        var serializedFields = new List<string>();

        // Look for serialized fields
        var fields = root.DescendantNodes().OfType<FieldDeclarationSyntax>();
        foreach (var field in fields)
        {
            // TODO: Does this include all types of fields or only serializable ones?
            bool isPublic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
            bool hasSerializeField = field.AttributeLists
                .SelectMany(a => a.Attributes)
                .Any(attr => attr.Name.ToString().Contains("SerializeField"));
            bool hasNonSerializedField = field.AttributeLists
                .SelectMany(a => a.Attributes)
                .Any(attr => attr.Name.ToString().Contains("NonSerialized"));

            if (!hasNonSerializedField && (isPublic || hasSerializeField))
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    serializedFields.Add(variable.Identifier.Text);
                }
            }
        }

        return serializedFields;
    }
}
