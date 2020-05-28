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

namespace Deslang.AST
{
    public interface IVisitor
    {
        // visitor methods for declaring AST nodes
        Object Visit(ProgramNode n, Object o);
        Object Visit(EmptyDeclaringSequenceNode n, Object o);
        Object Visit(SingleDeclaringSequenceNode n, Object o);
        Object Visit(MultipleDeclaringSequenceNode n, Object o);
        Object Visit(InterfaceDeclaringNode n, Object o);
        Object Visit(InterfaceMethodDeclaringNode n, Object o);
        Object Visit(ClassDeclaringNode n, Object o);
        Object Visit(ImplementDeclaringNode n, Object o);
        Object Visit(VarDeclaringNode n, Object o);
        Object Visit(AssignedVarDeclaringNode n, Object o);
        Object Visit(ConstructorDeclaringNode n, Object o);
        Object Visit(MethodDeclaringNode n, Object o);

        // visitor methods for action AST nodes
        Object Visit(AssigningActionNode n, Object o);
        Object Visit(EmptyActionNode n, Object o);
        Object Visit(EmptyActionSequenceNode n, Object o);
        Object Visit(SingleActionSequenceNode n, Object o);
        Object Visit(MultipleActionSequenceNode n, Object o);
        Object Visit(IfActionNode n, Object o);
        Object Visit(ElseifActionNode n, Object o);
        Object Visit(ElseActionNode n, Object o);
        Object Visit(WhileActionNode n, Object o);
        Object Visit(ForeachActionNode n, Object o);
        Object Visit(ThisReferenceAction n, Object o);
        Object Visit(MemberReferenceAction n, Object o);
        Object Visit(ReturnActionNode n, Object o);
        Object Visit(BreakActionNode n, Object o);

        // visitor methods for expression AST nodes
        Object Visit(BinaryExpressionNode n, Object o);
        Object Visit(UnaryExpressionNode n, Object o);
        Object Visit(CallExpressionNode n, Object o);
        Object Visit(InstantiationNode n, Object o);
        Object Visit(IntExpressionNode n, Object o);
        Object Visit(RealExpressionNode n, Object o);
        Object Visit(StringExpressionNode n, Object o);
        Object Visit(NullExpressionNode n, Object o);
        Object Visit(BooleanExpressionNode n, Object o);
        Object Visit(MemberExpressionNode n, Object o);
        Object Visit(ThisExpressionNode n, Object o);

        // visitor methods for member AST nodes
        Object Visit(SimpleMemberNode n, Object o);
        Object Visit(CalledMemberNode n, Object o);
        Object Visit(IndexedMemberNode n, Object o);
        Object Visit(CalledAndIndexedMemberNode n, Object o);
        Object Visit(ThisNode n, Object o);
        Object Visit(DotMemberNode n, Object o);

        // visitor methods for terminal AST nodes
        Object Visit(IdentifierNode n, Object o);
        Object Visit(BooleanLiteralNode n, Object o);
        Object Visit(IntLiteralNode n, Object o);
        Object Visit(NullLiteralNode n, Object o);
        Object Visit(OperatorNode n, Object o);
        Object Visit(RealLiteralNode n, Object o);
        Object Visit(StringLiteralNode n, Object o);

        // visitor methods for type AST nodes
        Object Visit(ArrayTypeNode n, Object o);
        Object Visit(BooleanTypeNode n, Object o);
        Object Visit(ClassTypeNode n, Object o);
        Object Visit(IntTypeNode n, Object o);
        Object Visit(RealTypeNode n, Object o);
        Object Visit(StringTypeNode n, Object o);
        Object Visit(VoidTypeNode n, Object o);
        Object Visit(ErrorTypeNode n, Object o);
        Object Visit(MethodTypeNode n, Object o);
        Object Visit(NullTypeNode n, Object o);
        Object Visit(AnyTypeNode n, Object o);

        // visitor methods for parameters AST nodes
        Object Visit(EmptyParameterSequenceNode n, Object o);
        Object Visit(SingleParameterSequenceNode n, Object o);
        Object Visit(MultipleParameterSequenceNode n, Object o);
        Object Visit(MethodParameterNode n, Object o);

        // visitor methods for arguments AST nodes
        Object Visit(EmptyArgumentSequenceNode n, Object o);
        Object Visit(SingleArgumentSequenceNode n, Object o);
        Object Visit(MultipleArgumentSequenceNode n, Object o);
        Object Visit(ExpressionArgumentNode n, Object o);
    }
}
