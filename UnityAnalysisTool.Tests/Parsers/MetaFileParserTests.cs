using Xunit;
using UnityAnalysisTool.Parsers;
using System;
using System.IO;

namespace UnityAnalysisTool.Tests.Parsers;

public class MetaFileParserTests : IDisposable
{
    private readonly string _tempScriptFilePath;
    
    public MetaFileParserTests()
    {
        _tempScriptFilePath = Path.Combine(Path.GetTempPath(), "ParseMetaFileTest.cs");
    }

    public void Dispose()
    {
        File.Delete(_tempScriptFilePath + ".meta");
    }
    
    [Fact]
    public void ParseMetaFile_ShouldReturnGUID_WhenGivenValidScriptPath()
    {
        string expectedResult = "a89aff9953cad48d587e40b4f4c88d9b";
        string content = @$"fileFormatVersion: 2
guid: {expectedResult}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: ";
        File.WriteAllText(_tempScriptFilePath + ".meta", content);

        string result = MetaFileParser.ParseMetaFile(_tempScriptFilePath);
        Assert.Equal(expectedResult, result);
    }
}