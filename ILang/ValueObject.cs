namespace ILang;

public enum ValueObjectType
{
    String,
    Number,
    Bool,
    Void
}

public class ValueObject
{
    public string Name { get; set; } = "";
    public ValueObjectType Type { get; set; } = ValueObjectType.String;
}