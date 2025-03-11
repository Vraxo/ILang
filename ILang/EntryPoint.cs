namespace ILang;

public class EntryPoint
{
    public static void Main()
    {
        try
        {
            // Step 1: Read the main input file
            string inputFilePath = "Program.c";
            string input = File.ReadAllText(inputFilePath);
            string currentDirectory = Path.GetDirectoryName(Path.GetFullPath(inputFilePath))!;

            // Step 2: Preprocess imports
            var preprocessor = new Preprocessor();
            string processedInput = preprocessor.ProcessImports(input, currentDirectory);

            // Step 3: Save the preprocessed code for debugging
            File.WriteAllText("Preprocessed.c", processedInput);
            Console.WriteLine("Preprocessed code saved to Preprocessed.c");

            // Step 4: Tokenize the processed input
            Tokenizer tokenizer = new();
            List<string> tokens = tokenizer.Tokenize(processedInput);

            // Step 5: Parse the tokens into a program
            Parser parser = new();
            var parsedProgram = parser.Parse(tokens);

            // Step 6: Save the parsed program to a YAML file (optional)
            parsedProgram.SaveToYaml("Program.yaml");
            Console.WriteLine("Program saved to Program.yaml.");

            // Step 7: Deserialize the YAML file (optional, for demonstration)
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            var program = deserializer.Deserialize<ParsedProgram>(File.ReadAllText("Program.yaml"));

            // Step 8: Execute the program
            Interpreter interpreter = new(program);
            interpreter.Execute();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}