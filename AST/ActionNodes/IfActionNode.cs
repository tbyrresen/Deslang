using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class IfActionNode : ActionNode
    {
        public ExpressionNode Expression { get; }
        public ActionSequenceNode Actions { get; }
        public ActionSequenceNode ElseIfs { get; } 
        public ActionNode Else { get; }

        
        public IfActionNode(ExpressionNode expr, ActionSequenceNode actions, ActionSequenceNode elseifs,
            ActionNode els, SourceCodePosition pos) : base(pos)
        {
            Expression = expr;
            Actions = actions;
            ElseIfs = elseifs;
            Else = els;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
