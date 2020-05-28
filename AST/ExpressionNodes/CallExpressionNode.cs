using Deslang.AST.ArgumentNodes;
using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ExpressionNodes
{
    public class CallExpressionNode : PrimaryExpressionNode
    {
        public IdentifierNode Identifier { get; }   // identifier associated with the call
        public ArgumentSequenceNode Arguments { get; }

        public CallExpressionNode(IdentifierNode ident, ArgumentSequenceNode args, SourceCodePosition pos) 
            : base(pos)
        {
            Identifier = ident;
            Arguments = args;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
