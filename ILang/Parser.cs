namespace ILang;

public class Parser
{
    private Function? _currentFunc; // Tracks the current function being parsed

    public ParsedProgram Parse(List<string> tokens)
    {
        Console.WriteLine("Token Stream:");
        Console.WriteLine(string.Join(" ", tokens.Select((t, i) => $"[{i}:{t}]")));

        var program = new ParsedProgram();
        int pos = 0;

        while (pos < tokens.Count)
        {
            if (tokens[pos] == "extern")
            {
                // Handle external function declarations
                var externalFunc = ParseExternalFunction(tokens, ref pos);
                program.Functions.Add(externalFunc);
            }
            else if (tokens[pos] == "fun")
            {
                // Handle regular function declarations
                pos++; // Skip 'fun'
                var func = ParseRegularFunction(tokens, ref pos);
                program.Functions.Add(func);
            }
            else
            {
                pos++; // Skip unrecognized tokens
            }
        }

        return program;
    }

    private ExternalFunction ParseExternalFunction(List<string> tokens, ref int pos)
    {
        pos++; // Skip 'extern'
        string externalPath = tokens[pos++].Trim('"');
        string funcName = tokens[pos++];

        // Explicitly consume '(' after function name
        Expect(tokens, ref pos, "(");

        var parameters = ParseParams(tokens, ref pos); // Handles parameters and ')'

        Expect(tokens, ref pos, "->");
        var returnType = ParseType(tokens[pos++]);
        Expect(tokens, ref pos, ";");

        return new ExternalFunction
        {
            ExternalPath = externalPath,
            Name = funcName,
            Parameters = parameters,
            ReturnType = returnType
        };
    }

    private Function ParseRegularFunction(List<string> tokens, ref int pos)
    {
        _currentFunc = new Function { Name = tokens[pos++] }; // Set function name

        // Parse parameters
        Expect(tokens, ref pos, "(");
        _currentFunc.Parameters = ParseParams(tokens, ref pos);
        Expect(tokens, ref pos, ")");
        Expect(tokens, ref pos, "->");
        _currentFunc.ReturnType = ParseType(tokens[pos++]);

        // Parse function body
        Expect(tokens, ref pos, "{");
        _currentFunc.Operations = ParseBlock(tokens, ref pos);

        var parsedFunc = _currentFunc;
        _currentFunc = null; // Reset current function
        return parsedFunc;
    }

    private List<ValueObject> ParseParams(List<string> tokens, ref int pos)
    {
        var parameters = new List<ValueObject>();

        // Process parameters until closing ')'
        while (pos < tokens.Count && tokens[pos] != ")")
        {
            var param = new ValueObject { Name = tokens[pos++] };
            Expect(tokens, ref pos, ":");
            param.Type = ParseType(tokens[pos++]);
            parameters.Add(param);

            // Handle comma separator
            if (pos < tokens.Count && tokens[pos] == ",") pos++;
            else if (pos < tokens.Count && tokens[pos] != ")")
                throw new Exception($"Expected ',' or ')', got {tokens[pos]}");
        }

        if (pos < tokens.Count) pos++; // Consume closing ')'
        return parameters;
    }

    private List<Operation> ParseBlock(List<string> tokens, ref int pos)
    {
        var ops = new List<Operation>();
        while (pos < tokens.Count && tokens[pos] != "}")
        {
            switch (tokens[pos])
            {
                case "let":
                    ops.AddRange(ParseLet(tokens, ref pos)); // Variable declaration
                    break;
                case "if":
                    ops.AddRange(ParseIfStatement(tokens, ref pos)); // If statement
                    break;
                case "while":
                    ops.AddRange(ParseWhileStatement(tokens, ref pos)); // While loop
                    break;
                case "return":
                    ops.AddRange(ParseReturn(tokens, ref pos)); // Return statement
                    break;
                default:
                    if (pos + 1 < tokens.Count && tokens[pos + 1] == "=")
                    {
                        ops.AddRange(ParseAssignment(tokens, ref pos)); // Assignment
                    }
                    else if (tokens[pos] == "print" || tokens[pos] == "num_to_string")
                    {
                        ops.AddRange(ParseCall(tokens[pos++], tokens, ref pos)); // Built-in function
                    }
                    else if (char.IsLetter(tokens[pos][0]))
                    {
                        ops.AddRange(ParseCall(tokens[pos++], tokens, ref pos)); // Function call
                    }
                    else
                    {
                        pos++; // Skip unrecognized tokens
                    }
                    break;
            }
        }
        pos++; // Skip closing '}'
        return ops;
    }

    private List<Operation> ParseLet(List<string> tokens, ref int pos)
    {
        pos++; // Skip 'let'
        string varName = tokens[pos++]; // Variable name
        _currentFunc!.Variables.Add(varName); // Track variable

        Expect(tokens, ref pos, ":");
        pos++; // Skip type
        Expect(tokens, ref pos, "=");

        var ops = ParseExpr(tokens, ref pos); // Parse expression
        ops.Add(new Operation { Command = "store_var", Argument = varName }); // Store variable
        Expect(tokens, ref pos, ";");
        return ops;
    }

    private List<Operation> ParseAssignment(List<string> tokens, ref int pos)
    {
        string varName = tokens[pos]; // Variable name
        pos += 2; // Skip variable name and '='

        var ops = ParseExpr(tokens, ref pos); // Parse right-hand side expression
        ops.Add(new Operation { Command = "store_var", Argument = varName }); // Store result

        Expect(tokens, ref pos, ";"); // Ensure semicolon exists
        return ops;
    }

    private List<Operation> ParseReturn(List<string> tokens, ref int pos)
    {
        pos++; // Skip "return"
        var returnOps = ParseExpr(tokens, ref pos); // Parse return value
        Expect(tokens, ref pos, ";"); // Ensure semicolon exists
        returnOps.Add(new Operation { Command = "return" });
        return returnOps;
    }

    private List<Operation> ParseIfStatement(List<string> tokens, ref int pos)
    {
        pos++; // Skip 'if'
        Expect(tokens, ref pos, "(");
        var condition = ParseExpr(tokens, ref pos); // Parse condition
        Expect(tokens, ref pos, ")");
        Expect(tokens, ref pos, "{");
        var ifOperations = ParseBlock(tokens, ref pos); // Parse if block
        var elseOperations = new List<Operation>();

        // Parse else block (if present)
        if (pos < tokens.Count && tokens[pos] == "else")
        {
            pos++;
            Expect(tokens, ref pos, "{");
            elseOperations = ParseBlock(tokens, ref pos);
        }

        // Combine condition and blocks into an if operation
        var result = new List<Operation>();
        result.AddRange(condition);
        result.Add(new Operation
        {
            Command = "if",
            NestedOperations = ifOperations,
            ElseOperations = elseOperations
        });
        return result;
    }

    private List<Operation> ParseWhileStatement(List<string> tokens, ref int pos)
    {
        pos++; // Skip 'while'
        Expect(tokens, ref pos, "(");
        var condition = ParseExpr(tokens, ref pos); // Parse condition
        Expect(tokens, ref pos, ")");
        Expect(tokens, ref pos, "{");
        var body = ParseBlock(tokens, ref pos); // Parse loop body

        // Create a loop operation with condition and body
        return new List<Operation>
        {
            new Operation
            {
                Command = "loop",
                NestedOperations = condition,
                ElseOperations = body
            }
        };
    }

    private List<Operation> ParseCall(string name, List<string> tokens, ref int pos)
    {
        pos++; // Skip '('
        var ops = ParseExpr(tokens, ref pos); // Parse arguments
        Expect(tokens, ref pos, ")");
        if (pos < tokens.Count && tokens[pos] == ";") pos++; // Skip semicolon (if present)
        ops.Add(new Operation { Command = "call", Argument = name }); // Add call operation
        return ops;
    }

    private List<Operation> ParseExpr(List<string> tokens, ref int pos)
    {
        var output = new List<Operation>();
        var stack = new Stack<string>();

        while (pos < tokens.Count && !"});".Contains(tokens[pos]))
        {
            // Handle function calls (e.g., num_to_string(...))
            if (char.IsLetter(tokens[pos][0]) && pos + 1 < tokens.Count && tokens[pos + 1] == "(")
            {
                string funcName = tokens[pos];
                pos += 2; // Skip function name and '('

                // Parse arguments recursively
                var args = new List<List<Operation>>();
                while (pos < tokens.Count && tokens[pos] != ")")
                {
                    var argOps = ParseExpr(tokens, ref pos);
                    args.Add(argOps);
                    if (pos < tokens.Count && tokens[pos] == ",")
                        pos++;
                }
                pos++; // Skip ')'

                // Add arguments and function call operation
                foreach (var arg in args)
                {
                    output.AddRange(arg);
                }
                output.Add(new Operation { Command = "call", Argument = funcName });
            }
            else if (tokens[pos] == "(")
            {
                stack.Push(tokens[pos++]);
            }
            else if (tokens[pos] == ")")
            {
                while (stack.Peek() != "(")
                    output.Add(new Operation { Command = stack.Pop() });
                stack.Pop();
                pos++;
            }
            else if (IsOperator(tokens[pos]))
            {
                while (stack.Count > 0 && GetPrec(stack.Peek()) >= GetPrec(tokens[pos]))
                    output.Add(new Operation { Command = stack.Pop() });
                stack.Push(tokens[pos++]);
            }
            else
            {
                output.Add(new Operation
                {
                    Command = IsIdentifier(tokens[pos]) ? "load_var" : "push",
                    Argument = tokens[pos]
                });
                pos++;
            }
        }

        while (stack.Count > 0)
        {
            string op = stack.Pop();
            if (op != "(" && op != ")")
                output.Add(new Operation { Command = op });
        }

        return output;
    }

    private bool IsOperator(string token) =>
        token is "+" or "-" or "*" or "/" or "==" or "!=" or "!" or "<" or ">";

    private int GetPrec(string op) => op switch
    {
        "!" => 4,    // Highest precedence (unary operator)
        "*" or "/" => 2,
        "+" or "-" => 1,
        "==" or "!=" or "<" or ">" => 0,
        _ => 0
    };

    private bool IsIdentifier(string token)
    {
        return _currentFunc != null &&
              (_currentFunc.Variables.Contains(token) ||
               _currentFunc.Parameters.Any(p => p.Name == token));
    }

    private ValueObjectType ParseType(string type) => type switch
    {
        "num" => ValueObjectType.Number,
        "bool" => ValueObjectType.Bool,
        "string" => ValueObjectType.String,
        "void" => ValueObjectType.Void,
        _ => throw new Exception($"Unknown type: {type}")
    };

    private void Expect(List<string> tokens, ref int pos, string expected)
    {
        if (pos >= tokens.Count)
            throw new Exception($"Unexpected end of input. Expected '{expected}'.");

        if (tokens[pos] != expected)
            throw new Exception($"Expected '{expected}' at position {pos}, but found '{tokens[pos]}'.");

        pos++;
    }
}