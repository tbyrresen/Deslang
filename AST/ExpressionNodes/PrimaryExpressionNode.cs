using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public abstract class PrimaryExpressionNode : ExpressionNode
    {
        public PrimaryExpressionNode(SourceCodePosition pos) : base(pos)
        {
        }
    }
}
