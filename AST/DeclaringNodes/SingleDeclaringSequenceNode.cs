using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class SingleDeclaringSequenceNode : DeclaringSequenceNode
    {
        public DeclaringNode Declaring { get; }

        public SingleDeclaringSequenceNode(DeclaringNode declaring, SourceCodePosition pos) : base(pos)
        {
            Declaring = declaring;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
