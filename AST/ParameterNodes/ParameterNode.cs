using Deslang.AST.DeclaringNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ParameterNodes
{
    public abstract class ParameterNode : DeclaringNode
    {
        public ParameterNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
