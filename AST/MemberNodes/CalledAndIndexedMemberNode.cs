using Deslang.AST.ExpressionNodes;
using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.MemberNodes
{
    public class CalledAndIndexedMemberNode : MemberNode
    {
        public MemberNode Member { get; }
        public CallExpressionNode Call { get; }
        public ExpressionNode Index { get; }

        public CalledAndIndexedMemberNode(MemberNode member, CallExpressionNode call, ExpressionNode index,
            SourceCodePosition pos) : base(pos)
        {
            Member = member;
            Call = call;
            Index = index;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
