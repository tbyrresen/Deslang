using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class ImplementDeclaringNode : DeclaringNode
    {
        public IdentifierNode Identifier { get; }

        public ImplementDeclaringNode(IdentifierNode ident, SourceCodePosition pos) : base(pos)
        {
            Identifier = ident;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
