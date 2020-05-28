using Deslang.AST;
using Deslang.AST.ActionNodes;
using Deslang.AST.ArgumentNodes;
using Deslang.AST.DeclaringNodes;
using Deslang.AST.ExpressionNodes;
using Deslang.AST.MemberNodes;
using Deslang.AST.ParameterNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.CodeGeneration
{  
    public class CodeGenerator : IVisitor
    {
        private StringBuilder _CSharpCode;
        private int _currentIndent;
        private StringBuilder _tmp;
        private bool _useTmpString;
        private int _currentScopeLevel;  // used to determine how to write variable declarings, i.e. when to apply the 'public' attribute
        private bool _buildingImplSequence;

        public CodeGenerator()
        {
            _CSharpCode = new StringBuilder();
            _currentIndent = 0;
            _tmp = new StringBuilder();
            _useTmpString = false;
            _currentScopeLevel = 0;
            _buildingImplSequence = false;
        }

        // translates the Deslang code into the equivalent C# code by visiting all AST root nodes
        // assumes that the ASTs have all been properly type checked prior to translating the code
        public string Generate(List<ProgramNode> ASTRoots)
        {
            _CSharpCode.AppendLine("using System;");
            _CSharpCode.AppendLine("using Deslang.CodeGeneration;"); // reference to the namespace containing stdlib
            _CSharpCode.AppendLine("namespace Deslang"); // build the C# code in the 'Deslang' namespace so the runtime can correctly identify it
            _CSharpCode.AppendLine("{");
            IncreaseIndent();
            foreach (ProgramNode AST in ASTRoots)
            {
                AST.Accept(this, null);
            }
            DecreaseIndent();
            _CSharpCode.AppendLine("}");
            return _CSharpCode.ToString();
        }

        private void IncreaseIndent() => _currentIndent += 4;

        private void DecreaseIndent() => _currentIndent -= 4;

        private string AddIndent()
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i++ < _currentIndent)
            {
                sb.Append(" ");
            }
            return sb.ToString();
        }

        private void AppendLine(string s)
        {
            _CSharpCode.AppendLine(s);
        }

        private void AppendLine()
        {
            _CSharpCode.AppendLine();
        }

        private void Append(string s)
        {
            if (_useTmpString)
            {
                _tmp.Append(s);
            }
            else
            {
                _CSharpCode.Append(s);
            }
        }

        // helper method to ensure that strings that cannot be correctly executed are written to the target code
        private void CheckAndWriteTmp()
        {          
            string tmp = _tmp.ToString();
            if (tmp.Contains("("))
            {
                tmp += ";";
                _CSharpCode.AppendLine($"{AddIndent()}{tmp}");
            }
            _useTmpString = false;
            _tmp.Clear();
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
            if (_buildingImplSequence)
            {
                Append(", ");
            }
            n.Declarings.Accept(this, null);
            return null;
        }

        public object Visit(InterfaceDeclaringNode n, object o)
        {
            _currentScopeLevel = 1;
            AppendLine($"{AddIndent()}public interface {n.Identifier.Value}");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.InterfaceMethods.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            AppendLine();
            return null;
        }

        public object Visit(InterfaceMethodDeclaringNode n, object o)
        {
            _currentScopeLevel = 2;
            Append(AddIndent());
            n.Type.Accept(this, null);
            Append($" {n.Identifier.Value}(");
            n.Parameters.Accept(this, null);
            AppendLine($");");
            return null;
        }

        public object Visit(ClassDeclaringNode n, object o)
        {
            _currentScopeLevel = 1;
            Append($"{AddIndent()}public class {n.Identifier.Value}");
            if (!(n.Implements is EmptyDeclaringSequenceNode))
            {
                Append(" : ");  // check for empty sequence to avoid writing this code when no interfaces are listed on the class
                _buildingImplSequence = true;
                n.Implements.Accept(this, null);
                _buildingImplSequence = false;
            }
            AppendLine();
            _CSharpCode.AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.Variables.Accept(this, null);
            n.Constructor.Accept(this, null);
            n.Methods.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            AppendLine();
            return null;
        }

        public object Visit(ImplementDeclaringNode n, object o)
        {
            Append($"{n.Identifier.Value}");
            return null;
        }

        // we create variable declarings as public fields with generic getters and setters to adhere to the semantics of Deslang
        public object Visit(VarDeclaringNode n, object o)
        {
            if (_currentScopeLevel == 1)
            {
                Append($"{AddIndent()}public ");
                n.Type.Accept(this, null);
                Append($" {n.Identifier.Value} {{ get; set; }}");
                AppendLine();               
            }
            else
            {
                Append($"{AddIndent()}");
                n.Type.Accept(this, null);
                Append($" {n.Identifier.Value};");
                AppendLine();            
            }
            return null;            
        }

        public object Visit(AssignedVarDeclaringNode n, object o)
        {
            if (_currentScopeLevel == 1)
            {
                Append($"{AddIndent()}public ");
                n.Type.Accept(this, null);
                Append($" {n.Identifier.Value} = ");
                n.Expression.Accept(this, null);
                AppendLine(";");
            }
            else
            {
                Append($"{AddIndent()}");
                n.Type.Accept(this, null);
                Append($" {n.Identifier.Value} = ");
                n.Expression.Accept(this, null);
                AppendLine(";");
            }
            return null;
        }

        public object Visit(ConstructorDeclaringNode n, object o)
        {
            _currentScopeLevel = 2;
            Append($"{AddIndent()}public {n.Identifier.Value}(");
            n.Parameters.Accept(this, null);
            AppendLine($")");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.VarDeclarings.Accept(this, null);
            n.Actions.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            AppendLine();
            return null;
        }

        public object Visit(MethodDeclaringNode n, object o)
        {
            _currentScopeLevel = 2;
            if (n.Identifier.Value.Equals("Main"))
            {
                Append($"{AddIndent()}public static void {n.Identifier.Value}(");  // static Main required by runtime
            }
            else
            {
                Append($"{AddIndent()}public ");
                n.Type.Accept(this, null);
                Append($" {n.Identifier.Value}(");
            }
            n.Parameters.Accept(this, null);
            AppendLine($")");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.VarDeclarings.Accept(this, null);
            n.Actions.Accept(this, null);
            n.Return.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            AppendLine();
            return null;
        }

        public object Visit(AssigningActionNode n, object o)
        {
            Append(AddIndent());
            n.LHS.Accept(this, null);
            Append(" = ");
            n.RHS.Accept(this, null);
            AppendLine(";");
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
            Append($"{AddIndent()}if (");
            n.Expression.Accept(this, null);
            AppendLine(")");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.Actions.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            n.ElseIfs.Accept(this, null);
            n.Else.Accept(this, null);
            return null;
        }

        public object Visit(ElseifActionNode n, object o)
        {
            Append($"{AddIndent()}else if (");
            n.Expression.Accept(this, null);
            AppendLine(")");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.Actions.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            return null;
        }

        public object Visit(ElseActionNode n, object o)
        {
            AppendLine($"{AddIndent()}else");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.Actions.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            return null;
        }

        public object Visit(WhileActionNode n, object o)
        {
            Append($"{AddIndent()}while (");
            n.Expression.Accept(this, null);
            AppendLine(")");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.Actions.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            return null;
        }

        public object Visit(ForeachActionNode n, object o)
        {
            Append($"{AddIndent()}foreach (var {n.Identifier.Value} in "); // use C# var keyword to account for any Deslang type
            n.Expression.Accept(this, null);
            AppendLine(")");
            AppendLine($"{AddIndent()}{{");
            IncreaseIndent();
            n.Actions.Accept(this, null);
            DecreaseIndent();
            AppendLine($"{AddIndent()}}}");
            return null;
        }

        public object Visit(ThisReferenceAction n, object o)
        {
            _useTmpString = true;   // write to tmp string and check before writing since this reference may contain invalid C# commands
            n.MemberReference.Accept(this, null);
            CheckAndWriteTmp();
            return null;
        }
         
        public object Visit(MemberReferenceAction n, object o)
        {
            _useTmpString = true;   // write to tmp string and check before writing since this reference may contain invalid C# commands
            n.MemberReference.Accept(this, null);
            CheckAndWriteTmp();
            return null;
        }
       
        public object Visit(ReturnActionNode n, object o)
        {
            if (n.Expression is null)
            {
                AppendLine($"{AddIndent()}return;");
            }
            else
            {
                Append($"{AddIndent()}return ");
                n.Expression.Accept(this, null);
                AppendLine(";");
            }
            return null;
        }

        public object Visit(BreakActionNode n, object o)
        {
            AppendLine($"{AddIndent()}break;");
            return null;
        }

        public object Visit(BinaryExpressionNode n, object o)
        {
            Append("("); // add parenthesis to ensure correct order of evaluation
            if (n.Operator.Value == "^")
            {
                Append("Math.Pow(");
                n.Child1.Accept(this, null);
                Append(",");
                n.Child2.Accept(this, null);
                Append(")");
            }
            else
            {
                n.Child1.Accept(this, null);
                n.Operator.Accept(this, null);
                n.Child2.Accept(this, null);
            }         
            Append(")");
            return null;
        }

        public object Visit(UnaryExpressionNode n, object o)
        {
            Append("(");    // add parenthesis to ensure correct order of evaluation
            n.Operator.Accept(this, null);           
            n.Child.Accept(this, null);
            Append(")");
            return null;
        }

        public object Visit(CallExpressionNode n, object o)
        {
            Append("(");
            n.Arguments.Accept(this, null);
            if (StandardLibrary.IsStdlibMethod(n.Identifier.Value))
            {
                // if the call is to a stdlib method, append the source code position to the string such that errors
                // from external libraries can be emitted with the position of the offending Deslang code piece
                Append($", \"{n.SourcePosition}\"");
            }  
            Append(")");
            return null;
        }

        public object Visit(InstantiationNode n, object o)
        {
            Append($"new ");
            if (n.Type is ArrayTypeNode)
            {
                // if the type is an array, visit the type of that array to enclose the expression in brackets
                ((ArrayTypeNode)n.Type).Type.Accept(this, null);
                Append("[");
                n.Expression.Accept(this, null);
                Append("]");
            }
            else
            {
                n.Type.Accept(this, null);
                n.Expression.Accept(this, null);
            }           
            return null;
        }

        public object Visit(IntExpressionNode n, object o)
        {
            n.IntLiteral.Accept(this, null);
            return null;
        }

        public object Visit(RealExpressionNode n, object o)
        {
            n.RealLiteral.Accept(this, null);
            return null;
        }

        public object Visit(StringExpressionNode n, object o)
        {
            n.StringLiteral.Accept(this, null);
            return null;
        }

        public object Visit(NullExpressionNode n, object o)
        {
            n.NullLiteral.Accept(this, null);
            return null;
        }

        public object Visit(BooleanExpressionNode n, object o)
        {
            n.BooleanLiteral.Accept(this, null);
            return null;
        }

        public object Visit(MemberExpressionNode n, object o)
        {
            n.Member.Accept(this, null);
            return null;
        }

        public object Visit(ThisExpressionNode n, object o)
        {
            n.Member.Accept(this, null);
            return null;
        }

        public object Visit(SimpleMemberNode n, object o)
        {           
            switch (n.Identifier.Value)
            {
                case "print":
                    Append("StandardLibrary.Print");
                    break;
                case "printLine":
                    Append("StandardLibrary.PrintLine");
                    break;
                case "length":
                    Append("StandardLibrary.Length");
                    break;
                case "exponential":
                    Append("StandardLibrary.Exponential");
                    break;
                case "normal":
                    Append("StandardLibrary.Normal");
                    break;
                case "discreteUniform":
                    Append("StandardLibrary.DiscreteUniform");
                    break;
                case "continuousUniform":
                    Append("StandardLibrary.ContinuousUniform");
                    break;
                default:
                    Append(n.Identifier.Value);
                    break;
            }         
            return null; 
        }

        public object Visit(CalledMemberNode n, object o)
        {
            n.Member.Accept(this, null);
            n.Call.Accept(this, null);
            return null;
        }

        public object Visit(IndexedMemberNode n, object o)
        {
            n.Member.Accept(this, null);
            Append("[");
            n.Index.Accept(this, null);
            Append("]");
            return null;
        }

        public object Visit(CalledAndIndexedMemberNode n, object o)
        {
            n.Member.Accept(this, null);
            n.Call.Accept(this, null);
            Append("[");
            n.Index.Accept(this, null);
            Append("]");
            return null;
        }

        public object Visit(ThisNode n, object o)
        {
            if (n.Member is null)
            {
                Append("this");
            }
            else
            {
                Append("this.");
                n.Member.Accept(this, null);
            }        
            return null;
        }

        public object Visit(DotMemberNode n, object o)
        {          
            n.Parent.Accept(this, null);
            Append($".{n.Identifier.Value}");
            return null;
        }

        public object Visit(IdentifierNode n, object o)
        {
            Append(n.Value);
            return null; 
        }

        public object Visit(BooleanLiteralNode n, object o)
        {
            Append(n.Value);
            return null;
        }

        public object Visit(IntLiteralNode n, object o)
        {
            Append(n.Value);
            return null;
        }

        public object Visit(NullLiteralNode n, object o)
        {
            Append(n.Value);
            return null;
        }

        public object Visit(OperatorNode n, object o)
        {
            if (n.Value == "not")
            {
                Append("!");
            }
            else if (n.Value == "and")
            {
                Append("&&");
            }
            else if (n.Value == "or")
            {
                Append("||");
            }
            else
            {
                Append(n.Value);
            }                 
            return null;
        }

        public object Visit(RealLiteralNode n, object o)
        {
            Append(n.Value);
            return null;
        }

        public object Visit(StringLiteralNode n, object o)
        {
            Append($"\"{n.Value}\"");
            return null;
        }

        public object Visit(ArrayTypeNode n, object o)
        {
            n.Type.Accept(this, null);
            Append("[]");
            return null;
        }

        public object Visit(BooleanTypeNode n, object o)
        {
            Append("bool");
            return null;
        }

        public object Visit(ClassTypeNode n, object o)
        {
            Append(n.ClassName.Value);
            return null;
        }

        public object Visit(IntTypeNode n, object o)
        {
            Append("int");
            return null;
        }

        public object Visit(RealTypeNode n, object o)
        {
            Append("double");
            return null;
        }

        public object Visit(StringTypeNode n, object o)
        {
            Append("string");
            return null;
        }

        public object Visit(VoidTypeNode n, object o)
        {
            Append("void");
            return null;
        }

        public object Visit(ErrorTypeNode n, object o)
        {
            return null;
        }

        public object Visit(MethodTypeNode n, object o)
        {
            n.Type.Accept(this, null);
            return null;
        }

        public object Visit(NullTypeNode n, object o)
        {
            Append("null");
            return null;
        }

        public object Visit(AnyTypeNode n, object o)
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
            Append(", ");
            n.Parameters.Accept(this, null);
            return null;
        }

        public object Visit(MethodParameterNode n, object o)
        {
            n.Type.Accept(this, null);
            Append($" {n.Identifier.Value}");
            return null;
        }

        public object Visit(EmptyArgumentSequenceNode n, object o)
        {
            return null;
        }

        public object Visit(SingleArgumentSequenceNode n, object o)
        {
            n.Argument.Accept(this, null);
            return null;
        }

        public object Visit(MultipleArgumentSequenceNode n, object o)
        {
            n.Argument.Accept(this, null);
            Append(", ");
            n.Arguments.Accept(this, null);
            return null;
        }

        public object Visit(ExpressionArgumentNode n, object o)
        {
            n.Expression.Accept(this, null);
            return null;
        }
    }
}
