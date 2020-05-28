using Deslang.AST.DeclaringNodes;
using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TerminalNodes
{
    public class IdentifierNode : TerminalNode
    {
        public TypeNode Type { get; set; }
        public DeclaringNode Declaring { get; set; }

        public IdentifierNode(Token identifierToken) 
            : base(identifierToken.Value, identifierToken.SourcePosition)
        {
        }

        public IdentifierNode(Token identifierToken, TypeNode type) 
            : base(identifierToken.Value, identifierToken.SourcePosition)
        {
            Type = type;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
