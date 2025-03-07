using System;
using System.Collections.Generic;

namespace ILang;

public class ExpressionParser
{
    public List<Operation> Parse(TokenStream tokens, string[] terminators)
    {
        var output = new List<Operation>();
        var operatorStack = new Stack<string>();

        while (!tokens.IsEof && !terminators.Contains(tokens.Current))
        {
            string token = tokens.Current;

            if (ParserUtils.IsNumber(token))
            {
                output.Add(new Operation { Command = "push", Argument = token });
                tokens.Advance();
            }
            else if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                output.Add(new Operation { Command = "push", Argument = token });
                tokens.Advance();
            }
            else if (ParserUtils.IsIdentifier(token))
            {
                if (tokens.Peek() == "(")
                {
                    output.AddRange(ParseFunctionCall(tokens));
                }
                else
                {
                    output.Add(new Operation { Command = "load_var", Argument = token });
                    tokens.Advance();
                }
            }
            else if (token is "+" or "-" or "*" or "/")
            {
                while (operatorStack.Count > 0 && ParserUtils.GetPrecedence(operatorStack.Peek()) >= ParserUtils.GetPrecedence(token))
                {
                    output.Add(new Operation { Command = operatorStack.Pop() });
                }
                operatorStack.Push(token);
                tokens.Advance();
            }
            else if (token == "(")
            {
                operatorStack.Push(token);
                tokens.Advance();
            }
            else if (token == ")")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                {
                    output.Add(new Operation { Command = operatorStack.Pop() });
                }
                operatorStack.Pop(); // Remove '('
                tokens.Advance();
            }
            else
            {
                throw new InvalidOperationException($"Unexpected token: {token}");
            }
        }

        while (operatorStack.Count > 0)
        {
            output.Add(new Operation { Command = operatorStack.Pop() });
        }

        return output;
    }

    private List<Operation> ParseFunctionCall(TokenStream tokens)
    {
        var operations = new List<Operation>();
        string funcName = tokens.Consume();
        tokens.Expect("(", "Expected '(' after function name.");
        operations.AddRange(Parse(tokens, new[] { ")" }));
        tokens.Expect(")", "Expected ')' after arguments.");
        operations.Add(new Operation { Command = "call", Argument = funcName });
        return operations;
    }
}