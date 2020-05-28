using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.MemberNodes
{
    public class ThisNode : MemberNode
    {
        public MemberExpressionNode Member { get; }   // null if the node has no member (i.e is the 'this' keyword)

        public ThisNode(SourceCodePosition pos) : base(pos)
        {
        }
        
        public ThisNode(MemberExpressionNode member, SourceCodePosition pos) : base(pos)
        {
            Member = member;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
