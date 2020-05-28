using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ArgumentNodes
{
    public abstract class ArgumentSequenceNode : AST
    {
        public ArgumentSequenceNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
