using Deslang.AST.DeclaringNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class MethodTypeNode : TypeNode
    {
        public TypeNode Type { get; }
        public string ParentClassName { get; } // name of the class in which the method is defined

        public MethodTypeNode(TypeNode type, string parentClassName, SourceCodePosition pos) : base(pos)
        {
            Type = type;
            ParentClassName = parentClassName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MethodTypeNode))
            {
                // AnyTypeNode is only used by the standard library methods and never during type checking
                if (obj is AnyTypeNode)
                {
                    return true;
                }
                return false;
            }
            return Type.Equals(((MethodTypeNode)obj).Type);
        }

        public override int GetHashCode()
        {
            return typeof(MethodTypeNode).GetHashCode();
        }

        public override object Accept(IVisitor v, object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
