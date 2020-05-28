using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class EmptyActionSequenceNode : ActionSequenceNode
    {
        public EmptyActionSequenceNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
