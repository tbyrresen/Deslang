using Deslang.AST.DeclaringNodes;
using Deslang.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deslang.ContextualAnalysis
{
    namespace Deslang.ContextualAnalysis
    {
        // symbol table for classes and interfaces. Contains inner method symbol tables if any are present in the class or interface. 
        // note that for simplicity we use this class for both class and interface types
        public class ClassSymbolTable
        {
            private readonly Dictionary<string, DeclaringNode> _classVarDeclarings;
            private readonly Dictionary<string, MethodSymbolTable> _classMethodSymbolTables;
            private readonly Dictionary<string, DeclaringNode> _stdlib;     // reference to standard library functionality
            private readonly ErrorSummary _errorSummary;

            public DeclaringNode ClassDeclaring { get; }    // the declaration for the class or interface itself

            public ClassSymbolTable(DeclaringNode n, Dictionary<string, DeclaringNode> stdlib, ErrorSummary errorSummary)
            {
                _classVarDeclarings = new Dictionary<string, DeclaringNode>();
                _classMethodSymbolTables = new Dictionary<string, MethodSymbolTable>();
                ClassDeclaring = n;
                _stdlib = stdlib;
                _errorSummary = errorSummary;
            }

            // adds a local method ST to the class. Semantically this corresponds to opening a new scope in the class. 
            public void AddLocalMethodST(string methodName, MethodSymbolTable methodST)
            {
                if (_classMethodSymbolTables.ContainsKey(methodName))
                {
                    _errorSummary.AddError(new SemanticError($"'{methodName}' is already declared", methodST.MethodDeclaring.SourcePosition));
                }
                else
                {
                    _classMethodSymbolTables.Add(methodName, methodST);
                }              
            }

            public void EnterSymbol(string ident, DeclaringNode declaring)
            {
                if (_classVarDeclarings.ContainsKey(ident))
                {
                    _errorSummary.AddError(new SemanticError($"'{ident}' is already declared", declaring.SourcePosition));
                }
                else
                {
                    _classVarDeclarings.Add(ident, declaring);
                }     
            }

            // retrieves a symbol from the class ST by first looking in the class declarations. If the symbol is not found in the class declarations,
            // the lookup continues to look in the standard library. 
            public DeclaringNode RetrieveSymbol(string ident)
            {             
                if (_classVarDeclarings.ContainsKey(ident))
                {
                    return _classVarDeclarings[ident];
                }
                else if (_stdlib.ContainsKey(ident))
                {
                    return _stdlib[ident];
                }
                return null;
            }

            public MethodSymbolTable RetrieveMethodST(string ident)
            {
                if (_classMethodSymbolTables.ContainsKey(ident))
                {
                    return _classMethodSymbolTables[ident];
                }
                return null;
            }
        }
    }
}
