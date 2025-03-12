namespace ILang;

public class ExternalFunction : Function
{
    public string ExternalPath { get; set; } = string.Empty;

    // Hide operations since external functions don't have them
    public new List<Operation> Operations => throw new NotSupportedException("External functions don't have operations");
    public new HashSet<string> Variables => throw new NotSupportedException("External functions don't have variables");

    public override string ToString() => $"extern {ExternalPath} {Name}";
}