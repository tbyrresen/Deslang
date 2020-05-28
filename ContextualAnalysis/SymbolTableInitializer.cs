using Deslang.AST;
using Deslang.AST.ActionNodes;
using Deslang.AST.ArgumentNodes;
using Deslang.AST.DeclaringNodes;
using Deslang.AST.ExpressionNodes;
using Deslang.AST.MemberNodes;
using Deslang.AST.ParameterNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.ContextualAnalysis.Deslang.ContextualAnalysis;
using Deslang.Errors;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;

namespace Deslang.ContextualAnalysis
{
    public class SymbolTableInitializer : IVisitor
    {
        private readonly Dictionary<string, DeclaringNode> _stdlib; // stdlibrary reference added to class symbol tables
        private readonly Dictionary<string, ClassSymbolTable> _globalTable; // scope level 0
        private ClassSymbolTable _currentClassST; // scope level 1
        private MethodSymbolTable _currentMethodST; // scope level 2
        private int _currentScopeLevel;
        private SourceCodePosition _dummyPos;
        private IdentifierNode _dummyIdent;
        private readonly ErrorSummary _errorSummary;
        private bool _hasMainMethod;
        private string _mainMethodParentClassName;

        public SymbolTableInitializer(ErrorSummary errorSummary)
        {
            _stdlib = new Dictionary<string, DeclaringNode>();
            _globalTable = new Dictionary<string, ClassSymbolTable>();
            _currentScopeLevel = 0; 
            _dummyPos = new SourceCodePosition(0, 0, null);
            _dummyIdent = new IdentifierNode(new Token(Token.TokenType.Identifier, _dummyPos));
            _errorSummary = errorSummary;
            _hasMainMethod = false;
            _mainMethodParentClassName = "";
        }

        // Fills the global symbol table by creating a mapping from class and interface names to their respective symbol tables.
        // The global symbol table maps to class symbol tables; class symbol tables maps to class declarations and finally
        // method symbol tables maps to method declarations thus effectively creating the desired nested scope structure. 
        // Note that for simplicity we use class symbol tables for both classes and interfaces.
        // The symbol tables created in this class are used in the later type checking and enables the type checker to correctly handle
        // forward referencing of variables. 
        // also returns the name of the class in which the 'Main' method is declared. if no such method is found, an error is produced. 
        public (Dictionary<string, ClassSymbolTable>, string) FillTable(List<ProgramNode> ASTRoots)
        {
            BuiltStdLib();
            foreach (ProgramNode n in ASTRoots)
            {
                n.Accept(this, null);
            }
            if (!_hasMainMethod)
            {
                _errorSummary.AddError(new SemanticError("No viable 'Main' method found", null));
            }
            return (_globalTable, _mainMethodParentClassName);
        }

        // builds the standard library functionality
        private void BuiltStdLib()
        {
            _stdlib.Add("print", CreateStdMethod(new SingleParameterSequenceNode(new MethodParameterNode(new AnyTypeNode(_dummyPos), _dummyIdent, _dummyPos), _dummyPos), new VoidTypeNode(_dummyPos)));
            _stdlib.Add("printLine", CreateStdMethod(new SingleParameterSequenceNode(new MethodParameterNode(new AnyTypeNode(_dummyPos), _dummyIdent, _dummyPos), _dummyPos), new VoidTypeNode(_dummyPos)));
            _stdlib.Add("length", CreateStdMethod(new SingleParameterSequenceNode(new MethodParameterNode(new ArrayTypeNode(new AnyTypeNode(_dummyPos), null, _dummyPos), _dummyIdent, _dummyPos), _dummyPos), new IntTypeNode(_dummyPos)));
            _stdlib.Add("normal", CreateStdMethod(new MultipleParameterSequenceNode(new MethodParameterNode(new RealTypeNode(_dummyPos), _dummyIdent, _dummyPos),
                new SingleParameterSequenceNode(new MethodParameterNode(new RealTypeNode(_dummyPos), _dummyIdent, _dummyPos), _dummyPos), _dummyPos), new RealTypeNode(_dummyPos)));
            _stdlib.Add("exponential", CreateStdMethod(new SingleParameterSequenceNode(new MethodParameterNode(new RealTypeNode(_dummyPos), _dummyIdent, _dummyPos), _dummyPos), new RealTypeNode(_dummyPos)));
            _stdlib.Add("discreteUniform", CreateStdMethod(new MultipleParameterSequenceNode(new MethodParameterNode(new IntTypeNode(_dummyPos), _dummyIdent, _dummyPos),
                new SingleParameterSequenceNode(new MethodParameterNode(new IntTypeNode(_dummyPos), _dummyIdent, _dummyPos), _dummyPos), _dummyPos), new IntTypeNode(_dummyPos)));
            _stdlib.Add("continuousUniform", CreateStdMethod(new MultipleParameterSequenceNode(new MethodParameterNode(new IntTypeNode(_dummyPos), _dummyIdent, _dummyPos),
                new SingleParameterSequenceNode(new MethodParameterNode(new IntTypeNode(_dummyPos), _dummyIdent, _dummyPos), _dummyPos), _dummyPos), new RealTypeNode(_dummyPos)));
        }

        private MethodDeclaringNode CreateStdMethod(ParameterSequenceNode parameters, TypeNode type)
        {
            MethodDeclaringNode declaring = new MethodDeclaringNode(_dummyIdent, type, parameters, new EmptyDeclaringSequenceNode(_dummyPos), new EmptyActionSequenceNode(_dummyPos), new ReturnActionNode(null, _dummyPos), _dummyPos);
            return declaring;
        }

        private void AddToGlobalTable(string ident, ClassSymbolTable classST)
        {
            if (_globalTable.ContainsKey(ident))
            {
                _errorSummary.AddError(new SemanticError($"'{ident}' is already declared", classST.ClassDeclaring.SourcePosition));
            }
            else
            {
                _globalTable.Add(ident, classST);
            }
        }

        public object Visit(ProgramNode n, object o)
        {
            n.Declarings.Accept(this, null);
            return null;
        }

        public object Visit(EmptyDeclaringSequenceNode n, object o)
        {
            return null;
        }

        public object Visit(SingleDeclaringSequenceNode n, object o)
        {
            n.Declaring.Accept(this, null);
            return null;
        }

        public object Visit(MultipleDeclaringSequenceNode n, object o)
        {
            n.Declaring.Accept(this, null);
            n.Declarings.Accept(this, null);
            return null;
        }

        public object Visit(InterfaceDeclaringNode n, object o)
        {
            _currentScopeLevel = 1;
            _currentClassST = new ClassSymbolTable(n, _stdlib, _errorSummary);
            n.InterfaceMethods.Accept(this, null);
            AddToGlobalTable(n.Identifier.Value, _currentClassST);
            return null;
        }

        public object Visit(InterfaceMethodDeclaringNode n, object o)
        {
            _currentScopeLevel = 2;
            _currentClassST.EnterSymbol(n.Identifier.Value, n);
            _currentMethodST = new MethodSymbolTable(_currentClassST, n, _errorSummary);
            n.Parameters.Accept(this, null);
            _currentClassST.AddLocalMethodST(n.Identifier.Value, _currentMethodST);
            return null;
        }

        public object Visit(ClassDeclaringNode n, object o)
        {
            _currentScopeLevel = 1;
            _currentClassST = new ClassSymbolTable(n, _stdlib, _errorSummary);
            n.Implements.Accept(this, null);
            n.Variables.Accept(this, null);
            n.Constructor.Accept(this, null);
            n.Methods.Accept(this, null);
            AddToGlobalTable(n.Identifier.Value, _currentClassST);
            return null;
        }

        public object Visit(ImplementDeclaringNode n, object o)
        {
            _currentClassST.EnterSymbol(n.Identifier.Value, n);
            return null;
        }

        // uses the current scope level to determine whether the variable should be added to the current class ST or the current method ST
        public object Visit(VarDeclaringNode n, object o)
        {
            if (_currentScopeLevel == 1)
            {
                _currentClassST.EnterSymbol(n.Identifier.Value, n);
            }
            else if (_currentScopeLevel == 2)
            {
                _currentMethodST.EnterSymbol(n.Identifier.Value, n);
            }
            return null;
        }

        public object Visit(AssignedVarDeclaringNode n, object o)
        {
            if (_currentScopeLevel == 1)
            {
                _currentClassST.EnterSymbol(n.Identifier.Value, n);
            }
            else if (_currentScopeLevel == 2)
            {
                _currentMethodST.EnterSymbol(n.Identifier.Value, n);
            }
            return null;
        }

        public object Visit(ConstructorDeclaringNode n, object o)
        {
            _currentScopeLevel = 2;
            _currentClassST.EnterSymbol(n.Identifier.Value, n);
            _currentMethodST = new MethodSymbolTable(_currentClassST, n, _errorSummary);
            n.Parameters.Accept(this, null);
            n.VarDeclarings.Accept(this, null);
            n.Actions.Accept(this, null);
            _currentClassST.AddLocalMethodST(n.Identifier.Value, _currentMethodST);
            return null;
        }

        public object Visit(MethodDeclaringNode n, object o)
        {
            CheckIfMain(n);
            _currentScopeLevel = 2;
            _currentClassST.EnterSymbol(n.Identifier.Value, n);
            _currentMethodST = new MethodSymbolTable(_currentClassST, n, _errorSummary);
            n.Parameters.Accept(this, null);
            n.VarDeclarings.Accept(this, null);
            n.Actions.Accept(this, null);
            _currentClassST.AddLocalMethodST(n.Identifier.Value, _currentMethodST);
            return null;
        }

        private void CheckIfMain(MethodDeclaringNode n)
        {
            if (n.Identifier.Value == "Main"  && _hasMainMethod == false)
            {
                if (!(n.Type is VoidTypeNode))
                {
                    _errorSummary.AddError(new SemanticError("Main method must be of type 'void'", n.SourcePosition));
                }
                else
                {
                    _hasMainMethod = true;
                    _mainMethodParentClassName = ((ClassDeclaringNode)_currentClassST.ClassDeclaring).Identifier.Value;
                }              
            }
            else if (n.Identifier.Value == "Main" && _hasMainMethod == true)
            {
                _errorSummary.AddError(new SemanticError("Main method already declared", n.SourcePosition));
            }
        }

        public object Visit(AssigningActionNode n, object o)
        {
            return null;
        }

        public object Visit(EmptyActionNode n, object o)
        {
            return null;
        }

        public object Visit(EmptyActionSequenceNode n, object o)
        {
            return null;
        }

        public object Visit(SingleActionSequenceNode n, object o)
        {
            n.Action.Accept(this, null);
            return null;
        }

        public object Visit(MultipleActionSequenceNode n, object o)
        {
            n.Action.Accept(this, null);
            n.Actions.Accept(this, null);
            return null;
        }

        public object Visit(IfActionNode n, object o)
        {
            return null;
        }

        public object Visit(ElseifActionNode n, object o)
        {
            return null;
        }

        public object Visit(ElseActionNode n, object o)
        {
            return null;
        }

        public object Visit(WhileActionNode n, object o)
        {
            return null;
        }

        public object Visit(ForeachActionNode n, object o)
        {
            n.itsDeclaring.Accept(this, null);  // visit the declaration associated with the foreach loop
            return null;
        }

        public object Visit(BreakActionNode n, object o)
        {
            return null;
        }

        public object Visit(ThisReferenceAction n, object o)
        {
            return null;
        }

        public object Visit(MemberReferenceAction n, object o)
        {
            return null;
        }

        public object Visit(ReturnActionNode n, object o)
        {
            return null;
        }

        public object Visit(BinaryExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(UnaryExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(CallExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(InstantiationNode n, object o)
        {
            return null;
        }

        public object Visit(IntExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(RealExpressionNode n, object o)
        {
            return null; ;
        }

        public object Visit(StringExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(NullExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(BooleanExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(MemberExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(ThisExpressionNode n, object o)
        {
            return null;
        }

        public object Visit(SimpleMemberNode n, object o)
        {
            return null;
        }

        public object Visit(CalledMemberNode n, object o)
        {
            return null;
        }

        public object Visit(IndexedMemberNode n, object o)
        {
            return null;
        }

        public object Visit(CalledAndIndexedMemberNode n, object o)
        {
            return null;
        }

        public object Visit(ThisNode n, object o)
        {
            return null;
        }

        public object Visit(DotMemberNode n, object o)
        {
            return null;
        }

        public object Visit(IdentifierNode n, object o)
        {
            return null;
        }

        public object Visit(BooleanLiteralNode n, object o)
        {
            return null;
        }

        public object Visit(IntLiteralNode n, object o)
        {
            return null;
        }

        public object Visit(NullLiteralNode n, object o)
        {
            return null;
        }

        public object Visit(OperatorNode n, object o)
        {
            return null;
        }

        public object Visit(RealLiteralNode n, object o)
        {
            return null;
        }

        public object Visit(StringLiteralNode n, object o)
        {
            return null;
        }

        public object Visit(ArrayTypeNode n, object o)
        {
            return null;
        }

        public object Visit(BooleanTypeNode n, object o)
        {
            return null;
        }

        public object Visit(ClassTypeNode n, object o)
        {
            return null;
        }

        public object Visit(IntTypeNode n, object o)
        {
            return null;
        }

        public object Visit(RealTypeNode n, object o)
        {
            return null;
        }

        public object Visit(StringTypeNode n, object o)
        {
            return null;
        }

        public object Visit(VoidTypeNode n, object o)
        {
            return null;
        }

        public object Visit(ErrorTypeNode n, object o)
        {
            return null;
        }

        public object Visit(MethodTypeNode n, object o)
        {
            return null;
        }

        public object Visit(AnyTypeNode n, object o)
        {
            return null;
        }

        public object Visit(NullTypeNode n, object o)
        {
            return null;
        }

        public object Visit(EmptyParameterSequenceNode n, object o)
        {
            return null;
        }

        public object Visit(SingleParameterSequenceNode n, object o)
        {
            n.Parameter.Accept(this, null);
            return null;
        }

        public object Visit(MultipleParameterSequenceNode n, object o)
        {
            n.Parameter.Accept(this, null);
            n.Parameters.Accept(this, null);
            return null;
        }

        public object Visit(MethodParameterNode n, object o)
        {
            _currentMethodST.EnterSymbol(n.Identifier.Value, n);
            return null;
        }

        public object Visit(EmptyArgumentSequenceNode n, object o)
        {
            return null;
        }

        public object Visit(SingleArgumentSequenceNode n, object o)
        {
            return null;
        }

        public object Visit(MultipleArgumentSequenceNode n, object o)
        {
            return null;
        }

        public object Visit(ExpressionArgumentNode n, object o)
        {
            return null;
        }
    }
}
