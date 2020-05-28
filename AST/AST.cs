using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST
{
    public abstract class AST
    {
        public SourceCodePosition SourcePosition { get; }

        public AST(SourceCodePosition pos)
        {
            SourcePosition = pos;
        }

        public abstract Object Accept(IVisitor v, Object o);
    }
}
