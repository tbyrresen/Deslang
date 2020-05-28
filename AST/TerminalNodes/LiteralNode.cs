using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TerminalNodes
{
    public abstract class LiteralNode : TerminalNode
    {
        public LiteralNode(string value, SourceCodePosition pos) : base(value, pos)
        {
        }
    }
}
