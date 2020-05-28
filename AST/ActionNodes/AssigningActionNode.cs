using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;

namespace Deslang.AST.ActionNodes
{
    public class AssigningActionNode : ActionNode
    {
        public MemberExpressionNode LHS { get; }
        public ExpressionNode RHS { get; }

        public AssigningActionNode(MemberExpressionNode lhs, ExpressionNode rhs, SourceCodePosition pos) 
            : base(pos)
        {
            LHS = lhs;
            RHS = rhs;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
