using Deslang.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deslang.AST.TypeNodes
{
    public abstract class TypeNode : AST, IEquatable<Object>
    {
        public TypeNode(SourceCodePosition pos) : base(pos)
        {
        }

        public abstract override bool Equals(Object obj);
        public abstract override int GetHashCode();
    }
}
