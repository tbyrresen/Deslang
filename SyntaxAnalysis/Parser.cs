using Deslang.AST;
using Deslang.AST.ActionNodes;
using Deslang.AST.ArgumentNodes;
using Deslang.AST.DeclaringNodes;
using Deslang.AST.ExpressionNodes;
using Deslang.AST.MemberNodes;
using Deslang.AST.ParameterNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.Errors;
using System;

namespace Deslang.SyntaxAnalysis
{
    public class Parser
    {
        private static readonly Token.TokenType[] _typeFirstSet = {
            Token.TokenType.Identifier, Token.TokenType.Real, Token.TokenType.String, Token.TokenType.Boolean, Token.TokenType.Int
        };
        private static readonly Token.TokenType[] _actionStatementFirstSet = {
            Token.TokenType.Identifier, Token.TokenType.This, Token.TokenType.While, Token.TokenType.Foreach, Token.TokenType.If,
            Token.TokenType.Break
        };
        private static readonly Token.TokenType[] _expressionFirstSet = {
            Token.TokenType.Identifier, Token.TokenType.LeftParen, Token.TokenType.IntLiteral, Token.TokenType.RealLiteral,
            Token.TokenType.StringLiteral, Token.TokenType.BooleanLiteral, Token.TokenType.Null, Token.TokenType.AdditiveOperator,
            Token.TokenType.New, Token.TokenType.This, Token.TokenType.Not
        };
        private static readonly Token.TokenType[] _literalFirstSet = {
            Token.TokenType.IntLiteral, Token.TokenType.RealLiteral, Token.TokenType.StringLiteral, Token.TokenType.BooleanLiteral,
            Token.TokenType.Null
        };

        private readonly TokenStream _tokenStream;
        private readonly ErrorSummary _errorSummary;
        private Token _currentToken;

        public Parser(CharStream charStream, ErrorSummary errorSummary, string fileName)
        {
            _tokenStream = new TokenStream(charStream, errorSummary, fileName);
            _errorSummary = errorSummary;
            _currentToken = _tokenStream.Read();
        }

        // Parses the source code based on the grammar of the language. 
        // The result of the parsing is an AST representing the source code. 
        // Each node in the built AST contains the source code position of the corresponding source code,
        // such that future processing of the AST can always refer back to this position. 
        // If any syntax error is found, a SyntaxError exception is thrown.
        public ProgramNode Program()
        {            
            ProgramNode itsAST = null; // set to null in case of syntax error
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            DeclaringSequenceNode itsDeclarings;
            if (_currentToken.Type == Token.TokenType.Interface)
            {               
                itsDeclarings = new SingleDeclaringSequenceNode(InterfaceDeclaring(), itsPos);
                while (_currentToken.Type == Token.TokenType.Interface)
                {
                    itsDeclarings = new MultipleDeclaringSequenceNode(InterfaceDeclaring(), itsDeclarings, itsPos);
                }
                while (_currentToken.Type == Token.TokenType.Class)
                {
                    itsDeclarings = new MultipleDeclaringSequenceNode(ClassDeclaring(), itsDeclarings, itsPos);
                }
                Accept(Token.TokenType.EOF);
                itsAST = new ProgramNode(itsDeclarings, itsPos);
            }
            else if (_currentToken.Type == Token.TokenType.Class)
            {
                itsDeclarings = new SingleDeclaringSequenceNode(ClassDeclaring(), itsPos);
                while (_currentToken.Type == Token.TokenType.Class)
                {
                    itsDeclarings = new MultipleDeclaringSequenceNode(ClassDeclaring(), itsDeclarings, itsPos);
                }
                Accept(Token.TokenType.EOF);
                itsAST = new ProgramNode(itsDeclarings, itsPos);
            }      
            else if (_currentToken.Type == Token.TokenType.EOF)
            {
                Accept(Token.TokenType.EOF);
            }
            else
            {
                Error("Expected interface, class or EOF");
            }
            return itsAST;
        }

        private InterfaceDeclaringNode InterfaceDeclaring()
        {
            InterfaceDeclaringNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;           
            Accept(Token.TokenType.Interface);
            IdentifierNode itsName = new IdentifierNode(_currentToken);
            Accept(Token.TokenType.Identifier);
            Accept(Token.TokenType.LeftBrace);
            DeclaringSequenceNode itsInterfaceMethodDeclarings = InterfaceMethods();
            Accept(Token.TokenType.RightBrace);
            itsAST = new InterfaceDeclaringNode(itsName, itsInterfaceMethodDeclarings, itsPos);
            return itsAST;
        }

        private DeclaringSequenceNode InterfaceMethods()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Method)
            {
                itsAST = ParseInterfaceMethods();
            }
            else
            {
                itsAST = new EmptyDeclaringSequenceNode(itsPos);
            }
            return itsAST;
        }

        private DeclaringSequenceNode ParseInterfaceMethods()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            DeclaringNode itsInterfaceMethod = ParseInterfaceMethod();
            if (_currentToken.Type == Token.TokenType.Method)
            {
                DeclaringSequenceNode itsNextInterfaceMethods = ParseInterfaceMethods();
                itsAST = new MultipleDeclaringSequenceNode(itsInterfaceMethod, itsNextInterfaceMethods, itsPos);
            }
            else
            {
                itsAST = new SingleDeclaringSequenceNode(itsInterfaceMethod, itsPos);
            }
            return itsAST;
        }

        private DeclaringNode ParseInterfaceMethod()
        {
            DeclaringNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.Method);
            TypeNode itsType = ReturnType();
            IdentifierNode itsName = new IdentifierNode(_currentToken, itsType);
            Accept(Token.TokenType.Identifier);
            Accept(Token.TokenType.LeftParen);
            ParameterSequenceNode itsParams = Parameters();
            Accept(Token.TokenType.RightParen);
            Accept(Token.TokenType.Semicolon);
            itsAST = new InterfaceMethodDeclaringNode(itsName, itsType, itsParams, itsPos);
            return itsAST;
        }

        private ClassDeclaringNode ClassDeclaring()
        {
            ClassDeclaringNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.Class);
            IdentifierNode itsName = new IdentifierNode(_currentToken);
            Accept(Token.TokenType.Identifier);
            DeclaringSequenceNode itsImplements = Implements();
            Accept(Token.TokenType.LeftBrace);
            DeclaringSequenceNode itsVars = VariableDeclarings();
            DeclaringNode itsConstructor = ConstructorDeclaring();
            DeclaringSequenceNode itsMethods = MethodDeclarings();
            Accept(Token.TokenType.RightBrace);
            itsAST = new ClassDeclaringNode(itsName, itsImplements, itsVars, itsConstructor, itsMethods, itsPos);
            return itsAST;
        }

        private DeclaringSequenceNode VariableDeclarings()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Var)
            {
                itsAST = ParseVariableDeclarings();
            }
            else
            {
                itsAST = new EmptyDeclaringSequenceNode(itsPos);
            }
            return itsAST;
        }

        private DeclaringSequenceNode ParseVariableDeclarings()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            DeclaringNode itsVar = ParseVariableDeclaring();
            if (_currentToken.Type == Token.TokenType.Var)
            {
                DeclaringSequenceNode itsNextVars = ParseVariableDeclarings();
                itsAST = new MultipleDeclaringSequenceNode(itsVar, itsNextVars, itsPos);
            }
            else
            {
                itsAST = new SingleDeclaringSequenceNode(itsVar, itsPos);
            }
            return itsAST;
        }

        private VarDeclaringNode ParseVariableDeclaring()
        {
            VarDeclaringNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.Var);
            TypeNode itsType = Type();
            IdentifierNode itsName = new IdentifierNode(_currentToken, itsType);
            Accept(Token.TokenType.Identifier);
            itsAST = new VarDeclaringNode(itsName, itsType, itsPos);
            if (_currentToken.Type == Token.TokenType.AssignmentOperator)
            {
                itsAST = new AssignedVarDeclaringNode(itsName, itsType, Assigning(), itsPos);
            }
            Accept(Token.TokenType.Semicolon);
            return itsAST;
        }

        private ConstructorDeclaringNode ConstructorDeclaring()
        {
            ConstructorDeclaringNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.Constructor);
            IdentifierNode itsName = new IdentifierNode(_currentToken);
            TypeNode itsType = new ClassTypeNode(itsName, itsPos); 
            Accept(Token.TokenType.Identifier);
            Accept(Token.TokenType.LeftParen);
            ParameterSequenceNode itsParams = Parameters();
            Accept(Token.TokenType.RightParen);
            Accept(Token.TokenType.LeftBrace);
            DeclaringSequenceNode itsVars = VariableDeclarings();
            ActionSequenceNode itsActions = ActionStatements();
            Accept(Token.TokenType.RightBrace);
            itsAST = new ConstructorDeclaringNode(itsName, itsType, itsParams, itsVars, itsActions, itsPos);
            return itsAST;
        }

        private DeclaringSequenceNode MethodDeclarings()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Method)
            {
                itsAST = ParseMethodDeclarings();
            }
            else
            {
                itsAST = new EmptyDeclaringSequenceNode(itsPos);
            }
            return itsAST;
        }

        private DeclaringSequenceNode ParseMethodDeclarings()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            DeclaringNode itsMethod = ParseMethodDeclaring();
            if (_currentToken.Type == Token.TokenType.Method)
            {
                DeclaringSequenceNode itsNextMethods = ParseMethodDeclarings();
                itsAST = new MultipleDeclaringSequenceNode(itsMethod, itsNextMethods, itsPos);
            }
            else
            {
                itsAST = new SingleDeclaringSequenceNode(itsMethod, itsPos);
            }
            return itsAST;
        }

        private MethodDeclaringNode ParseMethodDeclaring()
        {
            MethodDeclaringNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.Method);
            TypeNode itsType = ReturnType();
            IdentifierNode itsName = new IdentifierNode(_currentToken);
            Accept(Token.TokenType.Identifier);
            Accept(Token.TokenType.LeftParen);
            ParameterSequenceNode itsParams = Parameters();
            Accept(Token.TokenType.RightParen);
            Accept(Token.TokenType.LeftBrace);
            DeclaringSequenceNode itsVars = VariableDeclarings();
            ActionSequenceNode itsActions = ActionStatements();
            ReturnActionNode itsReturn = ReturnStatement();   
            Accept(Token.TokenType.RightBrace);
            itsAST = new MethodDeclaringNode(itsName, itsType, itsParams, itsVars, itsActions, itsReturn, itsPos);
            return itsAST;
        }

        private ActionSequenceNode ActionStatements()
        {
            ActionSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (Array.Exists(_actionStatementFirstSet, e => e == _currentToken.Type))
            {
                itsAST = ParseActionStatements();
            }
            else
            {
                itsAST = new EmptyActionSequenceNode(itsPos);
            }
            return itsAST;
        }

        private ActionSequenceNode ParseActionStatements()
        {
            ActionSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            ActionNode itsAction = ParseActionStatement();
            if (Array.Exists(_actionStatementFirstSet, e => e == _currentToken.Type))
            {
                ActionSequenceNode itsNextActions = ParseActionStatements();
                itsAST = new MultipleActionSequenceNode(itsAction, itsNextActions, itsPos);
            }
            else
            {
                itsAST = new SingleActionSequenceNode(itsAction, itsPos);
            }
            return itsAST;
        }

        private ActionNode ParseActionStatement()
        {
            ActionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Identifier)
            {
                MemberExpressionNode itsMemberExpr = MemberReference();
                if (_currentToken.Type == Token.TokenType.AssignmentOperator)
                {
                    itsAST = new AssigningActionNode(itsMemberExpr, Assigning(), itsPos);
                }
                else
                {
                    itsAST = new MemberReferenceAction(itsMemberExpr, itsPos);
                }
                Accept(Token.TokenType.Semicolon);
            }
            else if (_currentToken.Type == Token.TokenType.This)
            {
                ThisExpressionNode itsMemberExpr = ThisReference();
                if (_currentToken.Type == Token.TokenType.AssignmentOperator)
                {
                    itsAST = new AssigningActionNode(itsMemberExpr, Assigning(), itsPos);
                }
                else
                {
                    itsAST = new ThisReferenceAction(itsMemberExpr, itsPos);
                }
                Accept(Token.TokenType.Semicolon);
            }           
            else
            {
                itsAST = ControlStatement();
            }
            return itsAST;
        }

        private ActionNode ControlStatement()
        {
            ActionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.While || _currentToken.Type == Token.TokenType.Foreach)
            {
                itsAST = IterationStatement();
            }
            else if (_currentToken.Type == Token.TokenType.Break)
            {
                Accept(Token.TokenType.Break);
                itsAST = new BreakActionNode(itsPos);
                Accept(Token.TokenType.Semicolon);
            }
            else
            {
                itsAST = SelectionStatement();
            }
            return itsAST;
        }

        private ActionNode IterationStatement()
        {
            ActionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.While)
            {
                Accept(Token.TokenType.While);
                Accept(Token.TokenType.LeftParen);
                ExpressionNode itsExpr = Expression();
                Accept(Token.TokenType.RightParen);
                Accept(Token.TokenType.LeftBrace);
                ActionSequenceNode itsActions = ActionStatements();
                Accept(Token.TokenType.RightBrace);
                itsAST = new WhileActionNode(itsExpr, itsActions, itsPos);               
            }
            else
            {
                Accept(Token.TokenType.Foreach);
                Accept(Token.TokenType.LeftParen);
                VarDeclaringNode itsDecl;
                Accept(Token.TokenType.Var);
                TypeNode itsType = Type();
                IdentifierNode itsName = new IdentifierNode(_currentToken, itsType);
                Accept(Token.TokenType.Identifier);
                itsDecl = new VarDeclaringNode(itsName, itsType, itsPos);
                Accept(Token.TokenType.In);
                ExpressionNode itsExpr = Expression();
                Accept(Token.TokenType.RightParen);
                Accept(Token.TokenType.LeftBrace);
                ActionSequenceNode itsActions = ActionStatements();
                Accept(Token.TokenType.RightBrace);
                itsAST = new ForeachActionNode(itsDecl, itsName, itsExpr, itsActions, itsPos);
            }
            return itsAST;
        }

        private ActionNode SelectionStatement()
        {
            ActionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.If);
            Accept(Token.TokenType.LeftParen);
            ExpressionNode itsExpr = Expression();
            Accept(Token.TokenType.RightParen);
            Accept(Token.TokenType.LeftBrace);
            ActionSequenceNode itsActions = ActionStatements();
            Accept(Token.TokenType.RightBrace);
            ActionSequenceNode itsElseifs = ElseIfStatements();
            ActionNode itsElse = ElseStatement();
            itsAST = new IfActionNode(itsExpr, itsActions, itsElseifs, itsElse, itsPos);
            return itsAST;
        }

        private ActionSequenceNode ElseIfStatements()
        {
            ActionSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Elseif)
            {
                itsAST = ParseElseifStatements();
            }
            else
            {
                itsAST = new EmptyActionSequenceNode(itsPos);
            }
            return itsAST;
        }

        private ActionSequenceNode ParseElseifStatements()
        {
            ActionSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            ActionNode itsAction = ParseElseifStatement();
            if (_currentToken.Type == Token.TokenType.Elseif)
            {
                ActionSequenceNode itsNextActions = ParseElseifStatements();
                itsAST = new MultipleActionSequenceNode(itsAction, itsNextActions, itsPos);
            }
            else
            {
                itsAST = new SingleActionSequenceNode(itsAction, itsPos);
            }
            return itsAST;
        }

        private ActionNode ParseElseifStatement()
        {
            ActionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.Elseif);
            Accept(Token.TokenType.LeftParen);
            ExpressionNode itsExpr = Expression();
            Accept(Token.TokenType.RightParen);
            Accept(Token.TokenType.LeftBrace);
            ActionSequenceNode itsActions = ActionStatements();
            Accept(Token.TokenType.RightBrace);
            itsAST = new ElseifActionNode(itsExpr, itsActions, itsPos);
            return itsAST;
        }

        private ActionNode ElseStatement()
        {
            ActionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Else)
            {
                Accept(Token.TokenType.Else);
                Accept(Token.TokenType.LeftBrace);
                ActionSequenceNode itsActions = ActionStatements();
                Accept(Token.TokenType.RightBrace);
                itsAST = new ElseActionNode(itsActions, itsPos);
            }
            else
            {
                itsAST = new EmptyActionNode(itsPos);
            }                                 
            return itsAST;
        }

        private ReturnActionNode ReturnStatement()
        {
            ReturnActionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.Return);
            if (_currentToken.Type == Token.TokenType.Semicolon)
            {
                itsAST = new ReturnActionNode(null, itsPos);   // null indicates void return type
            }
            else
            {
                itsAST = new ReturnActionNode(Expression(), itsPos);
            }        
            Accept(Token.TokenType.Semicolon);
            return itsAST;
        }

        private ExpressionNode Assigning()
        {
            ExpressionNode itsAST;
            Accept(Token.TokenType.AssignmentOperator);
            itsAST = Expression();
            return itsAST;
        }

        private ExpressionNode Expression()
        {
            ExpressionNode itsAST = null;  // in case there is a syntactic error
            if (Array.Exists(_expressionFirstSet, e => e == _currentToken.Type))
            {
                itsAST = BooleanOr();
            }
            else
            {
                Error("Expected identifier, leftParen, intLiteral, realLiteral, stringLiteral, booleanLiteral, null, additiveOperator, new, this or not");
            }
            return itsAST;
        }

        private ExpressionNode BooleanOr()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            itsAST = BooleanAnd();
            while (_currentToken.Type == Token.TokenType.Or)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.Or);             
                ExpressionNode otherChild = BooleanAnd();
                itsAST = new BinaryExpressionNode(itsAST, op, otherChild, itsPos);
            }
            return itsAST;
        }

        private ExpressionNode BooleanAnd()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            itsAST = BooleanNot();
            while (_currentToken.Type == Token.TokenType.And)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.And);             
                ExpressionNode otherChild = BooleanNot();
                itsAST = new BinaryExpressionNode(itsAST, op, otherChild, itsPos);
            }
            return itsAST;
        }

        private ExpressionNode BooleanNot()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Not)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.Not);             
                ExpressionNode child = BooleanNot();
                itsAST = new UnaryExpressionNode(op, child, itsPos);
            }
            else
            {
                itsAST = Compare();
            }
            return itsAST;
        }

        private ExpressionNode Compare()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            itsAST = Additive();
            while (_currentToken.Type == Token.TokenType.CompareOperator)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.CompareOperator);
                ExpressionNode otherChild = Additive();
                itsAST = new BinaryExpressionNode(itsAST, op, otherChild, itsPos);
            }
            return itsAST;
        }

        private ExpressionNode Additive()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            itsAST = Multiplicative();
            while (_currentToken.Type == Token.TokenType.AdditiveOperator)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.AdditiveOperator);               
                ExpressionNode otherChild = Multiplicative();
                itsAST = new BinaryExpressionNode(itsAST, op, otherChild, itsPos);
            }
            return itsAST;
        }

        private ExpressionNode Multiplicative()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            itsAST = Unary();
            while (_currentToken.Type == Token.TokenType.MultiplicativeOperator)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.MultiplicativeOperator);             
                ExpressionNode otherChild = Unary();
                itsAST = new BinaryExpressionNode(itsAST, op, otherChild, itsPos);
            }
            return itsAST;
        }

        private ExpressionNode Unary()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.AdditiveOperator)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.AdditiveOperator);              
                ExpressionNode child = Unary();
                itsAST = new UnaryExpressionNode(op, child, itsPos);
            }
            else
            {
                itsAST = Exponent();
            }
            return itsAST;
        }

        private ExpressionNode Exponent()
        {
            ExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            itsAST = Primary();
            if (_currentToken.Type == Token.TokenType.ExponentOperator)
            {
                OperatorNode op = new OperatorNode(_currentToken.Value, _currentToken.SourcePosition);
                Accept(Token.TokenType.ExponentOperator);              
                ExpressionNode otherChild = Exponent();
                itsAST = new BinaryExpressionNode(itsAST, op, otherChild, itsPos);
            }
            return itsAST;
        }

        private ExpressionNode Primary()
        {
            ExpressionNode itsAST = null; // in case there is a syntactic error
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.LeftParen)
            {
                Accept(Token.TokenType.LeftParen);
                itsAST = Expression();
                Accept(Token.TokenType.RightParen);
            }
            else if (Array.Exists(_literalFirstSet, e => e == _currentToken.Type))
            {
                switch (_currentToken.Type)
                {
                    case Token.TokenType.IntLiteral:
                        IntLiteralNode intLiteral = new IntLiteralNode(_currentToken.Value, _currentToken.SourcePosition);
                        Accept(Token.TokenType.IntLiteral);
                        itsAST = new IntExpressionNode(intLiteral, itsPos);
                        break;
                    case Token.TokenType.RealLiteral:
                        RealLiteralNode realLiteral = new RealLiteralNode(_currentToken.Value, _currentToken.SourcePosition);
                        Accept(Token.TokenType.RealLiteral);
                        itsAST = new RealExpressionNode(realLiteral, itsPos);
                        break;
                    case Token.TokenType.StringLiteral:
                        StringLiteralNode stringLiteral = new StringLiteralNode(_currentToken.Value, _currentToken.SourcePosition);
                        Accept(Token.TokenType.StringLiteral);
                        itsAST = new StringExpressionNode(stringLiteral, itsPos);
                        break;
                    case Token.TokenType.BooleanLiteral:
                        BooleanLiteralNode booleanLiteral = new BooleanLiteralNode(_currentToken.Value, _currentToken.SourcePosition);
                        Accept(Token.TokenType.BooleanLiteral);
                        itsAST = new BooleanExpressionNode(booleanLiteral, itsPos);
                        break;
                    case Token.TokenType.Null:
                        NullLiteralNode nullLiteral = new NullLiteralNode(_currentToken.Value, _currentToken.SourcePosition);
                        Accept(Token.TokenType.Null);
                        itsAST = new NullExpressionNode(nullLiteral, itsPos);
                        break;
                    default:
                        itsAST = null;  // should not happen
                        break;
                }
            }
            else if (_currentToken.Type == Token.TokenType.Identifier)
            {
                itsAST = MemberReference();
            }
            else if (_currentToken.Type == Token.TokenType.This)
            {
                itsAST = ThisReference();
            }
            else if (_currentToken.Type == Token.TokenType.New)
            {
                itsAST = Instantiation();
            }
            else
            {
                Error("The token is invalid in the current context");
            }
            return itsAST;
        }

        private ExpressionNode Instantiation()
        {
            ExpressionNode itsAST = null;  // in case there is a syntactic error
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.New);
            if (Array.Exists(_typeFirstSet, e => e == _currentToken.Type && e != Token.TokenType.Identifier))
            {
                TypeNode itsType = ValueType();
                ExpressionNode itsSize = ArrayExpression();
                ArrayTypeNode itsArray = new ArrayTypeNode(itsType, itsSize, itsPos); 
                itsAST = new InstantiationNode(itsArray, itsSize, itsPos);
            }
            else
            {
                IdentifierNode itsName = new IdentifierNode(_currentToken);
                Accept(Token.TokenType.Identifier);
                ClassTypeNode itsType = new ClassTypeNode(itsName, _currentToken.SourcePosition);
                if (_currentToken.Type == Token.TokenType.LeftBracket)
                {
                    ExpressionNode itsSize = ArrayExpression();
                    ArrayTypeNode itsArray = new ArrayTypeNode(itsType, itsSize, itsPos);
                    itsAST = new InstantiationNode(itsArray, itsSize, itsPos);
                }
                else if (_currentToken.Type == Token.TokenType.LeftParen)
                {
                    itsAST = new InstantiationNode(itsType, Call(itsName), itsPos);
                }
                else
                {
                    Error("Expected leftBracket or leftParen");
                }
            }
            return itsAST;
        }

        private ThisExpressionNode ThisReference()
        {
            ThisExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            ThisNode itsThis = new ThisNode(itsPos);
            itsAST = new ThisExpressionNode(itsThis, itsPos);
            Accept(Token.TokenType.This);
            if (_currentToken.Type == Token.TokenType.Dot)
            {
                Accept(Token.TokenType.Dot);
                itsThis = new ThisNode(MemberReference(), itsPos);
                itsAST = new ThisExpressionNode(itsThis, itsPos);
            }
            return itsAST;
        }

        private MemberExpressionNode MemberReference()
        {
            MemberExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            IdentifierNode itsName = new IdentifierNode(_currentToken);
            Accept(Token.TokenType.Identifier);
            MemberNode itsMember = new SimpleMemberNode(itsName, itsPos);

            if (_currentToken.Type == Token.TokenType.LeftParen)
            {
                CallExpressionNode itsCall = Call(itsName);     // add the associated identifier to the call
                itsMember = new CalledMemberNode(itsMember, itsCall, itsPos);
                if (_currentToken.Type == Token.TokenType.LeftBracket)
                {
                    ExpressionNode itsIndex = ArrayExpression();
                    itsMember = new CalledAndIndexedMemberNode(itsMember, itsCall, itsIndex, itsPos);
                }
            }
            else if (_currentToken.Type == Token.TokenType.LeftBracket)
            {
                ExpressionNode itsIndex = ArrayExpression();
                itsMember = new IndexedMemberNode(itsMember, itsIndex, itsPos);
            }

            while (_currentToken.Type == Token.TokenType.Dot)
            {
                Accept(Token.TokenType.Dot);
                itsName = new IdentifierNode(_currentToken);              
                Accept(Token.TokenType.Identifier);
                itsMember = new DotMemberNode(itsMember, itsName, itsPos); 

                if (_currentToken.Type == Token.TokenType.LeftParen)
                {
                    CallExpressionNode itsCall = Call(itsName);     // add the associated identifier to the call
                    itsMember = new CalledMemberNode(itsMember, itsCall, itsPos);
                    if (_currentToken.Type == Token.TokenType.LeftBracket)
                    {
                        ExpressionNode itsIndex = ArrayExpression();
                        itsMember = new CalledAndIndexedMemberNode(itsMember, itsCall, itsIndex, itsPos);
                    }
                }
                else if (_currentToken.Type == Token.TokenType.LeftBracket)
                {
                    ExpressionNode itsIndex = ArrayExpression();
                    itsMember = new IndexedMemberNode(itsMember, itsIndex, itsPos);
                }
            }
            itsAST = new MemberExpressionNode(itsMember, itsPos);
            return itsAST;
        }

        private ExpressionNode ArrayExpression()
        {
            ExpressionNode itsAST;
            Accept(Token.TokenType.LeftBracket);
            itsAST = Expression();
            Accept(Token.TokenType.RightBracket);
            return itsAST;
        }

        private CallExpressionNode Call(IdentifierNode ident)
        {
            CallExpressionNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.LeftParen);
            ArgumentSequenceNode itsArgs = Arguments();
            Accept(Token.TokenType.RightParen);
            itsAST = new CallExpressionNode(ident, itsArgs, itsPos);
            return itsAST;
        }

        private TypeNode ReturnType()
        {
            TypeNode itsAST = null;  // in case there is a syntactic error
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (Array.Exists(_typeFirstSet, e => e == _currentToken.Type))
            {
                itsAST = Type();
            }
            else if (_currentToken.Type == Token.TokenType.Void)
            {
                itsAST = new VoidTypeNode(itsPos);
                Accept(Token.TokenType.Void);
            }
            else
            {
                Error("Expected int, real, string, boolean, identifier, void or array type");
            }
            return itsAST;
        }

        private DeclaringSequenceNode Implements()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Implements)
            {
                Accept(Token.TokenType.Implements);
                itsAST = ParseImplements();
            }
            else
            {
                itsAST = new EmptyDeclaringSequenceNode(itsPos);
            }
            return itsAST;
        }

        private DeclaringSequenceNode ParseImplements()
        {
            DeclaringSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            DeclaringNode itsImplement = ParseImplement();
            if (_currentToken.Type == Token.TokenType.Comma)
            {
                Accept(Token.TokenType.Comma);
                DeclaringSequenceNode itsNextImplements = ParseImplements();
                itsAST = new MultipleDeclaringSequenceNode(itsImplement, itsNextImplements, itsPos);
            }
            else
            {
                itsAST = new SingleDeclaringSequenceNode(itsImplement, itsPos);
            }
            return itsAST;
        }

        private DeclaringNode ParseImplement()
        {
            DeclaringNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            IdentifierNode interfaceName = new IdentifierNode(_currentToken);
            Accept(Token.TokenType.Identifier);
            itsAST = new ImplementDeclaringNode(interfaceName, itsPos);        
            return itsAST;
        }

        private ParameterSequenceNode Parameters()
        {
            ParameterSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (Array.Exists(_typeFirstSet, e => e == _currentToken.Type))
            {
                itsAST = ParseParameters();
            }
            else
            {
                itsAST = new EmptyParameterSequenceNode(itsPos);
            }
            return itsAST;
        }

        private ParameterSequenceNode ParseParameters()
        {
            ParameterSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            ParameterNode itsParam = ParseParameter();
            if (_currentToken.Type == Token.TokenType.Comma)
            {
                Accept(Token.TokenType.Comma);
                ParameterSequenceNode nextParams = ParseParameters();
                itsAST = new MultipleParameterSequenceNode(itsParam, nextParams, itsPos);
            }
            else
            {
                itsAST = new SingleParameterSequenceNode(itsParam, itsPos);
            }
            return itsAST;
        }

        private ParameterNode ParseParameter()
        {
            ParameterNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            TypeNode itsType = Type();
            IdentifierNode itsName = new IdentifierNode(_currentToken);
            Accept(Token.TokenType.Identifier);
            itsAST = new MethodParameterNode(itsType, itsName, itsPos);
            return itsAST;
        }

        private ArgumentSequenceNode Arguments()
        {
            ArgumentSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (Array.Exists(_expressionFirstSet, e => e == _currentToken.Type))
            {
                itsAST = ParseArguments();
            }
            else
            {
                itsAST = new EmptyArgumentSequenceNode(itsPos);
            }
            return itsAST;
        }

        private ArgumentSequenceNode ParseArguments()
        {
            ArgumentSequenceNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            ExpressionArgumentNode itsArg = ParseArgument();
            if (_currentToken.Type == Token.TokenType.Comma)
            {
                Accept(Token.TokenType.Comma);
                ArgumentSequenceNode itsNextArgs = ParseArguments();
                itsAST = new MultipleArgumentSequenceNode(itsArg, itsNextArgs, itsPos);
            }
            else
            {
                itsAST = new SingleArgumentSequenceNode(itsArg, itsPos);
            }
            return itsAST;
        }

        private ExpressionArgumentNode ParseArgument()
        {
            ExpressionArgumentNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            itsAST = new ExpressionArgumentNode(Expression(), itsPos);
            return itsAST;
        }

        private TypeNode Type()
        {
            TypeNode itsAST = null;  // in case there is a syntactic error
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (Array.Exists(_typeFirstSet, e => e == _currentToken.Type && e != Token.TokenType.Identifier))
            {
                itsAST = ValueType();
                if (_currentToken.Type == Token.TokenType.LeftBracket)
                {
                    itsAST = ArrayType(itsAST);
                }
            }
            else if (_currentToken.Type == Token.TokenType.Identifier)
            {
                IdentifierNode itsName = new IdentifierNode(_currentToken);
                itsAST = new ClassTypeNode(itsName, itsPos);
                Accept(Token.TokenType.Identifier);
                if (_currentToken.Type == Token.TokenType.LeftBracket)
                {
                    itsAST = ArrayType(itsAST);
                }
            }
            else
            {
                Error("Expected int, real, string, boolean, identifier, or array type");
            }
            return itsAST;
        }

        private TypeNode ValueType()
        {
            TypeNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            if (_currentToken.Type == Token.TokenType.Int)
            {
                itsAST = new IntTypeNode(itsPos);
                Accept(Token.TokenType.Int);
            }
            else if (_currentToken.Type == Token.TokenType.Real)
            {
                itsAST = new RealTypeNode(itsPos);
                Accept(Token.TokenType.Real);
            }
            else if (_currentToken.Type == Token.TokenType.String)
            {
                itsAST = new StringTypeNode(itsPos);
                Accept(Token.TokenType.String);
            }
            else
            {
                itsAST = new BooleanTypeNode(itsPos);
                Accept(Token.TokenType.Boolean);
            }
            return itsAST;
        }

        private TypeNode ArrayType(TypeNode node)
        {
            TypeNode itsAST;
            SourceCodePosition itsPos = _currentToken.SourcePosition;
            Accept(Token.TokenType.LeftBracket);
            Accept(Token.TokenType.RightBracket);
            itsAST = new ArrayTypeNode(node, new IntExpressionNode(new IntLiteralNode("", itsPos), itsPos), itsPos);
            return itsAST;
        }

        // checks whether the current token is of the expected type. If so, reads the next token and if not reports an error and performs error recovery
        private void Accept(Token.TokenType expectedType)
        {
            if (_currentToken.Type != expectedType)
            {
                _errorSummary.AddError(new SyntaxError($"Expected token of type '{expectedType}'",
                    _currentToken.SourcePosition));
                Recover(expectedType);
            }
            else
            {
                _currentToken = _tokenStream.Read();
            }
            
        }

        // does error recovery on faulty token type by discarding tokens until the expected token type or EOF is found
        private void Recover(Token.TokenType expectedType)
        {
            while (_currentToken.Type != expectedType && _currentToken.Type != Token.TokenType.EOF)
            {
                _currentToken = _tokenStream.Read();
            }
            if (_currentToken.Type == expectedType)
            {
                _currentToken = _tokenStream.Read();
            }
        }

        private void Error(string expectedTypesMsg)
        {
            _errorSummary.AddError(new SyntaxError($"Unexpected token of type '{_currentToken.Type}'. {expectedTypesMsg}",
                _currentToken.SourcePosition));
        }
    }
}