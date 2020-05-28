using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class NullExpressionNode : PrimaryExpressionNode
    {
        public NullLiteralNode NullLiteral { get; }

        public NullExpressionNode(NullLiteralNode stringLiteral, SourceCodePosition pos) : base(pos)
        {
            NullLiteral = stringLiteral;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
