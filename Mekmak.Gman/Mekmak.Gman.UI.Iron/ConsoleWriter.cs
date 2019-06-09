using System;

namespace Mekmak.Gman.UI.Iron
{
    public static class ConsoleWriter
    {
        public static void WriteInGreen(string message)
        {
            WriteInColor(ConsoleColor.Green, message);
        }

        public static void WriteInRed(string message)
        {
            WriteInColor(ConsoleColor.Red, message);
        }

        public static void WriteInYellow(string message)
        {
            WriteInColor(ConsoleColor.Yellow, message);
        }

        public static void WriteInColor(ConsoleColor color, string message)
        {
            using (var ccw = new ConsoleColorWriter(color))
            {
                ccw.Write(message);
            }
        }
    }

    public class ConsoleColorWriter : IDisposable
    {
        private readonly ConsoleColor _originalColor;

        public ConsoleColorWriter(ConsoleColor color)
        {
            _originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        public void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void Dispose()
        {
            Console.ForegroundColor = _originalColor;
        }
    }
}
