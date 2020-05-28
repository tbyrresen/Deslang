using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ArgumentNodes
{
    public class ExpressionArgumentNode : ArgumentNode
    {
        public ExpressionNode Expression { get; }

        public ExpressionArgumentNode(ExpressionNode expr, SourceCodePosition pos) : base(pos)
        {
            Expression = expr;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
