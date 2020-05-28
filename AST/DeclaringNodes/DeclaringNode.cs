using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public abstract class DeclaringNode : AST
    {
        public DeclaringNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
