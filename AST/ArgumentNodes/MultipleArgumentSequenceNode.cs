using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ArgumentNodes
{
    public class MultipleArgumentSequenceNode : ArgumentSequenceNode
    {
        public ArgumentNode Argument { get; }
        public ArgumentSequenceNode Arguments { get; }

        public MultipleArgumentSequenceNode(ArgumentNode arg, ArgumentSequenceNode args, SourceCodePosition pos) 
            : base(pos)
        {
            Argument = arg;
            Arguments = args;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
