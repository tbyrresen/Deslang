using Deslang.AST;
using Deslang.AST.ActionNodes;
using Deslang.AST.ArgumentNodes;
using Deslang.AST.DeclaringNodes;
using Deslang.AST.ExpressionNodes;
using Deslang.AST.MemberNodes;
using Deslang.AST.ParameterNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.CodeGeneration;
using Deslang.ContextualAnalysis.Deslang.ContextualAnalysis;
using Deslang.Errors;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;

namespace Deslang.ContextualAnalysis
{
    public class TypeChecker : IVisitor
    {
        private readonly Dictionary<string, ClassSymbolTable> _globalTable;  // global scope level (level 0)
        private readonly ErrorSummary _errorSummary;    
        private ClassSymbolTable _currentClassST;   // scope level 1 (class ST also contains interfaces)
        private MethodSymbolTable _currentMethodST;   // scope level 2
        private Object _currentST;  // reference to the symbol table corresponding to the identifier we are currently looking up 
        private SourceCodePosition _dummyPos;
        private int _loopNestings;

        public TypeChecker(Dictionary<string, ClassSymbolTable> globalTable, ErrorSummary errorSummary)
        {
            _globalTable = globalTable;
            _errorSummary = errorSummary;
            _dummyPos = new SourceCodePosition(0, 0, null);   // dummy source code position for error types found during type checking
            _loopNestings = 0;
        }

        // Performs type checking on the source program as represented by its AST to verify that it satisfies
        // the type and scope rules of the language. 
        // The type checking decorates the AST by linking each occurence of an identifier to its corresponding type
        // as denoted by its declaration. Also links each use of an expression to its corresponding type. 
        // The type checker always keeps a reference to the current symbol table, such that identifier lookups are done correctly.
        // Symbol table lookup is always done from the inner most scope before looking for an identifier in outer scopes.
        // The used symbol tables are initialized prior to the type checking to allow for forward referencing of variables. 
        // Type and scope errors found during the checking are denoted by an error type, such that processing of the remainder
        // of the source code is still viable. 
        // All errors found are added to an error summary. 
        public void Check(List<ProgramNode> ASTRoots)
        {
            foreach (ProgramNode AST in ASTRoots)
            {
                AST.Accept(this, null);
            }
        }

        public object Visit(ProgramNode n, object o)
        {
            n.Declarings.Accept(this, null);
            return null;
        }

        // declaring AST nodes

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
            // no need for checking
            return null;
        }

        public object Visit(InterfaceMethodDeclaringNode n, object o)
        {
            // no need for checking
            return null;
        }

        public object Visit(ClassDeclaringNode n, object o)
        {
            _currentClassST = _globalTable[n.Identifier.Value];
            _currentST = _currentClassST;
            n.Implements.Accept(this, null);
            n.Variables.Accept(this, null);
            n.Constructor.Accept(this, n.Identifier); // passes the name of the class to the constructor to check that they match
            n.Methods.Accept(this, null);
            return null;
        }

        public object Visit(ImplementDeclaringNode n, object o)
        {
            if (!_globalTable.ContainsKey(n.Identifier.Value))
            {
                AddNotDeclaredError(n.Identifier);
            }
            else if (!(_globalTable[n.Identifier.Value].ClassDeclaring is InterfaceDeclaringNode))
            {
                // the entity exists but is not of type interface
                SemanticError($"{n.Identifier.Value} is not an interface", n.Identifier.SourcePosition);
            }
            else
            {
                // checks that the interface is correctly implemented by the parent class
                CheckInterfaceImplemented(((InterfaceDeclaringNode)_globalTable[n.Identifier.Value].ClassDeclaring).InterfaceMethods, n);
            }
            return null;
        }

        private void AddNotDeclaredError(IdentifierNode ident)
        {
            SemanticError($"{ident.Value} is not declared", ident.SourcePosition);
        }

        // checks that the methods of an interface are all correctly implemented by a class
        // uses the provided implentedDeclaringNode to correctly emit error messages 
        private void CheckInterfaceImplemented(DeclaringSequenceNode interfaceMethods, ImplementDeclaringNode n)
        {
            if (interfaceMethods is MultipleDeclaringSequenceNode)
            {
                MultipleDeclaringSequenceNode methods = (MultipleDeclaringSequenceNode)interfaceMethods;
                InterfaceMethodDeclaringNode nextMethod = (InterfaceMethodDeclaringNode)methods.Declaring;
                DeclaringNode theImplementation = _currentClassST.RetrieveSymbol(nextMethod.Identifier.Value);
                if (theImplementation is null || !(theImplementation is MethodDeclaringNode))
                {
                    AddClassDoesNotImplementError(nextMethod, n);
                }
                else 
                {
                    MethodDeclaringNode theMethodImpl = (MethodDeclaringNode)theImplementation;
                    if (!theMethodImpl.Type.Equals(nextMethod.Type) || !ParametersAreEqual(theMethodImpl.Parameters, nextMethod.Parameters))
                    {
                        AddClassDoesNotImplementError(nextMethod, n);
                    }
                }
                CheckInterfaceImplemented(methods.Declarings, n);  // recursively call the method on the remaining interface methods 
            }
            else if (interfaceMethods is SingleDeclaringSequenceNode)
            {
                SingleDeclaringSequenceNode method = (SingleDeclaringSequenceNode)interfaceMethods;
                InterfaceMethodDeclaringNode nextMethod = (InterfaceMethodDeclaringNode)method.Declaring;
                DeclaringNode theImplementation = _currentClassST.RetrieveSymbol(nextMethod.Identifier.Value);
                if (theImplementation is null || !(theImplementation is MethodDeclaringNode))
                {
                    AddClassDoesNotImplementError(nextMethod, n);
                }
                else
                {
                    MethodDeclaringNode theMethodImpl = (MethodDeclaringNode)theImplementation;
                    if (!theMethodImpl.Type.Equals(nextMethod.Type) || !ParametersAreEqual(theMethodImpl.Parameters, nextMethod.Parameters))
                    {
                        AddClassDoesNotImplementError(nextMethod, n);
                    }
                }
            }
        }

        private void AddClassDoesNotImplementError(InterfaceMethodDeclaringNode interfaceMethod, ImplementDeclaringNode n)
        {
            SemanticError($"Class does not implement the method '{interfaceMethod.Identifier.Value}' required by interface '{n.Identifier.Value}'",
                _currentClassST.ClassDeclaring.SourcePosition);
        }

        // checks that two parameter sequences are identical. Used to check that an interface method is correctly implemented by a class
        private bool ParametersAreEqual(ParameterSequenceNode seq1, ParameterSequenceNode seq2)
        {
            if (seq1 is MultipleParameterSequenceNode)
            {
                if (!(seq2 is MultipleParameterSequenceNode))
                {
                    return false;
                }
                else
                {
                    MultipleParameterSequenceNode first = (MultipleParameterSequenceNode)seq1;
                    MultipleParameterSequenceNode second = (MultipleParameterSequenceNode)seq2;
                    MethodParameterNode firstParam = (MethodParameterNode)first.Parameter;
                    MethodParameterNode secondParam = (MethodParameterNode)second.Parameter;
                    if (!firstParam.Identifier.Value.Equals(secondParam.Identifier.Value) ||
                        !firstParam.Type.Equals(secondParam.Type))
                    {
                        return false;
                    }
                    return ParametersAreEqual(first.Parameters, second.Parameters);
                }
            }
            else if (seq1 is SingleParameterSequenceNode)
            {
                if (!(seq2 is SingleParameterSequenceNode))
                {
                    return false;
                }
                else
                {
                    SingleParameterSequenceNode first = (SingleParameterSequenceNode)seq1;
                    SingleParameterSequenceNode second = (SingleParameterSequenceNode)seq2;
                    MethodParameterNode firstParam = (MethodParameterNode)first.Parameter;
                    MethodParameterNode secondParam = (MethodParameterNode)second.Parameter;
                    if (!firstParam.Identifier.Value.Equals(secondParam.Identifier.Value) ||
                        !firstParam.Type.Equals(secondParam.Type))
                    {
                        return false;
                    }
                }
            }
            return true; 
        }

        // checks if the type of the declaring is valid and if not assigns its type to error type
        public object Visit(VarDeclaringNode n, object o)
        {
            if (n.Type is ClassTypeNode) 
            {
                ClassTypeNode theClass = (ClassTypeNode)n.Type;
                if (!(_globalTable.ContainsKey(theClass.ClassName.Value)))
                {
                    SemanticError($"Cannot declare variable '{n.Identifier.Value}' to undeclared type '{theClass.ClassName.Value}'",
                        n.SourcePosition);
                    n.Type = new ErrorTypeNode(_dummyPos);
                }
            }
            else if (n.Type is ArrayTypeNode)
            {
                TypeNode arrayType = ((ArrayTypeNode)n.Type).Type;
                if (arrayType is ClassTypeNode)
                {
                    ClassTypeNode theClass = (ClassTypeNode)arrayType;
                    if (!(_globalTable.ContainsKey(theClass.ClassName.Value)))
                    {
                        SemanticError($"Cannot declare variable '{n.Identifier.Value}' to array of undeclared type '{theClass.ClassName.Value}'",
                            n.SourcePosition);
                        n.Type = new ErrorTypeNode(_dummyPos);
                    }
                }
            }
            return null;
        }

        // visits an assigned variable declaring and sets is IsInitialized value to true iff the RHS is valid given the LHS
        // also checks whether a assigned var declaring on class levels makes illegal use of other class level variables in the assignment.
        public object Visit(AssignedVarDeclaringNode n, object o)
        {
            ((VarDeclaringNode)n.Identifier.Accept(this, null)).IsInitialized = true; // assume that the assignment is valid
            if (n.Expression is MemberExpressionNode && !CheckIsInitialized(((MemberExpressionNode)n.Expression).Member))
            {
                AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(((MemberExpressionNode)n.Expression).Member),
                    n.Expression.SourcePosition);
                ((VarDeclaringNode)n.Identifier.Accept(this, null)).IsInitialized = false;
            }
            // if the assignment is done at class level, check that it is legal before continuing
            else if (_currentST is ClassSymbolTable && !RHSLegalAssignmentAtClassLevel(n.Expression))
            {
                SemanticError("Illegal class level assignment", n.SourcePosition);
                ((VarDeclaringNode)n.Identifier.Accept(this, null)).IsInitialized = false;
            }
            else
            {               
                TypeNode RHSType = (TypeNode)n.Expression.Accept(this, null);
                if (n.Type is ClassTypeNode && RHSType is ClassTypeNode)
                {                   
                    ClassTypeNode LHSClass = (ClassTypeNode)n.Type;
                    ClassTypeNode RHSClass = (ClassTypeNode)RHSType;
                    if (!ValidClassAssigning(LHSClass, RHSClass))
                    {
                        AddIllegalTypeAssignmentError(n.Type, RHSType, n.SourcePosition);
                        ((VarDeclaringNode)n.Identifier.Accept(this, null)).IsInitialized = false;
                    }
                }
                else if (!n.Type.Equals(RHSType))
                {
                    AddIllegalTypeAssignmentError(n.Type, RHSType, n.SourcePosition);
                    ((VarDeclaringNode)n.Identifier.Accept(this, null)).IsInitialized = false;
                }
            }
            return null;         
        }

        // helper method to determine if the RHS of an assignment is legal on class level. Note that since Deslang contains no notion of static members
        // no references other than to value types can be legal since the class may not be initialized at this point.
        // the only exception are stdlib methods which can always be called as these are implemented in the backend as static methods
        private bool RHSLegalAssignmentAtClassLevel(ExpressionNode RHSExpr)
        {
            if (RHSExpr is MemberExpressionNode)
            {
                MemberNode theMember = ((MemberExpressionNode)RHSExpr).Member;
                if (!IsStdlibCall(theMember))
                {
                    SemanticError($"The member '{GetBaseNameOfMemberNode(theMember)}' cannot be referred at class level before the instance has been initialized." +
                        $" Consider moving the assignment to a method or the constructor", theMember.SourcePosition);
                    return false;
                }
                CalledMemberNode calledMember = (CalledMemberNode)theMember;
                return ArgumentsLegalAtClassLegal(calledMember.Call.Arguments); // if the reference is legal, check that any arguments are also legal at class level
            }
            else if (RHSExpr is BinaryExpressionNode)
            {
                ExpressionNode child1 = ((BinaryExpressionNode)RHSExpr).Child1;
                ExpressionNode child2 = ((BinaryExpressionNode)RHSExpr).Child2;
                return RHSLegalAssignmentAtClassLevel(child1) && RHSLegalAssignmentAtClassLevel(child2);
            }
            else if (RHSExpr is UnaryExpressionNode)
            {
                ExpressionNode child = ((UnaryExpressionNode)RHSExpr).Child;
                return RHSLegalAssignmentAtClassLevel(child);
            }
            return true;
        }

        // determines if a member node is a reference to a stdlib method
        private bool IsStdlibCall(MemberNode n)
        {
            if (!(n is CalledMemberNode))
            {
                return false;
            }
            CalledMemberNode theMember = (CalledMemberNode)n;
            SimpleMemberNode theParent = ((SimpleMemberNode)theMember.Member);
            return StandardLibrary.IsStdlibMethod(theParent.Identifier.Value);
        }

        // checks that the arguments associated with a call are legal at class level
        private bool ArgumentsLegalAtClassLegal(ArgumentSequenceNode args)
        {
            if (args is MultipleArgumentSequenceNode)
            {
                MultipleArgumentSequenceNode theArgs = (MultipleArgumentSequenceNode)args;
                ExpressionArgumentNode nextArgExpr = (ExpressionArgumentNode)theArgs.Argument;
                return RHSLegalAssignmentAtClassLevel(nextArgExpr.Expression);
            }
            else if (args is SingleArgumentSequenceNode)
            {
                SingleArgumentSequenceNode theArg = (SingleArgumentSequenceNode)args;
                ExpressionArgumentNode argExpr = (ExpressionArgumentNode)theArg.Argument;
                return RHSLegalAssignmentAtClassLevel(argExpr.Expression);
            }
            return true;
        }

        // return the identifier string of the base member of the member node n. 
        // if the member is always a base member (a simple member node) the name of that member is returned
        private string GetBaseNameOfMemberNode(MemberNode n)
        {
            string name = "";
            if (n is SimpleMemberNode)
            {
                name = ((SimpleMemberNode)n).Identifier.Value;
            }
            else if (n is CalledMemberNode)
            {
                name = GetBaseNameOfMemberNode(((CalledMemberNode)n).Member);
            }
            else if (n is IndexedMemberNode)
            {
                name = GetBaseNameOfMemberNode(((IndexedMemberNode)n).Member);
            }
            else if (n is CalledAndIndexedMemberNode)
            {
                name = GetBaseNameOfMemberNode(((CalledAndIndexedMemberNode)n).Member);
            }
            else if (n is DotMemberNode)
            {
                name = GetBaseNameOfMemberNode(((DotMemberNode)n).Parent);
            }
            else if (n is ThisNode && !(((ThisNode)n).Member is null))
            {
                name = GetBaseNameOfMemberNode(((ThisNode)n).Member.Member);
            }
            return name;
        }

        private void AddUseOfUnassignedVarError(string varName, SourceCodePosition pos)
        {
            SemanticError($"Use of unassigned variable '{varName}'", pos);
        }

        private void AddIllegalTypeAssignmentError(TypeNode LHSType, TypeNode RHSType, SourceCodePosition pos)
        {
            SemanticError($"Illegal assignment of type '{RHSType}' to declared type '{LHSType}'", pos);
        }

        // uses the provided object to verify that the constructor has the same name as its parent class
        public object Visit(ConstructorDeclaringNode n, object o)
        {
            _currentMethodST = _currentClassST.RetrieveMethodST(n.Identifier.Value);
            _currentST = _currentMethodST;
            IdentifierNode className = (IdentifierNode)o;
            if (!n.Identifier.Value.Equals(className.Value))
            {
                SemanticError("Constructor name does not match class name", n.Identifier.SourcePosition);
                n.Type = new ErrorTypeNode(_dummyPos);
            }
            n.VarDeclarings.Accept(this, null);
            n.Actions.Accept(this, null);
            return null;
        }

        public object Visit(MethodDeclaringNode n, object o)
        {
            _currentMethodST = _currentClassST.RetrieveMethodST(n.Identifier.Value);
            _currentST = _currentMethodST;       
            n.VarDeclarings.Accept(this, null);
            n.Actions.Accept(this, null);
            n.Return.Accept(this, null);
            return null;
        }

        // action AST nodes 

        // checks that the assigning action is valid by checking the validity of both the LHS and RHS
        // if both LHS and RHS are valid, their combatility is checked to ensure a legal assignment
        // sets the LHS to be initialized if succesful
        public object Visit(AssigningActionNode n, object o)
        {
            TypeNode LHSType = (TypeNode)n.LHS.Accept(this, null);
            TypeNode RHSType = (TypeNode)n.RHS.Accept(this, null);
            if (LHSType is ErrorTypeNode)   // cannot assign to LHS error type
            {
                SemanticError("LHS is invalid and cannot be used in assignment", n.SourcePosition);
            }
            else if (!n.LHS.Member.IsVariable)  // cannot assign to non variable member
            {
                SemanticError($"LHS identifier '{GetBaseNameOfMemberNode(n.LHS.Member)}' of assignment is not a variable",
                    n.SourcePosition);
            }
            // check if the LHS member contains a dot notation. If so, check if the base simple member of the
            // dot member is initialized. If not, the use of dot is not legal as the base member is not initialized. 
            // if the member does not contain a dot notation, the use of it as LHS is always valid
            else if (IsDotted(n.LHS.Member) && !CheckIsInitialized(n.LHS.Member))
            {
                AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(n.LHS.Member), n.LHS.SourcePosition);
            }
            else if (RHSType is ErrorTypeNode)  // cannot assign to RHS error type
            {
                SemanticError("RHS is invalid and cannot be used in assignment", n.RHS.SourcePosition);
            }
            // if the RHS of the assignment is a member expression, check that the RHS member is initialized
            else if (n.RHS is MemberExpressionNode && !CheckIsInitialized(((MemberExpressionNode)n.RHS).Member))
            {
                AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(((MemberExpressionNode)n.RHS).Member), n.RHS.SourcePosition);
            }
            // if both LHS and RHS are class types, check that the assignment is valid according to their types 
            // this also handles interface types
            else if (LHSType is ClassTypeNode && RHSType is ClassTypeNode)
            {
                ClassTypeNode LHSClass = (ClassTypeNode)LHSType;
                ClassTypeNode RHSClass = (ClassTypeNode)RHSType;
                if (!ValidClassAssigning(LHSClass, RHSClass))
                {
                    AddIllegalTypeAssignmentError(LHSType, RHSType, n.SourcePosition);
                }
                else
                {
                    SetInitialized(n.LHS.Member);
                }
            }
            else
            {
                if (!RHSType.Equals(LHSType))
                {
                    AddIllegalTypeAssignmentError(LHSType, RHSType, n.SourcePosition);
                }
                else
                {
                    SetInitialized(n.LHS.Member);
                }
            }
            return null;
        }

        // helper method to check that the RHS class type can be assigned to the LHS class side. 
        private bool ValidClassAssigning(ClassTypeNode LHS, ClassTypeNode RHS)
        {
            DeclaringNode LHSDeclaring = _globalTable[LHS.ClassName.Value].ClassDeclaring;
            DeclaringNode RHSDeclaring = _globalTable[RHS.ClassName.Value].ClassDeclaring;
            if (LHSDeclaring is InterfaceDeclaringNode && RHSDeclaring is InterfaceDeclaringNode) 
            {
                return LHS.Equals(RHS);
            }
            else if (LHSDeclaring is ClassDeclaringNode && RHSDeclaring is ClassDeclaringNode)
            {
                return LHS.Equals(RHS);
            }
            else if (LHSDeclaring is InterfaceDeclaringNode && RHSDeclaring is ClassDeclaringNode)
            {
                // if the LHS is an interface and the RHS a class, check that the RHS class implements the interface 
                ClassDeclaringNode RHSClassDecl = (ClassDeclaringNode)RHSDeclaring;
                InterfaceDeclaringNode LHSInterfaceDecl = (InterfaceDeclaringNode)LHSDeclaring;
                return ClassImplementsInterface(RHSClassDecl.Identifier.Value, LHSInterfaceDecl.Identifier.Value);
            }
            // the last case is an assigment of a RHS interface type to a LHS side class which is always invalid
            return false;
        }

        // helper method to check that the class with name className implements the interface with name interfaceName
        private bool ClassImplementsInterface(string className, string interfaceName)
        {
            ClassDeclaringNode classDecl = (ClassDeclaringNode)_globalTable[className].ClassDeclaring;
            return InterfaceInImplementsList(classDecl.Implements, interfaceName);
        }

        // helper method to check in the interface interfaceName is in the implement sequence list of a class
        private bool InterfaceInImplementsList(DeclaringSequenceNode implements, string interfaceName)
        {
            if (implements is MultipleDeclaringSequenceNode)
            {
                MultipleDeclaringSequenceNode implementSeq = (MultipleDeclaringSequenceNode)implements;
                ImplementDeclaringNode nextImplement = (ImplementDeclaringNode)implementSeq.Declaring;
                if (nextImplement.Identifier.Value.Equals(interfaceName))
                {
                    return true;
                }
                else
                {
                    return InterfaceInImplementsList(implementSeq.Declarings, interfaceName);
                }
            }
            else if (implements is SingleDeclaringSequenceNode)
            {
                SingleDeclaringSequenceNode implementSeq = (SingleDeclaringSequenceNode)implements;
                ImplementDeclaringNode nextImplement = (ImplementDeclaringNode)implementSeq.Declaring;
                if (nextImplement.Identifier.Value.Equals(interfaceName))
                {
                    return true;
                }
            }
            return false;
        }

        // helper method to check whether a member node uses dot notation
        private bool IsDotted(MemberNode n)
        {            
            if (n is SimpleMemberNode)
            {
                return false;
            }
            else if (n is ThisNode)
            {
                return IsDotted(((ThisNode)n).Member.Member);
            }
            else if (n is CalledMemberNode)
            {
                return IsDotted(((CalledMemberNode)n).Member);
            }
            else if (n is IndexedMemberNode)
            {
                return IsDotted(((IndexedMemberNode)n).Member);
            }
            else if (n is CalledAndIndexedMemberNode)
            {
                return IsDotted(((CalledAndIndexedMemberNode)n).Member);
            }
            return true;    // must be a dotted member if we get here 
        }

        // helper function to check if the base member node of any member node is initialized to a value
        // the base member node of any node is always either a SimpleMemberNode or a ThisNode with a child SimpleMemberNode
        // the method is applied recursively until we meet the base member on which we can check the initialized property
        private bool CheckIsInitialized(MemberNode n)
        {          
            bool result = true; // begin by assuming the member node is initialized to a value
            if (n is SimpleMemberNode)
            {
                DeclaringNode declaring = (DeclaringNode)((SimpleMemberNode)n).Identifier.Accept(this, null);
                if (declaring is VarDeclaringNode)
                {
                    result = ((VarDeclaringNode)declaring).IsInitialized;   // set the result to that of the declaration
                }
            }     
            else if (n is ThisNode)
            {
                MemberExpressionNode itsMemberExpr = ((ThisNode)n).Member;
                if (itsMemberExpr is null)  // the 'this' keyword
                {
                    result = true; // the current class is always trivially initialized
                }
                else
                {                          
                    MemberNode member = ((ThisNode)n).Member.Member;
                    return CheckIsInitialized(member); // dotted 'this'. Check that the child member of the 'this' is initialized
                }
            }
            else if (n is CalledMemberNode)
            {
                MemberNode member = ((CalledMemberNode)n).Member;
                return CheckIsInitialized(member);
            }
            else if (n is IndexedMemberNode)
            {
                MemberNode member = ((IndexedMemberNode)n).Member;
                return CheckIsInitialized(member);
            }
            else if (n is CalledAndIndexedMemberNode)
            {
                MemberNode member = ((CalledAndIndexedMemberNode)n).Member;
                return CheckIsInitialized(member);
            }
            else if (n is DotMemberNode)
            {
                MemberNode member = ((DotMemberNode)n).Parent;
                return CheckIsInitialized(member);
            }
            return result;
        }


        // sets the IsInitialized property associated with a simple member node to true. 
        // also sets the simple member node assicaited with a 'this' keyword to true
        private void SetInitialized(MemberNode n)
        {
            
            if (n is SimpleMemberNode)
            {
                DeclaringNode declaring = (DeclaringNode)((SimpleMemberNode)n).Identifier.Accept(this, null);
                if (declaring is VarDeclaringNode)
                {           
                    ((VarDeclaringNode)declaring).IsInitialized = true;
                }        
            }
            else if (n is ThisNode)
            {
                MemberNode member = ((ThisNode)n).Member.Member;
                if (!(member is null) && member is SimpleMemberNode)
                {
                    // if the member is not null, we switch ST scope to class level before assigning its simple member node to true
                    // to satisfy the semantics of the 'this' keyword
                    Object storedST = _currentST;
                    _currentST = _currentClassST;
                    ((VarDeclaringNode)((SimpleMemberNode)member).Identifier.Accept(this, null)).IsInitialized = true;
                    _currentST = storedST;
                }             
            }
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
            TypeNode exprType = (TypeNode)n.Expression.Accept(this, null);
            if (!(exprType is BooleanTypeNode))
            {
                SemanticError("Expression of conditional is not of type boolean", n.Expression.SourcePosition);
            }
            n.Actions.Accept(this, null);
            n.ElseIfs.Accept(this, null);
            n.Else.Accept(this, null);
            return null;
        }

        public object Visit(ElseifActionNode n, object o)
        {
            TypeNode exprType = (TypeNode)n.Expression.Accept(this, null);
            if (!(exprType is BooleanTypeNode))
            {
                SemanticError("Expression of conditional is not of type boolean", n.Expression.SourcePosition);
            }
            n.Actions.Accept(this, null);
            return null;
        }

        public object Visit(ElseActionNode n, object o)
        {
            n.Actions.Accept(this, null);
            return null;
        }

        public object Visit(WhileActionNode n, object o)
        {
            TypeNode exprType = (TypeNode)n.Expression.Accept(this, null);
            _loopNestings++;       
            if (!(exprType is BooleanTypeNode))
            {
                SemanticError("Expression of conditional is not of type boolean", n.Expression.SourcePosition);
            }
            n.Actions.Accept(this, null);
            _loopNestings--;
            return null;
        }

        public object Visit(ForeachActionNode n, object o)
        {
            DeclaringNode iterDeclaring = (DeclaringNode)n.Identifier.Accept(this, null);   // need to be in the correct ST
            TypeNode exprType = (TypeNode)n.Expression.Accept(this, null);
            _loopNestings++;
            if (iterDeclaring is null)
            {
                SemanticError($"Foreach loop variable '{n.Identifier.Value}' is not declared",
                    n.Identifier.SourcePosition);
            }
            else if (iterDeclaring is MethodParameterNode)
            {
                SemanticError($"Foreach loop variable '{n.Identifier.Value}' cannot be a method parameter",
                    n.Identifier.SourcePosition);
            }
            else if (!(iterDeclaring is VarDeclaringNode))
            {
                SemanticError($"Foreach loop variable '{n.Identifier.Value}' is not a variable",
                    n.Identifier.SourcePosition);
            }
            else
            {                
                TypeNode iterType = ((VarDeclaringNode)iterDeclaring).Type;
                if (!(exprType is ArrayTypeNode))
                {
                    SemanticError("Cannot loop over nonarray type", n.Expression.SourcePosition);
                }
                else if (!((ArrayTypeNode)exprType).Type.Equals(iterType))
                {
                    SemanticError($"Type of foreach loop variable '{n.Identifier.Value}' does not match the element type of the array",
                        n.Expression.SourcePosition);
                }
                else
                {
                    // set the IsInitialized property to true if the foreach loop is valid regardless of whether the variable
                    // has been assigned previously or not since the variable will always be assigned during the loop. 
                    ((VarDeclaringNode)iterDeclaring).IsInitialized = true;
                }
            }
            n.Actions.Accept(this, null);
            _loopNestings--;
            return null;
        }

        public object Visit(ThisReferenceAction n, object o)
        {
            return n.MemberReference.Accept(this, null);
        }

        public object Visit(MemberReferenceAction n, object o)
        {
            return n.MemberReference.Accept(this, null);
        }

        // Checks that the return type is equal to the declared type of the current method. The visitor method asssumes
        // that we are checking the return of a method and not an interface method as these cannot contain return statements
        // and therefore should never be checked by this visitor. 
        public object Visit(ReturnActionNode n, object o)
        {
            TypeNode methodReturnType = ((MethodDeclaringNode)_currentMethodST.MethodDeclaring).Type;
            MethodDeclaringNode methodDecl = (MethodDeclaringNode)_currentMethodST.MethodDeclaring;
            if (methodReturnType is VoidTypeNode)
            {
                if (!(n.Expression is null))
                {
                    SemanticError($"Return statement cannot return an expression since the method '{methodDecl.Identifier.Value}' is of type void",
                        n.SourcePosition);
                }
            }
            else
            {
                if (n.Expression is null)
                {
                    SemanticError($"Return statement must return an expression since the method '{methodDecl.Identifier.Value}' is not of type void",
                        n.SourcePosition);
                }
                else
                {
                    TypeNode exprType = (TypeNode)n.Expression.Accept(this, null);
                    if (exprType is MethodTypeNode)
                    {
                        SemanticError("Cannot return method type", n.SourcePosition);
                    }
                    else if (exprType is ClassTypeNode)
                    {
                        if (!(methodReturnType is ClassTypeNode))
                        {
                            SemanticError($"Type of return statement does not match the return type '{methodReturnType}' declared by the method",
                                n.SourcePosition);
                        }
                        // check that the returned class type is valid given the method class return type.
                        // this check also handles the case where the method type is of type interface and the returned expression
                        // is a class that may or may not implement the interface. 
                        else if (!ValidClassAssigning(((ClassTypeNode)methodReturnType), (ClassTypeNode)exprType))
                        {
                            SemanticError($"Type of return statement does not match the return type '{methodReturnType}' declared by the method",
                                n.SourcePosition);
                        }
                    }
                    else 
                    {
                        if (!methodReturnType.Equals(exprType))
                        {
                            SemanticError($"Type of return statement does not match the return type '{methodReturnType}' declared by the method",
                                n.SourcePosition);
                        }
                    }
                }             
            }          
            return null;
        }

        public object Visit(BreakActionNode n, object o)
        {
            if (_loopNestings == 0)
            {
                SemanticError("cannot use 'break' keyword outside looping constructs", n.SourcePosition);
            }
            return null;
        }

        public object Visit(BinaryExpressionNode n, object o)
        {
            TypeNode LHSType = (TypeNode)n.Child1.Accept(this, null);
            TypeNode RHSType = (TypeNode)n.Child2.Accept(this, null);

            // assume that the operation is initally valid. if either of the childdren of the binary expression are member expressions,
            // we check that these are initializd. if not, we set the flag to false, add a semantic error and never perform the operation.
            // if any used members are undeclared, their IsInitialized property defaults to false and the same logic applies.  
            bool validOp = true;
            if (n.Child1 is MemberExpressionNode)
            {
                if (!((MemberExpressionNode)n.Child1).Member.IsInitialized)
                {
                    MemberNode child1Member = ((MemberExpressionNode)n.Child1).Member;
                    AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(child1Member), child1Member.SourcePosition);
                    n.Type = new ErrorTypeNode(_dummyPos);
                    validOp = false;
                }
            }           
            if (n.Child2 is MemberExpressionNode)
            {
                if (!((MemberExpressionNode)n.Child2).Member.IsInitialized)
                {
                    MemberNode child2Member = ((MemberExpressionNode)n.Child2).Member;
                    AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(child2Member), child2Member.SourcePosition);
                    n.Type = new ErrorTypeNode(_dummyPos);
                    validOp = false;
                }
            }       
            if (validOp)
            {
                // if the operation is valid, we perform the operation corresponding to the operator type and set the 
                // type of the expression accordingly. 
                switch (n.Operator.Value)
                {
                    case "+":
                    case "-":
                    case "*":                   
                    case "%":
                    case "/":
                        n.Type = BinaryArithmeticResultType(LHSType, RHSType, n.SourcePosition);
                        break;
                    case "^":                    
                        n.Type = BinaryArithmeticResultTypeExp(LHSType, RHSType, n.SourcePosition);
                        break;
                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                        n.Type = BinaryCompareResultType(LHSType, RHSType, n.SourcePosition);
                        break;
                    case "!=":
                    case "==":
                        n.Type = BinaryEqualityResultType(LHSType, RHSType, n.SourcePosition);
                        break;
                    case "or":
                    case "and":
                        n.Type = BinaryBooleanResultType(LHSType, RHSType, n.SourcePosition);
                        break;
                    default:
                        SemanticError("Unknown operator type", n.Operator.SourcePosition);  // should not happen
                        n.Type = new ErrorTypeNode(_dummyPos);
                        break;
                }
            }        
            return n.Type;
        }

        // computes the resulting type of a binary arithmetic expression and returns it. 
        // does type conversion if necessary.
        // adds a semantic error if the LHS or RHS are invalid
        private TypeNode BinaryArithmeticResultType(TypeNode LHS, TypeNode RHS, SourceCodePosition pos)
        {
            TypeNode resultType = new ErrorTypeNode(_dummyPos);
            if (LHS is IntTypeNode && RHS is IntTypeNode)
            {
                resultType = new IntTypeNode(_dummyPos);
            }
            else if (LHS is IntTypeNode && RHS is RealTypeNode ||
                     LHS is RealTypeNode && RHS is IntTypeNode ||
                     LHS is RealTypeNode && RHS is RealTypeNode)
            {
                resultType = new RealTypeNode(_dummyPos);
            }
            else
            {
                SemanticError($"Incompatible types ({LHS} and {RHS}) of arithmetic operation", pos);
            }
            return resultType;
        }

        // computes the resulting type of a binary arithmetic division expression and returns it. 
        // adds a semantic error if the LHS or RHS are invalid
        private TypeNode BinaryArithmeticResultTypeExp(TypeNode LHS, TypeNode RHS, SourceCodePosition pos)
        {
            TypeNode resultType = new ErrorTypeNode(_dummyPos);
            if (LHS is IntTypeNode && RHS is IntTypeNode  ||
                LHS is IntTypeNode && RHS is RealTypeNode ||
                LHS is RealTypeNode && RHS is IntTypeNode ||
                LHS is RealTypeNode && RHS is RealTypeNode)
            {
                resultType = new RealTypeNode(_dummyPos);
            }
            else
            {
                SemanticError($"Incompatible types ({LHS} and {RHS}) of arithmetic operation", pos);
            }
            return resultType;
        }

        // computes the resulting type of a binary comparison expression and returns it. 
        // adds a semantic error if the LHS or RHS are invalid
        private TypeNode BinaryCompareResultType(TypeNode LHS, TypeNode RHS, SourceCodePosition pos)
        {
            TypeNode resultType = new ErrorTypeNode(_dummyPos);
            if (LHS is IntTypeNode && RHS is IntTypeNode ||
                LHS is IntTypeNode && RHS is RealTypeNode ||
                LHS is RealTypeNode && RHS is IntTypeNode ||
                LHS is RealTypeNode && RHS is RealTypeNode)
            {
                resultType = new BooleanTypeNode(_dummyPos);
            }
            else
            {
                SemanticError($"Incompatible types ({LHS} and {RHS}) of comparison operation", pos);
            }
            return resultType;
        }

        // computes the resulting type of a binary boolean expression and returns it. 
        // adds a semantic error if the LHS or RHS are invalid
        private TypeNode BinaryEqualityResultType(TypeNode LHS, TypeNode RHS, SourceCodePosition pos)
        {
            TypeNode resultType = new ErrorTypeNode(_dummyPos);
            if (LHS is IntTypeNode && RHS is IntTypeNode ||
                LHS is IntTypeNode && RHS is RealTypeNode ||
                LHS is RealTypeNode && RHS is IntTypeNode ||
                LHS is RealTypeNode && RHS is RealTypeNode ||
                LHS is BooleanTypeNode && RHS is BooleanTypeNode ||
                LHS is StringTypeNode && RHS is StringTypeNode)
            {
                resultType = new BooleanTypeNode(_dummyPos);
            }
            else
            {
                SemanticError($"Incompatible types ({LHS} and {RHS}) of equality operation", pos);
            }
            return resultType;
        }

        // computes the resulting type of a binary boolean expression and returns it
        // throws an error if the LHS or RHS is invalid
        private TypeNode BinaryBooleanResultType(TypeNode LHS, TypeNode RHS, SourceCodePosition pos)
        {
            TypeNode resultType = new ErrorTypeNode(_dummyPos);
            if (LHS is BooleanTypeNode && RHS is BooleanTypeNode)
            {
                resultType = new BooleanTypeNode(_dummyPos);
            }
            else
            {
                SemanticError($"Incompatible types ({LHS} and {RHS}) of boolean operation", pos);
            }
            return resultType;
        }

        public object Visit(UnaryExpressionNode n, object o)
        {
            TypeNode operandType = (TypeNode)n.Child.Accept(this, null);

            bool validOp = true;
            if (n.Child is MemberExpressionNode)
            {
                if (!((MemberExpressionNode)n.Child).Member.IsInitialized)
                {
                    MemberNode childMember = ((MemberExpressionNode)n.Child).Member;
                    AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(childMember), childMember.SourcePosition);
                    n.Type = new ErrorTypeNode(_dummyPos);
                    validOp = false;
                }
            }

            if (validOp)
            {
                switch (n.Operator.Value)
                {
                    case "+":
                    case "-":
                        n.Type = UnaryArithmeticResultType(operandType, n.SourcePosition);
                        break;
                    case "not":
                        n.Type = UnaryBooleanResultType(operandType, n.SourcePosition);
                        break;
                    default:
                        SemanticError("Unknown operator type", n.Operator.SourcePosition);  // should not happen
                        n.Type = new ErrorTypeNode(_dummyPos);
                        break;
                }
            }
            return n.Type;
        }

        private TypeNode UnaryArithmeticResultType(TypeNode operand, SourceCodePosition pos)
        {
            TypeNode resultType = new ErrorTypeNode(_dummyPos);
            if (operand is IntTypeNode)
            {
                resultType = new IntTypeNode(_dummyPos);
            }
            else if (operand is RealTypeNode)
            {
                resultType = new RealTypeNode(_dummyPos);
            }
            else
            {
                SemanticError($"Incompatible type ({operand}) of arithmetic operation", pos);
            }
            return resultType;
        }

        private TypeNode UnaryBooleanResultType(TypeNode operand, SourceCodePosition pos)
        {
            TypeNode resultType = new ErrorTypeNode(_dummyPos);
            if (operand is BooleanTypeNode)
            {
                resultType = new BooleanTypeNode(_dummyPos);
            }
            else
            {
                SemanticError($"Incompatible type ({operand}) of boolean operation", pos);
            }
            return resultType;
        }

        public object Visit(CallExpressionNode n, object o)
        {          
            n.Type = new ErrorTypeNode(_dummyPos);
            DeclaringNode declaring = (DeclaringNode)n.Identifier.Accept(this, null);  // needs to be in the correct ST before the call

            // the call will be provided a Tuple of stored symbol tables in case where the called method is in another class but the arguments needs to be evaluated in the current STs. The stored
            // STs are created in the visitor method for called member nodes
            if (o is Tuple<ClassSymbolTable, object>)
            {
                Tuple<ClassSymbolTable, object> storedSymbolTables = (Tuple<ClassSymbolTable, object>)o;
                _currentClassST = storedSymbolTables.Item1;
                _currentST = storedSymbolTables.Item2;
            }
            if (declaring is null)
            {
                SemanticError("Cannot call undeclared identifier", n.Identifier.SourcePosition);
            }
            else if (declaring is MethodDeclaringNode)
            {
                n.Arguments.Accept(this, ((MethodDeclaringNode)declaring).Parameters);
                n.Type = ((MethodDeclaringNode)declaring).Type;
            }
            else if (declaring is InterfaceMethodDeclaringNode)
            {
                n.Arguments.Accept(this, ((InterfaceMethodDeclaringNode)declaring).Parameters);
                n.Type = ((InterfaceMethodDeclaringNode)declaring).Type;
            }
            else
            {
                SemanticError($"{n.Identifier.Value} is not a method identifier", n.Identifier.SourcePosition);
            }         
            return n.Type;
        }

        public object Visit(InstantiationNode n, object o)
        {
            if (!(n.Type is ClassTypeNode || n.Type is ArrayTypeNode))
            {
                SemanticError("Cannot instantiate a non class or non array type", n.Expression.SourcePosition);
            }
            else if (n.Type is ClassTypeNode)
            {
                ClassTypeNode classType = (ClassTypeNode)n.Type;
                if (!_globalTable.ContainsKey(classType.ClassName.Value))
                {
                    SemanticError($"Cannot instantiate undeclared type '{classType.ClassName.Value}'",
                        n.SourcePosition);
                    n.Type = new ErrorTypeNode(_dummyPos);
                }
                else
                {
                    // check that the declaration is not of type interface before calling the constructor
                    if (_globalTable[((ClassTypeNode)n.Type).ClassName.Value].ClassDeclaring is InterfaceDeclaringNode)
                    {
                        SemanticError("Cannot instantiate interface types", n.SourcePosition);
                        n.Type = new ErrorTypeNode(_dummyPos);
                    }
                    else
                    {
                        n.Type = CallConstructor((CallExpressionNode)n.Expression, (ClassTypeNode)n.Type);
                    }         
                }               
            }
            else if (n.Type is ArrayTypeNode)
            {               
                TypeNode arrayType = ((ArrayTypeNode)n.Type).Type;
                if (arrayType is ClassTypeNode)
                {
                    // if the type of the array is a class type, check that it is a declared type
                    ClassTypeNode classType = (ClassTypeNode)arrayType;
                    if (!_globalTable.ContainsKey(((ClassTypeNode)arrayType).ClassName.Value))
                    {
                        SemanticError($"Cannot instantiate array of undeclared type '{classType.ClassName.Value}'", 
                            n.SourcePosition);
                        n.Type = new ErrorTypeNode(_dummyPos);
                    }                   
                }
                TypeNode arraySize = (TypeNode)n.Expression.Accept(this, null);
                if (!(arraySize is IntTypeNode))
                {
                    SemanticError("Arrays can only be instantiated with integer values",
                        n.Expression.SourcePosition);
                    n.Type = new ErrorTypeNode(_dummyPos);
                }
            }
            return n.Type;
        }

        // calls the constructor of a class using the provided call expression.
        // returns the type of the call i.e. the parent class of the constructor
        private TypeNode CallConstructor(CallExpressionNode expr, ClassTypeNode cl)
        {           
            ClassSymbolTable classST = _globalTable[cl.ClassName.Value];
            ConstructorDeclaringNode classConstructor = (ConstructorDeclaringNode)classST.RetrieveSymbol(cl.ClassName.Value);
            TypeNode result = new ErrorTypeNode(_dummyPos);
            if (!(classConstructor is null || classConstructor.Type is ErrorTypeNode))  // if the name of the constructor does not match the class, it will be invalid and denoted error type
            {
                expr.Arguments.Accept(this, classConstructor.Parameters);
                result = classConstructor.Type;
            }         
            return result;
        }

        // all type expressions returns the corresponding type to allow for type checking of expressions
        public object Visit(IntExpressionNode n, object o)
        {
            return new IntTypeNode(_dummyPos);  
        }

        public object Visit(RealExpressionNode n, object o)
        {
            return new RealTypeNode(_dummyPos);
        }

        public object Visit(StringExpressionNode n, object o)
        {
            return new StringTypeNode(_dummyPos);
        }

        public object Visit(NullExpressionNode n, object o)
        {
            return new NullTypeNode(_dummyPos);
        }

        public object Visit(BooleanExpressionNode n, object o)
        {
            return new BooleanTypeNode(_dummyPos);
        }

        public object Visit(MemberExpressionNode n, object o)
        {
            // since a member expression might switch symbol table to perform correct lookup, we store
            // the current symbol table in a temp variable so we can restore it after visiting the node
            Object storedST = _currentST;
            Object visitResult = n.Member.Accept(this, null);
            _currentST = storedST;
            return visitResult;
        }

        // visits the member of an expression node if the use of 'this' is valid
        public object Visit(ThisExpressionNode n, object o)
        {
            if (_currentST is ClassSymbolTable)
            {
                SemanticError("'this' keyword is not valid in the current context", n.SourcePosition);
                return new ErrorTypeNode(_dummyPos);
            }
            return n.Member.Accept(this, null);
        }

        // member AST nodes

        // visits a simple member node and sets its type to that of its declaration. Also sets the IsVariable 
        // property so we can determine if the member is legal in assignments. 
        public object Visit(SimpleMemberNode n, object o)
        {
            n.IsVariable = false;   // begin by assuming a non variable
            n.Type = new ErrorTypeNode(_dummyPos);
            DeclaringNode declaring = (DeclaringNode)n.Identifier.Accept(this, null);
            if (declaring == null)
            {
                AddNotDeclaredError(n.Identifier);
            }
            else
            {               
                if (declaring is VarDeclaringNode)
                {                 
                    n.Type = ((VarDeclaringNode)declaring).Type;
                    n.IsVariable = true;
                    n.IsInitialized = ((VarDeclaringNode)declaring).IsInitialized;
                }
                else if (declaring is MethodParameterNode)
                {
                    n.Type = ((MethodParameterNode)declaring).Type;
                    n.IsVariable = true;
                    // method params should always be initialized since uninitialized arguments are illegal
                    // this rule should be enforced by the argument visitor methods. 
                    n.IsInitialized = true;  
                }
                else if (declaring is MethodDeclaringNode)
                {      
                    // if the member is a method declaration, create a method type node with the type of the declaration
                    // and set the name of the owning class as the parent of the method type node. 
                    n.Type = new MethodTypeNode(
                        ((MethodDeclaringNode)declaring).Type,
                        ((ClassDeclaringNode)_currentClassST.ClassDeclaring).Identifier.Value,
                        declaring.SourcePosition
                    ); 
                    n.IsVariable = false;
                    n.IsInitialized = true; // methods are trivially initialized and so is their return values
                }
                else
                {
                    SemanticError($"{n.Identifier.Value} is not a variable identifier", n.Identifier.SourcePosition);
                }
            }
            return n.Type;
        }

        // visits a called member node and sets its type to that of if its declaration, which is found by visiting its parent member.
        // Also sets the IsVariable property so we can determine if the member is legal in assigments.
        public object Visit(CalledMemberNode n, object o)
        {         
            TypeNode memberType = (TypeNode)n.Member.Accept(this, null);
            n.IsVariable = n.Member.IsVariable; 
            if (!(memberType is MethodTypeNode))
            {
                SemanticError($"Expected method type", n.SourcePosition);
                n.Type = new ErrorTypeNode(n.SourcePosition);
            }
            else 
            {
                // if the member type is a method, we determine if the method is declared in another class than that of the current ST
                // if so, we build a tuple storing the current STs and provide these to the call expressions. This will allow the call to lookup the method in the correct ST
                // while also ensuring that any arguments are evaluated in the current STs
                ClassSymbolTable storedClassST = _currentClassST;
                Object storedCurrST = _currentST;
                DeclaringNode decl = _currentClassST.ClassDeclaring;
                if (decl is ClassDeclaringNode && !((MethodTypeNode)memberType).ParentClassName.Equals(((ClassDeclaringNode)decl).Identifier.Value))
                {
                    SetSTScope(((MethodTypeNode)memberType).ParentClassName);
                }
                else if (decl is InterfaceDeclaringNode && !((MethodTypeNode)memberType).ParentClassName.Equals(((InterfaceDeclaringNode)decl).Identifier.Value))
                {
                    SetSTScope(((MethodTypeNode)memberType).ParentClassName);
                }
                n.Type = (TypeNode)n.Call.Accept(this, new Tuple<ClassSymbolTable, object>(storedClassST, storedCurrST));
                if (!(n.Type is ErrorTypeNode))
                {
                    n.IsInitialized = true; // a method that is declared is trivially initialized and so is its return value
                }

            }
            return n.Type;
        }

        // helper method to switch symbol table to another class
        private void SetSTScope(string className)
        {
            if (_globalTable.ContainsKey(className))
            {
                _currentClassST = _globalTable[className];
                _currentST = _currentClassST;
            }
        }

        // visits a called member node and sets its type to that of if its declaration, which is found by visiting its parent member.
        // Also sets the IsVariable property so we can determine if the member is legal in assigments.
        public object Visit(IndexedMemberNode n, object o)
        {
            TypeNode memberType = (TypeNode)n.Member.Accept(this, null);
            n.IsVariable = n.Member.IsVariable;
            n.IsInitialized = n.Member.IsInitialized; // initialized only if its parent member is initialized 
            n.Type = n.Member.Type;     // error type if its parent member is an error type
            TypeNode indexType = (TypeNode)n.Index.Accept(this, null);
            if (!n.IsInitialized)
            {
                AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(n), n.SourcePosition);
            }
            else if (!(memberType is ErrorTypeNode))
            {
                if (!(memberType is ArrayTypeNode))
                {
                    SemanticError("Expected array type", n.SourcePosition);
                }
                else
                {
                    if (!(indexType is IntTypeNode))
                    {
                        SemanticError("Indexing on arrays can only be done using int values",
                            n.Index.SourcePosition);
                    } 
                    else
                    {
                        n.Type = ((ArrayTypeNode)memberType).Type;
                    }                    
                }
            }
            return n.Type;
        }

        // visits a called and indexed member node and sets its type to that of if its declaration, which is found by visiting its parent member.
        // Also sets the IsVariable property so we can determine if the member is legal in assigments.
        public object Visit(CalledAndIndexedMemberNode n, object o)
        {
            TypeNode memberType = (TypeNode)n.Member.Accept(this, null);   
            n.IsVariable = n.Member.IsVariable;
            n.IsInitialized = n.Member.IsInitialized;  // initialized only if its parent member is initialized 
            n.Type = n.Member.Type;     // error type if its member is an error type
            TypeNode indexType = (TypeNode)n.Index.Accept(this, null);
            if (!n.IsInitialized)
            {
                AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(n), n.SourcePosition);
            }
            else if (!(memberType is ErrorTypeNode))
            {
                if (!(memberType is ArrayTypeNode))
                {
                    SemanticError("Expected array type", n.SourcePosition);
                }
                else
                {
                    if (!(indexType is IntTypeNode))
                    {
                        SemanticError("Indexing on arrays can only be done using int values",
                            n.Index.SourcePosition);
                    }
                    else
                    {
                        n.Type = ((ArrayTypeNode)memberType).Type;
                    }
                }
            }
            return n.Type;
        }

        // visits a 'this' node and returns the parent class if the this has no parent member.
        // else it temporarily switches the ST to that of the member and returns the result of visiting said member. 
        public object Visit(ThisNode n, object o)
        {
            // if the member of the node is null, this is simply a 'this' reference so we return a type of the class of the current class ST
            if (n.Member is null)
            {
                n.IsVariable = false; 
                ClassDeclaringNode theClass = (ClassDeclaringNode)_currentClassST.ClassDeclaring;
                return new ClassTypeNode(theClass.Identifier, theClass.SourcePosition);
            }
            Object storedST = _currentST;
            _currentST = _currentClassST;            
            Object visitResult = n.Member.Accept(this, null); 
            n.IsVariable = n.Member.Member.IsVariable;
            _currentST = storedST;
            return visitResult;
        }

        // visits a member node that uses dot notation to access fields of a class. 
        // sets the type of the dot member node according to its parents type depending on the validity of the parent
        // the parent type is used to set the ST scope for the dot member identifier lookup. 
        public object Visit(DotMemberNode n, object o)
        {     
            n.Type = new ErrorTypeNode(_dummyPos);
            TypeNode parentType = (TypeNode)n.Parent.Accept(this, null);
            if (!(parentType is ClassTypeNode))
            {   
                SemanticError("Cannot use dot notation on a non class type", n.Parent.SourcePosition);
            }
            else if (!CheckIsInitialized(n.Parent))
            {             
                AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(n), n.SourcePosition); 
            }
            else
            {
                // set the current class ST to that of the parent class of the member if it exists
                // else add an error that the parent class doesnt exists and the dot notation is invalid
                if (_globalTable.ContainsKey(((ClassTypeNode)parentType).ClassName.Value))
                {
                    // store a reference to the current ST and current class ST so we can restore them before returning
                    ClassSymbolTable storedClassST = _currentClassST; 
                    Object storedST = _currentST;
                    SetSTScope(((ClassTypeNode)parentType).ClassName.Value);   
                    DeclaringNode declaring = (DeclaringNode)n.Identifier.Accept(this, null);
                    if (declaring == null)
                    {
                        AddNotDeclaredError(n.Identifier);
                    }                
                    else if (declaring is VarDeclaringNode)
                    {                       
                        n.Type = ((VarDeclaringNode)declaring).Type;
                        n.IsVariable = true;
                        n.IsInitialized = n.Parent.IsInitialized;
                    }
                    else if (declaring is MethodDeclaringNode)
                    {
                        // set the type to be a method type with the type of the declaring and with a reference to the parent class
                        n.Type = new MethodTypeNode(
                            ((MethodDeclaringNode)declaring).Type,
                            ((ClassTypeNode)parentType).ClassName.Value,
                            declaring.SourcePosition
                        ); 
                        n.IsVariable = false;   // cannot assign to a method call so we set the is variable property to false 
                    }
                    else if (declaring is InterfaceMethodDeclaringNode) 
                    {
                        // set the type to be a method type with the type of the declaring and with a reference to the parent interface
                        n.Type = new MethodTypeNode(
                            ((InterfaceMethodDeclaringNode)declaring).Type,
                            ((ClassTypeNode)parentType).ClassName.Value,
                            declaring.SourcePosition
                        );
                        n.IsVariable = false;   // cannot assign to a method call so we set the is variable property to false 
                    }
                    _currentClassST = storedClassST;    // restore the current class ST
                    _currentST = storedST;  // restore the current ST
                }
                else
                {
                    SemanticError($"Type {((ClassTypeNode)parentType).ClassName.Value} is undeclared and therefore cannot be used as a class",
                        n.Parent.SourcePosition);
                }     
            }
            return n.Type;
        }

        // terminal AST nodes

        // finds the declaration associated with the identifiernode in the current ST.
        // assumes that the current ST is the correct ST to perform the lookup in. 
        public object Visit(IdentifierNode n, object o)
        {            
            DeclaringNode declaring = null;
            if (_currentST is ClassSymbolTable)
            {
                declaring = ((ClassSymbolTable)_currentST).RetrieveSymbol(n.Value);
                n.Declaring = declaring;
            }
            else if (_currentST is MethodSymbolTable)
            {
                declaring = ((MethodSymbolTable)_currentST).RetrieveSymbol(n.Value);
                n.Declaring = declaring;              
            }       
            return declaring;
        }

        public object Visit(BooleanLiteralNode n, object o)
        {
            return new BooleanTypeNode(_dummyPos);
        }

        public object Visit(IntLiteralNode n, object o)
        {
            return new IntTypeNode(_dummyPos);
        }

        public object Visit(NullLiteralNode n, object o)
        {
            return n;
        }

        public object Visit(OperatorNode n, object o)
        {
            return n; 
        }

        public object Visit(RealLiteralNode n, object o)
        {
            return new RealTypeNode(_dummyPos);
        }

        public object Visit(StringLiteralNode n, object o)
        {
            return new StringTypeNode(_dummyPos);
        }

        public object Visit(ArrayTypeNode n, object o)
        {
            return n;
        }

        public object Visit(BooleanTypeNode n, object o)
        {
            return n;
        }

        public object Visit(ClassTypeNode n, object o)
        {
            return n;
        }

        public object Visit(MethodTypeNode n, object o)
        {
            return n;
        }

        public object Visit(IntTypeNode n, object o)
        {
            return n;
        }

        public object Visit(RealTypeNode n, object o)
        {
            return n;
        }

        public object Visit(StringTypeNode n, object o)
        {
            return n;
        }

        public object Visit(VoidTypeNode n, object o)
        {
            return n;
        }

        public object Visit(ErrorTypeNode n, object o)
        {
            return n;
        }

        public object Visit(NullTypeNode n, object o)
        {
            return n;
        }

        public object Visit(AnyTypeNode n, object o)
        {
            return n;
        }

        public object Visit(EmptyParameterSequenceNode n, object o)
        {
            return n;
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
            throw new NotImplementedException();
        }

        // argument AST nodes

        // compares the provided parameter sequence node to the argument sequence
        public object Visit(EmptyArgumentSequenceNode n, object o)
        {
            ParameterSequenceNode parameters = (ParameterSequenceNode)o;
            if (!(parameters is EmptyParameterSequenceNode))
            {
                SemanticError("Invalid number of arguments provided to call", n.SourcePosition);
            }
            return null;
        }

        public object Visit(SingleArgumentSequenceNode n, object o)
        {
            ParameterSequenceNode parameters = (ParameterSequenceNode)o;
            if (!(parameters is SingleParameterSequenceNode))
            {
                SemanticError("Invalid number of arguments provided to call", n.SourcePosition);
            }
            else
            {
                n.Argument.Accept(this, ((SingleParameterSequenceNode)parameters).Parameter);
            }
            return null;
        }

        public object Visit(MultipleArgumentSequenceNode n, object o)
        {
            ParameterSequenceNode parameters = (ParameterSequenceNode)o;
            if (!(parameters is MultipleParameterSequenceNode))
            {
                SemanticError("Invalid number of arguments provided to call", n.SourcePosition);
            }
            else
            {
                n.Argument.Accept(this, ((MultipleParameterSequenceNode)parameters).Parameter);
                n.Arguments.Accept(this, ((MultipleParameterSequenceNode)parameters).Parameters);
            }
            return null;
        }

        public object Visit(ExpressionArgumentNode n, object o)
        {
            ParameterNode parameter = (ParameterNode)o;
            TypeNode exprType = (TypeNode)n.Expression.Accept(this, null);
            TypeNode paramType = ((MethodParameterNode)parameter).Type;
            if (n.Expression is MemberExpressionNode)
            {
                // cannot use uninitialized variables as arguments
                MemberNode argMember = ((MemberExpressionNode)n.Expression).Member;
                if (!CheckIsInitialized(argMember))
                {
                    AddUseOfUnassignedVarError(GetBaseNameOfMemberNode(argMember), n.SourcePosition);
                }
            }
            if (exprType is MethodTypeNode)
            {
                SemanticError("Methods cannot be used as arguments", n.SourcePosition);
            }
            else if (paramType is ClassTypeNode && exprType is ClassTypeNode)
            {           
                if (!ValidClassAssigning((ClassTypeNode)paramType, (ClassTypeNode)exprType))
                {                  
                    SemanticError($"Provided argument type '{exprType}' cannot be substituted for type '{paramType}'",
                    n.SourcePosition);
                }
            }
            else if (!(exprType.Equals(paramType)))
            {
                SemanticError($"Provided argument type '{exprType}' does not match the expected parameter type '{paramType}'",
                    n.SourcePosition);
            }
            return null;
        }

        private void SemanticError(string msg, SourceCodePosition pos)
        {
            _errorSummary.AddError(new SemanticError(msg, pos));
        }
    }
}
