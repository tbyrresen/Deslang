using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.SyntaxAnalysis
{
    public class Token
    {
        public enum TokenType
        {
            Or,
            And,
            Not,
            AdditiveOperator,
            MultiplicativeOperator,
            ExponentOperator,
            CompareOperator,
            Int,
            IntLiteral,
            IntArray,
            Real,
            RealLiteral,
            RealArray,
            Boolean,
            BooleanLiteral,
            BooleanArray,
            String,
            StringLiteral,
            StringArray,
            Identifier,
            IdentifierArray,
            Void,
            Null,
            While,
            Foreach,
            In,
            If,
            Elseif,
            Else,
            Return,
            EOF,
            LeftParen,
            RightParen,
            LeftBracket,
            RightBracket,
            LeftBrace,
            RightBrace,
            Dot,
            Comma,
            Semicolon,
            AssignmentOperator,
            Implements,
            Class,
            Interface,
            This,
            New,
            Constructor,
            Var,
            Method,
            Break,
            Error // used for signaling error input during scanning phase
        }

        public TokenType Type { get; }
        public string Value { get; }
        public SourceCodePosition SourcePosition { get; }

        public Token(TokenType type, string value, SourceCodePosition sourcePosition)
        {
            Type = type;
            Value = value;
            SourcePosition = sourcePosition;
        }

        public Token(TokenType type, SourceCodePosition sourcePosition) : this(type, "", sourcePosition)
        {
        }

        public override string ToString() => Type.ToString();
    }
}
