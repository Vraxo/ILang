namespace ILang;

public class Function
{
    public string Name { get; set; } = "";
    public List<ValueObject> Parameters { get; set; } = [];
    public ValueObjectType ReturnType { get; set; } = ValueObjectType.Void;
    public List<Operation> Operations { get; set; } = [];
    public HashSet<string> Variables { get; set; } = new HashSet<string>(); // Track declared variables

    public override string ToString() => $"fun {Name}";
}