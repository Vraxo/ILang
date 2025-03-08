namespace ILang;

public class Operation
{
    public string Command { get; set; } = "";
    public string Argument { get; set; } = "";
    public List<Operation> NestedOperations { get; set; } = new();
    public List<Operation> ElseOperations { get; set; } = new();

    public override string ToString() => $"{Command} {Argument}".Trim();
}