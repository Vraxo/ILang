namespace ILang;

public class Interpreter
{
    private ParsedProgram _parsedProgram;
    private Stack<object> _operandStack = new Stack<object>();
    private Stack<Dictionary<string, object>> _callStack = new Stack<Dictionary<string, object>>();

    public Interpreter(ParsedProgram parsedProgram)
    {
        _parsedProgram = parsedProgram;
    }

    public void Execute()
    {
        // Find the 'main' function
        Function mainFunction = _parsedProgram.Functions
            .FirstOrDefault(f => f.Name == "main")
            ?? throw new InvalidOperationException("Error: No 'main' function found.");

        // Initialize the call stack with an empty frame
        _callStack.Push(new Dictionary<string, object>());

        // Execute the main function's operations
        ProcessOperations(mainFunction.Operations);
    }

    private void ProcessOperations(List<Operation> operations)
    {
        foreach (var operation in operations)
        {
            switch (operation.Command.ToLower())
            {
                case "push":
                    ProcessPush(operation.Argument);
                    break;
                case "store_var":
                    StoreVariable(operation.Argument);
                    break;
                case "load_var":
                    LoadVariable(operation.Argument);
                    break;
                case "+":
                    ProcessAdd();
                    break;
                case "-":
                    ProcessSubtract();
                    break;
                case "*":
                    ProcessMultiply();
                    break;
                case "/":
                    ProcessDivide();
                    break;
                case "call":
                    ProcessCall(operation.Argument);
                    break;
                default:
                    throw new InvalidOperationException($"Error: Unknown command '{operation.Command}'.");
            }
        }
    }

    private void ProcessPush(string argument)
    {
        if (argument.StartsWith("\"") && argument.EndsWith("\""))
        {
            // Handle string literals
            string value = argument.Substring(1, argument.Length - 2);
            _operandStack.Push(value);
        }
        else if (double.TryParse(argument, out double number))
        {
            // Handle numbers
            _operandStack.Push(number);
        }
        else
        {
            throw new InvalidOperationException($"Error: Invalid argument '{argument}' for 'push'.");
        }
    }

    private void StoreVariable(string varName)
    {
        if (_operandStack.Count == 0)
            throw new InvalidOperationException($"Error: No value to assign to variable '{varName}'.");

        // Pop the value from the stack and store it in the current call frame
        object value = _operandStack.Pop();
        _callStack.Peek()[varName] = value;
    }

    private void LoadVariable(string varName)
    {
        if (!_callStack.Peek().TryGetValue(varName, out object value))
        {
            throw new InvalidOperationException($"Error: Variable '{varName}' not found.");
        }

        // Push the variable's value onto the stack
        _operandStack.Push(value);
    }

    private void ProcessAdd()
    {
        if (_operandStack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '+' operation.");

        object right = _operandStack.Pop();
        object left = _operandStack.Pop();

        if (left is double l && right is double r)
        {
            // Numeric addition
            _operandStack.Push(l + r);
        }
        else if (left is string ls && right is string rs)
        {
            // String concatenation
            _operandStack.Push(ls + rs);
        }
        else
        {
            throw new InvalidOperationException("Cannot add values of different types.");
        }
    }

    private void ProcessSubtract()
    {
        if (_operandStack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '-' operation.");

        object right = _operandStack.Pop();
        object left = _operandStack.Pop();

        if (left is double l && right is double r)
        {
            _operandStack.Push(l - r);
        }
        else
        {
            throw new InvalidOperationException("Cannot subtract non-numeric values.");
        }
    }

    private void ProcessMultiply()
    {
        if (_operandStack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '*' operation.");

        object right = _operandStack.Pop();
        object left = _operandStack.Pop();

        if (left is double l && right is double r)
        {
            _operandStack.Push(l * r);
        }
        else
        {
            throw new InvalidOperationException("Cannot multiply non-numeric values.");
        }
    }

    private void ProcessDivide()
    {
        if (_operandStack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '/' operation.");

        object right = _operandStack.Pop();
        object left = _operandStack.Pop();

        if (left is double l && right is double r)
        {
            _operandStack.Push(l / r);
        }
        else
        {
            throw new InvalidOperationException("Cannot divide non-numeric values.");
        }
    }

    private void ProcessCall(string functionName)
    {
        switch (functionName.ToLower())
        {
            case "print":
                if (_operandStack.Count == 0)
                    throw new InvalidOperationException("Error: Stack underflow during 'print' call.");
                Console.WriteLine(_operandStack.Pop());
                break;
            case "num_to_string":
                if (_operandStack.Count == 0)
                    throw new InvalidOperationException("Error: Stack underflow during 'num_to_string' call.");
                object num = _operandStack.Pop();
                if (num is double d)
                    _operandStack.Push(d.ToString());
                else
                    throw new InvalidOperationException("num_to_string requires a numeric value.");
                break;
            default:
                // Handle user-defined functions
                Function userFunction = _parsedProgram.Functions
                    .FirstOrDefault(f => f.Name == functionName)
                    ?? throw new InvalidOperationException($"Error: Unknown function '{functionName}'.");

                // Capture arguments from the stack
                var args = new Dictionary<string, object>();
                for (int i = userFunction.Parameters.Count - 1; i >= 0; i--)
                {
                    if (_operandStack.Count == 0)
                        throw new InvalidOperationException($"Error: Not enough arguments for '{functionName}'.");
                    args[userFunction.Parameters[i].Name] = _operandStack.Pop();
                }

                // Push a new frame with parameters
                _callStack.Push(args);

                // Execute the function's operations
                ProcessOperations(userFunction.Operations);

                // Pop the frame after execution
                _callStack.Pop();
                break;
        }
    }
}