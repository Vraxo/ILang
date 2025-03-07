using System;
using System.Collections.Generic;

namespace ILang;

public class TokenStream
{
    private readonly List<string> _tokens;
    private int _currentIndex;

    public TokenStream(List<string> tokens)
    {
        _tokens = tokens;
        _currentIndex = 0;
    }

    public bool IsEof => _currentIndex >= _tokens.Count;
    public string Current => _tokens[_currentIndex];

    public void Advance(int steps = 1) => _currentIndex += steps;

    public bool Match(string expectedToken) =>
        !IsEof && _tokens[_currentIndex] == expectedToken;

    public void Expect(string expectedToken, string errorMessage)
    {
        if (!Match(expectedToken))
            throw new InvalidOperationException($"Error: {errorMessage}");
        Advance();
    }

    public string Consume()
    {
        string current = Current;
        Advance();
        return current;
    }

    public string Peek(int offset = 0)
    {
        int index = _currentIndex + offset;
        return index < _tokens.Count ? _tokens[index] : null;
    }
}