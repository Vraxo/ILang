namespace ILang;

public class Tokenizer
{
    public List<string> Tokenize(string input)
    {
        List<string> tokens = new();
        int currentIndex = 0;

        while (currentIndex < input.Length)
        {
            // Skip whitespace
            while (currentIndex < input.Length && char.IsWhiteSpace(input[currentIndex]))
            {
                currentIndex++;
            }
            if (currentIndex >= input.Length) break;

            char currentChar = input[currentIndex];

            // Handle keywords and identifiers
            if (char.IsLetter(currentChar))
            {
                int start = currentIndex;
                while (currentIndex < input.Length && (char.IsLetterOrDigit(input[currentIndex]) || input[currentIndex] == '_'))
                {
                    currentIndex++;
                }
                string token = input.Substring(start, currentIndex - start);

                if (token is "true" or "false")
                {
                    tokens.Add(token); // Add as boolean literal
                }
                else if (token is "if" or "else" or "fun" or "let" or "while")
                {
                    tokens.Add(token); // Add as keyword
                }
                else
                {
                    tokens.Add(token); // Treat as identifier
                }
            }
            // Handle strings
            else if (currentChar == '"')
            {
                int start = currentIndex;
                currentIndex++;
                while (currentIndex < input.Length && input[currentIndex] != '"')
                {
                    currentIndex++;
                }
                if (currentIndex >= input.Length)
                {
                    Error("Unterminated string literal.");
                }
                currentIndex++;
                tokens.Add(input.Substring(start, currentIndex - start));
            }
            // Handle numbers
            else if (char.IsDigit(currentChar) || currentChar == '.')
            {
                int start = currentIndex;
                while (currentIndex < input.Length && (char.IsDigit(input[currentIndex]) || input[currentIndex] == '.'))
                {
                    currentIndex++;
                }
                tokens.Add(input.Substring(start, currentIndex - start));
            }
            // Handle multi-character operators (e.g., '->', '==', '!=')
            else if (currentChar == '-' && currentIndex + 1 < input.Length && input[currentIndex + 1] == '>')
            {
                tokens.Add("->");
                currentIndex += 2;
            }
            else if (currentChar == '=' && currentIndex + 1 < input.Length && input[currentIndex + 1] == '=')
            {
                tokens.Add("==");
                currentIndex += 2;
            }
            else if (currentChar == '!' && currentIndex + 1 < input.Length && input[currentIndex + 1] == '=')
            {
                tokens.Add("!=");
                currentIndex += 2;
            }
            // Handle single-character symbols
            else
            {
                tokens.Add(currentChar.ToString());
                currentIndex++;
            }
        }

        return tokens;
    }

    private void Error(string message)
    {
        Console.Error.WriteLine($"Error: {message}");
        Environment.Exit(1);
    }
}