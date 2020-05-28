using Deslang.AST.ParameterNodes;
using Deslang.AST.TerminalNodes;
using Deslang.AST.TypeNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class InterfaceMethodDeclaringNode : DeclaringNode
    {
        public IdentifierNode Identifier { get; }
        public TypeNode Type { get; }
        public ParameterSequenceNode Parameters { get; }

        public InterfaceMethodDeclaringNode(IdentifierNode ident, TypeNode type, ParameterSequenceNode parameters,
            SourceCodePosition pos) : base(pos)
        {
            Identifier = ident;
            Type = type;
            Parameters = parameters;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
