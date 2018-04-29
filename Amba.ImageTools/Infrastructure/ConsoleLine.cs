using System;
using System.Threading.Tasks;

namespace Amba.ImageTools.Infrastructure
{
    public class ConsoleLine
    {
        private static readonly object SyncRoot = new object();

        public static void WriteSuccess(string text)
        {
            WriteLine(text, "OK");
        }

        public static void WriteError(string text)
        {
            WriteLine(text, "Error", ConsoleColor.Red);
        }

        public static void WriteError(string text, string errorMessage)
        {
            Task.Run(() =>
            {
                lock (SyncRoot)
                {
                    var line = new ConsoleLine()
                        .Write(text);
                     
                    line.Write(" [").Write("Error", ConsoleColor.Red).Write("] ");
                    line.Write(errorMessage, ConsoleColor.Red);
                    line.WriteLine();
                }
            });
        }

        public static void WriteLine(string text, string status, ConsoleColor statusColor = ConsoleColor.Green)
        {
            Task.Run(() =>
            {
                lock (SyncRoot)
                {
                    var line = new ConsoleLine().Write(text);

                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        line.Write(" [").Write(status, statusColor).Write("]");
                    }
                    line.WriteLine();
                }
            });
        }


        private ConsoleLine WriteLine(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            return Write(text, foregroundColor, backgroundColor);
        }
        
        private ConsoleLine Write(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            Console.ResetColor();
            return this;
        }

        private ConsoleLine WriteLine(string text = null)
        {            
            Console.WriteLine(text);
            return this;
        }
    }
}