using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.MemberNodes
{
    public class DotMemberNode :  MemberNode
    {
        public MemberNode Parent { get; }
        public IdentifierNode Identifier { get; }

        public DotMemberNode(MemberNode parent, IdentifierNode ident, SourceCodePosition pos) : base(pos)
        {
            Parent = parent;
            Identifier = ident;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
