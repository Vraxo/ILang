namespace ILang;

public class Interpreter
{
    private readonly ParsedProgram _program;
    private readonly Stack<object> _stack = new();
    private readonly Stack<Dictionary<string, object>> _frames = new();

    public Interpreter(ParsedProgram program) => _program = program;

    public void Execute()
    {
        var main = _program.Functions.Find(f => f.Name == "main")
            ?? throw new Exception("No main function");
        _frames.Push(new Dictionary<string, object>());
        ProcessOperations(main.Operations);
    }

    private void ProcessOperations(List<Operation> ops)
    {
        foreach (var op in ops)
        {
            switch (op.Command.ToLower())
            {
                case "push":
                    PushValue(op.Argument);
                    break;
                case "store_var":
                    StoreVariable(op.Argument);
                    break;
                case "load_var":
                    LoadVariable(op.Argument);
                    break;
                case "+":
                case "-":
                case "*":
                case "/":
                    ApplyOperator(op.Command);
                    break;
                case "call":
                    CallFunction(op.Argument);
                    break;
                case "if":
                    ProcessIf(op);
                    break;
                default:
                    throw new Exception($"Unknown command: {op.Command}");
            }
        }
    }

    private void ProcessIf(Operation op)
    {
        var condition = (bool)_stack.Pop();
        ProcessOperations(condition ? op.NestedOperations : op.ElseOperations);
    }

    private void PushValue(string arg)
    {
        if (arg == "true") _stack.Push(true);
        else if (arg == "false") _stack.Push(false);
        else if (arg.StartsWith("\""))
            _stack.Push(arg[1..^1]);
        else if (double.TryParse(arg, out var num))
            _stack.Push(num);
        else
            throw new Exception($"Invalid push value: {arg}");
    }

    private void StoreVariable(string name) =>
        _frames.Peek()[name] = _stack.Pop();

    private void LoadVariable(string name) =>
        _stack.Push(_frames.Peek()[name]);

    private void ApplyOperator(string op)
    {
        dynamic right = _stack.Pop();
        dynamic left = _stack.Pop();
        _stack.Push(op switch
        {
            "+" => left + right,
            "-" => left - right,
            "*" => left * right,
            "/" => left / right,
            _ => throw new NotImplementedException()
        });
    }

    private void CallFunction(string name)
    {
        switch (name.ToLower())
        {
            case "print":
                Console.WriteLine(_stack.Pop());
                break;
            default:
                var func = _program.Functions.Find(f => f.Name == name)
                    ?? throw new Exception($"Undefined function: {name}");
                var frame = new Dictionary<string, object>();
                foreach (var param in func.Parameters)
                    frame[param.Name] = _stack.Pop();
                _frames.Push(frame);
                ProcessOperations(func.Operations);
                _frames.Pop();
                break;
        }
    }
}