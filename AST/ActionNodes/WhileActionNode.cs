using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class WhileActionNode : ActionNode
    {
        public ExpressionNode Expression { get; }
        public ActionSequenceNode Actions { get; }

        public WhileActionNode(ExpressionNode expr, ActionSequenceNode actions, SourceCodePosition pos)
            : base(pos)
        {
            Expression = expr;
            Actions = actions;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
