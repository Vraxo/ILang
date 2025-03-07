using System;
using System.Collections.Generic;

namespace ILang;

public class ParameterParser
{
    public List<ValueObject> Parse(TokenStream tokens)
    {
        var parameters = new List<ValueObject>();

        // Expect '(' before parameters
        tokens.Expect("(", "Expected '(' after function name.");

        while (!tokens.Match(")"))
        {
            // Parameter name
            string paramName = tokens.Consume();

            // Colon after name
            tokens.Expect(":", "Expected ':' after parameter name.");

            // Parameter type
            ValueObjectType paramType = ParserUtils.ParseType(tokens.Consume());
            parameters.Add(new ValueObject { Name = paramName, Type = paramType });

            // Check for comma separator
            if (tokens.Match(",")) tokens.Consume();
        }

        tokens.Consume(); // Skip ')'
        return parameters;
    }
}