namespace ILang;

public class Parser
{
    private Function _currentFunction;

    public ParsedProgram Parse(List<string> tokens)
    {
        var parsedProgram = new ParsedProgram();
        int currentIndex = 0;

        while (currentIndex < tokens.Count)
        {
            if (tokens[currentIndex] == "fun")
            {
                currentIndex++;
                var function = ParseFunction(tokens, ref currentIndex);
                parsedProgram.Functions.Add(function);
            }
            else
            {
                currentIndex++;
            }
        }

        return parsedProgram;
    }

    private Function ParseFunction(List<string> tokens, ref int currentIndex)
    {
        _currentFunction = new Function();

        // Parse function name
        if (currentIndex >= tokens.Count)
        {
            Error("Expected function name after 'fun'.");
        }
        _currentFunction.Name = tokens[currentIndex++];

        // Parse '('
        if (currentIndex >= tokens.Count || tokens[currentIndex] != "(")
        {
            Error("Expected '(' after function name.");
        }
        currentIndex++;

        // Parse parameters
        _currentFunction.Parameters = ParseParameters(tokens, ref currentIndex);

        // Parse '->'
        if (currentIndex >= tokens.Count || tokens[currentIndex] != "->")
        {
            Error("Expected '->' after function parameters.");
        }
        currentIndex++;

        // Parse return type
        if (currentIndex >= tokens.Count)
        {
            Error("Expected return type after '->'.");
        }
        _currentFunction.ReturnType = ParseType(tokens[currentIndex++]);

        // Parse '{'
        if (currentIndex >= tokens.Count || tokens[currentIndex] != "{")
        {
            Error("Expected '{' after return type.");
        }
        currentIndex++;

        // Parse function body
        _currentFunction.Operations = ParseBody(tokens, ref currentIndex);

        // Parse '}'
        if (currentIndex >= tokens.Count || tokens[currentIndex] != "}")
        {
            Error("Expected '}' after function body.");
        }
        currentIndex++;

        return _currentFunction;
    }

    private List<ValueObject> ParseParameters(List<string> tokens, ref int currentIndex)
    {
        var parameters = new List<ValueObject>();

        while (currentIndex < tokens.Count && tokens[currentIndex] != ")")
        {
            // Parameter name
            if (!IsIdentifier(tokens[currentIndex]))
            {
                Error("Expected parameter name.");
            }
            string paramName = tokens[currentIndex++];

            // Colon after name
            if (currentIndex >= tokens.Count || tokens[currentIndex] != ":")
            {
                Error("Expected ':' after parameter name.");
            }
            currentIndex++;

            // Parameter type
            if (currentIndex >= tokens.Count || !IsTypeToken(tokens[currentIndex]))
            {
                Error("Expected type after ':'.");
            }
            ValueObjectType paramType = ParseType(tokens[currentIndex++]);

            parameters.Add(new ValueObject { Name = paramName, Type = paramType });

            // Check for comma or closing ')'
            if (currentIndex < tokens.Count && tokens[currentIndex] == ",")
            {
                currentIndex++;
                if (currentIndex >= tokens.Count || tokens[currentIndex] == ")")
                {
                    Error("Expected parameter after ','.");
                }
            }
            else if (currentIndex < tokens.Count && tokens[currentIndex] != ")")
            {
                Error("Expected ',' or ')' after parameter.");
            }
        }

        if (currentIndex >= tokens.Count || tokens[currentIndex] != ")")
        {
            Error("Expected ')' after parameters.");
        }
        currentIndex++;

        return parameters;
    }

    private List<Operation> ParseBody(List<string> tokens, ref int currentIndex)
    {
        var operations = new List<Operation>();

        while (currentIndex < tokens.Count && tokens[currentIndex] != "}")
        {
            string token = tokens[currentIndex];

            if (token == "print")
            {
                currentIndex++;
                operations.AddRange(ParseFunctionCall("print", tokens, ref currentIndex));
            }
            else if (token == "let")
            {
                currentIndex++;
                operations.AddRange(ParseLetStatement(tokens, ref currentIndex));
            }
            else if (IsIdentifier(token))
            {
                string funcName = token;
                currentIndex++;
                operations.AddRange(ParseFunctionCall(funcName, tokens, ref currentIndex));
            }
            else
            {
                Error($"Unknown statement: {token}");
            }
        }

        return operations;
    }

    private List<Operation> ParseLetStatement(List<string> tokens, ref int currentIndex)
    {
        var operations = new List<Operation>();

        // Parse variable name
        if (currentIndex >= tokens.Count || !IsIdentifier(tokens[currentIndex]))
        {
            Error("Expected variable name after 'let'.");
        }
        string varName = tokens[currentIndex++];

        // Track the variable in the current function
        _currentFunction.Variables.Add(varName);

        // Parse colon and type
        if (currentIndex >= tokens.Count || tokens[currentIndex] != ":")
        {
            Error("Expected ':' after variable name.");
        }
        currentIndex++;

        if (currentIndex >= tokens.Count || !IsTypeToken(tokens[currentIndex]))
        {
            Error("Expected type after ':'.");
        }
        ValueObjectType varType = ParseType(tokens[currentIndex++]);

        // Parse '='
        if (currentIndex >= tokens.Count || tokens[currentIndex] != "=")
        {
            Error("Expected '=' in variable declaration.");
        }
        currentIndex++;

        // Parse initializer expression
        operations.AddRange(ParseExpression(tokens, ref currentIndex, new[] { ";" }));

        // Add store operation
        operations.Add(new Operation { Command = "store_var", Argument = varName });

        // Parse ';'
        if (currentIndex >= tokens.Count || tokens[currentIndex] != ";")
        {
            Error("Expected ';' after variable declaration.");
        }
        currentIndex++;

        return operations;
    }

    private List<Operation> ParseFunctionCall(string functionName, List<string> tokens, ref int currentIndex)
    {
        var operations = new List<Operation>();

        // Parse '('
        if (currentIndex >= tokens.Count || tokens[currentIndex] != "(")
        {
            Error("Expected '(' after function name.");
        }
        currentIndex++;

        // Parse arguments as expressions
        operations.AddRange(ParseExpression(tokens, ref currentIndex, new[] { ")" }));

        // Parse ')'
        if (currentIndex >= tokens.Count || tokens[currentIndex] != ")")
        {
            Error("Expected ')' after arguments.");
        }
        currentIndex++;

        // Parse ';' if it's a statement
        if (currentIndex < tokens.Count && tokens[currentIndex] == ";")
        {
            currentIndex++;
        }

        operations.Add(new Operation { Command = "call", Argument = functionName });
        return operations;
    }

    private List<Operation> ParseExpression(List<string> tokens, ref int currentIndex, string[] terminators)
    {
        var output = new List<Operation>();
        var operatorStack = new Stack<string>();

        while (currentIndex < tokens.Count && !terminators.Contains(tokens[currentIndex]))
        {
            string token = tokens[currentIndex];

            if (IsNumber(token))
            {
                // Push numeric literals
                output.Add(new Operation { Command = "push", Argument = token });
                currentIndex++;
            }
            else if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                // Push string literals
                output.Add(new Operation { Command = "push", Argument = token });
                currentIndex++;
            }
            else if (IsIdentifier(token))
            {
                // Check if the identifier is a parameter or declared variable
                if (_currentFunction.Parameters.Any(p => p.Name == token) || _currentFunction.Variables.Contains(token))
                {
                    // Load the variable's value
                    output.Add(new Operation { Command = "load_var", Argument = token });
                    currentIndex++;
                }
                else if (currentIndex + 1 < tokens.Count && tokens[currentIndex + 1] == "(")
                {
                    // Handle function calls
                    string nestedFunc = token;
                    currentIndex++;
                    output.AddRange(ParseFunctionCall(nestedFunc, tokens, ref currentIndex));
                }
                else
                {
                    throw new InvalidOperationException($"Undefined variable or function: {token}");
                }
            }
            else if (token == "+" || token == "-" || token == "*" || token == "/")
            {
                // Handle operators with precedence
                while (operatorStack.Count > 0 && GetPrecedence(operatorStack.Peek()) >= GetPrecedence(token))
                {
                    output.Add(new Operation { Command = operatorStack.Pop() });
                }
                operatorStack.Push(token);
                currentIndex++;
            }
            else if (token == "(")
            {
                // Handle opening parenthesis
                operatorStack.Push(token);
                currentIndex++;
            }
            else if (token == ")")
            {
                // Handle closing parenthesis
                while (operatorStack.Peek() != "(")
                {
                    output.Add(new Operation { Command = operatorStack.Pop() });
                }
                operatorStack.Pop(); // Discard '('
                currentIndex++;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected token in expression: {token}");
            }
        }

        // Pop remaining operators
        while (operatorStack.Count > 0)
        {
            output.Add(new Operation { Command = operatorStack.Pop() });
        }

        return output;
    }

    private int GetPrecedence(string op)
    {
        return op switch
        {
            "+" or "-" => 1,
            "*" or "/" => 2,
            _ => 0
        };
    }

    private ValueObjectType ParseType(string typeToken)
    {
        return typeToken switch
        {
            "string" => ValueObjectType.String,
            "num" => ValueObjectType.Number,
            "void" => ValueObjectType.Void,
            _ => throw new ArgumentException($"Invalid type: {typeToken}")
        };
    }

    private bool IsIdentifier(string token) =>
        !string.IsNullOrEmpty(token) && (char.IsLetter(token[0]) || token[0] == '_');

    private bool IsTypeToken(string token) =>
        token is "string" or "num" or "void";

    private bool IsNumber(string token) =>
        double.TryParse(token, out _);

    private void Error(string message)
    {
        Console.Error.WriteLine($"Error: {message}");
        Environment.Exit(1);
    }
}