using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class IntTypeNode : TypeNode
    {
        public IntTypeNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is IntTypeNode))
            {
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
            return typeof(IntTypeNode).GetHashCode();
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return "int";
        }
    }
}
