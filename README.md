# unity-analysis-tool

This is a .NET tool that analyzes Unity projects. This tool scans Unity scenes and scripts to identify unused C# MonoBehaviour scripts and generates structured dumps of your scene hierarchies. It does this by analyzing the files in parallel and uses the Roslyn API to parse C# files.

## Getting Started

### Clone the repository

```bash
git clone git@github.com:Harry-258/unity-analysis-tool.git
cd unity-analysis-tool
```

### Run the tool:
From the root directory
```bash
cd UnityAnalysisTool
dotnet run <path-to-unity-project> <path-to-output-directory>
```

Example:
```bash
cd UnityAnalysisTool
dotnet run "C:\Projects\MyUnityGame" "C:\AnalysisOutput"
```

### Run the tests:

From the root directory
```bash
cd UnityAnalysisTool.Tests
dotnet test
```

## Notes:

The tool assumes that all .cs files inside the project are directly inherited from MonoBehaviour and ignores the prefab system. It does not use any Unity API and works without running the Unity Editor.

## Further Improvements:

This project was done over the course of 2 days as part of a challenge. With more time, these are the improvements that could be made:

- Optimize the tool by using more efficient data structures. This could allow the tool to run constantly in the background of an IDE. Some solutions may inlcude using binary trees to keep track of hierarchies.
- Add support for other types of scripts, such as those inheriting from ScriptableObject.
