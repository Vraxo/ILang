namespace ILang;

public class ParsedProgram
{
    public List<Function> Functions { get; set; } = [];

    public void SaveToYaml(string filePath)
    {
        var yaml = new YamlDotNet.Serialization.Serializer();
        string yamlContent = yaml.Serialize(this);
        File.WriteAllText(filePath, yamlContent);
    }
}