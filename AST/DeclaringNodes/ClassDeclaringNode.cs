using Deslang.AST.ArgumentNodes;
using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.DeclaringNodes
{
    public class ClassDeclaringNode : DeclaringNode
    {
        public IdentifierNode Identifier { get; }
        public DeclaringSequenceNode Implements { get; }
        public DeclaringSequenceNode Variables { get; }
        public DeclaringNode Constructor { get; }
        public DeclaringSequenceNode Methods { get; }

        public ClassDeclaringNode(IdentifierNode ident, DeclaringSequenceNode implements, DeclaringSequenceNode vars, 
            DeclaringNode constructor, DeclaringSequenceNode methods, SourceCodePosition pos) : base(pos)
        {
            Identifier = ident;
            Implements = implements;
            Variables = vars;
            Constructor = constructor;
            Methods = methods;
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }
    }
}
