using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DSharpLang
{
    public static class Grammar
    {
        public static string[] Keywords = new string[]
        {
            "import", "class", "struct", "enum", "const", "static", "region", "fn", "var",
            "break", "continue", "return", "this", "typeof",
            "if", "elseif", "else", "for", "foreach", "while", "null",
        };

        public static string[] Types = new string[]
        {
            "byte", "char", "short", "ushort", "int", "uint", "long", "ulong",
            "float", "double", "bool", "void",
        };

        public static string[] OutputTypes = new string[]
        {
            "uint8_t", "int8_t", "int16_t", "uint16_t", "int32_t", "uint32_t", "int64_t", "uint64_t",
            "float", "double", "bool", "void",
        };

        public static string[] Comparators = new string[]
        {
            "==", "!=", ">=", "<=", ">", "<",
        };

        public static string[] Operators = new string[]
        {
            "+=", "-=", "*=", "/=", "|=", "^=", "&=", "<<=", ">>=",
            "+", "-", "*", "/", "|", "^", "&", "<<", ">>", "!", "%",
        };

        public static string[] Symbols = new string[]
        {
            "(", ")", "[", "]", "{", "}", ":", ";", ",", "@",
        };

        public static bool IsKeyword(string str)
        {
            if (str == null) { return false; }
            for (int i = 0; i < Keywords.Length; i++) { if (Keywords[i] == str) { return true; } }
            return false;
        }

        public static bool IsType(string str)
        {
            if (str == null) { return false; }
            for (int i = 0; i < Types.Length; i++) { if (Types[i] == str) { return true; } }
            return false;
        }

        public static bool IsComparator(string str)
        {
            if (str == null) { return false; }
            for (int i = 0; i < Comparators.Length; i++) { if (Comparators[i] == str) { return true; } }
            return false;
        }

        public static bool IsOperator(string str)
        {
            if (str == null) { return false; }
            for (int i = 0; i < Operators.Length; i++) { if (Operators[i] == str) { return true; } }
            return false;
        }

        public static bool IsSymbol(string str)
        {
            if (str == null) { return false; }
            for (int i = 0; i < Symbols.Length; i++) { if (Symbols[i] == str) { return true; } }
            if (str == "=") { return true; }
            return false;
        }

        public static bool IsDecimal(string str)
        {
            if (str == null) { return false; }
            int v = 0;
            if (!int.TryParse(str, out v)) { return false; }
            return true;
        }

        public static bool IsFloat(string str)
        {
            if (str == null) { return false; }
            if (!str.ToUpper().EndsWith("F")) { return false; }
            str = str.Remove(str.Length - 1, 1);
            float v = 0.0f;
            if (!float.TryParse(str, out v)) { return false; }
            return true;
        }

        public static bool IsDouble(string str)
        {
            if (str == null) { return false; }
            if (str.EndsWith("d")) { str = str.Remove(str.Length - 1, 1); }
            double v = 0.0d;
            if (!double.TryParse(str, out v)) { return false; }
            return true;
        }

        public static bool IsHexChar(char c)
        {
            if (char.IsDigit(c)) { return true; }
            if ((c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')) { return true; }
            return false;
        }

        public static bool IsHex(string str)
        {
            if (str == null) { return false; }
            if (!str.ToUpper().StartsWith("0X")) { return false; }
            if (str.Length < 3) { return false; }
            for (int i = 2; i < str.Length; i++)
            {
                if (!IsHexChar(str[i])) { return false; }
            }
            return true;
        }

        public static bool IsBool(string str)
        {
            if (str == null) { return false; }
            if (str == "true") { return true; }
            if (str == "false") { return true; }
            return false;
        }

        public static int GetTypeIndex(string str)
        {
            for (int i = 0; i < Types.Length; i++) { if (Types[i] == str) { return i; } }
            return -1;
        }

        public static string ConvertDefaultType(string str)
        {
            int index = GetTypeIndex(str);
            if (index < 0 || index >= Types.Length) { return str; }
            return OutputTypes[index];
        }
    }
}
