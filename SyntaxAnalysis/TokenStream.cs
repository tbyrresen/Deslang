using Deslang.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.SyntaxAnalysis
{
    public class TokenStream
    {
        private Token _nextToken;
        private readonly Scanner _scanner;

        public TokenStream(CharStream charStream, ErrorSummary errorSummary, string fileName)
        {
            _scanner = new Scanner(charStream, errorSummary, fileName);
            Read();
        }

        public Token Read()
        {
            Token answer = _nextToken;
            _nextToken = _scanner.Scan();
            return answer;
        }
    }
}
