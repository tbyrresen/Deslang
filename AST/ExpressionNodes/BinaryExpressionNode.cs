using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class BinaryExpressionNode : ExpressionNode
    {       
        public ExpressionNode Child1 { get; }
        public OperatorNode Operator { get; }
        public ExpressionNode Child2 { get; }

        public BinaryExpressionNode(ExpressionNode child1, OperatorNode op, ExpressionNode child2,
            SourceCodePosition pos) : base(pos)
        { 
            Child1 = child1;
            Operator = op;            
            Child2 = child2;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}

