using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DSharpLang
{
    public static class Serializer
    {
        public static string[] SerializeTokens(List<Token> toks)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < toks.Count; i++)
            {
                output.Add(SerializeToken(toks[i]));
            }
            return output.ToArray();
        }

        public static string SerializeToken(Token tok, bool nl = false)
        {
            string s = string.Empty;
            s += "LINE:"  + tok.Line.ToString();
            s += ",TYPE:" + tok.Type.ToString();
            s += ",VAL:"  + tok.Value;
            if (nl) { s += "\n"; }
            return s;
        }

        public static Token DeserializeToken(string tok)
        {
            string[] parts = tok.Split(",");
            int line = DeserializeTokenLine(parts[0]);
            TokenType type = DeserializeTokenType(parts[1]);
            string value = DeserializeTokenValue(tok);

            return new Token(TokenType.Invalid, 0, "");
        }

        private static int DeserializeTokenLine(string str)
        {
            string[] parts = str.Split(":");
            if (parts.Length != 2) { Debug.Error("Invalid split count while deserializing token type from { " + str + " }"); return 0; }
            int n = 0;
            if (!int.TryParse(parts[1], out n)) { Debug.Error("Failed to parse line while deserializing token type from { " + str + " }"); return 0; }
            return n;
        }

        private static TokenType DeserializeTokenType(string str)
        {
            string[] parts = str.Split(":");
            if (parts.Length != 2) { Debug.Error("Invalid split count while deserializing token type from { " + str + " }"); return TokenType.Invalid;}

            TokenType[] types = (TokenType[])Enum.GetValues(typeof(TokenType));
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].ToString() == parts[1])
                {
                    return types[i];
                }
            }
            Debug.Error("Failed to deserialize token type from { " + str + " }");
            return TokenType.Invalid;
        }

        private static string DeserializeTokenValue(string str)
        {
            string value = string.Empty;

            string[] mainparts = str.Split(",");
            for (int i = 2; i < mainparts.Length; i++) { value += mainparts[i]; }
            if (value.StartsWith("VAL:")) { value = value.Substring(4); }

            return value;
        }
    }
}
