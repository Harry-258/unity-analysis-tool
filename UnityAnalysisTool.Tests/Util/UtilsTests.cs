using System.IO;
using UnityAnalysisTool.Util;
using UnityAnalysisTool.Parsers;
using System.Collections.Concurrent;
using Moq;

namespace UnityAnalysisTool.Tests.Util;

public class SetupTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _outputPath;
    private readonly string _projectPath;

    public SetupTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempRoot);

        _outputPath = Path.Combine(_tempRoot, "output");

        // Create empty directories where the project shoould be.
        _projectPath = Path.Combine(_tempRoot, "TestProject");
        Directory.CreateDirectory(Path.Combine(_projectPath, "Assets"));
    }

    public void Dispose()
    {
        Directory.Delete(_tempRoot, true);
    }

    [Fact]
    public void Setup_ShouldReturnCorrectPaths_WhenGivenValidArguments_AndWhenOutputDirectoryDoesNotExist()
    {
        string[] args = new[] { _projectPath, _outputPath };
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        var (outputPath, projectPath) = Utils.Setup(args);

        Assert.Equal(_projectPath, projectPath);
        Assert.Equal(Path.Combine(_outputPath, "output"), outputPath);

        string consoleOutputString = consoleOutput.ToString();
        Assert.Contains("Analyzing project at path: ", consoleOutputString);
        Assert.Contains("Writing ouput to: ", consoleOutputString);
    }

    [Fact]
    public void Setup_ShouldReturnCorrectPaths_WhenGivenValidArguments_AndWhenOutputDirectoryDoesExist()
    {
        string[] args = new[] { _projectPath, _outputPath };
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        Directory.CreateDirectory(Path.Combine(_outputPath, "output"));

        var (outputPath, projectPath) = Utils.Setup(args);

        Assert.Equal(_projectPath, projectPath);
        Assert.Equal(Path.Combine(_outputPath, "output"), outputPath);

        string consoleOutputString = consoleOutput.ToString();
        Assert.Contains("Analyzing project at path: ", consoleOutputString);
        Assert.Contains("Writing ouput to: ", consoleOutputString);
    }

    [Fact]
    public void Setup_ShouldThrowException_WhenGivenInvalidArguments()
    {
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);
        string[] args = new[] { "argument1" };


        var output = Assert.Throws<ArgumentException>(() =>
        {
            Utils.Setup(args);
        });

        string consoleOutputString = consoleOutput.ToString();

        Assert.Equal("Usage: ./tool.exe <path-to-unity-project> <path-to-output-directory>", output.Message);
        Assert.DoesNotContain("Analyzing project at path: ", consoleOutputString);
        Assert.DoesNotContain("Writing ouput to: ", consoleOutputString);
    }

    [Fact]
    public void Setup_ShouldThrowException_WhenAssetFolderDoesNotExist()
    {
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);
        string[] args = new[] { _projectPath, _outputPath };

        Directory.Delete(Path.Combine(_projectPath, "Assets"), true);

        var output = Assert.Throws<ArgumentException>(() =>
        {
            Utils.Setup(args);
        });

        string consoleOutputString = consoleOutput.ToString();

        Assert.Equal("Couldn't find Assets folder! Are you sure you specified the correct path to the project root?", output.Message);
        Assert.Contains("Analyzing project at path: ", consoleOutputString);
        Assert.Contains("Writing ouput to: ", consoleOutputString);

    }
}

// public class FindSceneFilesTests : IDisposable
// {
//     private readonly string _tempRootDir;
//     private readonly Mock<SceneParser> _sceneParserMock;
//
//     public FindSceneFilesTests()
//     {
//         _tempRootDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
//         Directory.CreateDirectory(_tempRootDir);
//
//         File.WriteAllText(Path.Combine(_tempRootDir, "Scene1.unity"), "test");
//         File.WriteAllText(Path.Combine(_tempRootDir, "Scene2.unity"), "test");
//
//         string subDir = Path.Combine(_tempRootDir, "subDir");
//         Directory.CreateDirectory(subDir);
//         File.WriteAllText(Path.Combine(subDir, "NestedScene.unity"), "test");
//
//         _sceneParserMock = new Mock<SceneParser>();
//
//         _sceneParserMock
//             .Setup(p => p.ParseUnitySceneFile(
//                 It.IsAny<string>(),
//                 It.IsAny<string?>(),
//                 It.IsAny<ConcurrentDictionary<string, Script>>()))
//             .Verifiable();
//     }
//
//     public void Dispose()
//     {
//         Directory.Delete(_tempRootDir, true);
//     }
//
//     [Fact]
//     public void FindSceneFiles_ShouldCallSceneParser_WhenGivenValidInput()
//     {
//         SceneParser.Instance = _sceneParserMock.Object;
//         ConcurrentDictionary<string, Script> scriptDictionary = new ConcurrentDictionary<string, Script>();
//
//         Utils.FindSceneFiles(_tempRootDir, Path.Combine(_tempRootDir, "output"), scriptDictionary);
//
//         _sceneParserMock.Verify(
//             p => p.ParseUnitySceneFile(
//                 It.IsAny<string>(),
//                 It.IsAny<string?>(),
//                 It.IsAny<ConcurrentDictionary<string, Script>>()),
//             Times.Exactly(3)
//         );
//     }
// }