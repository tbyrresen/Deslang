﻿using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TerminalNodes
{
    public class StringLiteralNode : LiteralNode
    {
        public StringLiteralNode(string value, SourceCodePosition pos) : base(value, pos)
        {
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
