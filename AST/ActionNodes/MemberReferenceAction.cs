using Deslang.AST.ExpressionNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ActionNodes
{
    public class MemberReferenceAction : ActionNode
    {
        public MemberExpressionNode MemberReference { get; }

        public MemberReferenceAction(MemberExpressionNode memberRef, SourceCodePosition pos) : base(pos)
        {
            MemberReference = memberRef;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
