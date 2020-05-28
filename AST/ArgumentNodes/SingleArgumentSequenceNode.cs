using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ArgumentNodes
{
    public class SingleArgumentSequenceNode : ArgumentSequenceNode
    {
        public ArgumentNode Argument { get; }

        public SingleArgumentSequenceNode(ArgumentNode arg, SourceCodePosition pos) : base(pos)
        {
            Argument = arg;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
