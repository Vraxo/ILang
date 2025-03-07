using System;
using System.Collections.Generic;

namespace ILang;

public class StatementParser
{
    private readonly ExpressionParser _expressionParser;

    public StatementParser(ExpressionParser expressionParser)
    {
        _expressionParser = expressionParser;
    }

    public List<Operation> ParseBlock(TokenStream tokens)
    {
        var operations = new List<Operation>();
        tokens.Expect("{", "Expected '{'");

        while (!tokens.Match("}"))
        {
            if (tokens.Match("let"))
            {
                operations.AddRange(ParseLetStatement(tokens));
            }
            else if (ParserUtils.IsIdentifier(tokens.Current))
            {
                operations.AddRange(ParseFunctionCall(tokens));
            }
            else
            {
                throw new InvalidOperationException($"Unknown statement: {tokens.Current}");
            }
        }

        tokens.Advance(); // Skip '}'
        return operations;
    }

    private List<Operation> ParseLetStatement(TokenStream tokens)
    {
        tokens.Advance(); // Skip 'let'
        string varName = tokens.Consume();

        tokens.Expect(":", "Expected ':' after variable name.");
        ValueObjectType varType = ParserUtils.ParseType(tokens.Consume());

        tokens.Expect("=", "Expected '=' in variable declaration.");
        var operations = _expressionParser.Parse(tokens, new[] { ";" });
        operations.Add(new Operation { Command = "store_var", Argument = varName });

        tokens.Expect(";", "Expected ';' after variable declaration.");
        return operations;
    }

    public List<Operation> ParseFunctionCall(TokenStream tokens)
    {
        string funcName = tokens.Consume();
        tokens.Expect("(", "Expected '(' after function name.");
        var args = _expressionParser.Parse(tokens, new[] { ")" });
        tokens.Expect(")", "Expected ')' after arguments.");

        if (tokens.Match(";")) tokens.Advance();

        args.Add(new Operation { Command = "call", Argument = funcName });
        return args;
    }
}