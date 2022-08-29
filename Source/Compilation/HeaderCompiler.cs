using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpLang
{
    class HeaderCompiler : Compiler
    {

        public HeaderCompiler(ASTBody node) : base(node)
        {
            
        }

        public override void Perform()
        {
            Output += Generate(Node);
            Debug.OK("Finished compiling AST\n");
            Console.WriteLine(Output);
        }

        public override string Generate(ASTBody body)
        {
            string output = string.Empty;
            for (int i = 0; i < body.Nodes.Count; i++)
            {
                if (body.Nodes[i].Type == AST.Region) { output += GenerateRegion((ASTRegion)body.Nodes[i]); }
                else if (body.Nodes[i].Type == AST.FunctionDeclare) { output += GenerateFunctionDecl((ASTFunctionDecl)body.Nodes[i]); }
                else if (body.Nodes[i].Type == AST.VarDeclare) { output += Indent() + GenerateVariableDecl((ASTVariableDecl)body.Nodes[i], null); }
                else if (body.Nodes[i].Type == AST.ClassDeclare) { output += GenerateClassDecl((ASTClassDecl)body.Nodes[i]); }
            }
            return output;
        }

        protected override string GenerateClass(ASTClassDecl body)
        {
            string output = string.Empty;
            for (int i = 0; i < body.Nodes.Count; i++)
            {
                if (body.Nodes[i].Type == AST.FunctionDeclare) { output += GenerateFunctionDecl((ASTFunctionDecl)body.Nodes[i], body); }
                else if (body.Nodes[i].Type == AST.VarDeclare) { output += GenerateVariableDecl((ASTVariableDecl)body.Nodes[i], body); }
            }
            return output;
        }

        protected override string GenerateFunctionDecl(ASTFunctionDecl n_func, ASTClassDecl n_class = null)
        {
            string output = "\n";
            if (n_class != null) { if (n_class.Static) { output += "static "; } }
            output += n_func.TypeString + " " + n_func.ID + "(";
            for (int i = 0; i < n_func.Arguments.Count; i++)
            {
                output += GenerateVariableDecl(n_func.Arguments[i], n_class, true);
            }
            output += ");\n";
            return output;
        }

        protected override string GenerateClassDecl(ASTClassDecl n_class)
        {
            string output = "class " + n_class.ID + "\n" + "{" + "\n";
            output += "public:\n";
            output += GenerateClass(n_class);
            output += "};\n";
            return output;
        }

        protected override string GenerateVariableDecl(ASTVariableDecl n_var, ASTClassDecl n_class, bool farg = false)
        {
            string output = string.Empty;
            bool stc = false;
            if (n_class != null) { stc = n_class.Static; }
            if (n_class != null && !farg) { if (stc) { output += "static "; } }
            if (n_var.Constant) { output += "const "; }
            if (Grammar.IsType(n_var.TypeString)) { output += Grammar.ConvertDefaultType(n_var.TypeString); }
            else { output += n_var.TypeString; }

            for (int i = 0; i < n_var.Pointer; i++) { output += "*"; }
            output += " " + n_var.ID;
            if (n_var.Array.Count > 0)
            {
                if (n_var.Constant && stc) { output += GenerateVariableDeclArgs(n_var, n_class); }
                else { output += "[]"; }
            }
            if (n_var.Constant)
            {
                if (n_var.Init.Count > 0)
                {
                    output += " = ";
                    for (int i = 0; i < n_var.Init.Count; i++) { output += n_var.Init[i].Value + " "; }
                    if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
                }
            }
            if (!farg) { output += ";"; }
            return output + (!farg ? "\n" : "");
        }

        protected override string GenerateVariableDeclArgs(ASTVariableDecl n_var, ASTClassDecl n_class)
        {
            string output = "[";
            for (int i = 0; i < n_var.Array.Count; i++) { output += n_var.Array[i].Value + " "; }
            if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            output += "]";
            return output;
        }
    }
}
