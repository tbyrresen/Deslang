using Deslang.AST.ExpressionNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class AssignedVarDeclaringNode : VarDeclaringNode
    {
        public ExpressionNode Expression { get; }

        public AssignedVarDeclaringNode(IdentifierNode ident, TypeNode type, ExpressionNode expr, 
            SourceCodePosition pos) : base(ident, type, pos)
        {
            Expression = expr;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
