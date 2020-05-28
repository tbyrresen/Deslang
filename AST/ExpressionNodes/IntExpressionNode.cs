using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class IntExpressionNode : PrimaryExpressionNode
    {
        public IntLiteralNode IntLiteral { get;  }

        public IntExpressionNode(IntLiteralNode intLiteral, SourceCodePosition pos) : base(pos)
        {
            IntLiteral = intLiteral;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
