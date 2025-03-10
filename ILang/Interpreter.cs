﻿namespace ILang;

public class Interpreter
{
    private readonly ParsedProgram _program;
    private readonly Stack<object> _stack = new();
    private readonly Stack<Dictionary<string, object>> _callStack = new();

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
                case "call":
                    ProcessCall(operation.Argument);
                    break;
                case "if":
                    ProcessIf(operation);
                    break;
                case "loop":
                    ProcessLoop(operation);
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

    private void ProcessCall(string functionName)
    {
        switch (functionName.ToLower())
        {
            case "print":
                if (_stack.Count == 0)
                    throw new InvalidOperationException("Error: Stack underflow during 'print' call.");
                Console.WriteLine(_stack.Pop());
                break;
            default:
                // Handle user-defined functions
                Function userFunction = _program.Functions
                    .FirstOrDefault(f => f.Name == functionName)
                    ?? throw new InvalidOperationException($"Error: Unknown function '{functionName}'.");

                // Capture arguments from the stack
                var args = new Dictionary<string, object>();
                for (int i = userFunction.Parameters.Count - 1; i >= 0; i--)
                {
                    if (_stack.Count == 0)
                        throw new InvalidOperationException($"Error: Not enough arguments for '{functionName}'.");
                    args[userFunction.Parameters[i].Name] = _stack.Pop();
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
}