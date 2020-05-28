using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class SingleActionSequenceNode : ActionSequenceNode
    {
        public ActionNode Action { get; }

        public SingleActionSequenceNode(ActionNode action, SourceCodePosition pos) : base(pos)
        {
            Action = action;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
