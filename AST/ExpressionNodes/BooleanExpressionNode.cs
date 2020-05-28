using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class BooleanExpressionNode : PrimaryExpressionNode
    {
        public BooleanLiteralNode BooleanLiteral { get; }
        public BooleanExpressionNode(BooleanLiteralNode stringLiteral, SourceCodePosition pos) : base(pos)
        {
            BooleanLiteral = stringLiteral;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
