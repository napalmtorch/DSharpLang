using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpLang
{
    public static class Debug
    {
        public static bool Enabled = false;

        public static void Init()
        {
            Console.Clear();
            Enabled = true;
        }

        public static void Header(string txt, ConsoleColor color)
        {
            if (!Enabled) { return; }
            Console.Write('[');
            ConsoleColor old_color = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(txt);
            Console.ForegroundColor = old_color;
            Console.Write("] ");
        }

        public static void OK(string txt)
        {
            if (!Enabled) { return; }
            Header("  OK  ", ConsoleColor.Green);
            Console.WriteLine(txt);
        }

        public static void Info(string txt)
        {
            if (!Enabled) { return; }
            Header("  >>  ", ConsoleColor.Cyan);
            Console.WriteLine(txt);
        }

        public static void Warning(string txt)
        {
            if (!Enabled) { return; }
            Header("  ??  ", ConsoleColor.Yellow);
            Console.WriteLine(txt);
        }

        public static void Error(string txt)
        {
            Header("  !!  ", ConsoleColor.Red);
            Console.WriteLine(txt);
            Console.Read();
            Environment.Exit(0);
        }
    }
}
