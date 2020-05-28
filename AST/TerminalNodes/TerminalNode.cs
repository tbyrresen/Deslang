using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TerminalNodes
{
    public abstract class TerminalNode : AST
    {
        public string Value { get; }

        public TerminalNode(string value, SourceCodePosition pos) : base(pos)
        {
            Value = value;
        }
    }
}
