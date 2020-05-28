using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.MemberNodes
{
    public class CalledMemberNode : MemberNode
    {
        public MemberNode Member { get; }
        public CallExpressionNode Call { get; }

        public CalledMemberNode(MemberNode member, CallExpressionNode call, SourceCodePosition pos) 
            : base(pos)
        {
            Member = member;
            Call = call;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
