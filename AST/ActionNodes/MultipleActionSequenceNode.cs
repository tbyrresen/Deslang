using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class MultipleActionSequenceNode : ActionSequenceNode
    {
        public ActionNode Action { get; }
        public ActionSequenceNode Actions { get; }

        public MultipleActionSequenceNode(ActionNode action, ActionSequenceNode actions, SourceCodePosition pos)
            : base(pos)
        {
            Action = action;
            Actions = actions;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
