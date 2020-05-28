using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ParameterNodes
{
    public class SingleParameterSequenceNode : ParameterSequenceNode
    {
        public ParameterNode Parameter { get; }

        public SingleParameterSequenceNode(ParameterNode param, SourceCodePosition pos) 
            : base(pos)
        {
            Parameter = param;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
