using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using UnityAnalysisTool.YamlFormats;

namespace UnityAnalysisTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: find a better way of parsing using the tag in the comment on the 2nd line?
            
            // TODO: Test this
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ./tool.exe <path-to-unity-project>");
                return;
            }
            
            // TODO: Check if there is a project at the given path and if path is valid

            string path = args[0];
            Console.WriteLine("Analyzing file at path: " + path);

            // TODO: remove
            string actualPath = "D:\\Unity\\Unity projects\\Analysis Test\\Analysis Test\\Assets\\Scenes\\SampleScene.unity";

            // Read file content
            string fileContent = File.ReadAllText(actualPath);
            string[] objects = fileContent.Split("--- !u!");
            
            // Initialize YAML deserializer
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

            // We want to skip the first element of the split array because it's just the header
            bool firstPass = true;
            
            foreach (string obj in objects)
            {
                if (firstPass)
                {
                    firstPass = false;
                    continue;
                }
                using var reader = new StringReader(obj);
                
                // Read the first line to get the object type and id
                string firstLine = reader.ReadLine();
                string objType = firstLine.Split(" &")[0], objId = firstLine.Split(" &")[1];
                
                if (objType == "1")
                {
                    // save the rest of the string content to deserialize
                    string yamlObj = reader.ReadToEnd();
                    
                    // Console.WriteLine(yamlObj);
                    var deserialzedObj = deserializer.Deserialize<YamlObject>(yamlObj);
                    Console.WriteLine($"{deserialzedObj.GameObject.m_Name}");
                }
            }
        }
    }
}