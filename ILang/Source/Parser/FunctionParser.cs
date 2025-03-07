using System;
using System.Collections.Generic;

namespace ILang;

public class FunctionParser
{
    private readonly ParameterParser _parameterParser;
    private readonly StatementParser _statementParser;

    public FunctionParser(ParameterParser parameterParser, StatementParser statementParser)
    {
        _parameterParser = parameterParser;
        _statementParser = statementParser;
    }

    public Function Parse(TokenStream tokens)
    {
        var function = new Function();

        // Parse function name
        function.Name = tokens.Consume();

        // Parse parameters (including '(' and ')')
        function.Parameters = _parameterParser.Parse(tokens);

        // Parse return type
        tokens.Expect("->", "Expected '->' after parameters.");
        function.ReturnType = ParserUtils.ParseType(tokens.Consume());

        // Parse '{' for function body
        tokens.Expect("{", "Expected '{' after return type.");

        // Parse function body
        function.Operations = _statementParser.ParseBlock(tokens);

        // Parse '}' to end function body
        tokens.Expect("}", "Expected '}' after function body.");

        return function;
    }
}