namespace ILang;

public enum ValueObjectType
{
    String,
    Number,
    Void
}

public class ValueObject
{
    public string Name { get; set; } = "";
    public ValueObjectType Type { get; set; } = ValueObjectType.String;
}