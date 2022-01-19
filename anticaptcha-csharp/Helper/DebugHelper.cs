using System;

namespace Anticaptcha_example.Helper
{
    public class DebugHelper
    {
        public enum Type
        {
            Error,
            Info,
            Success
        }

        public static bool VerboseMode { set; private get; }

        public static void JsonFieldParseError(string field, dynamic submitResult)
        {
            string error = field + " could not be parsed. Raw response: " + JsonHelper.AsString(submitResult);
            Out(error, Type.Error);
        }

        public static void Out(string message, Type? type = null)
        {
            if (!VerboseMode)
            {
                return;
            }

            if (type.Equals(Type.Error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (type.Equals(Type.Info))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (type.Equals(Type.Success))
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}