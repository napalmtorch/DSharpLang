using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpLang
{
    public enum TokenType : byte
    {
        Invalid,
        ID,
        Keyword,
        Type,
        Operator,
        Comparator,
        Symbol,

        LiteralDecimal,
        LiteralHex,
        LiteralBinary,
        LiteralFloat,
        LiteralDouble,
        LiteralBool,
        LiteralChar,
        LiteralString,
    }

    public class Token
    {
        public TokenType Type;
        public int       Line;
        public string    Value;

        public Token() { this.Type = TokenType.Invalid; this.Line = -1; this.Value = string.Empty; }

        public Token(TokenType type, int line, string value) { this.Type = type; this.Line = line; this.Value = value; }

        public override string ToString() { return "Line:" + Line.ToString().PadRight(6, ' ') + " Type:" + Type.ToString().PadRight(16, ' ') + " Value:{ " + Value + " }"; }
        
        public bool IsKeyword(string str)
        {
            if (Value != str) { return false; }
            return Grammar.IsKeyword(Value);
        }

        public bool IsDefaultType(string str)
        {
            if (Value != str) { return false; }
            return Grammar.IsType(str);
        }

        public bool IsOperator(string str)
        {
            if (Value != str) { return false; }
            return Grammar.IsOperator(str);
        }

        public bool IsComparator(string str)
        {
            if (Value != str) { return false; }
            return Grammar.IsComparator(str);
        }

        public bool IsSymbol(string str)
        {
            if (Value != str) { return false; }
            return Grammar.IsSymbol(str);
        }

        public bool IsID(string str)
        {
            if (Value != str) { return false; }
            return (Type == TokenType.ID);
        }
    }
}
