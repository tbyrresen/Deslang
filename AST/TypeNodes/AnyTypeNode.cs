using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class AnyTypeNode : TypeNode
    {
        public AnyTypeNode(SourceCodePosition pos) : base(pos)
        {
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return typeof(AnyTypeNode).GetHashCode();
        }

        public override object Accept(IVisitor v, object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return "anyType";
        }
    }
}
