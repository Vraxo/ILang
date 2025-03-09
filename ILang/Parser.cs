namespace ILang;

public class Parser
{
    private Function? _currentFunc;

    public ParsedProgram Parse(List<string> tokens)
    {
        var program = new ParsedProgram();
        int pos = 0;

        while (pos < tokens.Count)
        {
            if (tokens[pos] == "fun")
            {
                pos++;
                var func = ParseFunction(tokens, ref pos);
                program.Functions.Add(func);
            }
            else
            {
                pos++; // Always advance position
            }
        }

        return program;
    }

    private Function ParseFunction(List<string> tokens, ref int pos)
    {
        _currentFunc = new Function { Name = tokens[pos++] };

        Expect(tokens, ref pos, "(");
        _currentFunc.Parameters = ParseParams(tokens, ref pos);
        Expect(tokens, ref pos, "->");
        _currentFunc.ReturnType = ParseType(tokens[pos++]);

        Expect(tokens, ref pos, "{");
        _currentFunc.Operations = ParseBlock(tokens, ref pos);

        var parsedFunc = _currentFunc;
        _currentFunc = null;
        return parsedFunc;
    }

    private List<ValueObject> ParseParams(List<string> tokens, ref int pos)
    {
        var parameters = new List<ValueObject>();
        while (tokens[pos] != ")")
        {
            var param = new ValueObject { Name = tokens[pos++] };
            Expect(tokens, ref pos, ":");
            param.Type = ParseType(tokens[pos++]);
            parameters.Add(param);
            if (tokens[pos] == ",") pos++;
        }
        pos++; // Skip closing ')'
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
                    ops.AddRange(ParseLet(tokens, ref pos));
                    break;
                case "if":
                    ops.AddRange(ParseIfStatement(tokens, ref pos));
                    break;
                case "print":
                    ops.AddRange(ParseCall("print", tokens, ref pos));
                    break;
                default:
                    if (char.IsLetter(tokens[pos][0]))
                        ops.AddRange(ParseCall(tokens[pos++], tokens, ref pos));
                    else
                        pos++; // Prevent infinite loop
                    break;
            }
        }
        pos++; // Skip closing '}'
        return ops;
    }

    private List<Operation> ParseLet(List<string> tokens, ref int pos)
    {
        pos++; // Skip 'let'
        string varName = tokens[pos++];
        _currentFunc!.Variables.Add(varName);

        Expect(tokens, ref pos, ":");
        pos++; // Skip type
        Expect(tokens, ref pos, "=");

        var ops = ParseExpr(tokens, ref pos);
        ops.Add(new Operation { Command = "store_var", Argument = varName });
        Expect(tokens, ref pos, ";");
        return ops;
    }

    private List<Operation> ParseIfStatement(List<string> tokens, ref int pos)
    {
        pos++; // Skip 'if'
        Expect(tokens, ref pos, "(");

        // Parse condition FIRST
        var conditionOps = ParseExpr(tokens, ref pos);

        Expect(tokens, ref pos, ")");
        Expect(tokens, ref pos, "{");
        var ifOperations = ParseBlock(tokens, ref pos);
        var elseOperations = new List<Operation>();

        if (pos < tokens.Count && tokens[pos] == "else")
        {
            pos++;
            Expect(tokens, ref pos, "{");
            elseOperations = ParseBlock(tokens, ref pos);
        }

        // Combine condition and if operation
        var result = new List<Operation>();
        result.AddRange(conditionOps);
        result.Add(new Operation
        {
            Command = "if",
            NestedOperations = ifOperations,
            ElseOperations = elseOperations
        });

        return result;
    }

    private List<Operation> ParseCall(string name, List<string> tokens, ref int pos)
    {
        pos++; // Skip '('
        var ops = ParseExpr(tokens, ref pos);
        Expect(tokens, ref pos, ")");
        if (pos < tokens.Count && tokens[pos] == ";") pos++;
        ops.Add(new Operation { Command = "call", Argument = name });
        return ops;
    }

    private List<Operation> ParseExpr(List<string> tokens, ref int pos)
    {
        var output = new List<Operation>();
        var stack = new Stack<string>();
        bool expectUnary = true; // Track if the next operator can be unary

        while (pos < tokens.Count && !"});".Contains(tokens[pos]))
        {
            string token = tokens[pos];
            if (token == "(")
            {
                stack.Push(token);
                pos++;
                expectUnary = true;
            }
            else if (token == ")")
            {
                while (stack.Peek() != "(")
                    output.Add(new Operation { Command = stack.Pop() });
                stack.Pop();
                pos++;
                expectUnary = false;
            }
            else if (IsOperator(token))
            {
                // Handle unary operators (e.g., "!condition")
                if (expectUnary && token == "!")
                {
                    stack.Push("unary!");
                }
                else
                {
                    while (stack.Count > 0 && GetPrec(stack.Peek()) >= GetPrec(token))
                        output.Add(new Operation { Command = stack.Pop() });
                    stack.Push(token);
                }
                pos++;
                expectUnary = true;
            }
            else
            {
                output.Add(new Operation
                {
                    Command = IsIdentifier(token) ? "load_var" : "push",
                    Argument = token
                });
                pos++;
                expectUnary = false;
            }
        }

        while (stack.Count > 0)
            output.Add(new Operation { Command = stack.Pop() });

        return output;
    }

    private bool IsOperator(string token) =>
        token is "+" or "-" or "*" or "/" or "==" or "!=" or "!";

    private int GetPrec(string op) => op switch
    {
        "unary!" => 5, // Highest precedence for unary NOT
        "*" or "/" => 3,
        "+" or "-" => 2,
        "==" or "!=" => 1,
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
        _ => ValueObjectType.Void
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