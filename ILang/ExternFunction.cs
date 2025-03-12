namespace ILang;

public class ExternFunction
{
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public List<ValueObject> Parameters { get; set; } = [];
    public ValueObjectType ReturnType { get; set; } = ValueObjectType.Void;
}