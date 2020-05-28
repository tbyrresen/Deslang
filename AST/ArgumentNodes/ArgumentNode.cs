using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ArgumentNodes
{
    public abstract class ArgumentNode : AST
    {
        public ArgumentNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
