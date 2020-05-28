using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class MultipleDeclaringSequenceNode : DeclaringSequenceNode
    {
        public DeclaringNode Declaring { get; }
        public DeclaringSequenceNode Declarings { get; }

        public MultipleDeclaringSequenceNode(DeclaringNode declaring, DeclaringSequenceNode declarings, 
            SourceCodePosition pos) : base (pos)
        {
            Declaring = declaring;
            Declarings = declarings;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
