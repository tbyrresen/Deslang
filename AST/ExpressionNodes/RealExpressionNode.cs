using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class RealExpressionNode : PrimaryExpressionNode
    {
        public RealLiteralNode RealLiteral { get; }

        public RealExpressionNode(RealLiteralNode realLiteral, SourceCodePosition pos) : base(pos)
        {
            RealLiteral = realLiteral;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
