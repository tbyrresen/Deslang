using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class VarDeclaringNode : DeclaringNode
    {
        public IdentifierNode Identifier { get; }
        public TypeNode Type { get; set; }  // setter is used by the type checker to denote declarings of unknown types to error type
        public bool IsInitialized { get; set; } // used by the type checker to test for use of unitiliazed variables

        public VarDeclaringNode(IdentifierNode ident, TypeNode type, SourceCodePosition pos) : base(pos)
        {
            Identifier = ident;
            Type = type;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
