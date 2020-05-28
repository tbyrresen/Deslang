using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class InstantiationNode : PrimaryExpressionNode
    {
        public ExpressionNode Expression { get; }   // either a call expression or array expression

        public InstantiationNode(TypeNode type, ExpressionNode expr, SourceCodePosition pos) 
            : base(pos)
        {
            Type = type;
            Expression = expr;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
