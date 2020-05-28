using Deslang.AST;
using Deslang.CodeGeneration;
using Deslang.ContextualAnalysis;
using Deslang.ContextualAnalysis.Deslang.ContextualAnalysis;
using Deslang.Errors;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using CodeGenerator = Deslang.CodeGeneration.CodeGenerator;

namespace Deslang
{
    public class Compiler
    {
        private static CharStream _charStream;
        private static Parser _parser;
        private static ErrorSummary _errorSummary;
        private static List<ProgramNode> _ASTRoots;     
        private static SymbolTableInitializer _STInitializer;
        private static Dictionary<string, ClassSymbolTable> _globalST;
        private static TypeChecker _checker;
        private static CodeGenerator _codeGenerator;
        private static DeslangRuntime _runtime;
        private static string _mainMethodParentClassName;

        static void Compile(List<string> filePaths)
        {
            _errorSummary = new ErrorSummary(); // error summary passed to all compiler stages to store potential errors
            _ASTRoots = new List<ProgramNode>();

            foreach (string path in filePaths) 
            {
                _charStream = new CharStream(File.OpenRead(path));
                _parser = new Parser(_charStream, _errorSummary, Path.GetFileName(path));
                _ASTRoots.Add(_parser.Program());   // 1st pass
            }

            if (_errorSummary.SyntaxErrors.Count != 0) 
            {
                foreach (SyntaxError error in _errorSummary.SyntaxErrors)
                {
                    Console.WriteLine(error.Message + " " + error.Position);
                }
                Environment.Exit(1);
            }

            _STInitializer = new SymbolTableInitializer(_errorSummary);
            (_globalST, _mainMethodParentClassName) = _STInitializer.FillTable(_ASTRoots);    // 2nd pass

            if (_errorSummary.SemanticErrors.Count != 0)
            {
                foreach (SemanticError error in _errorSummary.SemanticErrors)
                {
                    Console.WriteLine(error.Message + " " + error.Position);
                }
                Environment.Exit(1);
            }

            _checker = new TypeChecker(_globalST, _errorSummary);
            _checker.Check(_ASTRoots);  // 3rd pass

            if (_errorSummary.SemanticErrors.Count != 0) 
            {
                foreach (SemanticError error in _errorSummary.SemanticErrors)
                {
                    Console.WriteLine(error.Message + " " + error.Position);
                }
                Environment.Exit(1);
            }

            _codeGenerator = new CodeGenerator();
            string CSharpCode = _codeGenerator.Generate(_ASTRoots);  // 4th pass

            _runtime = new DeslangRuntime();
            _runtime.Execute(CSharpCode, _mainMethodParentClassName);                      
        }

        static void Main(string[] args)
        {
            List<string> filePaths = new List<string>();
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: deslang file(s)");
                Environment.Exit(1);
            }
            foreach (string arg in args)
            {
                string filePath = new Uri($"{Directory.GetCurrentDirectory()}\\{arg}").LocalPath;
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File with name '{Path.GetFileName(filePath)}' could not be found");
                    Environment.Exit(1);
                }
                if (Path.GetExtension(filePath) != ".deslang")
                {
                    Console.WriteLine($"File extension of file '{Path.GetFileName(filePath)}' is not '.deslang'");
                    Environment.Exit(1);
                }               
                filePaths.Add(filePath);
            }

            Compile(filePaths);
        }
    }
}