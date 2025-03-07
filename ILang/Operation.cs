namespace ILang;

public class Operation
{
    public string Command { get; set; } = "";
    public string Argument { get; set; } = "";

    public override string ToString()
    {
        return $"{Command} {Argument}".Trim();
    }
}