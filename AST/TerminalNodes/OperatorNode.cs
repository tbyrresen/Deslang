using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TerminalNodes
{
    public class OperatorNode : TerminalNode
    {
        public OperatorNode(string value, SourceCodePosition pos) : base(value, pos)
        {
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
