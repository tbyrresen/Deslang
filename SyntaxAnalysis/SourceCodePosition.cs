using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.SyntaxAnalysis
{
    public class SourceCodePosition
    {
        public int LineNumber { get; }
        public int CharNumber { get; }
        public String FileName { get; }

        public SourceCodePosition(int lineNumber, int charNumber, string fileName)
        {
            LineNumber = lineNumber;
            CharNumber = charNumber;
            FileName = fileName;
        }

        public override string ToString()
        {
            return $"[Location ({LineNumber}:{CharNumber}) in file {FileName}]";
        }
    }
}
