using Deslang.AST.DeclaringNodes;
using Deslang.AST.ExpressionNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class ForeachActionNode : ActionNode
    {
        public VarDeclaringNode itsDeclaring { get; }   // the variable declarations assocaitd with the foreach loop
        public IdentifierNode Identifier { get; }
        public ExpressionNode Expression { get; }
        public ActionSequenceNode Actions { get; }

        public ForeachActionNode(VarDeclaringNode decl, IdentifierNode ident, ExpressionNode expr, 
            ActionSequenceNode actions, SourceCodePosition pos) : base(pos)
        {
            itsDeclaring = decl;
            Identifier = ident;
            Expression = expr;
            Actions = actions;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
