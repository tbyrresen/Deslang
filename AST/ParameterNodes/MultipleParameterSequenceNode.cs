using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ParameterNodes
{
    public class MultipleParameterSequenceNode : ParameterSequenceNode
    {
        public ParameterNode Parameter { get; }
        public ParameterSequenceNode Parameters { get; }

        public MultipleParameterSequenceNode(ParameterNode parameter, ParameterSequenceNode parameters, SourceCodePosition pos)
            : base(pos)
        {
            Parameter = parameter;
            Parameters = parameters;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
