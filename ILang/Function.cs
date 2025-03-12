namespace ILang;

public class Function
{
    public string Name { get; set; } = "";
    public List<ValueObject> Parameters { get; set; } = [];
    public ValueObjectType ReturnType { get; set; } = ValueObjectType.Void;
    public List<Operation> Operations { get; set; } = [];
    public HashSet<string> Variables { get; set; } = new HashSet<string>();

    // New properties for external DLL functions
    public bool IsExternal { get; set; } = false;
    public string? DllPath { get; set; }

    public override string ToString() => $"fun {Name}";
}