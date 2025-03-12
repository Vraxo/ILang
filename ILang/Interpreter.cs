using System.Diagnostics;
using System.Text.Json;

namespace ILang;

public class Interpreter
{
    private readonly ParsedProgram _program;
    private readonly Stack<object> _stack = new(); // Operand stack
    private readonly Stack<Dictionary<string, object>> _callStack = new(); // Call stack for variables

    public Interpreter(ParsedProgram program)
    {
        _program = program;
    }

    public void Execute()
    {
        // Find the 'main' function
        Function mainFunction = _program.Functions
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
                case "==":
                    ProcessEquals();
                    break;
                case "!=":
                    ProcessNotEquals();
                    break;
                case "!":
                    ProcessNot();
                    break;
                case "<":
                    ProcessLessThan();
                    break;
                case ">":
                    ProcessGreaterThan();
                    break;
                case "<=":
                    ProcessLessOrEqual();
                    break;
                case ">=":
                    ProcessGreaterOrEqual();
                    break;
                case "call":
                    ProcessCall(operation.Argument);
                    break;
                case "if":
                    ProcessIf(operation);
                    break;
                case "loop":
                    ProcessLoop(operation);
                    break;
                case "return":
                    ProcessReturn();
                    break;
                default:
                    throw new InvalidOperationException($"Error: Unknown command '{operation.Command}'.");
            }
        }
    }

    private void ProcessPush(string argument)
    {
        if (argument == "true" || argument == "false")
        {
            // Handle boolean literals
            _stack.Push(bool.Parse(argument));
        }
        else if (argument.StartsWith("\""))
        {
            // Handle string literals
            string value = argument.Substring(1, argument.Length - 2);
            _stack.Push(value);
        }
        else if (double.TryParse(argument, out double number))
        {
            // Handle numeric literals
            _stack.Push(number);
        }
        else
        {
            throw new InvalidOperationException($"Error: Invalid argument '{argument}' for 'push'.");
        }
    }

    private void StoreVariable(string varName)
    {
        if (_stack.Count == 0)
            throw new InvalidOperationException($"Error: No value to assign to variable '{varName}'.");

        // Pop the value from the stack and store it in the current call frame
        object value = _stack.Pop();
        _callStack.Peek()[varName] = value;
    }

    private void LoadVariable(string varName)
    {
        if (!_callStack.Peek().TryGetValue(varName, out object value))
        {
            throw new InvalidOperationException($"Error: Variable '{varName}' not found.");
        }

        // Push the variable's value onto the stack
        _stack.Push(value);
    }

    private void ProcessAdd()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '+' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            // Numeric addition
            _stack.Push(l + r);
        }
        else if (left is string ls && right is string rs)
        {
            // String concatenation
            _stack.Push(ls + rs);
        }
        else
        {
            throw new InvalidOperationException("Cannot add values of different types.");
        }
    }

    private void ProcessSubtract()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '-' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            _stack.Push(l - r);
        }
        else
        {
            throw new InvalidOperationException("Cannot subtract non-numeric values.");
        }
    }

    private void ProcessMultiply()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '*' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            _stack.Push(l * r);
        }
        else
        {
            throw new InvalidOperationException("Cannot multiply non-numeric values.");
        }
    }

    private void ProcessDivide()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '/' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            _stack.Push(l / r);
        }
        else
        {
            throw new InvalidOperationException("Cannot divide non-numeric values.");
        }
    }

    private void ProcessEquals()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '==' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            // Numeric equality
            _stack.Push(Math.Abs(l - r) < 1e-9);
        }
        else
        {
            // General equality
            _stack.Push(left.Equals(right));
        }
    }

    private void ProcessNotEquals()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '!=' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            // Numeric inequality
            _stack.Push(Math.Abs(l - r) >= 1e-9);
        }
        else
        {
            // General inequality
            _stack.Push(!left.Equals(right));
        }
    }

    private void ProcessNot()
    {
        if (_stack.Count == 0)
            throw new InvalidOperationException("Stack underflow during '!' operation.");

        object value = _stack.Pop();
        if (value is bool b)
        {
            _stack.Push(!b);
        }
        else
        {
            throw new InvalidOperationException("'!' operator requires a boolean operand.");
        }
    }

    private void ProcessLessThan()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '<' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            _stack.Push(l < r);
        }
        else
        {
            throw new InvalidOperationException("Cannot compare non-numeric values with '<'.");
        }
    }

    private void ProcessGreaterThan()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '>' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            _stack.Push(l > r);
        }
        else
        {
            throw new InvalidOperationException("Cannot compare non-numeric values with '>'.");
        }
    }

    private void ProcessLessOrEqual()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '<=' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            _stack.Push(l <= r);
        }
        else
        {
            throw new InvalidOperationException("Cannot compare non-numeric values with '<='.");
        }
    }

    private void ProcessGreaterOrEqual()
    {
        if (_stack.Count < 2)
            throw new InvalidOperationException("Stack underflow during '>=' operation.");

        object right = _stack.Pop();
        object left = _stack.Pop();

        if (left is double l && right is double r)
        {
            _stack.Push(l >= r);
        }
        else
        {
            throw new InvalidOperationException("Cannot compare non-numeric values with '>='.");
        }
    }

    private void ProcessCall(string functionName)
    {
        switch (functionName.ToLower())
        {
            case "print":
                if (_stack.Count == 0)
                    throw new InvalidOperationException("Error: Stack underflow during 'print' call.");
                Console.WriteLine(_stack.Pop());
                break;
            case "num_to_string":
                if (_stack.Count == 0)
                    throw new InvalidOperationException("Error: Stack underflow during 'num_to_string' call.");
                object num = _stack.Pop();
                if (num is double d)
                    _stack.Push(d.ToString());
                else
                    throw new InvalidOperationException("Error: 'num_to_string' requires a numeric argument.");
                break;
            default:
                // Check user-defined functions
                Function userFunc = _program.Functions.FirstOrDefault(f => f.Name == functionName);
                if (userFunc != null)
                {
                    // Capture arguments from the stack
                    var args = new Dictionary<string, object>();
                    for (int i = userFunc.Parameters.Count - 1; i >= 0; i--)
                    {
                        if (_stack.Count == 0)
                            throw new InvalidOperationException($"Error: Not enough arguments for '{functionName}'.");
                        args[userFunc.Parameters[i].Name] = _stack.Pop();
                    }

                    // Push a new frame with parameters
                    _callStack.Push(args);

                    // Execute the function's operations
                    ProcessOperations(userFunc.Operations);

                    // Pop the frame after execution
                    _callStack.Pop();
                }
                else
                {
                    // Check extern functions
                    ExternFunction externFunc = _program.ExternFunctions
                        .FirstOrDefault(f => f.Name == functionName);
                    if (externFunc != null)
                    {
                        ProcessExternCall(externFunc);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Error: Unknown function '{functionName}'.");
                    }
                }
                break;
        }
    }

    private void ProcessExternCall(ExternFunction externFunc)
    {
        // Collect arguments from the stack
        var args = new List<object>();
        for (int i = externFunc.Parameters.Count - 1; i >= 0; i--)
        {
            if (_stack.Count == 0)
                throw new InvalidOperationException($"Not enough arguments for '{externFunc.Name}'.");
            args.Add(_stack.Pop());
        }
        args.Reverse(); // Original order

        // Serialize arguments with escaped backslashes
        string jsonArgs = JsonSerializer.Serialize(args);
        jsonArgs = jsonArgs.Replace("\\", "\\\\"); // Escape for Regex.Unescape

        // Generate result token if needed
        string resultToken = null;
        if (externFunc.ReturnType != ValueObjectType.Void)
            resultToken = Guid.NewGuid().ToString("N");

        // Prepare process
        var startInfo = new ProcessStartInfo
        {
            FileName = externFunc.Path,
            Arguments = $"{externFunc.Name} {EscapeArg(jsonArgs)} {resultToken ?? "null"}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        bool resultReceived = false;
        object result = null;

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                if (resultToken != null && e.Data.StartsWith($"<{resultToken}>"))
                {
                    string json = e.Data.Substring(resultToken.Length + 2);
                    result = DeserializeResult(json, externFunc.ReturnType);
                    resultReceived = true;
                }
                else
                {
                    Console.WriteLine(e.Data);
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        string error = process.StandardError.ReadToEnd();

        if (!process.WaitForExit(5000))
        {
            process.Kill();
            throw new InvalidOperationException($"Extern function '{externFunc.Name}' timed out.");
        }

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"Extern call failed: {error}");

        if (externFunc.ReturnType != ValueObjectType.Void && !resultReceived)
            throw new InvalidOperationException($"No result from '{externFunc.Name}'.");

        if (result != null)
            _stack.Push(result);
    }

    private string EscapeArg(string arg) => $"\"{arg.Replace("\"", "\\\"")}\"";

    private object DeserializeResult(string json, ValueObjectType type)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return type switch
        {
            ValueObjectType.Number => root.GetDouble(),
            ValueObjectType.String => root.GetString(),
            ValueObjectType.Bool => root.GetBoolean(),
            _ => null
        };
    }

    private void ProcessIf(Operation operation)
    {
        if (_stack.Count == 0)
            throw new InvalidOperationException("No condition for 'if' statement.");

        bool condition = (bool)_stack.Pop();

        if (condition)
        {
            ProcessOperations(operation.NestedOperations);
        }
        else
        {
            ProcessOperations(operation.ElseOperations);
        }
    }

    private void ProcessLoop(Operation loop)
    {
        while (true)
        {
            // Evaluate condition
            ProcessOperations(loop.NestedOperations);
            if (_stack.Count == 0)
                throw new InvalidOperationException("No condition for loop.");

            bool condition = (bool)_stack.Pop();
            if (!condition) break;

            // Execute loop body
            ProcessOperations(loop.ElseOperations);
        }
    }

    private void ProcessReturn()
    {
        if (_stack.Count == 0)
            throw new InvalidOperationException("Nothing to return.");

        // Pop the return value from the stack
        object returnValue = _stack.Pop();

        // Propagate the return value to the caller (if needed)
        // For simplicity, we'll assume the function returns to the main context
        _stack.Push(returnValue);
    }
}