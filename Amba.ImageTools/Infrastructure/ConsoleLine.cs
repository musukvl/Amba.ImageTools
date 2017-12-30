using System;

namespace Amba.ImageTools.Infrastructure
{
    public class ConsoleLine
    {         
        public ConsoleLine WriteLine(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            return Write(text, foregroundColor, backgroundColor);
        }
        
        public ConsoleLine Write(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            Console.ResetColor();
            return this;
        }

        public ConsoleLine WriteLine(string text = null)
        {            
            Console.WriteLine(text);
            return this;
        }
    }
}