using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class ElseActionNode : ActionNode
    {
        public ActionSequenceNode Actions { get; }

        public ElseActionNode(ActionSequenceNode actions, SourceCodePosition pos) : base(pos)
        {
            Actions = actions;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
