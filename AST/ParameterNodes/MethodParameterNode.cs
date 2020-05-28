using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.ParameterNodes
{
    public class MethodParameterNode : ParameterNode
    {
        public TypeNode Type { get; set; }
        public IdentifierNode Identifier { get; }        

        public MethodParameterNode(TypeNode type, IdentifierNode ident, SourceCodePosition pos) 
            : base(pos)
        {          
            Type = type;
            Identifier = ident;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
