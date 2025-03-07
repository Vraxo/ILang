namespace ILang;

public class EntryPoint
{
    public static void Main()
    {
        string input = File.ReadAllText("Program.c");
        Tokenizer tokenizer = new();
        List<string> tokens = tokenizer.Tokenize(input);

        Parser parser = new();
        var parsedProgram = parser.Parse(tokens);

        parsedProgram.SaveToYaml("Program.yaml");
        Console.WriteLine("Program saved to Program.yaml.");

        var deserializer = new YamlDotNet.Serialization.Deserializer();
        var program = deserializer.Deserialize<ParsedProgram>(File.ReadAllText("Program.yaml"));

        Interpreter interpreter = new(program);
        interpreter.Execute();
    }
}