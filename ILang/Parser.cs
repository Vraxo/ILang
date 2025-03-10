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
        pos++;
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
                case "while":
                    ops.AddRange(ParseWhileStatement(tokens, ref pos));
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
        var condition = ParseExpr(tokens, ref pos);
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
        var condition = ParseExpr(tokens, ref pos);
        Expect(tokens, ref pos, ")");
        Expect(tokens, ref pos, "{");
        var body = ParseBlock(tokens, ref pos);

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

        while (pos < tokens.Count && !"});".Contains(tokens[pos]))
        {
            if (tokens[pos] == "(")
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

    private bool IsOperator(string token)
    {
        return token == "+" || token == "-" || token == "*" || token == "/" ||
               token == "==" || token == "!=" || token == "!" || token == "<" || token == ">";
    }

    private int GetPrec(string op) => op switch
    {
        "!" => 4,    // Highest precedence (unary operator)
        "*" or "/" => 2,
        "+" or "-" => 1,
        "==" or "!=" => 0,
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