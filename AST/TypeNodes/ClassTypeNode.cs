using Deslang.AST.TerminalNodes;
using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public class ClassTypeNode : TypeNode
    {
        public IdentifierNode ClassName { get; }

        public ClassTypeNode(IdentifierNode className, SourceCodePosition pos) : base(pos)
        {
            ClassName = className;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ClassTypeNode))
            {
                // AnyTypeNode is only used by the standard library methods and never during type checking
                if (obj is AnyTypeNode)
                {
                    return true;
                }
                return false;
            }
            return ClassName.Value.CompareTo(((ClassTypeNode)obj).ClassName.Value) == 0; 
        }

        public override int GetHashCode()
        {
            return ClassName.Value.GetHashCode();
        }

        public override Object Accept(IVisitor v, Object o)
        {
            return v.Visit(this, o);
        }

        public override string ToString()
        {
            return ClassName.Value;
        }
    }
}
