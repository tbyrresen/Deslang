using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class ReturnActionNode : ActionNode
    {
        public ExpressionNode Expression { get; }   // if expression is null no expression follows the return statement

        public ReturnActionNode(ExpressionNode expr, SourceCodePosition pos) : base(pos)
        {
            Expression = expr;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
