using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class UnaryExpressionNode : ExpressionNode
    {
        public OperatorNode Operator { get; }
        public ExpressionNode Child { get; }

        public UnaryExpressionNode(OperatorNode op, ExpressionNode child, SourceCodePosition pos) 
            : base(pos)
        {
            Operator = op;
            Child = child;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
