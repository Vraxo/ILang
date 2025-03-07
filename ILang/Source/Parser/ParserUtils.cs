using System;

namespace ILang;

public static class ParserUtils
{
    public static bool IsIdentifier(string token) =>
        !string.IsNullOrEmpty(token) && (char.IsLetter(token[0]) || token[0] == '_');

    public static bool IsTypeToken(string token) =>
        token is "string" or "num" or "void";

    public static bool IsNumber(string token) =>
        double.TryParse(token, out _);

    public static ValueObjectType ParseType(string typeToken) =>
        typeToken switch
        {
            "string" => ValueObjectType.String,
            "num" => ValueObjectType.Number,
            "void" => ValueObjectType.Void,
            _ => throw new ArgumentException($"Invalid type: {typeToken}")
        };

    public static int GetPrecedence(string op) =>
        op switch
        {
            "+" or "-" => 1,
            "*" or "/" => 2,
            _ => 0
        };
}