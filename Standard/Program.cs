using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Standard;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 3) return;

        try
        {
            var function = args[0];
            var argData = JsonSerializer.Deserialize<JsonElement>(
                Regex.Unescape(args[1]));
            var resultToken = args[2];

            switch (function)
            {
                case "console_print":
                    Console.WriteLine(argData.GetString());
                    break;

                case "random_int":
                    var rnd = new Random();
                    SendResult(resultToken, rnd.Next(
                        argData[0].GetInt32(),
                        argData[1].GetInt32()));
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"EXTERNAL_ERROR: {ex.Message}");
        }
    }

    private static void SendResult(string resultToken, object value)
    {
        Console.WriteLine($"\n<{resultToken}>{JsonSerializer.Serialize(value)}");
    }
}