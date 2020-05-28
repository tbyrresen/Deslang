using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ParameterNodes
{
    public abstract class ParameterSequenceNode : AST
    {
        public ParameterSequenceNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
