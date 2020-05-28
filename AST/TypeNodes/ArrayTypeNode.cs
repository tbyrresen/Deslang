using Deslang.AST.ExpressionNodes;
using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class ArrayTypeNode : TypeNode
    {
        public TypeNode Type { get; }
        public ExpressionNode Size { get; }   

        public ArrayTypeNode(TypeNode type, ExpressionNode size, SourceCodePosition pos) : base(pos)
        {
            Type = type;
            Size = size;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ArrayTypeNode))
            {
                // AnyTypeNode is only used by the standard library methods and never during type checking
                if (obj is AnyTypeNode)
                {
                    return true;
                }
                return false;
            }
            return Type.Equals(((ArrayTypeNode)obj).Type);
        }

        public override int GetHashCode()
        {
            return typeof(ArrayTypeNode).GetHashCode();
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return Type.ToString() + "[]";
        }
    }
}