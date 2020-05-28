using Deslang.AST.MemberNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class MemberExpressionNode : PrimaryExpressionNode
    {
        public MemberNode Member { get; }

        public MemberExpressionNode(MemberNode member, SourceCodePosition pos) : base(pos)
        {
            Member = member;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
