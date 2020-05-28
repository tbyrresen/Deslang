using Deslang.Errors;
using System;
using System.IO;
using System.Text;

namespace Deslang.SyntaxAnalysis
{
    public class Scanner
    {
        private readonly CharStream _charStream;
        private readonly ErrorSummary _errorSummary;
        private readonly string _fileName;
        private static readonly char[] _mathStartChars = { '+', '-', '*', '/', '^', '%', '=', '<', '>', '!' };
        private static readonly char[] _controlChars = { '(', ')', '{', '}', '[', ']', ',', '.', ';' };
        private static readonly char[] _skipChars = { ' ', '\t', '\n', '\r', '#' };
        private int _lineNumber = 1;
        private int _charNumber = 1;

        public Scanner(CharStream charStream, ErrorSummary errorSummary, string fileName)
        {
            _charStream = charStream;
            _errorSummary = errorSummary;
            _fileName = fileName;
        }

        public Token Scan()
        {
            Token answer;
            while (IsSkipChar(_charStream.Peek()))
            {
                Skip();
            }
            if (_charStream.EOF())
            {
                answer = new Token(Token.TokenType.EOF, new SourceCodePosition(_lineNumber, _charNumber, _fileName));
            }
            else if (Char.IsDigit(_charStream.Peek()))
            {
                answer = ScanNumberToken();
            }
            else if (IsValidIdentStartChar(_charStream.Peek()))
            {
                answer = ScanIdentOrKeywordToken();
            }
            else if (IsValidMathOpStartChar(_charStream.Peek()))
            {
                answer = ScanMathOpToken();
            }
            else if (IsValidControlChar(_charStream.Peek()))
            {
                answer = ScanControlToken();
            }
            else if (_charStream.Peek() == '"')  // string
            {
                answer = ScanStringToken();
            }
            else
            {
                _errorSummary.AddError(new SyntaxError($"invalid symbol '{_charStream.Peek()}'",
                    new SourceCodePosition(_lineNumber, _charNumber, _fileName)));
                _charStream.Read(); // read and discard the invalid symbol to continue scanning
                return Scan();
            }
            return answer;
        }

        private bool IsSkipChar(char c) => Array.Exists(_skipChars, e => e == c);

        private void Skip()
        {
            if (_charStream.Peek() == ' ' || _charStream.Peek() == '\t')  // whitespace
            {
                _charStream.Read();
                _charNumber++;
            }
            else if (_charStream.Peek() == '\n' || _charStream.Peek() == '\r')  // EOL
            {
                ConsumeEOL();
                _charNumber = 1;
                _lineNumber++;
            }
            else if (_charStream.Peek() == '#')  // comment
            {
                SkipLine();
                _charNumber = 1;
                _lineNumber++;
            }
        }

        private void SkipLine()
        {
            while (!_charStream.EOF() && _charStream.Peek() != '\n' && _charStream.Peek() != '\r')
            {
                _charStream.Read();
            }
            if (!_charStream.EOF())
            {
                ConsumeEOL();
            }
        }

        // assumes the next character(s) in stream is EOL. Consumes all EOL characters for the environment. 
        private void ConsumeEOL()
        {
            StringBuilder EOL = new StringBuilder();
            EOL.Append(_charStream.Read());
            while (EOL.ToString() != Environment.NewLine)
            {
                EOL.Append(_charStream.Read());
            }
        }

        // int-literals: [0-9] | [1-9][0-9]*
        // real-literals: [0-9].[0-9]+ | [1-9][0-9]*.[0-9]+
        // assumes the next char in stream is a valid number start char

        private Token ScanNumberToken()
        {
            Token.TokenType type;
            string value;
            int tokenCharPos = _charNumber;
            StringBuilder valueBuilder = new StringBuilder();
            valueBuilder.Append(_charStream.Read());
            _charNumber++;

            if (int.Parse(valueBuilder.ToString()) == 0 && Char.IsDigit(_charStream.Peek()))
            {
                _errorSummary.AddError(new SyntaxError("Number literals cannot be prefixed with '0'",
                    new SourceCodePosition(_lineNumber, _charNumber - 1, _fileName)));
            }
            while (Char.IsDigit(_charStream.Peek()))
            {
                valueBuilder.Append(_charStream.Read());
                _charNumber++;
            }
            if (_charStream.Peek() != '.')
            {
                type = Token.TokenType.IntLiteral;
                value = valueBuilder.ToString();
            }
            else
            {
                char errorChar = _charStream.Peek(); // for emitting correct error character on possible lexical error 
                valueBuilder.Append(_charStream.Read());
                _charNumber++;
                if (!Char.IsDigit(_charStream.Peek()))
                {
                    _errorSummary.AddError(new SyntaxError($"Invalid symbol '{errorChar}'. Did you forget to add decimals to a real number?",
                        new SourceCodePosition(_lineNumber, _charNumber - 1, _fileName)));
                }
                while (Char.IsDigit(_charStream.Peek()))
                {
                    valueBuilder.Append(_charStream.Read());
                    _charNumber++;
                }
                type = Token.TokenType.RealLiteral;
                value = valueBuilder.ToString();
            }
            return new Token(type, value, new SourceCodePosition(_lineNumber, tokenCharPos, _fileName));
        }

        private bool IsValidIdentStartChar(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';

        // _*[a-zA-Z]+[_0-9a-zA-Z]*
        // assumes the next char in stream is a valid start char
        // scans for reserved words by considering these to be a subset of the possible identifiers
        private Token ScanIdentOrKeywordToken()
        {
            Token.TokenType type;
            string value;
            int tokenCharPos = _charNumber;
            StringBuilder valueBuilder = new StringBuilder();
            while (IsValidIdentChar(_charStream.Peek()))
            {
                valueBuilder.Append(_charStream.Read());
                _charNumber++;
            }

            value = valueBuilder.ToString();
            switch (value)
            {
                case "and":
                    type = Token.TokenType.And;
                    break;
                case "or":
                    type = Token.TokenType.Or;
                    break;
                case "not":
                    type = Token.TokenType.Not;
                    break;
                case "true":
                    type = Token.TokenType.BooleanLiteral;
                    break;
                case "false":
                    type = Token.TokenType.BooleanLiteral;
                    break;
                case "null":
                    type = Token.TokenType.Null;
                    break;
                case "int":
                    type = Token.TokenType.Int;
                    break;
                case "real":
                    type = Token.TokenType.Real;
                    break;
                case "boolean":
                    type = Token.TokenType.Boolean;
                    break;
                case "string":
                    type = Token.TokenType.String;
                    break;
                case "void":
                    type = Token.TokenType.Void;
                    break;
                case "while":
                    type = Token.TokenType.While;
                    break;
                case "foreach":
                    type = Token.TokenType.Foreach;
                    break;
                case "in":
                    type = Token.TokenType.In;
                    break;
                case "if":
                    type = Token.TokenType.If;
                    break;
                case "elseif":
                    type = Token.TokenType.Elseif;
                    break;
                case "else":
                    type = Token.TokenType.Else;
                    break;
                case "return":
                    type = Token.TokenType.Return;
                    break;
                case "class":
                    type = Token.TokenType.Class;
                    break;
                case "constructor":
                    type = Token.TokenType.Constructor;
                    break;
                case "interface":
                    type = Token.TokenType.Interface;
                    break;
                case "implements":
                    type = Token.TokenType.Implements;
                    break;
                case "this":
                    type = Token.TokenType.This;
                    break;
                case "new":
                    type = Token.TokenType.New;
                    break;
                case "var":
                    type = Token.TokenType.Var;
                    break;
                case "method":
                    type = Token.TokenType.Method;
                    break;
                case "break":
                    type = Token.TokenType.Break;
                    break;
                default:
                    type = Token.TokenType.Identifier;
                    break;
            }
            return new Token(type, value, new SourceCodePosition(_lineNumber, tokenCharPos, _fileName));
        }

        private bool IsValidIdentChar(char c) => IsValidIdentStartChar(c) || Char.IsDigit(c);

        private bool IsValidMathOpStartChar(char c) => Array.Exists(_mathStartChars, e => e == c);

        // assumes the next char in stream is a math operator char
        private Token ScanMathOpToken()
        {
            Token.TokenType type;
            string value;
            int tokenCharPos = _charNumber;
            StringBuilder valueBuilder = new StringBuilder();
            valueBuilder.Append(_charStream.Read());
            _charNumber++;

            value = valueBuilder.ToString();
            switch (value)
            {
                case "+":
                case "-":
                    type = Token.TokenType.AdditiveOperator;
                    value = valueBuilder.ToString();
                    break;
                case "*":
                case "/":
                case "%":
                    type = Token.TokenType.MultiplicativeOperator;
                    value = valueBuilder.ToString();
                    break;
                case "^":
                    type = Token.TokenType.ExponentOperator;
                    value = valueBuilder.ToString();
                    break;
                case "=":
                    if (_charStream.Peek() == '=')  // case: '=='
                    {
                        type = Token.TokenType.CompareOperator;
                        value = valueBuilder.Append(_charStream.Read()).ToString();
                    }
                    else
                    {
                        type = Token.TokenType.AssignmentOperator; ;
                        value = valueBuilder.ToString();
                    }
                    break;
                case ">":
                case "<":
                    if (_charStream.Peek() == '=')  // cases: '<=' and '>='
                    {
                        type = Token.TokenType.CompareOperator;
                        value = valueBuilder.Append(_charStream.Read()).ToString();
                    }
                    else
                    {
                        type = Token.TokenType.CompareOperator;
                        value = valueBuilder.ToString();
                    }
                    break;
                case "!":
                    if (_charStream.Peek() == '=')  // case: '!='
                    {
                        type = Token.TokenType.CompareOperator;
                        value = valueBuilder.Append(_charStream.Read()).ToString();
                        break;
                    }
                    else
                    {
                        type = Token.TokenType.Error;
                        value = "";
                        _errorSummary.AddError(new SyntaxError($"Invalid symbol '{valueBuilder[0]}'. Did you forget the '=' symbol in a '!=' compare operator?",
                            new SourceCodePosition(_lineNumber, _charNumber - 1, _fileName)));
                        break;
                    }
                default:
                    type = Token.TokenType.Error;
                    value = "";
                    _errorSummary.AddError(new SyntaxError($"Invalid symbol '{valueBuilder[0]}'",
                        new SourceCodePosition(_lineNumber, _charNumber - 1, _fileName)));
                    break;
            }
            return new Token(type, value, new SourceCodePosition(_lineNumber, tokenCharPos, _fileName));
        }

        private bool IsValidControlChar(char c) => Array.Exists(_controlChars, e => e == c);

        // assumes the next char in stream is a control char
        private Token ScanControlToken()
        {
            Token.TokenType type;
            char errorChar = _charStream.Peek();  // for emitting correct error character on possible lexical error due to faulty call
            int tokenCharPos = _charNumber;
            string value = _charStream.Read().ToString();
            _charNumber++;

            switch (value)
            {
                case "(":
                    type = Token.TokenType.LeftParen;
                    break;
                case ")":
                    type = Token.TokenType.RightParen;
                    break;
                case "{":
                    type = Token.TokenType.LeftBrace;
                    break;
                case "}":
                    type = Token.TokenType.RightBrace;
                    break;
                case "[":
                    type = Token.TokenType.LeftBracket;
                    break;
                case "]":
                    type = Token.TokenType.RightBracket;
                    break;
                case ",":
                    type = Token.TokenType.Comma;
                    break;
                case ".":
                    type = Token.TokenType.Dot;
                    break;
                case ";":
                    type = Token.TokenType.Semicolon;
                    break;
                default:
                    type = Token.TokenType.Error;
                    _errorSummary.AddError(new SyntaxError($"Lexical error on character '{errorChar}'",
                        new SourceCodePosition(_lineNumber, _charNumber - 1, _fileName)));
                    break;
            }
            return new Token(type, value, new SourceCodePosition(_lineNumber, tokenCharPos, _fileName));
        }

        // assumes the next char in stream is '"'
        private Token ScanStringToken()
        {
            Token.TokenType type = Token.TokenType.StringLiteral;
            string value;
            int tokenCharPos = _charNumber;
            StringBuilder valueBuilder = new StringBuilder();
            valueBuilder.Append(_charStream.Read());
            _charNumber++;

            while (_charStream.Peek() != '"')
            {
                if (_charStream.Peek() == '\r' || _charStream.Peek() == '\n' || _charStream.EOF())  // add an error if the string is not terminated before new line or EOF
                {
                    _errorSummary.AddError(new SyntaxError($"Lexical error. Did you forget to terminate a string?",
                        new SourceCodePosition(_lineNumber, _charNumber - 1, _fileName)));
                    return new Token(type, valueBuilder.ToString(), new SourceCodePosition(_lineNumber, tokenCharPos, _fileName));
                }
                valueBuilder.Append(_charStream.Read());
                _charNumber++;
            }
            valueBuilder.Append(_charStream.Read());
            _charNumber++;
            value = valueBuilder.ToString().Trim(new char[] { '"' });
            return new Token(type, value, new SourceCodePosition(_lineNumber, tokenCharPos, _fileName));
        }
    }
}
