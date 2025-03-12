namespace ILang;

public class Function
{
    public virtual string Name { get; set; } = "";
    public virtual List<ValueObject> Parameters { get; set; } = [];
    public virtual ValueObjectType ReturnType { get; set; } = ValueObjectType.Void;
    public virtual List<Operation> Operations { get; set; } = [];
    public virtual HashSet<string> Variables { get; set; } = new();

    public override string ToString() => $"fun {Name}";
}