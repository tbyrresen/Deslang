using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class ElseifActionNode : ActionNode
    {
        public ExpressionNode Expression { get; }
        public ActionSequenceNode Actions { get; }

        public ElseifActionNode(ExpressionNode expression, ActionSequenceNode actions, SourceCodePosition pos)
            : base(pos)
        {
            Expression = expression;
            Actions = actions;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
