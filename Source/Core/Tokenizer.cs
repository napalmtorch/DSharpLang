using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DSharpLang
{
    public class Tokenizer
    {
        // input characters in a form of character stream
        public Stream<char> Input;

        // output list of tokens generated from input stream
        public List<Token> Tokens;

        // temporary word value
        private string _word;

        // current line
        private int _line;

        // current filename
        private string _fname;

        // load source code file from file
        public Tokenizer(string filename)
        {
            // validate that file exists
            if (!File.Exists(filename)) { Debug.Error("Tokenizer failed to locate file '" + filename + "'"); return; }

            // read data into string and remove specified characters
            string data = File.ReadAllText(filename);
            data = data.Replace("\r", "");
            data = data.Replace("\t", " ");

            // setup properties
            _fname = filename;
            Input  = new Stream<char>(data.ToCharArray(), data.Length);
            Tokens = new List<Token>();
            Debug.OK("Tokenizer loaded file at '" + filename + "'");
        }

        public void Print()
        {
            string[] toks = Serializer.SerializeTokens(Tokens);
            for (int i = 0; i < toks.Length; i++)
            {
                Debug.Info("Token - { " + toks[i] + " }");
            }
        }

        public void Save(string fname)
        {
            string[] toks = Serializer.SerializeTokens(Tokens);
            File.WriteAllLines(fname, toks);
            Debug.OK("Saved serialized tokens to '" + fname + "'");
        }

        // Convert input characters into list of tokens
        public void Perform(string save = "")
        {
            _line = 1;
            _word = string.Empty;
            while (!Input.Done())
            {
                // get next character in stream
                char c = Input.Next();

                // explicitly handle symbols
                if (Grammar.IsSymbol(c.ToString())) { HandleWord(); AddToken(TokenType.Symbol, _line, c.ToString()); continue; }

                // handle other symbols
                switch (c)
                {
                    default:   { _word += c; break; }
                    case '\r': { break; }
                    case '\t': { break; }
                    case '\n': { HandleWord(); _line++; break; }
                    case ' ':  { HandleWord(); break; }
                    case '\'': { HandleQuotes(false); break; }
                    case '\"': { HandleQuotes(true); break; }
                }
            }

            // hack to fix equals token types - should be fixed at some point*
            for (int i = 0; i < Tokens.Count; i++) { if (Tokens[i].IsID("=")) { Tokens[i].Type = TokenType.Symbol; } }

            // finished
            Debug.OK("Finished tokenizing '" + _fname + "' - " + Tokens.Count + " tokens created");
            if (save.Length > 0) { Save(save); }
        }

        // handle current word
        private void HandleWord()
        {
            // ignore if empty
            if (_word.Length == 0) { return; }
            string value = _word;

            // determine word type and create token accordingly
            TokenType type = TokenType.ID;
            if (Grammar.IsKeyword(_word)) { type = TokenType.Keyword; value = _word; }
            else if (Grammar.IsComparator(_word)) { type = TokenType.Comparator; value = _word; }
            else if (Grammar.IsOperator(_word)) { type = TokenType.Operator; value = _word; }
            else if (Grammar.IsType(_word)) { type = TokenType.Type; value = _word; }
            else if (Grammar.IsDecimal(_word)) { type = TokenType.LiteralDecimal; value = _word; }
            else if (Grammar.IsHex(_word)) { type = TokenType.LiteralHex; value = "0x" + _word.Substring(2).ToUpper(); }
            else if (Grammar.IsBool(_word)) { type = TokenType.LiteralBool; value = _word; }
            else if (Grammar.IsFloat(_word)) { type = TokenType.LiteralFloat; value = _word; }
            else if (Grammar.IsDouble(_word)) { type = TokenType.LiteralDouble; value = _word; }

            // add token to list and reset current word
            AddToken(type, _line, value);
            _word = string.Empty;
        }

        // handle string/char quotation literals
        private void HandleQuotes(bool str)
        {
            // handle current word
            HandleWord();

            // create quoted string and append until string is terminated
            string value = string.Empty;
            while (true)
            {
                char c = Input.Next();
                if (c == 0) { Debug.Error("Expected '" + (str ? "\"" : "\'") + "' for " + (str ? "string" : "char") + " literal at line " + (_line + 1)); return; }
                if (!str && c == '\'') { break; }
                if (str && c == '\"')  { break; }
                value += c;
            }
            // add literal token
            AddToken((str ? TokenType.LiteralString : TokenType.LiteralChar), _line, value);
        }

        // add token to list
        private Token AddToken(TokenType type, int line, string value, bool msg = false)
        {
            if (type == TokenType.Invalid) { Debug.Error("Attempt to create invalid token at line " + (line + 1)); }
            Token tok = new Token(type, line, value);
            Tokens.Add(tok);
            if (msg) { Debug.Info("Created token - " + tok.ToString()); }
            return Tokens[Tokens.Count - 1];
        }
    }
}
