using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class StringExpressionNode : PrimaryExpressionNode
    {
        public StringLiteralNode StringLiteral { get; }

        public StringExpressionNode(StringLiteralNode stringLiteral, SourceCodePosition pos) : base(pos)
        {
            StringLiteral = stringLiteral;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
