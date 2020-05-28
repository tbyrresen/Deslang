using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.Errors
{
    public class SemanticError : Exception
    {
        public SourceCodePosition Position { get; }

        public SemanticError(string msg, SourceCodePosition pos) : base(msg)
        {
            Position = pos;
        }
    }
}
