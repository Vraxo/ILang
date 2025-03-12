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

            // Handle single-line comments (//)
            if (currentChar == '/' && currentIndex + 1 < input.Length && input[currentIndex + 1] == '/')
            {
                // Skip everything until the end of the line
                while (currentIndex < input.Length && input[currentIndex] != '\n')
                {
                    currentIndex++;
                }
                continue; // Move to the next character after the comment
            }

            // Handle multi-line comments (/* ... */)
            if (currentChar == '/' && currentIndex + 1 < input.Length && input[currentIndex + 1] == '*')
            {
                currentIndex += 2; // Skip '/*'
                while (currentIndex + 1 < input.Length &&
                      !(input[currentIndex] == '*' && input[currentIndex + 1] == '/'))
                {
                    currentIndex++;
                }
                if (currentIndex + 1 >= input.Length)
                {
                    Error("Unterminated multi-line comment.");
                }
                currentIndex += 2; // Skip '*/'
                continue; // Move to the next character after the comment
            }

            // Handle keywords and identifiers
            if (char.IsLetter(currentChar))
            {
                int start = currentIndex;
                while (currentIndex < input.Length &&
                      (char.IsLetterOrDigit(input[currentIndex]) || input[currentIndex] == '_'))
                {
                    currentIndex++;
                }
                string token = input.Substring(start, currentIndex - start);

                // Add as boolean literal, keyword, or identifier
                if (token is "true" or "false")
                {
                    tokens.Add(token); // Boolean literal
                }
                else if (token is "if" or "else" or "fun" or "let" or "while")
                {
                    tokens.Add(token); // Keyword
                }
                else
                {
                    tokens.Add(token); // Identifier (e.g., num_to_string)
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
                while (currentIndex < input.Length &&
                      (char.IsDigit(input[currentIndex]) || input[currentIndex] == '.'))
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
            // Handle single-character comparison operators (e.g., '<', '>')
            else if (currentChar == '<' || currentChar == '>')
            {
                tokens.Add(currentChar.ToString());
                currentIndex++;
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