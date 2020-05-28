using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class InterfaceDeclaringNode : DeclaringNode
    {
        public IdentifierNode Identifier { get; }
        public DeclaringSequenceNode InterfaceMethods { get; }

        public InterfaceDeclaringNode(IdentifierNode ident, DeclaringSequenceNode interfaceMethods, 
            SourceCodePosition pos) : base(pos)
        {
            Identifier = ident;
            InterfaceMethods = interfaceMethods;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
