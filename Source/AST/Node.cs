using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DSharpLang
{
    public enum AST : byte
    { 
        Node,
        Body,
        Primary,
        Region,

        ClassDeclare,
        StructDeclare,              //
        FunctionDeclare,
        VarDeclare,

        ReturnStatement,
        CallStatement,      
        VarSetStatement,
        IfStatement,                
        ElseIfStatement,            
        ElseStatement,              
        ForStatement,               //
        ForEachStatement,           //
        WhileStatement,
        ImportStatement,
    }

    public abstract class ASTNode
    {
        public AST Type;
        public ASTNode(AST type) { this.Type = type; }
    }

    public class ASTBody : ASTNode
    { 
        public List<ASTNode> Nodes;

        public ASTBody() : base(AST.Body) { this.Nodes = new List<ASTNode>(); }
        public ASTBody(AST type, params ASTNode[] nodes) : base(type) { this.Nodes = nodes.ToList<ASTNode>(); }
    }

    public class ASTImport : ASTNode
    {
        public string ID;

        public ASTImport(string id = "") : base(AST.ImportStatement) { this.ID = id; }
        public override string ToString() { return "Type:" + Type.ToString().PadRight(20, ' ') + " ID:" + ID; }
    }

    public class ASTRegion : ASTBody
    { 
        public string ID;

        public ASTRegion(string id = "") : base(AST.Region) { this.ID = id; }
        public override string ToString() { return "Type:" + Type.ToString().PadRight(20, ' ') + " ID:" + ID + " Nodes:" + Nodes.Count; }
    }

    public class ASTClassDecl : ASTBody
    { 
        public string ID;
        public bool   Static;

        public ASTClassDecl(string id, bool s = false) : base(AST.ClassDeclare) { this.ID = id; this.Static = s; }

        public override string ToString() { return "Type:" + Type.ToString().PadRight(20, ' ') + " ID:" + ID + " Static:" + (Static ? "1" : "0") + " Nodes:" + Nodes.Count; }
    }

    public class ASTStructDecl : ASTBody
    {
        public string ID;
        public bool Static;

        public ASTStructDecl(string id, bool s = false) : base(AST.StructDeclare) { this.ID = id; this.Static = s; }

        public override string ToString() { return "Type:" + Type.ToString().PadRight(20, ' ') + " ID:" + ID + " Static:" + (Static ? "1" : "0") + " Nodes:" + Nodes.Count; }
    }

    public class ASTVariableDecl : ASTNode
    { 
        public string      ID;
        public string      TypeString;
        public int         Pointer;
        public bool        Constant;
        public List<Token> Array;
        public List<Token> Init;

        public ASTVariableDecl(string id, string typestr, int ptr, bool cnst, params Token[] toks) : base(AST.VarDeclare)
        {
            this.ID         = id;
            this.TypeString = typestr;
            this.Pointer    = ptr;
            this.Constant   = cnst;
            this.Array      = new List<Token>();
            this.Init       = new List<Token>();
            if (toks.Length > 0) { this.Init.AddRange(toks); }
        }

        public override string ToString()
        {
            string init = string.Empty;
            for (int i = 0; i < Init.Count; i++) { init += Init[i].Value + " "; }
            if (init.EndsWith(" ")) { init = init.Remove(init.Length - 1, 1); }
            return "Type:" + Type.ToString().PadRight(20, ' ') + " TypeString:" + TypeString.PadRight(15, ' ') + " Array:" + Array.Count + " Pointer:" + Pointer + " Constant:" + (Constant? "1" : "0") + " ID:" + ID + " Init: { " + init + " }";
        }
    }

    public class ASTFunctionDecl : ASTBody
    {
        public string                ID;
        public string                TypeString;
        public int                   Pointer;
        public List<ASTVariableDecl> Arguments;

        public ASTFunctionDecl(string id, string typestr, params ASTVariableDecl[] args) : base(AST.FunctionDeclare)
        {
            this.ID         = id;
            this.Pointer    = 0;
            this.TypeString = typestr;
            this.Arguments  = new List<ASTVariableDecl>();
            if (args.Length > 0) { this.Arguments.AddRange(args); }
        }

        public override string ToString() { return "Type:" + Type.ToString().PadRight(20, ' ') + " TypeString:" + TypeString.PadRight(15, ' ') + "Arguments:" + Arguments.Count + " ID:" + ID; }
    }

    public class ASTVariableSet : ASTNode
    {
        public string      ID;
        public string      Operation;
        public List<Token> Indexer;
        public List<Token> Expression;

        public ASTVariableSet(string id, string op) : base(AST.VarSetStatement)
        {
            this.ID         = id;
            this.Operation  = op;
            this.Indexer    = new List<Token>();
            this.Expression = new List<Token>();
        }

        public override string ToString()
        {
            string expr = string.Empty;
            for (int i = 0; i < Expression.Count; i++) { expr += Expression[i].Value + " "; }
            if (expr.EndsWith(" ")) { expr = expr.Remove(expr.Length - 1, 1); }
            return "Type:" + Type.ToString().PadRight(20, ' ') + "Op:" + Operation + " ID:" + ID + " Expr:{ " + expr + " }";
        }
    }

    public class ASTWhile : ASTBody
    {
        public List<Token> Expression;

        public ASTWhile() : base(AST.WhileStatement)
        {
            this.Expression = new List<Token>();
        }

        public override string ToString()
        {
            string expr = string.Empty;
            for (int i = 0; i < Expression.Count; i++) { expr += Expression[i].Value + " "; }
            if (expr.EndsWith(" ")) { expr = expr.Remove(expr.Length - 1, 1); }
            return "Type:" + Type.ToString().PadRight(20, ' ') + " Expr:{ " + expr + " }";
        }
    }

    public class ASTReturn : ASTNode
    {
        public List<Token> Expression;

        public ASTReturn() : base(AST.ReturnStatement) { this.Expression = new List<Token>(); }

        public override string ToString()
        {
            string expr = string.Empty;
            for (int i = 0; i < Expression.Count; i++) { expr += Expression[i].Value + " "; }
            if (expr.EndsWith(" ")) { expr = expr.Remove(expr.Length - 1, 1); }
            return "Type:" + Type.ToString().PadRight(20, ' ') + " Expr:{ " + expr + " }";
        }
    }

    public class ASTIf : ASTBody
    {
        public List<Token> Expression;

        public ASTIf() : base(AST.IfStatement) { this.Expression = new List<Token>(); }

        public override string ToString()
        {
            string expr = string.Empty;
            for (int i = 0; i < Expression.Count; i++) { expr += Expression[i].Value + " "; }
            if (expr.EndsWith(" ")) { expr = expr.Remove(expr.Length - 1, 1); }
            return "Type:" + Type.ToString().PadRight(20, ' ') + " Expr:{ " + expr + " }";
        }
    }

    public class ASTElseIf : ASTBody
    {
        public List<Token> Expression;

        public ASTElseIf() : base(AST.ElseIfStatement) { this.Expression = new List<Token>(); }

        public override string ToString()
        {
            string expr = string.Empty;
            for (int i = 0; i < Expression.Count; i++) { expr += Expression[i].Value + " "; }
            if (expr.EndsWith(" ")) { expr = expr.Remove(expr.Length - 1, 1); }
            return "Type:" + Type.ToString().PadRight(20, ' ') + " Expr:{ " + expr + " }";
        }
    }

    public class ASTElse : ASTBody
    {
        public ASTElse() : base(AST.ElseStatement) { }

        public override string ToString()
        {
            return "Type:" + Type.ToString().PadRight(20, ' ') + " Nodes:" + Nodes.Count;
        }
    }

    public class ASTCall : ASTNode
    {
        public string ID;
        public List<Token> Arguments;

        public ASTCall(string id) : base(AST.CallStatement)
        { 
            this.ID = id;
            this.Arguments = new List<Token>();
        }

        public override string ToString()
        {
            string expr = string.Empty;
            for (int i = 0; i < Arguments.Count; i++) { expr += Arguments[i].Value + " "; }
            if (expr.EndsWith(" ")) { expr = expr.Remove(expr.Length - 1, 1); }
            return "Type:" + Type.ToString().PadRight(20, ' ') + " ID:" + ID + " Expr:{ " + expr + " }";
        }
    }
}
