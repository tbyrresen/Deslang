using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public abstract class DeclaringSequenceNode : AST 
    {
        public DeclaringSequenceNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
