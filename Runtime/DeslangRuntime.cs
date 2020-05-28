using System.Reflection;
using System.Text;
using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace Deslang.CodeGeneration
{
    public class DeslangRuntime
    {
        private CSharpCodeProvider _provider;
        private CompilerParameters _options;
        
        public DeslangRuntime()
        {
            _provider = new CSharpCodeProvider();
            _options = new CompilerParameters();
           
            // add reference to required assemblies
            _options.ReferencedAssemblies.Add("System.dll");

            // add a reference to the current assembly to enable use of Deslang standard library methods
            _options.ReferencedAssemblies.Add(new Uri(typeof(CodeGenerator).Assembly.CodeBase).LocalPath);

            // set GenerateInMemory to true to execute in memory without creating external file 
            _options.GenerateInMemory = true;
        }

        public void Execute(string code, string mainMethodClassName)
        {
            CompilerResults results = _provider.CompileAssemblyFromSource(_options, code);

            if (results.Errors.HasErrors)
            {
                StringBuilder errorStr = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    errorStr.AppendLine($"Error: {error.ErrorText}");
                }

                Console.WriteLine($"\n{errorStr}");
                Environment.Exit(1);
            }
            
            try
            {
                Type DeslangProgram = results.CompiledAssembly.GetType($"Deslang.{mainMethodClassName}");
                MethodInfo mainMethod = DeslangProgram.GetMethod("Main");
                mainMethod.Invoke(null, null);
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Console.WriteLine($"\n{e.InnerException.Message}");
                }
                else
                {
                    Console.WriteLine($"\n{e.Message}");                    
                }
                Environment.Exit(1);
            }
        }
    }
}
