using System;
using System.Text;
using System.Collections.Generic;

namespace DSharpLang
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.Init();
            Debug.Info("DSharp Language Compiler");
            HandleArguments(args);
        }

        static void HandleArguments(string[] args)
        {
            if (args.Length == 0) { Debug.Error("No input files."); }
            else
            {
                Tokenizer tokenizer = new Tokenizer(args[0]);
                tokenizer.Perform("tokens.txt");

                ASTGenerator astgen = new ASTGenerator(tokenizer.Tokens);
                astgen.Perform(astgen.PrimaryNode);

                HeaderCompiler comp = new HeaderCompiler(astgen.PrimaryNode);
                comp.Perform();

                SourceCompiler scomp = new SourceCompiler(astgen.PrimaryNode);
                scomp.Perform();
            }
        }
    }
}
