using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ParameterNodes
{
    public class EmptyParameterSequenceNode : ParameterSequenceNode
    {
        public EmptyParameterSequenceNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
