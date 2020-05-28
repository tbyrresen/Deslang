using Deslang.AST.ExpressionNodes;
using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.MemberNodes
{
    public class IndexedMemberNode : MemberNode
    {
        public MemberNode Member { get; }
        public ExpressionNode Index { get; }

        public IndexedMemberNode(MemberNode member, ExpressionNode index, SourceCodePosition pos) 
            : base(pos)
        {
            Member = member;
            Index = index;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
