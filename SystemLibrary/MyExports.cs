using System;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace SystemLibrary
{
    public static class UnmanagedExports
    {
        // Explicitly specify the export name and calling convention
        [DllExport("console_print", CallingConvention = CallingConvention.StdCall)]
        public static void ConsolePrint([MarshalAs(UnmanagedType.LPStr)] string message)
        {
            Console.WriteLine($"[DLL] {message}");
        }
    }
}