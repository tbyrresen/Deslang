using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public abstract class ExpressionNode : AST
    {
        public TypeNode Type { get; set; }  // type of the expression, used during type checking

        public ExpressionNode(SourceCodePosition pos) : base(pos)
        {
            Type = null;
        }
    }
}
