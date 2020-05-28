using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ArgumentNodes
{
    public class EmptyArgumentSequenceNode : ArgumentSequenceNode
    {
        public EmptyArgumentSequenceNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
