using System;
using System.Collections.Generic;

namespace ILang;

public class Parser
{
    private readonly FunctionParser _functionParser;

    public Parser()
    {
        var parameterParser = new ParameterParser();
        var expressionParser = new ExpressionParser();
        var statementParser = new StatementParser(expressionParser);
        _functionParser = new FunctionParser(parameterParser, statementParser);
    }

    public ParsedProgram Parse(List<string> tokens)
    {
        var tokenStream = new TokenStream(tokens);
        var parsedProgram = new ParsedProgram();

        while (!tokenStream.IsEof)
        {
            if (tokenStream.Match("fun"))
            {
                tokenStream.Consume();
                parsedProgram.Functions.Add(_functionParser.Parse(tokenStream));
            }
            else
            {
                tokenStream.Advance();
            }
        }

        return parsedProgram;
    }
}