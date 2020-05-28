using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.Errors
{
    public class SyntaxError : Exception
    {
        public SourceCodePosition Position { get; set; }

        public SyntaxError(string msg, SourceCodePosition pos) : base(msg)
        {
            Position = pos;
        }
    }
}
