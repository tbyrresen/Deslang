using Deslang.AST.DeclaringNodes;
using Deslang.ContextualAnalysis.Deslang.ContextualAnalysis;
using Deslang.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deslang.ContextualAnalysis
{
    public class MethodSymbolTable
    {
        private readonly ClassSymbolTable _parentClass;
        private readonly Dictionary<string, DeclaringNode> _methodVarDeclarings;
        private readonly ErrorSummary _errorSummary;

        public DeclaringNode MethodDeclaring { get; }   // the method declaration itself

        public MethodSymbolTable(ClassSymbolTable parentClass, DeclaringNode methodDeclaring, ErrorSummary errorSummary)
        {
            _parentClass = parentClass;
            MethodDeclaring = methodDeclaring;
            _methodVarDeclarings = new Dictionary<string, DeclaringNode>();
            _errorSummary = errorSummary;
        }

        public void EnterSymbol(string ident, DeclaringNode declaring)
        {
            if (_methodVarDeclarings.ContainsKey(ident))
            {
                _errorSummary.AddError(new SemanticError($"'{ident}' is already declared", declaring.SourcePosition));
            }
            else
            {
                _methodVarDeclarings.Add(ident, declaring);
            }        
        }

        // retrieves a symbol from the method ST by first looking in the method declarations. If the symbol is not found in the method declarations,
        // the lookup continues to look in the parent class.
        public DeclaringNode RetrieveSymbol(string ident)
        {
            if (_methodVarDeclarings.ContainsKey(ident))
            {
                return _methodVarDeclarings[ident];
            }
            return _parentClass.RetrieveSymbol(ident);
        }
    }
}

