using System.Text;

namespace ILang;

public class Preprocessor
{
    private readonly HashSet<string> _includedFiles = new();

    public string ProcessImports(string input, string currentDirectory)
    {
        using var reader = new StringReader(input);
        var output = new StringBuilder();
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            string trimmedLine = line.Trim();

            // Process import statements
            if (trimmedLine.StartsWith("import "))
            {
                string importPath = ExtractImportPath(trimmedLine);
                string fullPath = Path.Combine(currentDirectory, importPath + ".c");
                ProcessFile(fullPath, output, Path.GetDirectoryName(fullPath)!);
            }
            else
            {
                output.AppendLine(line); // Keep non-import lines
            }
        }

        return output.ToString();
    }

    private void ProcessFile(string filePath, StringBuilder output, string currentDirectory)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        if (_includedFiles.Contains(filePath))
            return; // Prevent duplicate includes

        _includedFiles.Add(filePath);

        string fileContent = File.ReadAllText(filePath);
        using var reader = new StringReader(fileContent);
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            string trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("import "))
            {
                string nestedImportPath = ExtractImportPath(trimmedLine);
                string nestedFullPath = Path.Combine(currentDirectory, nestedImportPath + ".c");
                ProcessFile(nestedFullPath, output, Path.GetDirectoryName(nestedFullPath)!);
            }
            else
            {
                output.AppendLine(line); // Add non-import lines
            }
        }
    }

    private static string ExtractImportPath(string line)
    {
        // Expected format: "import math;"
        string[] parts = line.Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || parts[0] != "import")
            throw new FormatException($"Invalid import statement: {line}");

        return parts[1];
    }
}