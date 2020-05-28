using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class NullTypeNode : TypeNode
    {
        public NullTypeNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override bool Equals(object obj)
        {
            // AnyTypeNode is only used by the standard library methods and never during type checking
            if (obj is AnyTypeNode)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return typeof(NullTypeNode).GetHashCode();
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return "null";
        }
    }
}
