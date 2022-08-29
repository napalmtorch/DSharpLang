using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpLang
{
    public class SourceCompiler : Compiler
    {
        public SourceCompiler(ASTBody node) : base(node)
        { 

        }

        public override void Perform()
        {
            _indent = 0;
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
                else if (body.Nodes[i].Type == AST.VarDeclare) { output += GenerateVariableDecl((ASTVariableDecl)body.Nodes[i], null, false, false); }
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
                else if (body.Nodes[i].Type == AST.VarSetStatement) { output += GenerateVariableSet((ASTVariableSet)body.Nodes[i], body); }
                if (body.Static)
                {
                    if (body.Nodes[i].Type == AST.VarDeclare) { output += GenerateVariableDecl((ASTVariableDecl)body.Nodes[i], body, false, false); }
                }
            }
            return output;
        }

        private string GenerateStatement(ASTBody node, ASTClassDecl body)
        {
            string output = string.Empty;
            for (int i = 0; i < node.Nodes.Count; i++)
            {
                if (node.Nodes[i].Type == AST.VarSetStatement) { output += GenerateVariableSet((ASTVariableSet)node.Nodes[i], body); }
                else if (node.Nodes[i].Type == AST.VarDeclare) { output += GenerateVariableDecl((ASTVariableDecl)node.Nodes[i], body, false, true); }
                else if (node.Nodes[i].Type == AST.WhileStatement) { output += GenerateWhile((ASTWhile)node.Nodes[i], body); }
                else if (node.Nodes[i].Type == AST.ReturnStatement) { output += GenerateReturn((ASTReturn)node.Nodes[i]); }
                else if (node.Nodes[i].Type == AST.CallStatement) { output += GenerateCall((ASTCall)node.Nodes[i], body); }
                else if (node.Nodes[i].Type == AST.IfStatement) { output += GenerateIf((ASTIf)node.Nodes[i], body); }
                else if (node.Nodes[i].Type == AST.ElseIfStatement) { output += GenerateElseIf((ASTElseIf)node.Nodes[i], body); }
                else if (node.Nodes[i].Type == AST.ElseStatement) { output += GenerateElse((ASTElse)node.Nodes[i], body); }
            }
            return output;
        }

        protected override string GenerateFunctionDecl(ASTFunctionDecl n_func, ASTClassDecl n_class = null)
        {
            string output = "\n" + n_func.TypeString + " " + (n_class != null ? (n_class.ID + "::") : "") + n_func.ID + "(";
            for (int i = 0; i < n_func.Arguments.Count; i++)
            {
                output += GenerateVariableDecl(n_func.Arguments[i], n_class, true, true);
            }
            output += ")\n" + "{" + "\n";
            string gen = GenerateStatement(n_func, n_class);
            output += gen;
            output += "}\n";
            return output;
        }

        protected override string GenerateClassDecl(ASTClassDecl n_class)
        {
            return GenerateClass(n_class) + "\n";
        }

        private string GenerateVariableDecl(ASTVariableDecl n_var, ASTClassDecl n_class, bool farg, bool local)
        {
            string output = string.Empty;
            bool stc = false;
            if (n_class != null) { stc = n_class.Static; }
            if (Grammar.IsType(n_var.TypeString)) { output += Grammar.ConvertDefaultType(n_var.TypeString); }
            else { output += n_var.TypeString; }

            for (int i = 0; i < n_var.Pointer; i++) { output += "*"; }
            output += " ";
            if (n_class != null && !local) { output += n_class.ID + "::"; }
            output += n_var.ID;
            if (n_var.Array.Count > 0) { output += GenerateVariableDeclArgs(n_var, n_class); }
            if (!n_var.Constant)
            {
                if (n_var.Init.Count > 0)
                {
                    output += " = ";
                    for (int i = 0; i < n_var.Init.Count; i++) { output += n_var.Init[i].Value + " "; }
                    if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
                }
            }
            if (stc && n_var.Constant) { return string.Empty; }
            if (!farg) { output += ";"; }
            return output + (!farg ? "\n" : "");
        }

        private string GenerateVariableSet(ASTVariableSet n_var, ASTClassDecl n_class = null)
        {
            string output = string.Empty;
            output += n_var.ID += " ";
            output += n_var.Operation += " ";
            if (n_var.Expression.Count > 0)
            {
                for (int i = 0; i < n_var.Expression.Count; i++) { output += n_var.Expression[i].Value + " "; }
                if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            }
            return output + ";\n";
        }

        protected override string GenerateVariableDeclArgs(ASTVariableDecl n_var, ASTClassDecl n_class)
        {
            string output = "[";
            for (int i = 0; i < n_var.Array.Count; i++) { output += n_var.Array[i].Value + " "; }
            if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            output += "]";
            return output;
        }

        private string GenerateWhile(ASTWhile n_while, ASTClassDecl n_class)
        {
            string output = "while (";
            for (int i = 0; i < n_while.Expression.Count; i++) { output += n_while.Expression[i].Value + " "; }
            if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            output += ")\n" + "{\n";
            output += GenerateStatement(n_while, n_class);
            output += "}\n";
            return output;
        }
        
        private string GenerateReturn(ASTReturn n_return)
        {
            string output = string.Empty;
            output += "return ";
            for (int i = 0; i < n_return.Expression.Count; i++) { output += n_return.Expression[i].Value + " "; }
            if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            output += ";\n";
            return output;
        }

        private string GenerateCall(ASTCall n_call, ASTClassDecl n_class)
        {
            string output = string.Empty;
            output += n_call.ID + "(";
            for (int i = 0; i < n_call.Arguments.Count; i++) { output += n_call.Arguments[i].Value + " "; }
            if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            output += ");\n";
            return output;
        }

        private string GenerateIf(ASTIf n_if, ASTClassDecl n_class)
        {
            string output = "if (";
            for (int i = 0; i < n_if.Expression.Count; i++) { output += n_if.Expression[i].Value + " "; }
            if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            output += ")\n" + "{\n";
            output += GenerateStatement(n_if, n_class);
            output += "}\n";
            return output;
        }


        private string GenerateElseIf(ASTElseIf n_if, ASTClassDecl n_class)
        {
            string output = "else if (";
            for (int i = 0; i < n_if.Expression.Count; i++) { output += n_if.Expression[i].Value + " "; }
            if (output.EndsWith(" ")) { output = output.Remove(output.Length - 1, 1); }
            output += ")\n" + "{\n";
            output += GenerateStatement(n_if, n_class);
            output += "}\n";
            return output;
        }

        private string GenerateElse(ASTElse n_if, ASTClassDecl n_class)
        {
            string output = "else\n" + "{\n";
            output += GenerateStatement(n_if, n_class);
            output += "}\n";
            return output;
        }
    }
}