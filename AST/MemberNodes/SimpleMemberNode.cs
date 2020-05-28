using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.MemberNodes
{
    public class SimpleMemberNode : MemberNode
    {
        public IdentifierNode Identifier { get; }

        public SimpleMemberNode(IdentifierNode ident, SourceCodePosition pos) : base(pos)
        {
            Identifier = ident;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
