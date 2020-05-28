using Deslang.AST.MemberNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class ThisExpressionNode : MemberExpressionNode
    {
        public ThisNode This { get; }

        public ThisExpressionNode(ThisNode thisNode, SourceCodePosition pos)
            : base(thisNode, pos)
        {
            This = thisNode;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}

