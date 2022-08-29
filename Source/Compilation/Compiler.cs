using System;
using System.Collections.Generic;
using System.Text;

namespace DSharpLang
{
    public abstract class Compiler
    {
        public ASTBody Node;
        public string Output;

        protected int _indent = 0;

        public Compiler(ASTBody node)
        {
            this.Node = node;
            this.Output = string.Empty;
        }

        public abstract void   Perform();
        public abstract string Generate(ASTBody node);

        protected abstract string GenerateClass(ASTClassDecl n_class);
        protected abstract string GenerateFunctionDecl(ASTFunctionDecl n_func, ASTClassDecl n_class = null);
        protected abstract string GenerateClassDecl(ASTClassDecl n_class);
        protected virtual string GenerateVariableDecl(ASTVariableDecl n_var, ASTClassDecl n_class, bool farg = false) { return string.Empty; }
        protected virtual string GenerateVariableDeclArgs(ASTVariableDecl n_var, ASTClassDecl n_class) { return string.Empty; }

        protected string GenerateRegion(ASTRegion n_region)
        {
            string output = "namespace " + n_region.ID + "\n{\n";
            output += Generate(n_region);
            output += "}\n";
            return output;
        }

        protected string Indent()
        {
            string output = string.Empty;
            for (int i = 0; i < _indent; i++) { output += " "; }
            return output;
        }
    }
}
