using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class ErrorTypeNode : TypeNode
    {
        // type used only to indicate error on types in type checker
        public ErrorTypeNode(SourceCodePosition pos) : base(pos)
        {
        }      

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return typeof(ErrorTypeNode).GetHashCode();
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return "ErrorType";
        }
    }
}
