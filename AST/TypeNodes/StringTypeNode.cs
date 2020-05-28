using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class StringTypeNode : TypeNode
    {
        public StringTypeNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is StringTypeNode))
            {
                // AnyTypeNode is only used by the standard library methods and never during type checking
                if (obj is AnyTypeNode)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return typeof(StringTypeNode).GetHashCode();
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return "string";
        }
    }
}
