using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpLang
{
    public class ASTGenerator
    {
        public Stream<Token> Tokens;
        public ASTBody       PrimaryNode;

        public ASTGenerator(List<Token> tokens)
        {
            this.Tokens      = new Stream<Token>(tokens);
            this.PrimaryNode = new ASTBody(AST.Primary);
        }

        public void Perform(ASTBody parent)
        {            
            while (!Tokens.Done())
            {
                Token tok = Tokens.Next();
                if (tok.Type == TokenType.Invalid) { break; }
                ASTNode node = Handle(tok);
                if (node != null) { AddNode(parent, node); }
            }
            Debug.OK("Finished generating AST nodes");
        }

        public ASTNode Handle(Token tok)
        {
            if (tok.IsKeyword("import")) { return HandleImport(); }
            if (tok.IsKeyword("region")) { return HandleRegion(); }
            if (tok.IsKeyword("class"))  { return HandleClassDecl(); }
            if (tok.IsKeyword("var"))    { return HandleVariableDecl(false); }
            if (tok.IsKeyword("const"))  { return HandleVariableDecl(true); }
            if (tok.IsKeyword("fn"))     { return HandleFunctionDecl(); }
            if (tok.IsKeyword("while"))  { return HandleWhile(); }
            if (tok.IsKeyword("return")) { return HandleReturn(); }
            if (tok.IsKeyword("if"))     { return HandleIf(); }
            if (tok.IsKeyword("elseif")) { return HandleElseIf(); }
            if (tok.IsKeyword("else"))   { return HandleElse(); }
            if (tok.Type == TokenType.ID)
            {
                Token next = Tokens.Next();
                Tokens.Back();
                if (next.IsSymbol("=")) { Tokens.Back(); return HandleVariableSet(); }
                if (next.IsSymbol("[")) { Tokens.Back(); return HandleVariableSet(); }
                if (next.IsSymbol("(")) { Tokens.Back(); return HandleCall(); }
                if (next.Type == TokenType.Operator) { Tokens.Back(); return HandleVariableSet(); }
            }
            return null;
        }

        private List<ASTNode> HandleScope(ASTBody parent)
        {
            int  scope  = 0;
            bool closed = false;
            List<ASTNode> nodes = new List<ASTNode>();
            while (!Tokens.Done())
            {
                Token tok = Tokens.Next();
                if (tok.Type == TokenType.Invalid) { closed = false; break; }
                if (tok.IsSymbol("{")) { scope++; continue; }
                if (tok.IsSymbol("}")) { scope--; }
                if (scope == 0) { closed = true; break; }
                ASTNode node = Handle(tok);
                if (node != null) { nodes.Add(node); Debug.Info("Created node  - " + node.ToString()); }
            }
            if (!closed) { Debug.Error("Expected '}' at line " + Tokens.Peek().Line); return null; }
            return nodes;
        }

        private ASTImport HandleImport()
        {
            Token tok_id = Tokens.Next();
            if (tok_id.Type != TokenType.LiteralString) { Debug.Error("Expected filename for import statement at line " + tok_id.Line); }

            ASTImport node = new ASTImport(tok_id.Value);
            return node;
        }

        private ASTRegion HandleRegion()
        {
            Token tok_id = Tokens.Next();
            if (tok_id.Type != TokenType.ID) { Debug.Error("Expected identifier for region at line " + tok_id.Line); }

            ASTRegion node = new ASTRegion(tok_id.Value);
            node.Nodes = HandleScope(node);
            return node;
        }

        private ASTClassDecl HandleClassDecl()
        {
            Token tok_static = Tokens.Peek(Tokens.Position - 2);

            Token tok_id = Tokens.Next();
            if (tok_id.Type != TokenType.ID) { Debug.Error("Expected identifier for region at line " + tok_id.Line); }

            ASTClassDecl node = new ASTClassDecl(tok_id.Value, tok_static.IsKeyword("static"));
            node.Nodes = HandleScope(node);
            return node;
        }

        private ASTVariableDecl HandleVariableDecl(bool cnst = false)
        {
            Token tok_id    = Tokens.Next();
            Token tok_colon = Tokens.Next();

            int ptr = 0;
            Token tok_next = Tokens.Next();
            while (tok_next.IsSymbol("@")) { ptr++; tok_next = Tokens.Next(); }

            Token tok_type = tok_next;

            if (tok_id.Type != TokenType.ID)                                      { Debug.Error("Expected identifier for variable declaration at line " + tok_id.Line); }
            if (!tok_colon.IsSymbol(":")) { Debug.Error("Expected ':' at line " + tok_id.Line); }
            if (tok_type.Type != TokenType.Type && tok_type.Type != TokenType.ID) { Debug.Error("Expected type for variable declaration at line " + tok_id.Line); }

            ASTVariableDecl node = new ASTVariableDecl(tok_id.Value, tok_type.Value, ptr, cnst);

            tok_next = Tokens.Next();
            if (tok_next.IsSymbol("["))
            {
                int scope = 1;
                bool closed = false;
                while (true)
                {
                    tok_next = Tokens.Next();
                    if (Tokens.Done()) { closed = false; break; }
                    if (tok_next.IsSymbol(";")) { break; }
                    if (tok_next.IsSymbol("[")) { scope++; continue; }
                    if (tok_next.IsSymbol("]")) { scope--; }
                    if (scope == 0) { closed = true; break; }
                    node.Array.Add(tok_next);
                }
                if (!closed) { Debug.Error("Expected ']' at for variable declaration line " + tok_id.Line); }
            }
            else { Tokens.Back(); }

            Token tok_eql = Tokens.Next();
            if (tok_eql.IsSymbol(";")) { return node; }
            if (tok_eql.IsSymbol("="))
            {
                while (true)
                {
                    tok_next = Tokens.Next();
                    if (Tokens.Done()) { Debug.Error("Expected ';' for variable declaration at line " + tok_id.Line); }
                    if (tok_next.IsSymbol(";")) { break; }
                    node.Init.Add(tok_next);
                }
            }
            return node;
        }

        private ASTFunctionDecl HandleFunctionDecl()
        {
            Token tok_id = Tokens.Next();
            if (tok_id.Type != TokenType.ID) { Debug.Error("Expected identifier for function declaration at line " + tok_id.Line); }

            ASTFunctionDecl node = new ASTFunctionDecl(tok_id.Value, "");

            bool closed = false;
            while (true)
            {
                Token tok = Tokens.Next();
                if (Tokens.Done()) { closed = false; break; }
                if (tok.IsSymbol(")")) { closed = true; break; }
                if (tok.Type == TokenType.ID)
                {
                    Tokens.Back();
                    node.Arguments.Add(HandleVariableDecl());
                    Debug.Info("Created fnarg - " + node.Arguments[node.Arguments.Count - 1].ToString());
                    Tokens.Back(); 
                }
            }
            if (!closed) { Debug.Error("Expected ')' for function declaration at line " + tok_id.Line); }

            Token tok_colon = Tokens.Next();
            if (!tok_colon.IsSymbol(":")) { Debug.Error("Expected ':' for function declaration at line " + tok_id.Line); }

            Token tok_type = Tokens.Next();
            while (tok_type.IsSymbol("@")) { node.Pointer++; tok_type = Tokens.Next(); }
            if (tok_type.Type != TokenType.Type && tok_type.Type != TokenType.ID) { Debug.Error("Expected type for function declaration at line " + tok_id.Line); }
            node.TypeString = tok_type.Value;

            node.Nodes = HandleScope(node);
            return node;
        }

        private ASTVariableSet HandleVariableSet()
        {
            Token tok_id = Tokens.Next();
            if (tok_id.Type != TokenType.ID) { Debug.Error("Expected identifier for function declaration at line " + tok_id.Line); }

            if (tok_id.Value == "addr")
            {
                bool fuck = false;
            }


            string op = string.Empty;
            Token tok_op = Tokens.Next();
            if (tok_op.Type != TokenType.Operator && !tok_op.IsSymbol("=") && !tok_op.IsSymbol("[")) 
                { 
                Debug.Error("Expected assignment value at line " + tok_id.Line); }
            op += tok_op.Value;
            tok_op = Tokens.Next();
            if (tok_op.IsSymbol("=")) { op += "="; }
            Tokens.Back();

            ASTVariableSet node = new ASTVariableSet(tok_id.Value, op);

            while (true)
            {
                Token tok_next = Tokens.Next();
                if (Tokens.Done()) { Debug.Error("Expected ';' for at line " + tok_id.Line); }
                if (tok_next.IsSymbol(";")) { break; }
                node.Expression.Add(tok_next);
            }

            return node;
        }

        public ASTWhile HandleWhile()
        {
            Token tok_open = Tokens.Next();
            if (!tok_open.IsSymbol("(")) { Debug.Error("Expected '(' at line " + tok_open.Line); }

            ASTWhile node = new ASTWhile();

            while (true)
            {
                Token tok_next = Tokens.Next();
                if (Tokens.Done()) { Debug.Error("Expected ')' for at line " + tok_open.Line); }
                if (tok_next.IsSymbol(")")) { break; }
                node.Expression.Add(tok_next);
            }

            node.Nodes = HandleScope(node);
            return node;
        }

        public ASTReturn HandleReturn()
        {
            ASTReturn node = new ASTReturn();

            while (true)
            {
                Token tok_next = Tokens.Next();
                if (Tokens.Done()) { Debug.Error("Expected ';' for at line " + tok_next.Line); }
                if (tok_next.IsSymbol(";")) { break; }
                node.Expression.Add(tok_next);
            }

            return node;
        }

        public ASTCall HandleCall()
        {
            Token tok_id = Tokens.Next();
            if (tok_id.Type != TokenType.ID) { return null; }

            ASTCall node = new ASTCall(tok_id.Value);

            Tokens.Next();
            while (true)
            {
                Token tok_next = Tokens.Next();
                if (Tokens.Done()) { Debug.Error("Expected ';' for at line " + tok_next.Line); }
                if (tok_next.IsSymbol(";")) { if (node.Arguments[node.Arguments.Count - 1].IsSymbol(")")) { node.Arguments.RemoveAt(node.Arguments.Count - 1); } break; }
                node.Arguments.Add(tok_next);
            }

            return node;
        }

        public ASTIf HandleIf()
        {
            Token tok_open = Tokens.Next();
            if (!tok_open.IsSymbol("(")) { Debug.Error("Expected '(' at line " + tok_open.Line); }

            ASTIf node = new ASTIf();

            while (true)
            {
                Token tok_next = Tokens.Next();
                if (Tokens.Done()) { Debug.Error("Expected ')' for at line " + tok_open.Line); }
                if (tok_next.IsSymbol(")")) { break; }
                node.Expression.Add(tok_next);
            }

            node.Nodes = HandleScope(node);
            return node;
        }

        public ASTElseIf HandleElseIf()
        {
            Token tok_open = Tokens.Next();
            if (!tok_open.IsSymbol("(")) { Debug.Error("Expected '(' at line " + tok_open.Line); }

            ASTElseIf node = new ASTElseIf();

            while (true)
            {
                Token tok_next = Tokens.Next();
                if (Tokens.Done()) { Debug.Error("Expected ')' for at line " + tok_open.Line); }
                if (tok_next.IsSymbol(")")) { break; }
                node.Expression.Add(tok_next);
            }

            node.Nodes = HandleScope(node); ;
            return node;
        }

        public ASTElse HandleElse()
        {
            ASTElse node = new ASTElse();
            node.Nodes = HandleScope(node);
            return node;
        }

        private void AddNode(ASTBody parent, ASTNode node)
        {
            if (parent == null) { Debug.Error("Attempt to add node with null parent"); }
            parent.Nodes.Add(node);
            Debug.Info("Created node  - " + node.ToString());
        }
    }
}
