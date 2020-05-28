using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class BooleanTypeNode : TypeNode
    {
        public BooleanTypeNode(SourceCodePosition pos) : base(pos)
        {

        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BooleanTypeNode))
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
            return typeof(BooleanTypeNode).GetHashCode();
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return "boolean";
        }
    }
}
