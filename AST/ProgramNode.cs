using Deslang.AST.DeclaringNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST
{
    public class ProgramNode : AST
    {
        public DeclaringSequenceNode Declarings { get; }

        public ProgramNode(DeclaringSequenceNode declarings, SourceCodePosition pos) : base(pos)
        {
            Declarings = declarings;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
